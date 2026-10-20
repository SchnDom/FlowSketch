namespace PID2Process;
public sealed class MediaPalette
{
    public string Empty {get;set;}="#808080";
    public string Liquid {get;set;}="#0000FF";
    public string Solid {get;set;}="#FF00FF";
    public string Gas {get;set;}="#FFAA33";
    public string Mixed {get;set;}="#8000FF";
    public string Get(MediumType? type)=>type switch{MediumType.Liquid=>Liquid,MediumType.Solid=>Solid,MediumType.Gas=>Gas,MediumType.Mixed=>Mixed,_=>Empty};
}
public sealed class MediumDefinition
{
    public Guid Id {get;set;}=Guid.NewGuid();
    public string Name {get;set;}="";
    public MediumType Type {get;set;}=MediumType.Liquid;
    public string Color {get;set;}="#0000FF";
}
internal static class MediaSettings
{
    internal static void Normalize(EngineeringProject p)
    {
        if(p.Media.Count==0)foreach(var row in p.Equipment.Where(e=>e.Type==EquipmentType.Legend).SelectMany(e=>e.LegendEntries).GroupBy(r=>r.Text).Select(g=>g.First()))p.Media.Add(new(){Name=row.Text,Color=row.Color,Type=row.Text.Contains("Luft",StringComparison.OrdinalIgnoreCase)||row.Text.Contains("Dampf",StringComparison.OrdinalIgnoreCase)?MediumType.Gas:MediumType.Liquid});
        foreach(var e in p.Equipment.Where(x=>x.Type is EquipmentType.MediaSource or EquipmentType.Vessel))
        {
            var m=p.Media.FirstOrDefault(m=>m.Id==e.MediumId)??p.Media.FirstOrDefault(m=>m.Name==e.MediaName&&m.Type==e.MediumType);
            if(m is null&&!string.IsNullOrWhiteSpace(e.MediaName)){m=new(){Name=e.MediaName,Type=e.MediumType,Color=p.MediaColors.Get(e.MediumType)};p.Media.Add(m);}
            if(m is not null){e.MediumId=m.Id;e.MediaName=m.Name;e.MediumType=m.Type;}
        }
        Synchronize(p);
    }
    internal static void Synchronize(EngineeringProject p)
    {
        foreach(var e in p.Equipment)if(p.Media.FirstOrDefault(m=>m.Id==e.MediumId) is {} m){e.MediaName=m.Name;e.MediumType=m.Type;if(e.Type==EquipmentType.MediaSource)e.Color=m.Color;}
        foreach(var e in p.Equipment.Where(x=>x.Type==EquipmentType.Legend&&x.AutomaticLegend))e.LegendEntries=p.Media.Select(m=>new LegendEntry{MediumId=m.Id,Text=m.Name,Color=m.Color}).ToList();
    }
    internal static string Color(EngineeringProject p,MediumType? type,string name)=>p.Media.FirstOrDefault(m=>m.Type==type&&m.Name==name)?.Color??p.MediaColors.Get(type);
    internal static void CreateSources(EngineeringProject p,int page)
    {
        Normalize(p);var i=0;
        foreach(var m in p.Media.Where(m=>!p.Equipment.Any(x=>x.Type==EquipmentType.MediaSource&&x.MediumId==m.Id&&x.Page==page)).ToArray())
        {
            var tag=m.Name;var n=1;while(p.Equipment.Any(x=>x.Tag==tag))tag=m.Name+"-"+n++;
            p.Equipment.Add(new(){Type=EquipmentType.MediaSource,Variant="Medienquelle",Tag=tag,MediumId=m.Id,MediaName=m.Name,MediumType=m.Type,Color=m.Color,Page=page,X=.05+(i%4)*.2,Y=.05+(i/4)*.09,Width=.085,Height=.07});i++;
        }
    }
}
