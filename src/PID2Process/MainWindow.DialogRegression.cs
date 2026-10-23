using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
public partial class MainWindow
{
    internal void CheckDialogAndFlowUi(Action<bool,string> check,string output)
    {
        OpenDemo();var p=Current!.Project;var scenario=p.Scenarios[1];var step=ScenarioSteps.First(p,scenario.Id)!;SetPlanSelection(p,scenario.Id,step.Id);RefreshInspector();Draw();var root=(FrameworkElement)Content;root.UpdateLayout();
        IEnumerable<DependencyObject> Visuals(DependencyObject parent){for(var i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);yield return child;foreach(var descendant in Visuals(child))yield return descendant;}}
        check(Visuals(ScenarioBox).OfType<TextBlock>().Any(x=>x.Text==scenario.Name)&&Visuals(StepBox).OfType<TextBlock>().Any(x=>x.Text==step.Name),"Obere Auswahl zeigt Szenario-/Schrittnamen statt .NET-Typnamen");
        check(InspectorTabs.Items.Count==3&&((TabItem)InspectorTabs.Items[1]).Header.ToString()=="Abläufe","Rechte Verwaltung vereint Szenarien und Schritte im Tab Abläufe");
        check(_inspectLists[2].Items.OfType<ControlStep>().All(x=>x.ScenarioId==scenario.Id)&&_inspectLists[2].Items.Count==p.ControlSteps.Count(x=>x.ScenarioId==scenario.Id),"Abläufe zeigt ausschließlich die Schritte des ausgewählten Szenarios");
        _inspectLists[1].SelectedItem=p.Scenarios[2];check((ScenarioBox.SelectedItem as Scenario)?.Id==p.Scenarios[2].Id&&_inspectLists[2].Items.OfType<ControlStep>().All(x=>x.ScenarioId==p.Scenarios[2].Id),"Szenarioauswahl im Ablauf-Tab aktualisiert Plan und untergeordnete Schritte");
        InspectorTabs.SelectedIndex=1;root.UpdateLayout();Save(root,"flowsketch-0.7.0.png");
        var device=p.Equipment.First(SymbolCatalog.Valve);var before=System.Text.Json.JsonSerializer.Serialize(device,ProjectServices.JsonOptions);
        EquipmentDialogResult=dialog=>
        {
            var body=(FrameworkElement)dialog.Content;body.Width=740;body.Height=720;body.Measure(new Size(740,720));body.Arrange(new Rect(0,0,740,720));body.UpdateLayout();
            var groups=Visuals(body).OfType<GroupBox>().Where(x=>x.Visibility==Visibility.Visible).Select(x=>x.Header?.ToString()).ToArray();
            check(new[]{"Kennzeichnung","Objekteigenschaften","Farben","Größe"}.All(groups.Contains),"Echte Gerätedaten gruppieren Identität, Objektmerkmale und Darstellung kompakt");
            check(dialog.Background.ToString()=="#FF20262E"&&dialog.Control("Tag").ForegroundValue()=="#FFE7EDF5","Gerätedialog und Eingaben besitzen dunklen Hintergrund mit heller Schrift");check(dialog.Title=="Gerätedaten – "+device.Tag,"Gerätedialog trägt den Objekt-Tag im Fenstertitel");Save(body,"device-dialog-0.7.0.png");return false;
        };
        check(!EditEquipment(device)&&before==System.Text.Json.JsonSerializer.Serialize(device,ProjectServices.JsonOptions),"Abbruch des neu gruppierten Gerätedialogs verändert das Objekt nicht");
        EquipmentDialogResult=dialog=>{var body=(FrameworkElement)dialog.Content;body.Width=740;body.Height=800;body.Measure(new Size(740,800));body.Arrange(new Rect(0,0,740,800));body.UpdateLayout();Save(body,"sensor-dialog-0.7.0.png");return false;};EditEquipment(p.Equipment.First(x=>x.Type==EquipmentType.Instrument));EquipmentDialogResult=null;
        var free=new EditorDialog(null,"Freie Eingabe");free.Choice("Funktion","Freier Wert","Option");var box=(ComboBox)free.Control("Funktion");box.ApplyTemplate();check(box.Template.FindName("PART_EditableTextBox",box) is TextBox&&free.Value("Funktion")=="Freier Wert","Dunkle ComboBox erhält freie Texteingaben");free.Close();
        var media=new MediaEditor(null,p);RenderWindow(media,720,650,"media-dialog-0.7.0.png");var states=new StateEditor(null,p,scenario,step);RenderWindow(states,1100,780,"scenario-dialog-0.7.0.png");
        void RenderWindow(Window window,double width,double height,string file){var body=(FrameworkElement)window.Content;body.Margin=new Thickness(0);body.Width=width;body.Height=height;body.Measure(new Size(width,height));body.Arrange(new Rect(0,0,width,height));body.UpdateLayout();Save(body,file);window.Close();}
        void Save(FrameworkElement visual,string file){visual.UpdateLayout();var bitmap=new RenderTargetBitmap((int)visual.ActualWidth,(int)visual.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=System.IO.File.Create(System.IO.Path.Combine(output,file));encoder.Save(stream);}
    }
}
internal static class ControlColorTest
{
    internal static string ForegroundValue(this FrameworkElement control)=>control is Control input?input.Foreground.ToString():"";
}
