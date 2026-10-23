using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace PID2Process;

internal sealed class StateRow : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Guid Id {get;set;}
    public string Tag {get;set;}="";
    public string Function {get;set;}="";
    public string Location {get;set;}="";
    public string Type {get;set;}="";
    private string _State="Normal";
    public string State {get=>_State;set{if(_State==value)return;_State=value;PropertyChanged?.Invoke(this,new(nameof(State)));}}
    private string _TextValue="";
    public string TextValue {get=>_TextValue;set{if(_TextValue==value)return;_TextValue=value;PropertyChanged?.Invoke(this,new(nameof(TextValue)));}}
    private bool _OverrideText;
    public bool OverrideText {get=>_OverrideText;set{if(_OverrideText==value)return;_OverrideText=value;PropertyChanged?.Invoke(this,new(nameof(OverrideText)));}}
    public string[] Options {get;set;}=[];
    public bool IsText {get;set;}
    public bool IsPipe {get;set;}
    public bool IsTank {get;set;}
    public string Filled {get;set;}="Nein";
    public string Pressurized {get;set;}="Nein";
    private string _Medium="Szenario übernehmen";
    public string Medium {get=>_Medium;set{if(_Medium==value)return;_Medium=value;PropertyChanged?.Invoke(this,new(nameof(Medium)));}}
    public string[] MediaOptions {get;set;}=[];
    public bool IsSensor {get;set;}
    public string Unit {get;set;}="";
    private string _RawValue="";
    public string RawValue {get=>_RawValue;set{if(_RawValue==value)return;_RawValue=value;PropertyChanged?.Invoke(this,new(nameof(RawValue)));}}
    public string[] TankOptions {get;set;}=["Nein","Ja"];
}

internal sealed partial class StateEditor : Window
{
    internal Scenario? Scenario {get;private set;}
    internal ControlStep? Step {get;private set;}
    internal EngineeringProject Working {get;}
    internal ObservableCollection<StateRow> Rows {get;}=[];
    internal DataGrid StateGrid {get;}=new();
    internal TextBox Search {get;}=new(){Width=210,Margin=new Thickness(4),ToolTip="Tag, Typ, Funktion oder Einbauort suchen"};
    internal ComboBox FunctionFilter {get;}=new(){Width=190,Margin=new Thickness(4)};
    private readonly TextBox _name=new(){Tag="Name"},_description=new(){Tag="Beschreibung",AcceptsReturn=true,MinHeight=45,TextWrapping=TextWrapping.Wrap};
    private readonly TextBox _pre=new(){Tag="Voraussetzungen",AcceptsReturn=true,MinHeight=45},_actions=new(){Tag="Aktionen",AcceptsReturn=true,MinHeight=45},_transition=new(){Tag="Übergangsbedingung",AcceptsReturn=true,MinHeight=45};
    private readonly ComboBox _scenario=new(){DisplayMemberPath="Name",SelectedValuePath="Id"};
    private readonly TextBox _tags=new(){Tag="Beteiligte Tags"};
    private readonly ICollectionView _view;
    private readonly DataGrid _steps=new(){AutoGenerateColumns=false,IsReadOnly=true,CanUserAddRows=false};
    internal Action<EngineeringProject,Scenario?,ControlStep?>? Preview {get;set;}
    internal CheckBox ShowSelectionInCurrentView {get;}=new(){Content="Szenario- und Schrittauswahl in aktueller Ansicht anzeigen",IsChecked=true,Margin=new Thickness(0,8,0,0)};

