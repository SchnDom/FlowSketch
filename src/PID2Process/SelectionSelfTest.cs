using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

internal static class SelectionSelfTest
{
    internal static void Run(EngineeringProject original,string output,Action<bool,string> check)
    {
        var p=DocumentExport.Snapshot(original);var valve=p.Equipment.Single(x=>x.Tag=="XV-101");var source=p.Equipment.Single(x=>x.Type==EquipmentType.MediaSource);var pipe=p.Pipes.Single(x=>x.Tag=="L-VOR");var child=p.Pipes.Single(x=>x.Tag=="L-ABZ");var parent=p.Pipes.Single(x=>x.Tag=="L-NACH");
        var ids=OverlaySelection.InRectangle(p,1,new Rect(0,0,.9,.7));check(ids.Contains(valve.Id)&&ids.Contains(pipe.Id)&&ids.Contains(child.Id)&&!ids.Contains(p.Equipment.Last().Id),"Auswahlrahmen erfasst Symbole und Rohrpfade nur auf aktiver Seite");
        var partial=OverlaySelection.InRectangle(p,1,new Rect(.34,.29,.03,.03));check(!partial.Contains(valve.Id),"Fensterauswahl erfordert vollständiges Einschließen");
        var selected=new HashSet<Guid>{valve.Id,source.Id,pipe.Id};var group=OverlaySelection.Group(p,selected);check(OverlaySelection.ExpandGroups(p,[pipe.Id],1).SetEquals(selected),"Gruppenklick erweitert Auswahl über Symbole und Rohr");
        var batch=new BatchPropertiesEditor(null,p,selected);batch.SetField("Funktion","Versorgung");batch.SetField("Einbauort (+)","+B");batch.SetField("Farbe","#D93282");batch.SetField("Deckkraft (%)","60");batch.SetField("Breite je Symbol (px)","360");batch.SetField("Höhe je Symbol (px)","198,24");check(batch.TryBuildPatch(out _),"Gemeinsame Eigenschaften validiert, einschließlich deutscher Dezimalzahl");
        var oldPipe=PipeNetwork.Resolve(p,pipe);OverlaySelection.Apply(p,selected,batch.Patch!);check(valve.Function=="Versorgung"&&pipe.Function==valve.Function&&pipe.InstallationLocation=="+B"&&pipe.Color==valve.Color&&pipe.Opacity==.6&&Math.Abs(valve.Width-.18)<1e-9&&valve.Height==.14,"Batch ändert Funktion/Ort/Farbe/Deckkraft und Symbolmaße");
        check(pipe.Points.Count==oldPipe.Count&&pipe.StartConnection?.EquipmentId==source.Id,"Symbolmaße lösen keine Rohranschlüsse");
        var pathBefore=PipeNetwork.Resolve(p,child)[0];OverlaySelection.Move(p,new HashSet<Guid>{parent.Id},.04,.03);check(PipeNetwork.Resolve(p,child)[0].X>pathBefore.X,"Gruppenverschiebung führt angeschlossene Abzweige nach");
        var saved=Path.Combine(output,"selection-roundtrip.pid2process.json");ProjectServices.Save(saved,p);var roundtrip=ProjectServices.Load(saved);check(roundtrip.Equipment.Single(x=>x.Id==source.Id).GroupId==group&&roundtrip.Pipes.Single(x=>x.Id==pipe.Id).Color=="#D93282"&&roundtrip.Equipment.Single(x=>x.Id==source.Id).TemperatureCelsius==20,"JSON speichert Gruppierung, Rohrstil und Medienwerte");
        var snapshot=PipeNetwork.Resolve(p,child);p.Scenarios[0].PipeStates[parent.Id]="Leer";OverlaySelection.Delete(p,new HashSet<Guid>{parent.Id,valve.Id});check(!p.Pipes.Any(x=>x.Id==parent.Id)&&!p.Equipment.Any(x=>x.Id==valve.Id)&&child.StartConnection is null&&Math.Abs(child.Points[0].X-snapshot[0].X)<1e-9&&!p.Scenarios[0].PipeStates.ContainsKey(parent.Id),"Mehrfachlöschen löst Abzweige geometrieschonend und bereinigt Zustände");
        var invalid=new BatchPropertiesEditor(null,p,new HashSet<Guid>{source.Id});invalid.SetField("Deckkraft (%)","NaN");check(!invalid.TryBuildPatch(out _),"Ungültige Zahlen werden nicht übernommen");
        var wheel=new ColorWheel();wheel.SetColor(ColorPicker.Parse("#D93282"));check(wheel.Selected==ColorPicker.Parse("#D93282"),"HSV-Farbrad bewahrt ausgewählte RGB-Farbe");wheel.SelectAt(new Point(236,120));check(wheel.Selected.R>wheel.Selected.B,"Farbrad-Punkt wählt roten Farbton");check(!ColorPicker.IsValid("#XYZXYZ")&&ColorPicker.IsValid("#01ABEF"),"Hex-Farben validiert");
        check(MediaDisplay.Text(source).Contains("T: 20 °C")&&MediaDisplay.Text(source).Contains("p: 6 bar")&&MediaDisplay.Text(source).Contains("v: 2,5 m/s")&&MediaDisplay.Text(source).Contains("Netzversorgung"),"Medienobjekt zeigt Beschreibung, Temperatur, Druck und Geschwindigkeit");
        check(MediaDisplay.TryOptional("-12,5",out var temperature)&&temperature==-12.5&&!MediaDisplay.TryOptional("Infinity",out _),"Medienwerte erlauben negative Temperatur, aber keine unendlichen Werte");
        Capture((FrameworkElement)batch.Content,480,590,Path.Combine(output,"batch-properties.png"));var colorDialog=new ColorWheelDialog(null,"#D93282");Capture((FrameworkElement)colorDialog.Content,308,390,Path.Combine(output,"color-wheel.png"));
    }
    private static void Capture(FrameworkElement content,int width,int height,string path){if(content.Parent is Window w)w.Content=null;var border=new Border{Background=Brushes.White,Child=content};border.Measure(new Size(width,height));border.Arrange(new Rect(0,0,width,height));border.UpdateLayout();var image=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);image.Render(border);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var file=File.Create(path);encoder.Save(file);}
}
