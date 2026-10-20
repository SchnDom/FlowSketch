using System.Globalization;
using System.Windows;
namespace PID2Process;

internal sealed record SelectionPatch(string? Function=null,string? Location=null,string? Color=null,double? Opacity=null,double? Width=null,double? Height=null);
internal static class OverlaySelection
{
    internal static Rect Bounds(EquipmentItem x)=>new(x.X,x.Y,x.Width,x.Height);
    internal static Rect Bounds(EngineeringProject p,PipeSegment pipe)
    {
        var points=PipeNetwork.Resolve(p,pipe);return new Rect(new Point(points.Min(x=>x.X),points.Min(x=>x.Y)),new Point(points.Max(x=>x.X),points.Max(x=>x.Y)));
    }
    internal static Rect Bounds(EngineeringProject p,SignalLine signal)
    {
        var points=SignalGeometry.Resolve(p,signal);return points.Count==0?Rect.Empty:new Rect(new Point(points.Min(x=>x.X),points.Min(x=>x.Y)),new Point(points.Max(x=>x.X),points.Max(x=>x.Y)));
    }
    // Window selection: all of an object's geometry must lie inside the frame.
    internal static HashSet<Guid> InRectangle(EngineeringProject p,int page,Rect rect)
    {
        return p.Equipment.Where(x=>x.Page==page&&rect.Contains(Bounds(x))).Select(x=>x.Id).Concat(p.Pipes.Where(x=>x.Page==page&&rect.Contains(Bounds(p,x))).Select(x=>x.Id)).Concat(p.SignalLines.Where(x=>x.Page==page&&rect.Contains(Bounds(p,x))).Select(x=>x.Id)).ToHashSet();
    }
    internal static HashSet<Guid> ExpandGroups(EngineeringProject p,IEnumerable<Guid> ids,int page)
    {
        var result=ids.ToHashSet();var groups=p.Equipment.Where(x=>result.Contains(x.Id)).Select(x=>x.GroupId).Concat(p.Pipes.Where(x=>result.Contains(x.Id)).Select(x=>x.GroupId)).Concat(p.SignalLines.Where(x=>result.Contains(x.Id)).Select(x=>x.GroupId)).Where(x=>x.HasValue).ToHashSet();
        result.UnionWith(p.Equipment.Where(x=>x.Page==page&&x.GroupId.HasValue&&groups.Contains(x.GroupId)).Select(x=>x.Id));result.UnionWith(p.Pipes.Where(x=>x.Page==page&&x.GroupId.HasValue&&groups.Contains(x.GroupId)).Select(x=>x.Id));result.UnionWith(p.SignalLines.Where(x=>x.Page==page&&x.GroupId.HasValue&&groups.Contains(x.GroupId)).Select(x=>x.Id));return result;
    }
    internal static Guid Group(EngineeringProject p,ISet<Guid> ids)
    {
        var group=Guid.NewGuid();foreach(var x in p.Equipment.Where(x=>ids.Contains(x.Id)))x.GroupId=group;foreach(var x in p.Pipes.Where(x=>ids.Contains(x.Id)))x.GroupId=group;foreach(var x in p.SignalLines.Where(x=>ids.Contains(x.Id)))x.GroupId=group;return group;
    }
    internal static void Ungroup(EngineeringProject p,ISet<Guid> ids){foreach(var x in p.Equipment.Where(x=>ids.Contains(x.Id)))x.GroupId=null;foreach(var x in p.Pipes.Where(x=>ids.Contains(x.Id)))x.GroupId=null;foreach(var x in p.SignalLines.Where(x=>ids.Contains(x.Id)))x.GroupId=null;}
    internal static void Apply(EngineeringProject p,ISet<Guid> ids,SelectionPatch patch)
    {
        foreach(var x in p.Equipment.Where(x=>ids.Contains(x.Id))){if(patch.Function is not null)x.Function=patch.Function;if(patch.Location is not null)x.InstallationLocation=patch.Location;if(patch.Color is not null)x.Color=patch.Color;if(patch.Opacity is double o)x.Opacity=Math.Clamp(o,0,1);if(patch.Width is double w)x.Width=Math.Clamp(w,.001,1-x.X);if(patch.Height is double h)x.Height=Math.Clamp(h,.001,1-x.Y);}
        foreach(var x in p.Pipes.Where(x=>ids.Contains(x.Id))){if(patch.Function is not null)x.Function=patch.Function;if(patch.Location is not null)x.InstallationLocation=patch.Location;if(patch.Color is not null)x.Color=patch.Color;if(patch.Opacity is double o)x.Opacity=Math.Clamp(o,0,1);}
        foreach(var x in p.SignalLines.Where(x=>ids.Contains(x.Id))){if(patch.Function is not null)x.Function=patch.Function;if(patch.Location is not null)x.InstallationLocation=patch.Location;if(patch.Color is not null)x.Color=patch.Color;if(patch.Opacity is double o)x.Opacity=Math.Clamp(o,0,1);}
    }
    internal static void Delete(EngineeringProject p,ISet<Guid> ids)
    {
        // Resolve all retained paths before removing any node. Deletion order must
        // not move an attached child or leave stale references in scenarios.
        foreach(var pipe in p.Pipes.Where(x=>!ids.Contains(x.Id))){var points=PipeNetwork.Resolve(p,pipe);bool Removed(PipeConnection? c)=>c?.EquipmentId is Guid e&&ids.Contains(e)||c?.PipeId is Guid r&&ids.Contains(r);if(Removed(pipe.StartConnection)){pipe.Points=points;pipe.StartConnection=null;}if(Removed(pipe.EndConnection)){pipe.Points=points;pipe.EndConnection=null;}}
        foreach(var signal in p.SignalLines.Where(x=>!ids.Contains(x.Id))){var points=SignalGeometry.Resolve(p,signal);if(signal.StartConnection is {} start&&ids.Contains(start.EquipmentId)){signal.Points=points;signal.StartConnection=null;}if(signal.EndConnection is {} end&&ids.Contains(end.EquipmentId)){signal.Points=points;signal.EndConnection=null;}}
        foreach(var sensor in p.Equipment.Where(x=>x.Type==EquipmentType.Instrument&&x.Sensor.Connection?.PipeId is Guid pipeId&&ids.Contains(pipeId))){var points=SensorConnections.Resolve(p,sensor);sensor.Sensor.ConnectionEnd=points.LastOrDefault();sensor.Sensor.Connection=null;}
        p.SignalLines.RemoveAll(x=>ids.Contains(x.Id));
        var tags=p.Equipment.Where(x=>ids.Contains(x.Id)).Select(x=>x.Tag).ToHashSet();p.Equipment.RemoveAll(x=>ids.Contains(x.Id));p.Pipes.RemoveAll(x=>ids.Contains(x.Id));
        foreach(var s in p.Scenarios)foreach(var id in ids){s.EquipmentStates.Remove(id);s.PipeStates.Remove(id);s.TextValues.Remove(id);s.TankStates.Remove(id);s.SensorValues.Remove(id);}
        foreach(var s in p.ControlSteps){foreach(var id in ids){s.EquipmentStates.Remove(id);s.PipeStates.Remove(id);s.TextValues.Remove(id);s.TankStates.Remove(id);s.SensorValues.Remove(id);}s.Tags.RemoveAll(tags.Contains);}
    }
    internal static void Move(EngineeringProject p,ISet<Guid> ids,double dx,double dy)
    {
        var devices=p.Equipment.Where(x=>ids.Contains(x.Id)).ToArray();var pipes=p.Pipes.Where(x=>ids.Contains(x.Id)).ToArray();var signals=p.SignalLines.Where(x=>ids.Contains(x.Id)).ToArray();
        var bounds=devices.Select(Bounds).Concat(pipes.Select(x=>Bounds(p,x))).Concat(signals.Select(x=>Bounds(p,x))).Where(x=>!x.IsEmpty).ToArray();if(bounds.Length==0)return;dx=Math.Clamp(dx,-bounds.Min(x=>x.Left),1-bounds.Max(x=>x.Right));dy=Math.Clamp(dy,-bounds.Min(x=>x.Top),1-bounds.Max(x=>x.Bottom));
        // Materialize legacy points before moving devices, to avoid a double offset.
        foreach(var pipe in pipes)pipe.Points=PipeNetwork.Resolve(p,pipe);
        foreach(var signal in signals)signal.Points=SignalGeometry.Resolve(p,signal);
        foreach(var d in devices){d.X+=dx;d.Y+=dy;if(d.Drawing.Start is {} a){a.X+=dx;a.Y+=dy;}if(d.Drawing.End is {} b){b.X+=dx;b.Y+=dy;}}
        foreach(var pipe in pipes)foreach(var point in pipe.Points){point.X+=dx;point.Y+=dy;}
        foreach(var signal in signals)foreach(var point in signal.Points){point.X+=dx;point.Y+=dy;}
    }
}

internal static class MediaDisplay
{
    internal static string Text(EquipmentItem x)
    {
        var lines=new List<string>{x.MediaName};if(!string.IsNullOrWhiteSpace(x.Description))lines.Add(x.Description);
        string Number(double n)=>n.ToString("0.###",CultureInfo.GetCultureInfo("de-DE"));
        if(x.TemperatureCelsius is double t)lines.Add("T: "+Number(t)+" °C");if(x.PressureBar is double p)lines.Add("p: "+Number(p)+" bar");if(x.FlowVelocityMetersPerSecond is double v)lines.Add("v: "+Number(v)+" m/s");return string.Join("\n",lines);
    }
    internal static bool TryOptional(string value,out double? number)
    {
        number=null;if(string.IsNullOrWhiteSpace(value))return true;
        if(!double.TryParse(value.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out var n)||!double.IsFinite(n))return false;number=n;return true;
    }
}
