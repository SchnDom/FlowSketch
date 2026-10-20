using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
namespace PID2Process;

internal static class LegendSelfTest
{
    internal static async Task Run(string output,Action<bool,string> check)
    {
        var project=ProjectServices.NewBlank();var item=new EquipmentItem{Type=EquipmentType.Legend,Tag="LEG-1",Variant="Medienlegende",X=.1,Y=.15,Width=.55,Height=.493,Color="#303030",Opacity=1,LegendEntries=LegendVisual.PidEntries()};
        project.Equipment.Add(item);
        check(item.LegendEntries.Select(x=>x.Text).SequenceEqual(new[]{"Luft","Druckluft","Dampf (Wasserdampf)","Wasser","Silikonöl","Wasser-Glykol Mischung"}),"Legendenbezeichnungen aus beiden vorhandenen PIDs");
        check(PipeNetwork.Ports(item).Length==0&&SignalGeometry.Ports(item).Length==0,"Legende hat keine Rohr-/Wirklinienanschlüsse");
        var editor=new LegendEditor(null,item,project);editor.Rows[0].Text="Geänderter Text";editor.Rows[0].Color="ungültig";editor.TitleBox.Text="Eigene Legende";
        check(editor.TryApply() is not null&&item.LegendTitle=="Legende"&&item.LegendEntries[0].Text=="Luft","Ungültige Legende übernimmt keine Teiländerungen");
        editor.Rows[0].Color="#939598";check(editor.TryApply() is null&&item.LegendTitle=="Eigene Legende"&&item.LegendEntries[0].Text=="Geänderter Text","Legendtitel und Textfenster vollständig editierbar");
        editor.Rows.Add(new(){Text="Neues Medium",Color="#AA22BB"});check(item.LegendEntries.Count==6,"Dialogänderungen nach Übernahme beeinflussen Modell nicht nachträglich");
        item.LegendTitle="Legende";item.LegendEntries=LegendVisual.PidEntries();
        var file=Path.Combine(output,"legend.pid2process.json");ProjectServices.Save(file,project);var loaded=ProjectServices.Load(file);
        check(loaded.Equipment[0].LegendEntries[2].Color=="#FF00FF"&&loaded.Equipment[0].LegendEntries.Count==6,"Legendeneinträge und Farben im JSON-Roundtrip");
        var copy=OverlayTransforms.Copy(project,item);copy.LegendEntries[0].Text="Kopiert";check(item.LegendEntries[0].Text=="Luft","Kopierte Legenden besitzen unabhängige Textfenster");project.Equipment.Remove(copy);
        var bitmap=PageVisual.Render(project,1,1200,210d/297,null,await new DocumentBackground(project).RenderAsync(0,1200));
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(Path.Combine(output,"legend.png")))png.Save(stream);
        var view=(FrameworkElement)editor.Content;view.Margin=new Thickness(0);view.Width=680;view.Height=660;view.Measure(new Size(680,660));view.Arrange(new Rect(0,0,680,660));view.UpdateLayout();var shot=new RenderTargetBitmap(680,660,96,96,System.Windows.Media.PixelFormats.Pbgra32);shot.Render(view);var shotPng=new PngBitmapEncoder();shotPng.Frames.Add(BitmapFrame.Create(shot));using(var stream=File.Create(Path.Combine(output,"legend-editor.png")))shotPng.Save(stream);
        await DocumentExport.ExportAsync(project,Path.Combine(output,"legend.pdf"),true,0,null);
        await ExportService.ExportAsync(project,Path.Combine(output,"legend-review"));
        check(File.Exists(Path.Combine(output,"legend-review","viewer.html")),"Legende im PDF- und Review-Export verfügbar");
    }
}
