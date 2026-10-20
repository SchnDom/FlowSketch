namespace PID2Process;
public sealed class SensorSettings
{
    public string ConnectionSide {get;set;}="Bottom";
    public PipeConnection? Connection {get;set;}
    public PipePoint? ConnectionEnd {get;set;}
    public string Signal {get;set;}="PE";
    public string Number {get;set;}="1234";
    public double RawMin {get;set;}
    public double RawMax {get;set;}=27648;
    public double ScaledMin {get;set;}
    public double ScaledMax {get;set;}=100;
    public string Unit {get;set;}="%";
    public List<SensorLimit> Limits {get;set;}=[new(){Name="High-High",Value=99},new(){Name="High",Value=95},new(){Name="Low",Value=5},new(){Name="Low-Low",Value=1}];
    public double Scale(double raw)=>ScaledMin+(raw-RawMin)/(RawMax-RawMin)*(ScaledMax-ScaledMin);
}
public sealed class SensorLimit
{
    public string Name {get;set;}="";
    public bool Enabled {get;set;}=true;
    public double Value {get;set;}
}
internal sealed record SensorAlarmView(string UpperColor,string LowerColor,string Text);
internal static class SensorEvaluation
{
    internal static SensorAlarmView Display(EquipmentItem item,SceneValues v)
    {
        if(!v.SensorValues.TryGetValue(item.Id,out var raw))return new("#FFFFFF","#FFFFFF","");
        var s=item.Sensor;var value=s.Scale(raw);
        SensorLimit? Limit(string name)=>s.Limits.FirstOrDefault(l=>l.Name==name&&l.Enabled&&(name.StartsWith("High")?value>=l.Value:value<=l.Value));
        var upper=Limit("High-High")??Limit("High");var lower=Limit("Low-Low")??Limit("Low");
        string Color(SensorLimit? l)=>l is null?"#FFFFFF":l.Name is "High-High" or "Low-Low"?"#FF0000":"#FFFF00";
        string Text(SensorLimit l)=>(l.Name switch{"High-High"=>"HH","High"=>"H","Low-Low"=>"LL",_=>"L"})+": "+l.Value.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+" "+s.Unit;
        return new(Color(upper),Color(lower),string.Join(" · ",new[]{lower,upper}.Where(l=>l is not null).Select(l=>Text(l!))));
    }
    internal static List<Alarm> Alarms(EngineeringProject p,SceneValues v)=>p.Equipment.Where(x=>x.Type==EquipmentType.Instrument&&v.SensorValues.ContainsKey(x.Id)).SelectMany(x=>{
        var value=x.Sensor.Scale(v.SensorValues[x.Id]);
        return x.Sensor.Limits.Where(l=>l.Enabled&&(l.Name.StartsWith("High")?value>=l.Value:value<=l.Value)).Select(l=>new Alarm{IsDerived=true,Tag=x.Tag,Name=x.Tag+" · "+l.Name+" aktiv",Trigger=$"{value:0.###} {x.Sensor.Unit} · Grenzwert {l.Value:0.###}",Priority=l.Name is "High-High" or "Low-Low"?AlarmPriority.Critical:AlarmPriority.Warning});
    }).ToList();
}
