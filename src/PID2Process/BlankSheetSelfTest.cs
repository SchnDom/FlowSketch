using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfSharp.Pdf.IO;
namespace PID2Process;

internal static class BlankSheetSelfTest
{
    internal static async Task Run(string output,Action<bool,string> check)
    {
        var p=ProjectServices.NewBlank();check(p.BlankSheet is not null&&p.Pdf.FilePath==""&&p.Pdf.PageCount==1&&ProjectServices.ValidatePdf(p,out _),"Leeres A4-Standardblatt ohne PDF verfügbar");
        check(new EngineeringProject().BlankSheet is null,"Altprojekte bleiben PDF-Projekte; fehlende Referenz wird nicht als Leerblatt umgedeutet");
        var source=new EquipmentItem{Tag="Quelle",Type=EquipmentType.MediaSource,X=.1,Y=.2,Width=.1,Height=.08,MediaName="Wasser"};
        var valve=new EquipmentItem{Tag="V-1",Type=EquipmentType.IsolationValve,Actuator=ActuatorType.Manual,X=.6,Y=.3,Width=.08,Height=.1};
        p.Equipment.AddRange([source,valve]);p.Pipes.Add(new(){Points=[new(.2,.24),new(.6,.35)],StartConnection=new(){EquipmentId=source.Id,Port="Right"},EndConnection=new(){EquipmentId=valve.Id,Port="Left"}});
        p.SignalLines.Add(new(){Points=[new(.2,.28),new(.64,.3)],StartConnection=new(){EquipmentId=source.Id,Port="Bottom"},EndConnection=new(){EquipmentId=valve.Id,Port="Top",Fraction=.5}});
        var physicalX=source.X*297;var physicalWidth=source.Width*297;
        foreach(var (format,width,height) in new[]{("A4",297d,210d),("A3",420d,297d),("A2",594d,420d)})
        {
            BlankLayout.Set(p,format,true);check(p.BlankSheet!.WidthMm==width&&p.BlankSheet.HeightMm==height,"Querformat "+format);
            BlankLayout.Set(p,format,false);check(p.BlankSheet.WidthMm==height&&p.BlankSheet.HeightMm==width,"Hochformat "+format);
        }
        check(Math.Abs(source.X*p.BlankSheet!.WidthMm-physicalX)<1e-9&&Math.Abs(source.Width*p.BlankSheet.WidthMm-physicalWidth)<1e-9,"Formatwechsel erhält physische Symbolposition und Größe");
        BlankLayout.Set(p,"A4",true);
        p.Approval=new(){IsApproved=true,ApprovedPdfHash=ProjectServices.ReferenceHash(p)};ProjectServices.ApplyApprovalStatus(p);check(!p.Approval.IsBlocked,"Leerblattfreigabe benötigt keine PDF und prüft die Blattrevision");
        BlankLayout.Set(p,"A3",true);ProjectServices.ApplyApprovalStatus(p);check(p.Approval.IsBlocked,"Layoutänderung widerruft gültige Blattrevision");
        BlankLayout.Set(p,"A4",true);BlankLayout.Set(p,"Formfrei",true);
        var oldEnd=PipeNetwork.Resolve(p,p.Pipes[0])[0];var oldSignal=SignalGeometry.Resolve(p,p.SignalLines[0])[0];
        BlankLayout.Resize(p,594,420,297,210);
        check(Math.Abs(source.X*594-(physicalX+297))<1e-9&&Math.Abs(source.Width*594-physicalWidth)<1e-9,"Formfreie Erweiterung verschiebt Ursprung ohne Geometrieverzerrung");
        var newEnd=PipeNetwork.Resolve(p,p.Pipes[0])[0];var newSignal=SignalGeometry.Resolve(p,p.SignalLines[0])[0];
        check(Math.Abs(newEnd.X*594-(oldEnd.X*297+297))<1e-9&&Math.Abs(newSignal.Y*420-(oldSignal.Y*210+210))<1e-9,"Erweiterung erhält Rohr- und Umrandungsbindungen");
        bool missing=false;try{BlankLayout.ExportSnapshot(p);}catch(InvalidOperationException){missing=true;}check(missing,"Formfrei verlangt ausdrücklich ein Exportfenster");
        p.BlankSheet!.ExportWindow=new(){XMm=297,YMm=210,WidthMm=297,HeightMm=210};
        var crop=BlankLayout.ExportSnapshot(p);check(Math.Abs(crop.Equipment[0].X*297-physicalX)<1e-9&&p.BlankSheet.WidthMm==594&&crop.BlankSheet!.WidthMm==297,"Exportfenster schneidet eigenen Snapshot; Originalzeichnung unverändert");
        var file=Path.Combine(output,"blank-sheet.pid2process.json");ProjectServices.Save(file,p);var loaded=ProjectServices.Load(file);
        check(loaded.BlankSheet!.Format=="Formfrei"&&loaded.BlankSheet.ExportWindow!.XMm==297&&loaded.Pipes[0].StartConnection!.EquipmentId==source.Id,"Leerblatt, Layout, Fenster und Verbindungen im JSON-Roundtrip");
        var pdf=Path.Combine(output,"blank-sheet.pdf");await DocumentExport.ExportAsync(p,pdf,true,0,null);await DocumentExport.ExportAsync(p,Path.Combine(output,"blank-sheet.jpg"),false,0,null);
        using(var document=PdfReader.Open(pdf,PdfDocumentOpenMode.Import))check(document.PageCount==1&&Math.Abs(document.Pages[0].Width.Millimeter-297)<.1&&Math.Abs(document.Pages[0].Height.Millimeter-210)<.1,"PDF-Export ohne Referenz mit gewählten Ausschnittmaßen");
        await ExportService.ExportAsync(p,Path.Combine(output,"blank-review"));check(File.Exists(Path.Combine(output,"blank-review","page-1.png"))&&ProjectServices.Load(Path.Combine(output,"blank-review","project-data.json")).BlankSheet!.WidthMm==297,"Review-Paket exportiert Leerblattausschnitt ohne Referenz-PDF");
        var window=new MainWindow();window.CreateBlankDocument();
        void SaveView(string name){var view=(FrameworkElement)window.Content;view.Width=1500;view.Height=900;view.Measure(new Size(1500,900));view.Arrange(new Rect(0,0,1500,900));view.UpdateLayout();var bitmap=new RenderTargetBitmap(1500,900,96,96,PixelFormats.Pbgra32);bitmap.Render(view);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(Path.Combine(output,name));encoder.Save(stream);}
        SaveView("blank-editor.png");check(window.ActiveProject!.BlankSheet is not null&&!window.IsVisible,"Standarddokument sofort verfügbar, ohne Fensteranzeige geprüft");
        object? Invoke(string name,params object?[] args)=>typeof(MainWindow).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(window,args);
        Invoke("Pipe_Click",window,new RoutedEventArgs());Invoke("AddPipePoint",new Point(120,120),false);Invoke("AddPipePoint",new Point(220,150),false);Invoke("FinishPipe",new object?[]{null});
        check(window.ActiveProject.Pipes.Count==1,"Rohrwerkzeug zeichnet sofort ohne PDF");
        BlankLayout.Set(window.ActiveProject,"Formfrei",true);Invoke("ExportWindow_Click",window,new RoutedEventArgs());Invoke("BeginExportWindow",new Point(100,100));Invoke("FinishExportWindow",new Point(600,400));
        check(window.ActiveProject.BlankSheet!.ExportWindow is {WidthMm:>0,HeightMm:>0},"Druck-/Exportfenster übernimmt aufgezogenen Bereich");
        var beforeFrameX=window.ActiveProject.BlankSheet.ExportWindow!.XMm;
        object?[] panArgs={-100d,-100d};typeof(MainWindow).GetMethod("ExtendFreeCanvas",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(window,panArgs);
        check(window.ActiveProject.BlankSheet.WidthMm==594&&window.ActiveProject.BlankSheet.HeightMm==420&&Math.Abs(window.ActiveProject.BlankSheet.ExportWindow.XMm-beforeFrameX-297)<1e-9,"Pan über linken/oberen Rand erweitert formfreie Fläche und führt Exportrahmen nach");
        Invoke("ExportWindow_Click",window,new RoutedEventArgs());Invoke("BeginExportWindow",new Point(20,20));Invoke("CancelPlacement");
        check(window.ActiveProject.BlankSheet.ExportWindow is not null&&Math.Abs(window.ActiveProject.BlankSheet.ExportWindow.XMm-beforeFrameX-297)<1e-9,"Abbruch einer neuen Rahmenauswahl erhält vorhandenes Exportfenster");
        window.ActiveProject.BlankSheet=new(){Format="Formfrei",ExportWindow=new(){XMm=15,YMm=20,WidthMm=250,HeightMm=160}};window.ActiveProject.Equipment.Clear();window.ActiveProject.Pipes.Clear();window.ActiveProject.SignalLines.Clear();window.ActiveProject.Equipment.AddRange(crop.Equipment);window.ActiveProject.Pipes.AddRange(crop.Pipes);window.ActiveProject.SignalLines.AddRange(crop.SignalLines);Invoke("Draw");Invoke("Fit");SaveView("free-frame-editor.png");
        window.ClearTestDirtyFlags();
    }
}
