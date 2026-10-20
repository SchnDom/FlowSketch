using System.Text.Json;
using System.Windows.Controls;
namespace PID2Process;
internal static class ScenarioViewSelfTest
{
    internal static void Run(Action<bool,string> check)
    {
        var window=new MainWindow();window.CreateBlankDocument();var p=window.ActiveProject!;
        var valve=new EquipmentItem{Tag="V1",Type=EquipmentType.IsolationValve};p.Equipment.Add(valve);
        var first=new Scenario{Name="Szenario 1"};var second=new Scenario{Name="Szenario 2"};p.Scenarios.AddRange([first,second]);
        var a=new ControlStep{Name="Schritt 1",ScenarioId=first.Id};var b=new ControlStep{Name="Schritt 1",ScenarioId=second.Id};var c=new ControlStep{Name="Schritt 2",ScenarioId=second.Id};p.ControlSteps.AddRange([a,b,c]);
        var scenarios=(ComboBox)window.FindName("ScenarioBox");var steps=(ComboBox)window.FindName("StepBox");
        typeof(MainWindow).GetMethod("RefreshInspector",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(window,[]);scenarios.SelectedItem=first;steps.SelectedItem=a;
        bool Selected(Guid? scenario,Guid? step)=>(scenarios.SelectedItem as Scenario)?.Id==scenario&&(steps.SelectedItem as ControlStep)?.Id==step;
        var original=JsonSerializer.Serialize(p,ProjectServices.JsonOptions);var editor=new StateEditor(null,p,null,b);var finish=window.BeginScenarioEdit(editor);
        check(editor.ShowSelectionInCurrentView.IsChecked==true&&Selected(second.Id,b.Id),"Ansichtsoption synchronisiert beide oberen Dropdowns schon beim Öffnen");
        editor.StepPicker.SelectedItem=editor.Working.ControlSteps.Single(t=>t.Id==c.Id);
        check(Selected(second.Id,c.Id)&&steps.Items.Cast<ControlStep>().All(t=>t.ScenarioId==second.Id),"Editor-Schrittwechsel synchronisiert Auswahl und zugehörige Schrittmenge");
        editor.ShowSelectionInCurrentView.IsChecked=false;check(Selected(first.Id,a.Id),"Ausgeschaltete Ansichtsoption stellt ursprüngliche Dropdown-Auswahl wieder her");
        editor.ShowSelectionInCurrentView.IsChecked=true;editor.Rows.Single().State="Offen";check(Selected(second.Id,c.Id),"Zustandsvorschau behält synchronisierte Szenario-/Schrittauswahl");
        finish(false);check(Selected(first.Id,a.Id)&&original==JsonSerializer.Serialize(p,ProjectServices.JsonOptions),"Abbrechen stellt Auswahl wieder her und verwirft Zustandsänderungen");
        var accepted=new StateEditor(null,p,null,b);var accept=window.BeginScenarioEdit(accepted);accepted.StepPicker.SelectedItem=accepted.Working.ControlSteps.Single(t=>t.Id==c.Id);accepted.Rows.Single().State="Offen";accept(true);
        check(Selected(second.Id,c.Id)&&ReferenceEquals(steps.SelectedItem,p.ControlSteps.Single(t=>t.Id==c.Id))&&p.ControlSteps.Single(t=>t.Id==c.Id).EquipmentStates[valve.Id]=="Offen","Übernehmen behält ausgewählten Schritt und verwendet bestätigte Projektobjekte");
        var basic=new StateEditor(null,p,p.Scenarios.Single(s=>s.Id==first.Id),null);var endBasic=window.BeginScenarioEdit(basic);check(Selected(first.Id,a.Id),"Szenariowechsel öffnet ersten Schritt auch im oberen Dropdown");endBasic(false);
        var disabled=new StateEditor(null,p,null,p.ControlSteps.Single(t=>t.Id==a.Id));var endDisabled=window.BeginScenarioEdit(disabled);disabled.ShowSelectionInCurrentView.IsChecked=false;endDisabled(true);check(Selected(second.Id,c.Id),"Übernehmen mit ausgeschalteter Option erhält bisherige Ansichtsauswahl");
        var next=new StateEditor(null,p,null,p.ControlSteps.Single(t=>t.Id==a.Id));var endNext=window.BeginScenarioEdit(next);check(next.ShowSelectionInCurrentView.IsChecked==false&&Selected(second.Id,c.Id),"Ansichtsoption bleibt für das aktuelle Dokument während der Sitzung erhalten");endNext(false);
        var create=new StateEditor(null,p,p.Scenarios[0],null);var endCreate=window.BeginScenarioEdit(create);create.ShowSelectionInCurrentView.IsChecked=true;create.AddScenario();var newScenario=create.Step!.ScenarioId;var newStep=create.Step.Id;endCreate(true);check(Selected(newScenario,newStep),"Neu angelegtes Szenario mit erstem Schritt wird in aktuelle Ansicht übernommen");
        // Discard only this in-memory test document before closing the never-shown window.
        foreach(var tab in ((TabControl)window.FindName("DocumentTabs")).Items.Cast<TabItem>())tab.Tag.GetType().GetField("Dirty")!.SetValue(tab.Tag,false);
        window.Close();
    }
}
