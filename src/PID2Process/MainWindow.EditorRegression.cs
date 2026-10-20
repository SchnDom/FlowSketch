using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
public partial class MainWindow
{
    internal async Task CheckEditorCorrections(Action<bool,string> check,string output)
    {
        var project=Current!.Project;
        var a=new EquipmentItem{Tag="VP101",Function="Zulauf",InstallationLocation="Skid A",X=.2,Y=.2};
        var b=new EquipmentItem{Tag="XV202",Function="Regelung VP",X=.5,Y=.5};project.Equipment.AddRange([a,b]);Draw();
        SelectObject(a.Id);ShowHandles(a);check(Sheet.Children.OfType<Thumb>().Any(),"Ausgewähltes Symbol besitzt Größenhandgriff");
        SelectObject(b.Id);check(!Sheet.Children.OfType<Thumb>().Any(),"Auswahlwechsel entfernt Griffe des vorherigen Symbols");
        ShowHandles(b);ClearSelection();check(!Sheet.Children.OfType<Thumb>().Any()&&!Sheet.Children.OfType<System.Windows.Shapes.Rectangle>().Any(x=>Equals(x.Tag,"selection-outline")),"Abwählen entfernt sämtliche Griffe und Auswahlrahmen sofort");
        string[] Entries()=>PlanMenu().Items.Cast<object>().Select(x=>x is MenuItem m?(string)m.Header:"|").ToArray();
        check(Entries().SequenceEqual(new[]{"Rohrleitung einfügen","Wirklinie einfügen","|","Kopieren","Einfügen","Löschen","|","Gemeinsame Eigenschaften"}),"Leere Planfläche: Kontextmenü entspricht gewünschter Reihenfolge");
        SelectObject(a.Id);check(Entries().SequenceEqual(new[]{"Objekt bearbeiten","|","Kopieren","Einfügen","Löschen","|","Rohrleitung einfügen","Wirklinie einfügen"}),"Einzelauswahl: Kontextmenü entspricht gewünschter Reihenfolge");
        SelectObject(b.Id,true);check(Entries().SequenceEqual(new[]{"Objekte bearbeiten","Gruppieren","Gruppierung aufheben","|","Kopieren","Einfügen","Löschen","|","Rohrleitung einfügen","Wirklinie einfügen"}),"Mehrfachauswahl: Kontextmenü mit gemeinsamen Eigenschaften und Gruppen");
        check(EquipmentSearch.Matches(a,"vp*")&&!EquipmentSearch.Matches(b,"VP*")&&EquipmentSearch.Matches(b,"*VP*")&&EquipmentSearch.Matches(a,"VP10?")&&!EquipmentSearch.Matches(a,"XP10")&&EquipmentSearch.Matches(a,"zulauf")&&EquipmentSearch.Matches(a,"Skid*")&&EquipmentSearch.Matches(a,""),"Betriebsmittelsuche: Wildcards pro Feld, Teiltext und Groß-/Kleinschreibung");
        _inspectSearch.Text="VP*";check(_inspectLists[0].Items.Count==1,"Wildcard-Suchfeld filtert tatsächlich die Betriebsmittelliste");((Grid)_inspectSearch.Parent).Children.OfType<Button>().Single().RaiseEvent(new RoutedEventArgs(Button.ClickEvent));check(_inspectSearch.Text==""&&_inspectLists[0].Items.Count==2,"Suchfilter zurücksetzen zeigt wieder alle Betriebsmittel");
        var demo=LoadEmbeddedDemo();check(demo.Equipment.Count>=30&&demo.Scenarios.Count==7&&demo.ControlSteps.Count==26,"Demo ist ohne externe Begleitdatei vollständig aus EXE ladbar");
        ClearSelection();Fit();((FrameworkElement)Content).UpdateLayout();QueueOverview();await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);UpdateOverview();var full=_overviewSheet;var pixels=_overviewThumbnail!;
        Zoom(.3);((FrameworkElement)Content).UpdateLayout();await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);UpdateOverview();
        check(Math.Abs(full.Width-_overviewSheet.Width)<.01&&Math.Abs(full.Height-_overviewSheet.Height)<.01&&_overviewThumbnail!.PixelWidth==pixels.PixelWidth&&_overviewThumbnail.PixelHeight==pixels.PixelHeight,"Planübersicht behält vollständige Dokumentgröße beim Herauszoomen unter Einpassen");
        check(_overviewWindow is {} frame&&Math.Abs(frame.Width-_overviewSheet.Width)<.01&&Math.Abs(frame.Height-_overviewSheet.Height)<.01,"Bei vollständig sichtbarem Dokument begrenzt sich das Sichtfenster auf das ganze Blatt");
        var bytes=new byte[4];new FormatConvertedBitmap(_overviewThumbnail,PixelFormats.Bgra32,null,0).CopyPixels(new Int32Rect(5,5,1,1),bytes,4,0);
        check(bytes[0]==255&&bytes[1]==255&&bytes[2]==255&&bytes[3]==255,"Unabhängiges Übersichtsbild zeigt vollständigen weißen Plan bis an die Ecke");
        var root=(FrameworkElement)Content;root.UpdateLayout();var bitmap=new RenderTargetBitmap(1500,900,96,96,PixelFormats.Pbgra32);bitmap.Render(root);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var file=System.IO.File.Create(System.IO.Path.Combine(output,"editor-corrections-0.5.1.png")))encoder.Save(file);
        project.Equipment.Clear();ClearSelection();Fit();Draw();
    }
}
