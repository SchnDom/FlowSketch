using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace PID2Process;
internal static class ModernUiSelfTest
{
    internal static async Task Run(string output,Action<bool,string> check)
    {
        var graphics=new List<object>();var atlas=ProjectServices.NewBlank();var i=0;
        foreach(var (variant,dx,dy) in new[]{("Linie",240d,0d),("Linie",0d,240d),("Pfeil",240d,0d),("Pfeil",0d,240d),("Pfeil",180d,180d),("Doppelpfeil",-180d,180d)})
        {
            var item=new EquipmentItem{Type=EquipmentType.Drawing,Variant=variant,Tag="Grafik-"+i,Color="#147FB3",Opacity=1};item.Drawing.StrokeWidth=3;var x=150+(i%3)*600;var y=180+(i/3)*600;
            DrawingGeometry.SetEnds(item,new(x/2000d,y/1400d),new((x+dx)/2000d,(y+dy)/1400d),2000,1400);
            var visual=(Grid)AnnotationVisual.Symbol(item,2000,1400);var canvas=visual.Children.OfType<Canvas>().Single();var line=canvas.Children.OfType<System.Windows.Shapes.Path>().Last();
            check(Math.Abs(line.StrokeThickness-3)<1e-10&&!visual.Children.OfType<Viewbox>().Any(),"Grafik neu gezeichnet mit konstanter Stärke: "+variant+" "+dx+"/"+dy);
            var rect=new Rect(item.X*2000,item.Y*1400,item.Width*2000,item.Height*1400);check(DrawingGeometry.Strokes(item,2000,1400).SelectMany(x=>x).All(rect.Contains),"Pfeil-/Linienkontur liegt innerhalb der Objektgrenzen "+i);
            graphics.Add(new{item=JsonSerializer.Deserialize<EquipmentItem>(JsonSerializer.Serialize(item,ProjectServices.JsonOptions),ProjectServices.JsonOptions),width=2000,height=1400,path=DrawingGeometry.Path(item,2000,1400)});atlas.Equipment.Add(item);i++;
        }
        var arrow=atlas.Equipment[4];var before=DrawingGeometry.Ends(arrow);DrawingGeometry.SetEnds(arrow,before.Start,new(.45,.65),2000,1400);check(DrawingGeometry.Ends(arrow).Start.X==before.Start.X&&DrawingGeometry.Ends(arrow).Start.Y==before.Start.Y,"Endpunktänderung erhält den Gegenpunkt");
        var end=DrawingGeometry.Ends(arrow).End.X;OverlaySelection.Move(atlas,new HashSet<Guid>{arrow.Id},.02,.01);check(Math.Abs(DrawingGeometry.Ends(arrow).End.X-end-.02)<1e-10,"Verschieben einer Linie versetzt beide Endpunkte");
        var source=ProjectServices.NewBlank();var valve=new EquipmentItem{Tag="XV-1",Type=EquipmentType.IsolationValve,Actuator=ActuatorType.Manual,X=.2,Y=.2};var pump=new EquipmentItem{Tag="P-1",Type=EquipmentType.PumpMotor,X=.4,Y=.2};source.Equipment.AddRange([valve,pump]);var pipe=new PipeSegment{Points=[new(.24,.22),new(.4,.22)],StartConnection=new(){EquipmentId=valve.Id},EndConnection=new(){EquipmentId=pump.Id,Port="Left"}};source.Pipes.Add(pipe);OverlaySelection.Group(source,new HashSet<Guid>{valve.Id,pump.Id,pipe.Id});
        var scenario=new Scenario{Name="Betrieb"};scenario.EquipmentStates[valve.Id]="Offen";source.Scenarios.Add(scenario);var step=new ControlStep{ScenarioId=scenario.Id,Name="Schritt 1"};step.EquipmentStates[pump.Id]="Ein";source.ControlSteps.Add(step);
        var json=JsonSerializer.Serialize(source,ProjectServices.JsonOptions);var clip=OverlayClipboard.Copy(source,new HashSet<Guid>{valve.Id},1);var target=ProjectServices.NewBlank();target.Equipment.Add(new(){Tag="XV-1"});var selected=OverlayClipboard.Paste(target,clip,1,2000,1400,new(.5,.5));
        check(selected.Count==3&&target.Equipment.Count==3&&target.Pipes.Count==1,"STRG-C/V-Modell übernimmt Mehrfachgruppe mit Rohr auf anderes Blatt");
        check(selected.All(id=>id!=valve.Id&&id!=pump.Id&&id!=pipe.Id)&&target.Equipment.Select(x=>x.Tag).Distinct().Count()==3,"Einfügen erzeugt neue IDs und kollisionsfreie Tags");
        var pastedValve=target.Equipment.Single(x=>x.Type==EquipmentType.IsolationValve);var pastedPump=target.Equipment.Single(x=>x.Type==EquipmentType.PumpMotor);check(target.Pipes[0].StartConnection?.EquipmentId==pastedValve.Id&&target.Pipes[0].EndConnection?.EquipmentId==pastedPump.Id,"Interne Rohrbindungen werden auf kopierte Geräte umgebunden");
        check(target.Scenarios.Single().EquipmentStates[pastedValve.Id]=="Offen"&&target.ControlSteps.Single().EquipmentStates[pastedPump.Id]=="Ein","Kopierte Szenario-/Schrittwerte bleiben den neuen IDs zugeordnet");
        check(JsonSerializer.Serialize(source,ProjectServices.JsonOptions)==json,"Kopieren und Einfügen verändern das Quellprojekt nicht");
        var lone=OverlayClipboard.Copy(source,new HashSet<Guid>{pipe.Id},1); // Group expansion intentionally copies the complete group.
        OverlaySelection.Ungroup(source,new HashSet<Guid>{valve.Id,pump.Id,pipe.Id});lone=OverlayClipboard.Copy(source,new HashSet<Guid>{pipe.Id},1);check(lone.Selection.Pipes[0].StartConnection is null&&lone.Selection.Pipes[0].EndConnection is null,"Nicht mitkopierte externe Rohrbindungen werden geometrieschonend gelöst");
        var userFile=System.IO.Path.Combine(Environment.CurrentDirectory,"bin","0.4.3","graphics.pid2process.json");
        if(File.Exists(userFile)){var hash=ProjectServices.HashFile(userFile);var user=ProjectServices.Load(userFile);Save(user,System.IO.Path.Combine(output,"graphics-corrected.png"));check(ProjectServices.HashFile(userFile)==hash,"Nutzerdatei graphics gelesen und Prüfbild erzeugt, Quelldatei unverändert");await ExportService.ExportAsync(user,System.IO.Path.Combine(output,"graphics-review"));}
        Save(atlas,System.IO.Path.Combine(output,"drawing-orientations.png"));File.WriteAllText(System.IO.Path.Combine(output,"drawing-geometry-cases.json"),JsonSerializer.Serialize(graphics,ProjectServices.JsonOptions));
        var window=new MainWindow();window.Width=1500;window.Height=900;window.CreateBlankDocument();var root=(FrameworkElement)window.Content;root.Width=1500;root.Height=900;root.Measure(new Size(1500,900));root.Arrange(new Rect(0,0,1500,900));root.UpdateLayout();typeof(MainWindow).GetMethod("Fit",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(window,null);root.UpdateLayout();await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);
        await window.CheckEditorCorrections(check,output);HistorySelfTest.Run(check);window.CheckChromeAndHistory(check,output);CheckValveSelfTest.Run(output,check);window.CheckDialogAndFlowUi(check,output);window.CheckMediaAndPipeUi(check,output);
        window.CheckReferenceUi(check);
        var bitmap=new RenderTargetBitmap(1500,900,96,96,PixelFormats.Pbgra32);bitmap.Render((Visual)window.Content);Write(bitmap,System.IO.Path.Combine(output,"modern-editor.png"));window.ClearTestDirtyFlags();window.Close();
        var icon=InformationWindow.IconImage();var converted=new FormatConvertedBitmap(icon,PixelFormats.Bgra32,null,0);var pixels=new byte[converted.PixelWidth*converted.PixelHeight*4];converted.CopyPixels(pixels,converted.PixelWidth*4,0);check(icon.PixelWidth>=256&&pixels[3]==0&&Enumerable.Range(0,converted.PixelWidth*converted.PixelHeight).Any(i=>pixels[i*4+3]>0),"Taskleistensymbol enthält hochauflösende transparente Iconframes");
    }
    private static void Save(EngineeringProject p,string path){var white=BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{255,255,255,255},4);Write(PageVisual.Render(p,1,1800,p.Pdf.PageAspectRatios.FirstOrDefault(.708),null,white),path);}
    private static void Write(BitmapSource bitmap,string path){var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var file=File.Create(path);encoder.Save(file);}
}
