using System.Text.Json;
using System.Windows;
namespace PID2Process;

public sealed class OverlayClip
{
    public string Kind {get;set;}="FlowSketch.Selection.1";
    public EngineeringProject Selection {get;set;}=new();
    public double PageWidth {get;set;}=2000;
    public double PageHeight {get;set;}=1400;
}
internal static class OverlayClipboard
{
    internal static OverlayClip Copy(EngineeringProject source,ISet<Guid> ids,int page)
    {
        var p=DocumentExport.Snapshot(source);DrawingGeometry.Normalize(p);
        var kept=OverlaySelection.ExpandGroups(p,ids,page);
        var removed=p.Equipment.Select(x=>x.Id).Concat(p.Pipes.Select(x=>x.Id)).Concat(p.SignalLines.Select(x=>x.Id)).Where(id=>!kept.Contains(id)).ToHashSet();OverlaySelection.Delete(p,removed);
        var width=source.BlankSheet is {} b?2000*b.WidthMm/297:2000;var aspect=source.BlankSheet is {} blank?blank.HeightMm/blank.WidthMm:source.Pdf.PageAspectRatios.ElementAtOrDefault(page-1);if(aspect<=0)aspect=.708;
        p.Pdf=new(){PageCount=1,PageAspectRatios=[aspect]};p.Composition=null;p.Approval=new();
        return new(){Selection=p,PageWidth=width,PageHeight=width*aspect};
    }
    internal static HashSet<Guid> Paste(EngineeringProject target,OverlayClip input,int page,double targetWidth,double targetHeight,Point center)
    {
        var working=DocumentExport.Snapshot(target);var result=PasteCore(working,input,page,targetWidth,targetHeight,center);if(result.Count==0)return result;
        target.Equipment=working.Equipment;target.Pipes=working.Pipes;target.SignalLines=working.SignalLines;target.Media=working.Media;target.Scenarios=working.Scenarios;target.ControlSteps=working.ControlSteps;target.Approval.IsApproved=false;return result;
    }
    private static HashSet<Guid> PasteCore(EngineeringProject target,OverlayClip input,int page,double targetWidth,double targetHeight,Point center)
    {
        if(input.Kind!="FlowSketch.Selection.1"||!double.IsFinite(input.PageWidth)||input.PageWidth<=0||!double.IsFinite(input.PageHeight)||input.PageHeight<=0)throw new InvalidOperationException("Ungültige FlowSketch-Auswahl.");
        var clip=JsonSerializer.Deserialize<OverlayClip>(JsonSerializer.Serialize(input,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;var p=clip.Selection;
        var ids=p.Equipment.Select(x=>x.Id).Concat(p.Pipes.Select(x=>x.Id)).Concat(p.SignalLines.Select(x=>x.Id)).ToDictionary(x=>x,_=>Guid.NewGuid());if(ids.Count==0)return [];if(ids.Count>20000)throw new InvalidOperationException("Die Auswahl ist zu groß.");
        var groups=new Dictionary<Guid,Guid>();Guid? Group(Guid? id){if(id is not Guid key)return null;if(!groups.ContainsKey(key))groups[key]=Guid.NewGuid();return groups[key];}
        var mediaBySource=new Dictionary<Guid,Guid>();foreach(var m in p.Media){var sourceId=m.Id;var existing=target.Media.FirstOrDefault(x=>x.Name==m.Name&&x.Type==m.Type);if(existing is null){m.Id=Guid.NewGuid();var name=m.Name;var i=2;while(target.Media.Any(x=>x.Name==m.Name))m.Name=name+" ("+i+++ ")";target.Media.Add(m);existing=m;}mediaBySource[sourceId]=existing.Id;}
        Guid? Medium(Guid? id)=>id is Guid key&&mediaBySource.TryGetValue(key,out var value)?value:null;
        var tags=new Dictionary<string,string>();string Tag(string original){var name=original;var i=2;while(target.Equipment.Any(x=>x.Tag==name)||tags.Values.Contains(name))name=original+"-Kopie"+(i++==2?"":"-"+(i-1));tags[original]=name;return name;}
        var sx=clip.PageWidth/targetWidth;var sy=clip.PageHeight/targetHeight;
        void Point(PipePoint q){q.X*=sx;q.Y*=sy;}
        void Connection(PipeConnection? c){if(c is null)return;if(c.EquipmentId is Guid e)c.EquipmentId=ids.GetValueOrDefault(e);if(c.PipeId is Guid r)c.PipeId=ids.GetValueOrDefault(r);if(c.PreservedPoint is {} q)Point(q);}
        foreach(var x in p.Equipment){x.Id=ids[x.Id];x.Tag=Tag(x.Tag);x.Page=page;x.GroupId=Group(x.GroupId);x.X*=sx;x.Y*=sy;x.Width*=sx;x.Height*=sy;x.TagOffsetX*=sx;x.TagOffsetY*=sy;x.MediumId=Medium(x.MediumId);foreach(var row in x.LegendEntries)row.MediumId=Medium(row.MediumId);if(x.Drawing.Start is {} a)Point(a);if(x.Drawing.End is {} z)Point(z);x.Drawing.PageWidth=targetWidth;if(x.Sensor.ConnectionEnd is {} end)Point(end);Connection(x.Sensor.Connection);}
        foreach(var x in p.Pipes){x.Id=ids[x.Id];x.Page=page;x.GroupId=Group(x.GroupId);foreach(var q in x.Points)Point(q);Connection(x.StartConnection);Connection(x.EndConnection);}
        foreach(var x in p.SignalLines){x.Id=ids[x.Id];x.Page=page;x.GroupId=Group(x.GroupId);foreach(var q in x.Points)Point(q);foreach(var c in new[]{x.StartConnection,x.EndConnection})if(c is not null)c.EquipmentId=ids[c.EquipmentId];}
        var bounds=p.Equipment.Select(OverlaySelection.Bounds).Concat(p.Pipes.Select(x=>OverlaySelection.Bounds(p,x))).Concat(p.SignalLines.Select(x=>OverlaySelection.Bounds(p,x))).Aggregate(Rect.Empty,(a,b)=>{a.Union(b);return a;});
        if(bounds.IsEmpty)return [];if(!double.IsFinite(bounds.X)||!double.IsFinite(bounds.Y)||bounds.Width>1||bounds.Height>1)throw new InvalidOperationException("Die kopierte Auswahl passt in dieser Größe nicht auf das Zielblatt. Größeres Blatt wählen.");
        var dx=Math.Clamp(center.X-bounds.Width/2,0,1-bounds.Width)-bounds.Left;var dy=Math.Clamp(center.Y-bounds.Height/2,0,1-bounds.Height)-bounds.Top;
        OverlaySelection.Move(p,ids.Values.ToHashSet(),dx,dy);
        void Assign<T>(Dictionary<Guid,T> from,Dictionary<Guid,T> to){foreach(var (key,value) in from)if(ids.TryGetValue(key,out var id))to[id]=value;}
        void Tanks(Dictionary<Guid,TankAssignment> from,Dictionary<Guid,TankAssignment> to){foreach(var t in from.Values)t.MediumId=Medium(t.MediumId);Assign(from,to);}
        var scenarios=new Dictionary<Guid,Guid>();foreach(var s in p.Scenarios)
        {
            var steps=p.ControlSteps.Where(t=>t.ScenarioId==s.Id).ToArray();if(!s.EquipmentStates.Keys.Concat(s.PipeStates.Keys).Concat(s.TextValues.Keys).Concat(s.SensorValues.Keys).Concat(s.TankStates.Keys).Concat(steps.SelectMany(t=>t.EquipmentStates.Keys.Concat(t.PipeStates.Keys).Concat(t.TextValues.Keys).Concat(t.SensorValues.Keys).Concat(t.TankStates.Keys))).Any(ids.ContainsKey))continue;
            var destination=target.Scenarios.FirstOrDefault(x=>x.Id==s.Id);if(destination is null){destination=new(){Name=s.Name,Description=s.Description};var name=s.Name;var i=2;while(target.Scenarios.Any(x=>x.Name==destination.Name))destination.Name=name+" (Kopie "+i+++ ")";target.Scenarios.Add(destination);}scenarios[s.Id]=destination.Id;
            Assign(s.EquipmentStates,destination.EquipmentStates);Assign(s.PipeStates,destination.PipeStates);Assign(s.TextValues,destination.TextValues);Assign(s.SensorValues,destination.SensorValues);Tanks(s.TankStates,destination.TankStates);
        }
        foreach(var t in p.ControlSteps)
        {
            if(t.ScenarioId is Guid parent&&!scenarios.ContainsKey(parent))continue;if(!t.EquipmentStates.Keys.Concat(t.PipeStates.Keys).Concat(t.TextValues.Keys).Concat(t.SensorValues.Keys).Concat(t.TankStates.Keys).Any(ids.ContainsKey))continue;
            var destination=target.ControlSteps.FirstOrDefault(x=>x.Id==t.Id);if(destination is null){destination=new(){Name=t.Name,Description=t.Description,Preconditions=t.Preconditions,Actions=t.Actions,Transition=t.Transition,ScenarioId=t.ScenarioId is Guid id?scenarios[id]:null};target.ControlSteps.Add(destination);}
            Assign(t.EquipmentStates,destination.EquipmentStates);Assign(t.PipeStates,destination.PipeStates);Assign(t.TextValues,destination.TextValues);Assign(t.SensorValues,destination.SensorValues);Tanks(t.TankStates,destination.TankStates);foreach(var tag in t.Tags.Where(tags.ContainsKey))if(!destination.Tags.Contains(tags[tag]))destination.Tags.Add(tags[tag]);
        }
        target.Equipment.AddRange(p.Equipment);target.Pipes.AddRange(p.Pipes);target.SignalLines.AddRange(p.SignalLines);ScenarioSteps.Ensure(target);MediaSettings.Synchronize(target);DrawingGeometry.Normalize(target);target.Approval.IsApproved=false;return ids.Values.ToHashSet();
    }
}
