using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace PID2Process;

internal sealed class AlarmAssignmentRow
{
    internal Alarm Alarm {get;}
    private readonly Guid? _scenario;
    internal AlarmAssignmentRow(Alarm alarm,Guid? scenario){Alarm=alarm;_scenario=scenario;}
    public bool Assigned {get=>_scenario is Guid id&&Alarm.ScenarioIds.Contains(id);set{if(_scenario is not Guid id)return;Alarm.ScenarioIds.Remove(id);if(value)Alarm.ScenarioIds.Add(id);}}
    public string Name {get=>Alarm.Name;set=>Alarm.Name=value;}
    public string Tag {get=>Alarm.Tag;set=>Alarm.Tag=value;}
    public AlarmPriority Priority {get=>Alarm.Priority;set=>Alarm.Priority=value;}
    public string Trigger {get=>Alarm.Trigger;set=>Alarm.Trigger=value;}
    public string Impact {get=>Alarm.Impact;set=>Alarm.Impact=value;}
    public string OperatorAction {get=>Alarm.OperatorAction;set=>Alarm.OperatorAction=value;}
}
internal sealed partial class StateEditor
{
    internal DataGrid AlarmGrid {get;}=new(){AutoGenerateColumns=false,CanUserAddRows=false};
    private DockPanel BuildAlarmsPage()
    {
        var panel=new DockPanel();var hint=new TextBlock{Text="Betriebsmittelalarme bearbeiten und dem gewählten Szenario zuordnen. Sensorgrenzwertalarme werden aus den Rohwerten abgeleitet.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(4)};DockPanel.SetDock(hint,Dock.Top);panel.Children.Add(hint);
        var buttons=new WrapPanel();var add=new Button{Content="Neuer Alarm"};add.Click+=(_,_)=>{var alarm=new Alarm{Name="Neuer Alarm",Tag=Working.Equipment.FirstOrDefault(x=>x.Type!=EquipmentType.Drawing)?.Tag??""};if(_activeScenario is {} s)alarm.ScenarioIds.Add(s.Id);Working.Alarms.Add(alarm);RefreshAlarmRows();};var remove=new Button{Content="Alarm löschen"};remove.Click+=(_,_)=>{if(AlarmGrid.SelectedItem is AlarmAssignmentRow row){Working.Alarms.Remove(row.Alarm);RefreshAlarmRows();}};buttons.Children.Add(add);buttons.Children.Add(remove);DockPanel.SetDock(buttons,Dock.Bottom);panel.Children.Add(buttons);
        AlarmGrid.Columns.Add(new DataGridCheckBoxColumn{Header="Im Szenario",Binding=new Binding("Assigned"){UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},Width=90});
        AlarmGrid.Columns.Add(new DataGridTextColumn{Header="Name",Binding=new Binding("Name"),Width=180});
        AlarmGrid.Columns.Add(new DataGridComboBoxColumn{Header="Betriebsmittel",ItemsSource=Working.Equipment.Where(x=>x.Type!=EquipmentType.Drawing).Select(x=>x.Tag).ToArray(),SelectedItemBinding=new Binding("Tag"),Width=150});
        AlarmGrid.Columns.Add(new DataGridComboBoxColumn{Header="Priorität",ItemsSource=Enum.GetValues<AlarmPriority>(),SelectedItemBinding=new Binding("Priority"),Width=110});
        foreach(var (header,path) in new[]{("Auslöser","Trigger"),("Auswirkung","Impact"),("Abhilfemaßnahme","OperatorAction")})AlarmGrid.Columns.Add(new DataGridTextColumn{Header=header,Binding=new Binding(path),Width=180});
        RefreshAlarmRows();panel.Children.Add(AlarmGrid);return panel;
    }
    private void RefreshAlarmRows()=>AlarmGrid.ItemsSource=Working.Alarms.Where(a=>!a.IsDerived).Select(a=>new AlarmAssignmentRow(a,_activeScenario?.Id)).ToArray();
    private bool ValidateAlarms()
    {
        if(!AlarmGrid.CommitEdit(DataGridEditingUnit.Cell,true)||!AlarmGrid.CommitEdit(DataGridEditingUnit.Row,true))return false;
        if(Working.Alarms.Any(a=>string.IsNullOrWhiteSpace(a.Name)||!Working.Equipment.Any(e=>e.Type!=EquipmentType.Drawing&&e.Tag==a.Tag))){if(IsVisible)MessageBox.Show(this,"Alarme benötigen einen Namen und ein vorhandenes Betriebsmittel.");return false;}return true;
    }
}
