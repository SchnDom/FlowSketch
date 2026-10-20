using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
internal static class ScenarioEditorSelfTest
{
    internal static void Run(string output,Action<bool,string> check)
    {
        var p=ProjectServices.NewBlank();p.Media.AddRange([new(){Name="Wasser",Type=MediumType.Liquid},new(){Name="Druckluft",Type=MediumType.Gas},new(){Name="Steine",Type=MediumType.Solid},new(){Name="Emulsion",Type=MediumType.Mixed}]);
        var dialog=new EditorDialog(null,"Medienfilter");MediaUi.AddSelection(dialog,p,MediumType.Liquid,"Wasser");var types=(ComboBox)dialog.Control("Mediumtyp");var names=(ComboBox)dialog.Control("Name des Mediums");
        foreach(var choice in MediaUi.Types)
        {
            types.SelectedItem=choice.Label;var expected=p.Media.Single(m=>m.Type==choice.Value).Name;
            check(names.Items.Cast<string>().SequenceEqual(new[]{expected})&&dialog.Value("Name des Mediums")==expected&&dialog.Value("Mediumtyp")==choice.Label,"Medienfilter verwendet aktuellen Typ: "+choice.Label);
        }
        types.SelectedItem="Gasförmig";var mediaView=(DockPanel)dialog.Content;mediaView.Background=PageVisual.Color("#20262E");mediaView.Measure(new Size(540,260));mediaView.Arrange(new Rect(0,0,540,260));mediaView.UpdateLayout();var mediaImage=new RenderTargetBitmap(540,260,96,96,PixelFormats.Pbgra32);mediaImage.Render(mediaView);var mediaEncoder=new PngBitmapEncoder();mediaEncoder.Frames.Add(BitmapFrame.Create(mediaImage));using(var mediaFile=File.Create(Path.Combine(output,"media-filter.png")))mediaEncoder.Save(mediaFile);
        types.SelectedItem="Flüssig";p.Media.RemoveAll(m=>m.Type==MediumType.Gas);types.SelectedItem="Gasförmig";check(names.Items.Count==0&&names.SelectedItem is null&&dialog.Value("Name des Mediums")=="","Leerer Medientyp behält keinen veralteten Namen");
        var valve=new EquipmentItem{Tag="V1",Type=EquipmentType.IsolationValve,Actuator=ActuatorType.Manual,X=.3,Y=.3,Width=.1,Height=.1};p.Equipment.Add(valve);
        var s1=new Scenario{Name="Szenario 1",Description="Erstes Szenario"};var s2=new Scenario{Name="Szenario 2"};p.Scenarios.AddRange([s1,s2]);
        var t1=new ControlStep{Name="Schritt 1",Description="Vorbereiten",Preconditions="Freigabe",ScenarioId=s1.Id};var t2=new ControlStep{Name="Schritt 2",Description="Öffnen",ScenarioId=s1.Id};var other=new ControlStep{Name="Schritt 1",ScenarioId=s2.Id};p.ControlSteps.AddRange([t1,t2,other]);
        var alarm=new Alarm{Name="Rückmeldung fehlt",Tag=valve.Tag,Trigger="Keine Rückmeldung",ScenarioIds=[s1.Id]};p.Alarms.Add(alarm);var original=JsonSerializer.Serialize(p,ProjectServices.JsonOptions);
        var editor=new StateEditor(null,p,s1,t1);int previews=0;string? previewState=null;Guid? previewScenario=null,previewStep=null;
        editor.Preview=(working,s,t)=>{previews++;previewScenario=s?.Id;previewStep=t?.Id;previewState=SymbolCatalog.State(working.Equipment.Single(),SceneValues.From(s,t));};
        check(editor.ScenarioPicker.Items.Cast<Scenario>().Count()==2&&editor.StepPicker.Items.Cast<ControlStep>().All(t=>t.Id==Guid.Empty||t.ScenarioId==s1.Id),"Getrennte Szenario-/Schrittwahl zeigt nur zugehörige Schritte");
        editor.Rows.Single().State="Offen";check(previews==1&&previewState=="Offen"&&previewScenario==s1.Id&&previewStep==t1.Id,"Ventiländerung aktualisiert Planvorschau unmittelbar ohne Timer");
        editor.StepPicker.SelectedItem=editor.Working.ControlSteps.Single(t=>t.Id==t2.Id);check(editor.Step?.Id==t2.Id&&previewStep==t2.Id,"Schrittwahl aktualisiert Vorschau und Kontext");
        editor.StepPicker.SelectedItem=editor.Working.ControlSteps.Single(t=>t.Id==t1.Id);check(editor.Rows.Single().State=="Offen","Schrittwechsel erhält unbestätigte Änderungen in Arbeitskopie");
        editor.ScenarioPicker.SelectedItem=editor.Working.Scenarios.Single(s=>s.Id==s2.Id);check(editor.Step?.Id==other.Id&&editor.Working.ControlSteps.Single(t=>t.Id==t1.Id).ScenarioId==s1.Id,"Szenariowechsel ordnet bisherigen Schritt nicht um");
        editor.ScenarioPicker.SelectedItem=editor.Working.Scenarios.Single(s=>s.Id==s1.Id);var assigned=(AlarmAssignmentRow)editor.AlarmGrid.Items[0];assigned.Assigned=false;assigned.Name="Ventilstörung";
        check(!editor.Working.Alarms[0].ScenarioIds.Contains(s1.Id)&&p.Alarms[0].ScenarioIds.Contains(s1.Id),"Alarmzuordnung und Bearbeitung bleiben bis Übernehmen in Arbeitskopie");
        editor.AddScenario();check(editor.Step is not null&&editor.Working.ControlSteps.Count(t=>t.ScenarioId==editor.Step.ScenarioId)==1,"Neues Szenario erzeugt und selektiert genau einen neuen Schritt");
        var createdScenario=editor.Step!.ScenarioId;editor.DeleteScenario();check(!editor.Working.Scenarios.Any(s=>s.Id==createdScenario)&&editor.Working.ControlSteps.Any(t=>t.ScenarioId is null),"Szenario löschen bewahrt Schritte ohne stille Neuzuordnung");
        var fresh=new StateEditor(null,p,new Scenario{Name="Neu"},null);check(fresh.Working.ControlSteps.Any(t=>t.ScenarioId==fresh.Step!.ScenarioId),"Neues Szenario über Hauptfenster erhält ebenfalls ersten Schritt");
        check(original==JsonSerializer.Serialize(p,ProjectServices.JsonOptions),"Abbrechen aller Szenario-, Schritt- und Alarmänderungen lässt Original unverändert");
        var saved=Path.Combine(output,"scenario-editor-fixture.pid2process.json");ProjectServices.Save(saved,editor.Working);var loaded=ProjectServices.Load(saved);
        check(loaded.ControlSteps.Single(t=>t.Id==t1.Id).Description=="Vorbereiten"&&loaded.Alarms.Single().Name=="Ventilstörung"&&!loaded.Alarms.Single().ScenarioIds.Contains(s1.Id),"Schrittbeschreibung und Alarmzuordnung bleiben im JSON-Roundtrip erhalten");
        IEnumerable<DependencyObject> Children(DependencyObject node){yield return node;for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)foreach(var child in Children(VisualTreeHelper.GetChild(node,i)))yield return child;}
        var window=new MainWindow();window.CreateBlankDocument();var active=window.ActiveProject!;active.Equipment.Add(new(){Tag="Auswahltest",Type=EquipmentType.IsolationValve,X=.2,Y=.2,Width=.1,Height=.1});
        typeof(MainWindow).GetMethod("RefreshInspector",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(window,[]);
        var lists=(ListBox[])typeof(MainWindow).GetField("_inspectLists",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(window)!;lists[0].SelectedItem=active.Equipment.Single();
        var sheet=(Canvas)window.FindName("Sheet");var selection=(HashSet<Guid>)typeof(MainWindow).GetField("_selection",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(window)!;
        check(selection.Contains(active.Equipment.Single().Id)&&sheet.Children.OfType<System.Windows.Shapes.Rectangle>().Any(r=>Equals(r.Tag,"selection-outline")),"Betriebsmittelliste selektiert Gerät und zeichnet Auswahlrahmen im Plan");
        var menu=(Menu)window.FindName("MainMenu");check(!menu.Items.OfType<MenuItem>().Any(m=>new[]{"_Betriebsmittel","_Szenarien","S_chritte","_Alarme"}.Contains(m.Header?.ToString())),"Doppelte Menüpunkte entfallen, rechte Inspector-Tabs bleiben erhalten");
        var root=(FrameworkElement)editor.Content;root.Measure(new Size(1080,700));root.Arrange(new Rect(0,0,1080,700));root.UpdateLayout();var tabs=Children(root).OfType<TabControl>().Single();
        check(tabs.Items.Cast<TabItem>().Select(t=>t.Header.ToString()).SequenceEqual(new[]{"Schritte","Symbolzustände","Alarme"}),"Szenarioeditor enthält Schritte, Symbolzustände und Alarme");
        foreach(var (index,file) in new[]{(1,"scenario-editor-states.png"),(0,"scenario-editor-steps.png"),(2,"scenario-editor-alarms.png")})
        {
            tabs.SelectedIndex=index;root.UpdateLayout();if(index==0)check(Children(root).OfType<DataGrid>().Single(g=>g.IsReadOnly).Columns.All(c=>c.ActualWidth>=140),"Schrittübersicht hat lesbare Spaltenbreiten");var bitmap=new RenderTargetBitmap(1080,700,96,96,PixelFormats.Pbgra32);bitmap.Render(root);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(Path.Combine(output,file));encoder.Save(stream);
        }
    }
}
