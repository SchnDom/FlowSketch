using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
namespace PID2Process;
internal sealed class SensorEditor
{
    internal sealed class ScaleRow {public string Name {get;set;}="";public double Value {get;set;}}
    private readonly ObservableCollection<ScaleRow> _scale;
    private readonly DataGrid _scales=new(){AutoGenerateColumns=false,CanUserAddRows=false,Height=85};
    private readonly DataGrid _limits=new(){AutoGenerateColumns=false,CanUserAddRows=false,Height=145};
    private readonly List<SensorLimit> _rows;
    private readonly SensorSettings _original;
    internal static readonly string[] Sides=["Ausgeblendet","Unten","Links","Rechts","Oben"];
    internal static readonly string[] SideCodes=["Hidden","Bottom","Left","Right","Top"];
    internal SensorEditor(EditorDialog dialog,SensorSettings s)
    {
        _original=s;dialog.FixedChoice("Prozessanschluss",Sides[Math.Max(0,Array.IndexOf(SideCodes,s.ConnectionSide))],Sides);
        dialog.Text("Signalkennung",s.Signal);dialog.Text("Tagnummer",s.Number);
        _scale=new([new(){Name="Prozesswert min",Value=s.ScaledMin},new(){Name="Prozesswert max",Value=s.ScaledMax}]);
        _scales.Columns.Add(new DataGridTextColumn{Header="Skalierung",Binding=new Binding("Name"),IsReadOnly=true,Width=230});_scales.Columns.Add(new DataGridTextColumn{Header="Wert",Binding=new Binding("Value"),Width=180});_scales.ItemsSource=_scale;dialog.Custom("Skalierung",new Expander{Header="Skalierung",IsExpanded=false,Content=_scales});dialog.Text("Messeinheit",s.Unit);
        _rows=s.Limits.Select(x=>new SensorLimit{Name=x.Name,Enabled=x.Enabled,Value=x.Value}).ToList();
        _limits.Columns.Add(new DataGridTextColumn{Header="Alarm",Binding=new Binding("Name"),IsReadOnly=true,Width=160});_limits.Columns.Add(new DataGridCheckBoxColumn{Header="Aktiviert",Binding=new Binding("Enabled"),Width=90});_limits.Columns.Add(new DataGridTextColumn{Header="Alarmwert",Binding=new Binding("Value"),Width=160});_limits.ItemsSource=_rows;dialog.Custom("Alarmwerte",new Expander{Header="Alarme",IsExpanded=false,Content=_limits});
    }
    internal string? Validate(EditorDialog d)
    {
        if(!_scales.CommitEdit(DataGridEditingUnit.Cell,true)||!_scales.CommitEdit(DataGridEditingUnit.Row,true)||!_limits.CommitEdit(DataGridEditingUnit.Cell,true)||!_limits.CommitEdit(DataGridEditingUnit.Row,true))return "Tabellenwerte als gültige Zahlen eingeben.";
        if(string.IsNullOrWhiteSpace(d.Value("Signalkennung"))||string.IsNullOrWhiteSpace(d.Value("Tagnummer")))return "Signalkennung und Tagnummer eingeben.";
        if(_scale.Any(x=>!double.IsFinite(x.Value))||_scale[1].Value<=_scale[0].Value||_rows.Any(x=>!double.IsFinite(x.Value)))return "Gültige Prozesswertgrenzen mit min < max und endliche Alarmwerte eingeben.";
        return null;
    }
    internal SensorSettings Result(EditorDialog d)=>new(){ConnectionSide=SideCodes[Array.IndexOf(Sides,d.Value("Prozessanschluss"))],Connection=SideCodes[Array.IndexOf(Sides,d.Value("Prozessanschluss"))]==_original.ConnectionSide?_original.Connection:null,ConnectionEnd=SideCodes[Array.IndexOf(Sides,d.Value("Prozessanschluss"))]==_original.ConnectionSide?_original.ConnectionEnd:null,Signal=d.Value("Signalkennung"),Number=d.Value("Tagnummer"),RawMin=_original.RawMin,RawMax=_original.RawMax,ScaledMin=_scale[0].Value,ScaledMax=_scale[1].Value,Unit=d.Value("Messeinheit"),Limits=_rows};
}
