using System.IO;
using System.Text.Json;
namespace PID2Process;
internal static class CheckValveSelfTest
{
    internal static void Run(string output,Action<bool,string> check)
    {
        var cases=new List<object>();
        foreach(var variant in new[]{"Rückschlagarmatur","Rückschlagklappe"})foreach(var reversed in new[]{false,true})foreach(var sourceRight in new[]{false,true})foreach(var outlet in new[]{false,true})
        {
            var p=ProjectServices.NewBlank();var valve=new EquipmentItem{Type=EquipmentType.IsolationValve,Variant=variant,CheckValveReversed=reversed,X=.4,Y=.4,Width=.1,Height=.1};var source=new EquipmentItem{Type=EquipmentType.MediaSource,MediumType=MediumType.Liquid,MediaName="Wasser",X=sourceRight?.8:.05,Y=.4};p.Equipment.AddRange([valve,source]);
            p.Pipes.Add(new(){Points=[new(.1,.45),new(.4,.45)],StartConnection=new(){EquipmentId=source.Id,Port="Right"},EndConnection=new(){EquipmentId=valve.Id,Port=sourceRight?"Right":"Left"}});
            if(outlet)p.Pipes.Add(new(){Points=[new(.5,.45),new(.9,.45)],StartConnection=new(){EquipmentId=valve.Id,Port=sourceRight?"Left":"Right"}});
            var scene=PipeNetwork.EvaluateScene(p,new());var expected=outlet&&reversed==sourceRight;
            check(scene.CheckValves[valve.Id]==expected&&scene.Pipes.Values.Any(x=>x.Flowing)==expected,$"{variant}: gespiegelt={reversed}, Quelle rechts={sourceRight}, Ziel={outlet}: Durchfluss={expected}");
            var status=PageVisual.Status(valve,new(),scene)!;check(SymbolCatalog.FillColor(valve,SymbolCatalog.Get(valve).Shapes.First(),status)==(expected?"#00FF00":"#808080"),"Rückschlagfarbe folgt tatsächlichem Netzpfad: "+status);
            cases.Add(new{project=p,values=new{equipmentStates=new Dictionary<Guid,string>(),tankStates=new Dictionary<Guid,TankAssignment>()},id=valve.Id,expected});
            if(reversed&&sourceRight&&outlet){var copy=JsonSerializer.Deserialize<EngineeringProject>(JsonSerializer.Serialize(p,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;check(copy.Equipment[0].CheckValveReversed,"Rückschlagrichtung bleibt im JSON-Roundtrip erhalten");}
        }
        File.WriteAllText(Path.Combine(output,"check-valve-cases.json"),JsonSerializer.Serialize(cases,ProjectServices.JsonOptions));
    }
}