    internal StateEditor(Window? owner,EngineeringProject project,Scenario? scenario,ControlStep? step)
    {
        if(owner is not null)Owner=owner;Title=step is null?"Szenario":"Regelschritt";Width=Math.Min(1100,SystemParameters.WorkArea.Width-60);Height=Math.Min(730,SystemParameters.WorkArea.Height-60);MinWidth=650;MinHeight=420;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        Working=DocumentExport.Snapshot(project);MediaSettings.Normalize(Working);
        if(scenario is not null){Scenario=System.Text.Json.JsonSerializer.Deserialize<Scenario>(System.Text.Json.JsonSerializer.Serialize(scenario,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;var existingIndex=Working.Scenarios.FindIndex(s=>s.Id==Scenario.Id);if(existingIndex<0)Working.Scenarios.Add(Scenario);else Working.Scenarios[existingIndex]=Scenario;}
        if(step is not null){Step=System.Text.Json.JsonSerializer.Deserialize<ControlStep>(System.Text.Json.JsonSerializer.Serialize(step,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;var index=Working.ControlSteps.FindIndex(t=>t.Id==Step.Id);if(index<0)Working.ControlSteps.Add(Step);else Working.ControlSteps[index]=Step;}
        if(scenario is not null&&!project.Scenarios.Any(s=>s.Id==scenario.Id))Working.ControlSteps.Add(new(){Name="Schritt 1",ScenarioId=scenario.Id});
        ScenarioSteps.Ensure(Working);if(Step is null&&Scenario is not null)Step=ScenarioSteps.First(Working,Scenario.Id);
        _name.Text=Step?.Name??Scenario!.Name;_description.Text=Scenario?.Description??"";
        var states=Step?.EquipmentStates??Scenario!.EquipmentStates;var texts=Step?.TextValues??Scenario!.TextValues;
        foreach(var item in Working.Equipment.Where(x=>x.Type!=EquipmentType.Drawing))
        {
            var options=SymbolCatalog.States(item);
            options=options.Concat(item.States.Select(s=>s.Name)).Concat(states.TryGetValue(item.Id,out var stored)?new[]{stored}:Array.Empty<string>()).Distinct().ToArray();
            if(item.Type==EquipmentType.Vessel)options=["Befüllt","Leer"];
            if(Step is not null)options=options.Prepend("Szenario übernehmen").ToArray();
            var tank=(Step?.TankStates??Scenario!.TankStates).GetValueOrDefault(item.Id);
            Rows.Add(new StateRow{IsSensor=item.Type==EquipmentType.Instrument,RawValue=(Step?.SensorValues??Scenario!.SensorValues).TryGetValue(item.Id,out var sample)?sample.ToString():"",Medium=Working.Media.FirstOrDefault(m=>m.Id==tank?.MediumId)?.Name??"Szenario übernehmen",MediaOptions=new[]{"Szenario übernehmen"}.Concat(Working.Media.Select(m=>m.Name)).ToArray(),IsTank=item.Type==EquipmentType.Vessel,TankOptions=Step is null?["Nein","Ja"]:["Szenario übernehmen","Nein","Ja"],Filled=tank?.Filled is bool filled?(filled?"Ja":"Nein"):Step is null?"Nein":"Szenario übernehmen",Pressurized=tank?.Pressurized is bool pressure?(pressure?"Ja":"Nein"):Step is null?"Nein":"Szenario übernehmen",Id=item.Id,Tag=item.Tag,Function=item.Function,Location=item.InstallationLocation,Type=item.Variant,Options=options,State=states.GetValueOrDefault(item.Id)??(Step is null?item.Type==EquipmentType.Vessel?(tank?.Filled==true||tank?.Pressurized==true?"Befüllt":"Leer"):options[0]:"Szenario übernehmen"),IsText=item.Type==EquipmentType.Text,OverrideText=texts.ContainsKey(item.Id),TextValue=item.Type==EquipmentType.Text?texts.GetValueOrDefault(item.Id)??item.TextContent:""});
        }
        var pipeStates=Step?.PipeStates??Scenario!.PipeStates;
        foreach(var pipe in Working.Pipes.Where(p=>p.StartConnection is null&&p.EndConnection is null))Rows.Add(new StateRow{Id=pipe.Id,Tag=pipe.Tag,Type="Rohr (unverbunden)",IsPipe=true,Options=Step is null?["Leer","Befüllt","Druckbeschlagen"]:["Szenario übernehmen","Leer","Befüllt","Druckbeschlagen"],State=pipeStates.GetValueOrDefault(pipe.Id)??(Step is null?"Leer":"Szenario übernehmen")});
        Height=Math.Min(SystemParameters.WorkArea.Height-60,780);
        _view=CollectionViewSource.GetDefaultView(Rows);
        _view.Filter=o=>o is StateRow r&&(FunctionFilter.SelectedIndex<=0||r.Function==FunctionFilter.SelectedItem?.ToString())&&($"{r.Tag} {r.Function} {r.Location} {r.Type}".Contains(Search.Text,StringComparison.OrdinalIgnoreCase));
        FunctionFilter.ItemsSource=new[]{"Alle Funktionen"}.Concat(Rows.Select(r=>r.Function).Where(f=>f.Length>0).Distinct().Order()).ToArray();FunctionFilter.SelectedIndex=0;Search.TextChanged+=(_,_)=>_view.Refresh();FunctionFilter.SelectionChanged+=(_,_)=>_view.Refresh();
        DialogTheme.Apply(this);var root=new DockPanel{Margin=new Thickness(14),Background=PageVisual.Color("#20262E")};var footer=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
        var ok=new Button{Content="Übernehmen",IsDefault=true};ok.Click+=(_,_)=>Apply();var cancel=new Button{Content="Abbrechen",IsCancel=true};footer.Children.Add(ok);footer.Children.Add(cancel);DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer);
        var metadata=new StackPanel();metadata.Children.Add(BuildNavigation());metadata.Children.Add(ShowSelectionInCurrentView);DockPanel.SetDock(metadata,Dock.Top);root.Children.Add(metadata);
        var tabs=new TabControl{Margin=new Thickness(0,12,0,0)};var statePage=new DockPanel();var filters=new StackPanel{Orientation=Orientation.Horizontal};filters.Children.Add(new TextBlock{Text="Suche",VerticalAlignment=VerticalAlignment.Center});filters.Children.Add(Search);filters.Children.Add(FunctionFilter);DockPanel.SetDock(filters,Dock.Top);statePage.Children.Add(filters);
        StateGrid.AutoGenerateColumns=false;StateGrid.CanUserAddRows=false;StateGrid.EnableRowVirtualization=true;StateGrid.EnableColumnVirtualization=true;StateGrid.SelectionMode=DataGridSelectionMode.Extended;StateGrid.ItemsSource=_view;
        foreach(var (header,path,width) in new[]{("Tag","Tag",150),("Funktion","Function",180),("Einbauort (+)","Location",100),("Typ","Type",120)})StateGrid.Columns.Add(new DataGridTextColumn{Header=header,Binding=new Binding(path),IsReadOnly=true,Width=width});
        var factory=new FrameworkElementFactory(typeof(ComboBox));factory.SetBinding(ItemsControl.ItemsSourceProperty,new Binding("Options"));factory.SetBinding(ComboBox.SelectedItemProperty,new Binding("State"){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});StateGrid.Columns.Add(new DataGridTemplateColumn{Header="Zustand",Width=160,CellTemplate=new DataTemplate{VisualTree=factory}});
        var medium=new FrameworkElementFactory(typeof(ComboBox));medium.SetBinding(ItemsControl.ItemsSourceProperty,new Binding("MediaOptions"));medium.SetBinding(ComboBox.SelectedItemProperty,new Binding("Medium"){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});medium.SetBinding(UIElement.IsEnabledProperty,new Binding("IsTank"));StateGrid.Columns.Add(new DataGridTemplateColumn{Header="Behältermedium",Width=150,CellTemplate=new DataTemplate{VisualTree=medium}});
        var rawStyle=new Style(typeof(TextBox));rawStyle.Setters.Add(new Setter(UIElement.IsEnabledProperty,new Binding("IsSensor")));StateGrid.Columns.Add(new DataGridTextColumn{Header="Prozesswert",Binding=new Binding("RawValue"){UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},EditingElementStyle=rawStyle,Width=120});
        foreach(var row in Rows.Where(x=>x.IsSensor))row.Unit=Working.Equipment.First(x=>x.Id==row.Id).Sensor.Unit;
        StateGrid.Columns.Add(new DataGridTextColumn{Header="Einheit",Binding=new Binding("Unit"),IsReadOnly=true,Width=75});
        var textSwitch=new FrameworkElementFactory(typeof(CheckBox));textSwitch.SetBinding(CheckBox.IsCheckedProperty,new Binding("OverrideText"){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});textSwitch.SetBinding(UIElement.IsEnabledProperty,new Binding("IsText"));StateGrid.Columns.Add(new DataGridTemplateColumn{Header="Text setzen",CellTemplate=new DataTemplate{VisualTree=textSwitch},Width=85});
        var textEditStyle=new Style(typeof(TextBox));textEditStyle.Setters.Add(new Setter(UIElement.IsEnabledProperty,new Binding("IsText")));StateGrid.Columns.Add(new DataGridTextColumn{Header="Textinhalt (Textfelder)",Binding=new Binding("TextValue"){UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},EditingElementStyle=textEditStyle,MinWidth=200,Width=new DataGridLength(1,DataGridLengthUnitType.Star)});
        var bulk=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,4,0,4)};var bulkState=new ComboBox{Width=170,ItemsSource=Rows.SelectMany(r=>r.Options).Distinct().ToArray(),SelectedIndex=0};var bulkButton=new Button{Content="Für Auswahl setzen"};var bulkHint=new TextBlock{Margin=new Thickness(8,0,0,0),VerticalAlignment=VerticalAlignment.Center};
        bulkButton.Click+=(_,_)=>{StateGrid.CommitEdit(DataGridEditingUnit.Cell,true);StateGrid.CommitEdit(DataGridEditingUnit.Row,true);var state=bulkState.SelectedItem?.ToString();var selected=StateGrid.SelectedItems.Cast<StateRow>().ToArray();var changed=0;foreach(var row in selected)if(state is not null&&row.Options.Contains(state)){row.State=state;changed++;}StateGrid.Items.Refresh();bulkHint.Text=$"{changed} / {selected.Length} passende Symbole gesetzt";};bulk.Children.Add(bulkState);bulk.Children.Add(bulkButton);bulk.Children.Add(bulkHint);DockPanel.SetDock(bulk,Dock.Bottom);statePage.Children.Add(bulk);
        statePage.Children.Add(StateGrid);tabs.Items.Add(new TabItem{Header="Symbolzustände",Content=statePage});
        tabs.Items.Insert(0,new TabItem{Header="Schritte",Content=BuildStepsPage()});tabs.Items.Add(new TabItem{Header="Alarme",Content=BuildAlarmsPage()});tabs.SelectedIndex=1;
        root.Children.Add(tabs);Content=root;
        _loading=false;foreach(var row in Rows)row.PropertyChanged+=(_,_)=>PublishPreview();
        ShowSelectionInCurrentView.Checked+=(_,_)=>PublishPreview();ShowSelectionInCurrentView.Unchecked+=(_,_)=>PublishPreview();
        Loaded+=(_,_)=>PublishPreview();
    }
    private void Apply()
    {
        if(SaveValues()&&ValidateAlarms())DialogResult=true;
    }
    // Separate commit from showing a modal window, for tests without desktop input.
    internal bool SaveValues(bool commitGrid=true)
    {
        if(commitGrid&&(!StateGrid.CommitEdit(DataGridEditingUnit.Cell,true)||!StateGrid.CommitEdit(DataGridEditingUnit.Row,true)))return false;
        if(Step is null&&_activeScenario is null)return true;
        if(string.IsNullOrWhiteSpace(_name.Text)){if(IsVisible&&commitGrid)MessageBox.Show(this,"Name eingeben.");return false;}
        if(commitGrid&&(!AlarmGrid.CommitEdit(DataGridEditingUnit.Cell,true)||!AlarmGrid.CommitEdit(DataGridEditingUnit.Row,true)))return false;
        if(_activeScenario is not null){if(string.IsNullOrWhiteSpace(_scenarioName.Text)){if(IsVisible&&commitGrid)MessageBox.Show(this,"Szenarionamen eingeben.");return false;}_activeScenario.Name=_scenarioName.Text.Trim();_activeScenario.Description=_description.Text;}
        if(Step is not null){Step.Name=_name.Text.Trim();Step.Description=_stepDescription.Text;Step.ScenarioId=_activeScenario?.Id??Step.ScenarioId;Step.Preconditions=_pre.Text;Step.Actions=_actions.Text;Step.Transition=_transition.Text;Step.Tags=_tags.Text.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Distinct().ToList();}
        var states=Step?.EquipmentStates??Scenario!.EquipmentStates;var pipeStates=Step?.PipeStates??Scenario!.PipeStates;var texts=Step?.TextValues??Scenario!.TextValues;
        var samples=Step?.SensorValues??Scenario!.SensorValues;
        foreach(var row in Rows.Where(r=>r.IsSensor)){if(string.IsNullOrWhiteSpace(row.RawValue))samples.Remove(row.Id);else if(double.TryParse(row.RawValue,out var raw)&&double.IsFinite(raw))samples[row.Id]=raw;else return false;}
        foreach(var row in Rows){var target=row.IsPipe?pipeStates:states;if(row.State=="Szenario übernehmen")target.Remove(row.Id);else target[row.Id]=row.State;if(row.IsText){if(row.OverrideText)texts[row.Id]=row.TextValue;else texts.Remove(row.Id);}}
        var tanks=Step?.TankStates??Scenario!.TankStates;
        foreach(var row in Rows.Where(r=>r.IsTank)){var old=tanks.GetValueOrDefault(row.Id);var medium=Working.Media.FirstOrDefault(m=>m.Name==row.Medium);var assignment=new TankAssignment{Filled=row.State=="Szenario übernehmen"?null:row.State=="Befüllt",MediumId=medium?.Id,Pressurized=old?.Pressurized};if(assignment.Filled is null&&assignment.MediumId is null&&assignment.Pressurized is null)tanks.Remove(row.Id);else tanks[row.Id]=assignment;}
        if(Step is not null){var index=Working.ControlSteps.FindIndex(t=>t.Id==Step.Id);if(index<0)Working.ControlSteps.Add(Step);else Working.ControlSteps[index]=Step;}

        return true;
    }
}
