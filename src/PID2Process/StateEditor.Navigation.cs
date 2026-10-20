using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace PID2Process;

internal sealed partial class StateEditor
{
    private bool _loading=true,_publishing;
    private Scenario? _activeScenario;
    private readonly TextBox _scenarioName=new();
    private readonly TextBox _stepDescription=new(){AcceptsReturn=true,MinHeight=45,TextWrapping=TextWrapping.Wrap};
    internal ComboBox ScenarioPicker=>_scenario;
    internal ComboBox StepPicker {get;}=new(){DisplayMemberPath="Name"};
    private Button? _deleteStepButton;
    private Grid BuildNavigation()
    {
        _activeScenario=Scenario??Working.Scenarios.FirstOrDefault(s=>s.Id==Step?.ScenarioId);
        var grid=new Grid();grid.ColumnDefinitions.Add(new());grid.ColumnDefinitions.Add(new());
        var left=new StackPanel{Margin=new Thickness(0,0,8,0)};var right=new StackPanel{Margin=new Thickness(8,0,0,0)};var scenarioGroup=DialogTheme.Group("Szenario",left);var stepGroup=DialogTheme.Group("Schritt",right);Grid.SetColumn(stepGroup,1);grid.Children.Add(scenarioGroup);grid.Children.Add(stepGroup);
        void Field(StackPanel panel,string label,FrameworkElement control){panel.Children.Add(new TextBlock{Text=label,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,3,0,2)});panel.Children.Add(control);}
        Field(left,"Szenario",_scenario);Field(left,"Name",_scenarioName);Field(left,"Beschreibung",_description);
        Field(right,"Schritt",StepPicker);Field(right,"Name",_name);Field(right,"Beschreibung",_stepDescription);
        var details=new StackPanel();foreach(var pair in new[]{("Voraussetzungen",_pre),("Aktionen",_actions),("Übergangsbedingung",_transition),("Beteiligte Tags",_tags)})Field(details,pair.Item1,pair.Item2);
        right.Children.Add(new Expander{Header="Schrittdetails",IsExpanded=false,Content=new ScrollViewer{Content=details,MaxHeight=190,VerticalScrollBarVisibility=ScrollBarVisibility.Auto}});
        var scenarioButtons=new WrapPanel();var stepButtons=new WrapPanel();left.Children.Add(scenarioButtons);right.Children.Add(stepButtons);
        void Button(Panel panel,string label,Action action){var button=new Button{Content=label};button.Click+=(_,_)=>action();panel.Children.Add(button);if(label=="Schritt löschen"){_deleteStepButton=button;button.ToolTip="Jedes Szenario behält mindestens einen Schritt.";}}
        Button(scenarioButtons,"Neues Szenario",AddScenario);Button(scenarioButtons,"Szenario löschen",DeleteScenario);
        Button(stepButtons,"Neuer Schritt",AddStep);Button(stepButtons,"Schritt löschen",DeleteStep);
        ReloadNavigation();LoadRows();
        _scenario.SelectionChanged+=(_,e)=>{if(_loading||e.Source!=_scenario)return;var next=_scenario.SelectedItem as Scenario;if(!SaveValues()){ReloadNavigation();return;}SetContext(next,next is null?null:ScenarioSteps.First(Working,next.Id));};
        StepPicker.SelectionChanged+=(_,e)=>{if(_loading||e.Source!=StepPicker)return;var next=StepPicker.SelectedItem as ControlStep;if(!SaveValues()){ReloadNavigation();return;}SetContext(_activeScenario,next);};
        _scenario.DropDownOpened+=(_,_)=>_scenario.Items.Refresh();StepPicker.DropDownOpened+=(_,_)=>StepPicker.Items.Refresh();
        foreach(var text in new[]{_scenarioName,_description,_name,_stepDescription,_pre,_actions,_transition,_tags})text.TextChanged+=(_,_)=>PublishPreview();
        return grid;
    }
    private void ReloadNavigation()
    {
        var previous=_loading;_loading=true;
        _scenario.ItemsSource=Working.Scenarios.ToArray();_scenario.SelectedItem=_activeScenario;
        var steps=Working.ControlSteps.Where(t=>_activeScenario is null?t.ScenarioId is null||!Working.Scenarios.Any(s=>s.Id==t.ScenarioId):t.ScenarioId==_activeScenario.Id).OrderBy(t=>t.Name,ActuatorExcelExport.NameOrder).ToArray();
        StepPicker.ItemsSource=steps;StepPicker.SelectedItem=Step;
        if(_deleteStepButton is not null)_deleteStepButton.IsEnabled=Step is not null&&(_activeScenario is null||steps.Length>1);
        _steps.ItemsSource=steps;_loading=previous;
    }
    private void SetContext(Scenario? scenario,ControlStep? step)
    {
        _loading=true;_activeScenario=scenario;Step=step;Scenario=step is null?scenario:null;Step??=scenario is null?null:ScenarioSteps.First(Working,scenario.Id);ReloadNavigation();LoadRows();RefreshAlarmRows();_loading=false;PublishPreview();
    }
    private void LoadRows()
    {
        _scenarioName.Text=_activeScenario?.Name??"";_description.Text=_activeScenario?.Description??"";_scenarioName.IsEnabled=_description.IsEnabled=_activeScenario is not null;
        _name.Text=Step?.Name??"";_stepDescription.Text=Step?.Description??"";_name.IsEnabled=_stepDescription.IsEnabled=Step is not null;
        _pre.Text=Step?.Preconditions??"";_actions.Text=Step?.Actions??"";_transition.Text=Step?.Transition??"";_tags.Text=string.Join(", ",Step?.Tags??[]);
        foreach(var field in new Control[]{_pre,_actions,_transition,_tags})field.IsEnabled=Step is not null;
        var states=Step?.EquipmentStates??_activeScenario?.EquipmentStates??[];var texts=Step?.TextValues??_activeScenario?.TextValues??[];var samples=Step?.SensorValues??_activeScenario?.SensorValues??[];var tanks=Step?.TankStates??_activeScenario?.TankStates??[];
        foreach(var row in Rows)
        {
            var item=Working.Equipment.FirstOrDefault(x=>x.Id==row.Id);var stored=(row.IsPipe?Step?.PipeStates??_activeScenario?.PipeStates??[]:states).GetValueOrDefault(row.Id);
            row.Options=(row.IsPipe?new[]{"Leer","Befüllt","Druckbeschlagen"}:SymbolCatalog.States(item!).Concat(item!.States.Select(s=>s.Name))).Concat(stored is null?[]:new[]{stored}).Concat(Step is null?[]:new[]{"Szenario übernehmen"}).Distinct().ToArray();
            var tank=tanks.GetValueOrDefault(row.Id);row.State=stored??(Step is not null?"Szenario übernehmen":row.IsTank?(tank?.Filled==true||tank?.Pressurized==true?"Befüllt":"Leer"):row.Options[0]);
            row.RawValue=samples.TryGetValue(row.Id,out var raw)?raw.ToString():"";row.Medium=Working.Media.FirstOrDefault(m=>m.Id==tank?.MediumId)?.Name??"Szenario übernehmen";row.OverrideText=texts.ContainsKey(row.Id);row.TextValue=texts.GetValueOrDefault(row.Id)??item?.TextContent??"";
        }
        StateGrid.Items.Refresh();
    }
    internal void PublishPreview()
    {
        if(_loading||_publishing||Preview is null)return;_publishing=true;
        try {if(SaveValues(false))Preview(Working,_activeScenario,Step);}finally{_publishing=false;}
    }
    internal void AddScenario()
    {
        if(!SaveValues())return;int n=1;while(Working.Scenarios.Any(s=>s.Name=="Szenario "+n))n++;
        var scenario=new Scenario{Name="Szenario "+n};var step=new ControlStep{Name="Schritt 1",ScenarioId=scenario.Id};Working.Scenarios.Add(scenario);Working.ControlSteps.Add(step);SetContext(scenario,step);
    }
    internal void DeleteScenario()
    {
        if(_activeScenario is not {} scenario)return;Working.Scenarios.Remove(scenario);foreach(var step in Working.ControlSteps.Where(t=>t.ScenarioId==scenario.Id))step.ScenarioId=null;foreach(var alarm in Working.Alarms)alarm.ScenarioIds.Remove(scenario.Id);
        var next=Working.Scenarios.FirstOrDefault();SetContext(next,ScenarioSteps.First(Working,next?.Id));
    }
    internal void AddStep()
    {
        if(_activeScenario is null||!SaveValues())return;int n=1;while(Working.ControlSteps.Any(t=>t.ScenarioId==_activeScenario.Id&&t.Name=="Schritt "+n))n++;
        var step=new ControlStep{Name="Schritt "+n,ScenarioId=_activeScenario.Id};Working.ControlSteps.Add(step);SetContext(_activeScenario,step);
    }
    internal void DeleteStep()
    {
        if(Step is not {} step||_activeScenario is not null&&Working.ControlSteps.Count(t=>t.ScenarioId==_activeScenario.Id)<=1)return;Working.ControlSteps.Remove(step);SetContext(_activeScenario,ScenarioSteps.First(Working,_activeScenario?.Id));
    }
    private DataGrid BuildStepsPage()
    {
        foreach(var (header,path) in new[]{("Name","Name"),("Beschreibung","Description"),("Voraussetzungen","Preconditions"),("Aktionen","Actions"),("Übergangsbedingung","Transition")})_steps.Columns.Add(new DataGridTextColumn{Header=header,Binding=new Binding(path),MinWidth=150,Width=new DataGridLength(1,DataGridLengthUnitType.Star)});
        _steps.MouseDoubleClick+=(_,_)=>{if(_steps.SelectedItem is ControlStep step)StepPicker.SelectedItem=step;};return _steps;
    }
}
