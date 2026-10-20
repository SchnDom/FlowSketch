using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
namespace PID2Process;

public partial class MainWindow
{
    private readonly ListBox[] _inspectLists=[new(),new(),new(),new()];
    private readonly TextBox _inspectSearch=new(){Margin=new Thickness(4),ToolTip="Nach Tag, Funktion oder Einbauort suchen"};
    private bool _refreshingInspector;
    private void InitializeInspector()
    {
        var captions=new[]{"Betriebsmittel","Szenarien","Schritte","Alarme"};
        var flow=new Grid();flow.RowDefinitions.Add(new());flow.RowDefinitions.Add(new(){Height=new GridLength(5)});flow.RowDefinitions.Add(new());
        for(var i=0;i<4;i++)
        {
            var index=i;_inspectLists[i].Background=PageVisual.Color("#202A35");_inspectLists[i].Foreground=System.Windows.Media.Brushes.AliceBlue;var root=new DockPanel{Background=PageVisual.Color("#29323D")};var buttons=new WrapPanel();foreach(var (label,action)in new (string,Action)[]{("Neu",()=>InspectorAdd(index)),("Bearbeiten",()=>InspectorEdit(index)),("Löschen",()=>InspectorDelete(index))}){if(index==0&&label=="Neu")continue;var button=new Button{Content=label};button.Click+=(_,_)=>action();buttons.Children.Add(button);}DockPanel.SetDock(buttons,Dock.Bottom);root.Children.Add(buttons);
            if(index==0)
            {
                _inspectSearch.Background=PageVisual.Color("#202A35");_inspectSearch.Foreground=System.Windows.Media.Brushes.AliceBlue;_inspectSearch.CaretBrush=System.Windows.Media.Brushes.White;
                _inspectSearch.Padding=new Thickness(7,5,34,5);_inspectSearch.ToolTip="Betriebsmittel suchen: VP* beginnt mit VP, *VP* enthält VP, ? ersetzt ein Zeichen";
                System.Windows.Automation.AutomationProperties.SetName(_inspectSearch,"Betriebsmittel suchen");
                var search=new Grid();search.Children.Add(_inspectSearch);
                var reset=new Button{Width=28,Height=26,Padding=new Thickness(4),Margin=new Thickness(0,0,7,0),HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Center,Background=System.Windows.Media.Brushes.Transparent,BorderThickness=new Thickness(0)};
                var icon=new System.Windows.Shapes.Path{Data=System.Windows.Media.Geometry.Parse("M 6,1 A 5,5 0 1 1 5.999,1 M 10,10 L 15,15"),Stroke=System.Windows.Media.Brushes.AliceBlue,StrokeThickness=1.6,Width=16,Height=16};
                reset.Content=icon;reset.ToolTip="Betriebsmittel suchen";reset.Click+=(_,_)=>{_inspectSearch.Clear();_inspectSearch.Focus();};search.Children.Add(reset);DockPanel.SetDock(search,Dock.Top);root.Children.Add(search);
                _inspectSearch.TextChanged+=(_,_)=>{var active=!string.IsNullOrWhiteSpace(_inspectSearch.Text);icon.Data=System.Windows.Media.Geometry.Parse(active?"M 3,3 L 13,13 M 13,3 L 3,13":"M 6,1 A 5,5 0 1 1 5.999,1 M 10,10 L 15,15");reset.ToolTip=active?"Suchfilter löschen":"Betriebsmittel suchen";System.Windows.Automation.AutomationProperties.SetName(reset,(string)reset.ToolTip);RefreshInspector();};
            }
            _inspectLists[i].DisplayMemberPath=i==0?"Tag":"Name";_inspectLists[i].MouseDoubleClick+=(_,_)=>InspectorEdit(index);_inspectLists[i].SelectionChanged+=(_,_)=>{if(_refreshingInspector)return;if(index==0&&_inspectLists[0].SelectedItem is EquipmentItem device)_=SelectInspectorEquipment(device);if(index==1&&_inspectLists[1].SelectedItem is Scenario s)ScenarioBox.SelectedItem=s;if(index==2&&_inspectLists[2].SelectedItem is ControlStep step){if(step.ScenarioId is Guid id)ScenarioBox.SelectedItem=Current?.Project.Scenarios.FirstOrDefault(s=>s.Id==id);StepBox.SelectedItem=step;}};root.Children.Add(_inspectLists[i]);if(i is 1 or 2){var group=DialogTheme.Group(i==1?"Szenario":"Schritte",root);group.Padding=new Thickness(5);Grid.SetRow(group,i==1?0:2);flow.Children.Add(group);if(i==1){InspectorTabs.Items.Add(new TabItem{Header="Abläufe",Content=flow});var splitter=new GridSplitter{Height=5,HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=PageVisual.Color("#455569")};Grid.SetRow(splitter,1);flow.Children.Add(splitter);}}else InspectorTabs.Items.Add(new TabItem{Header=captions[i],Content=root});
        }
    }
    private async Task SelectInspectorEquipment(EquipmentItem device)
    {
        if(Current is not {} doc)return;CancelPlacement();ClearSelection();
        if(doc.Page!=device.Page-1){doc.Page=device.Page-1;await RenderPage();}
        if(Current!=doc||doc.Page!=device.Page-1||_inspectLists[0].SelectedItem is not EquipmentItem selected||selected.Id!=device.Id)return;SelectObject(device.Id);
        Viewport.ScrollToHorizontalOffset(Math.Max(0,(device.X+device.Width/2)*Sheet.Width-Viewport.ViewportWidth/2));
        Viewport.ScrollToVerticalOffset(Math.Max(0,(device.Y+device.Height/2)*Sheet.Height-Viewport.ViewportHeight/2));
    }
    private void RefreshInspector()
    {
        if(_refreshingInspector)return;_refreshingInspector=true;
        try
        {
            var project=Current?.FollowScenarioEditorSelection==true?_previewProject??Current?.Project:Current?.Project;var selected=_inspectLists.Select(l=>l.SelectedItem).ToArray();
            _inspectLists[0].ItemsSource=project?.Equipment.Where(x=>x.Type!=EquipmentType.Drawing).Where(x=>EquipmentSearch.Matches(x,_inspectSearch.Text)).ToArray();_inspectLists[1].ItemsSource=project?.Scenarios.ToArray();
            var scenario=ScenarioBox.SelectedItem as Scenario;_inspectLists[2].ItemsSource=project?.ControlSteps.Where(s=>scenario is not null&&s.ScenarioId==scenario.Id).ToArray();_inspectLists[3].ItemsSource=project is null?null:project.Alarms.Concat(SensorEvaluation.Alarms(project,SceneValues.From(ScenarioBox.SelectedItem as Scenario,StepBox.SelectedItem as ControlStep))).ToArray();for(var i=0;i<4;i++)_inspectLists[i].SelectedItem=selected[i];
            var scenarioId=(ScenarioBox.SelectedItem as Scenario)?.Id;var stepId=(StepBox.SelectedItem as ControlStep)?.Id;ScenarioBox.ItemsSource=project?.Scenarios.ToArray();ScenarioBox.SelectedItem=project?.Scenarios.FirstOrDefault(s=>s.Id==scenarioId);StepBox.ItemsSource=project?.ControlSteps.Where(s=>scenarioId is null||s.ScenarioId==scenarioId).ToArray();StepBox.SelectedItem=project?.ControlSteps.FirstOrDefault(s=>s.Id==stepId&&(scenarioId is null||s.ScenarioId==scenarioId))??(project is not null&&scenarioId is not null?ScenarioSteps.First(project,scenarioId):null);
            _inspectLists[1].SelectedItem=ScenarioBox.SelectedItem;_inspectLists[2].SelectedItem=StepBox.SelectedItem;
        }
        finally {_refreshingInspector=false;}
    }
    private void InspectorAdd(int index)
    {
        if(Current is null)return;switch(index){case 0:StatusText.Text="Symbol links wählen, dann platzieren.";break;case 1:EditScenario(null);break;case 2:EditStep(null);break;case 3:EditAlarmData(null);break;}
    }
    private void InspectorEdit(int index)
    {
        if(Current is null)return;switch(_inspectLists[index].SelectedItem){case EquipmentItem device:if(EditEquipment(device)){Changed();Draw();}break;case Scenario s:EditScenario(s);break;case ControlStep step:EditStep(step);break;case Alarm alarm:if(alarm.IsDerived){MessageBox.Show(this,alarm.Trigger,"Aktiver Sensoralarm");break;}EditAlarmData(alarm);break;}
    }
    private void InspectorDelete(int index)
    {
        if(Current is not {} d)return;switch(_inspectLists[index].SelectedItem)
        {
            case EquipmentItem device:Delete(device);return;
            case Scenario s:d.Project.Scenarios.Remove(s);foreach(var step in d.Project.ControlSteps.Where(step=>step.ScenarioId==s.Id))step.ScenarioId=null;foreach(var alarm in d.Project.Alarms)alarm.ScenarioIds.Remove(s.Id);break;
            case ControlStep step:if(step.ScenarioId is Guid scenario&&d.Project.ControlSteps.Count(x=>x.ScenarioId==scenario)<=1){StatusText.Text="Der erste Schritt eines Szenarios kann nicht gelöscht werden.";return;}d.Project.ControlSteps.Remove(step);break;
            case Alarm alarm:if(alarm.IsDerived){StatusText.Text="Sensoralarm wird aus dem Messwert abgeleitet.";return;}d.Project.Alarms.Remove(alarm);break;
        }
        Changed();Draw();
    }
    private void EditScenario(Scenario? source)=>EditStates(source??new Scenario(),source is not null&&StepBox.SelectedItem is ControlStep selected&&selected.ScenarioId==source.Id?selected:null);
    private void EditStep(ControlStep? source)
    {
        if(Current is not {} d)return;if(d.Project.Scenarios.Count==0){EditScenario(null);if(d.Project.Scenarios.Count==0)return;}
        var scenarioId=(ScenarioBox.SelectedItem as Scenario)?.Id??d.Project.Scenarios[0].Id;var n=1;while(d.Project.ControlSteps.Any(t=>t.ScenarioId==scenarioId&&t.Name=="Schritt "+n))n++;
        EditStates(null,source??new ControlStep{ScenarioId=scenarioId,Name="Schritt "+n});
    }
    private void EditStates(Scenario? scenario,ControlStep? step)
    {
        if(Current is not {} d)return;var editor=new StateEditor(this,d.Project,scenario,step);var finish=BeginScenarioEdit(editor);var accepted=false;
        try {accepted=editor.ShowDialog()==true;}finally{finish(accepted);}
    }
    // Both dropdowns change atomically; no intermediate selection event may redraw another context.
    private void SetPlanSelection(EngineeringProject project,Guid? scenarioId,Guid? stepId)
    {
        var refreshing=_refreshingInspector;_refreshingInspector=true;
        try
        {
            ScenarioBox.ItemsSource=project.Scenarios.ToArray();ScenarioBox.SelectedItem=project.Scenarios.FirstOrDefault(s=>s.Id==scenarioId);
            var steps=project.ControlSteps.Where(t=>scenarioId is null||t.ScenarioId==scenarioId).ToArray();StepBox.ItemsSource=steps;StepBox.SelectedItem=steps.FirstOrDefault(t=>t.Id==stepId)??(scenarioId is not null?ScenarioSteps.First(project,scenarioId):null);
        }
        finally {_refreshingInspector=refreshing;}
    }
    internal Action<bool> BeginScenarioEdit(StateEditor editor)
    {
        if(Current is not {} doc)return _=>{};
        var previousScenario=(ScenarioBox.SelectedItem as Scenario)?.Id;var previousStep=(StepBox.SelectedItem as ControlStep)?.Id;var previousFollow=doc.FollowScenarioEditorSelection;
        editor.ShowSelectionInCurrentView.IsChecked=previousFollow;
        editor.Preview=(working,s,t)=>
        {
            if(Current!=doc)return;
            _previewProject=working;_previewScenario=s;_previewStep=t;doc.FollowScenarioEditorSelection=editor.ShowSelectionInCurrentView.IsChecked==true;
            SetPlanSelection(doc.FollowScenarioEditorSelection?working:doc.Project,doc.FollowScenarioEditorSelection?s?.Id:previousScenario,doc.FollowScenarioEditorSelection?t?.Id:previousStep);
            RefreshInspector();Draw();
            _inspectLists[3].ItemsSource=working.Alarms.Concat(SensorEvaluation.Alarms(working,SceneValues.From(s,t))).ToArray();
        };
        editor.PublishPreview();
        return accepted=>
        {
            if(Current!=doc)return;
            var scenarioId=accepted&&doc.FollowScenarioEditorSelection?_previewScenario?.Id:previousScenario;
            var stepId=accepted&&doc.FollowScenarioEditorSelection?_previewStep?.Id:previousStep;
            _previewProject=null;_previewScenario=null;_previewStep=null;
            if(accepted){doc.Project.Scenarios=editor.Working.Scenarios;doc.Project.ControlSteps=editor.Working.ControlSteps;doc.Project.Alarms=editor.Working.Alarms;}
            else doc.FollowScenarioEditorSelection=previousFollow;
            SetPlanSelection(doc.Project,scenarioId,stepId);
            if(accepted)Changed();else RefreshInspector();Draw();
        };
    }
    private void EditAlarmData(Alarm? source)
    {
        if(Current is not {} d)return;var alarm=source is null?new Alarm():JsonSerializer.Deserialize<Alarm>(JsonSerializer.Serialize(source,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;var dialog=new EditorDialog(this,"Alarm");dialog.Text("Name",alarm.Name);dialog.Choice("Betriebsmittel-Tag",alarm.Tag,d.Project.Equipment.Where(x=>x.Type!=EquipmentType.Drawing).Select(x=>x.Tag).ToArray());dialog.Choice("Priorität",alarm.Priority.ToString(),"Information","Warning","Critical");dialog.Text("Auslöser",alarm.Trigger,true);dialog.Text("Auswirkung",alarm.Impact,true);dialog.Text("Bedien-/Abhilfemaßnahme",alarm.OperatorAction,true);foreach(var s in d.Project.Scenarios){dialog.Check("Szenario "+s.Id,alarm.ScenarioIds.Contains(s.Id));dialog.Label("Szenario "+s.Id,s.Name);}if(dialog.ShowDialog()!=true)return;
        alarm.Name=dialog.Value("Name");alarm.Tag=dialog.Value("Betriebsmittel-Tag");alarm.Priority=Enum.TryParse<AlarmPriority>(dialog.Value("Priorität"),out var priority)?priority:AlarmPriority.Warning;alarm.Trigger=dialog.Value("Auslöser");alarm.Impact=dialog.Value("Auswirkung");alarm.OperatorAction=dialog.Value("Bedien-/Abhilfemaßnahme");alarm.ScenarioIds=d.Project.Scenarios.Where(s=>dialog.Checked("Szenario "+s.Id)).Select(s=>s.Id).ToList();d.Project.Alarms.RemoveAll(a=>a.Id==alarm.Id);d.Project.Alarms.Add(alarm);Changed();
    }
}
