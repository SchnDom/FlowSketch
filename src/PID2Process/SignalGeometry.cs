namespace PID2Process;

// Pure graphical relationships. No pipe references, media edges, or executable control logic.
internal static class SignalGeometry
{
    internal static string[] Ports(EquipmentItem item)=>item.Type is EquipmentType.Text or EquipmentType.Legend or EquipmentType.Drawing?[]:["Left","Right","Top","Bottom"];
    internal static PipePoint Port(EquipmentItem item,string port,double fraction=.5)
    {
        var f=Math.Clamp(fraction,0,1);return port switch{"Left"=>new(item.X,item.Y+item.Height*f),"Top"=>new(item.X+item.Width*f,item.Y),"Bottom"=>new(item.X+item.Width*f,item.Y+item.Height),_=>new(item.X+item.Width,item.Y+item.Height*f)};
    }
    internal static List<PipePoint> Resolve(EngineeringProject project,SignalLine line)
    {
        var points=line.Points.Select(p=>new PipePoint(p.X,p.Y)).ToList();if(points.Count<2)return points;
        PipePoint End(SignalConnection? c,PipePoint fallback)=>c is not null&&project.Equipment.FirstOrDefault(x=>x.Id==c.EquipmentId&&x.Page==line.Page) is {} item&&Ports(item).Contains(c.Port)?Port(item,c.Port,c.Fraction):fallback;
        points[0]=End(line.StartConnection,points[0]);points[^1]=End(line.EndConnection,points[^1]);return points;
    }
    internal static List<List<PipePoint>> Markers(List<PipePoint> points,double width,double height)
    {
        var result=new List<List<PipePoint>>();for(var i=0;i<points.Count-1;i++)
        {
            var a=points[i];var b=points[i+1];var dx=(b.X-a.X)*width;var dy=(b.Y-a.Y)*height;var length=Math.Sqrt(dx*dx+dy*dy);if(length<1)continue;
            var radius=Math.Min(width/160,length/6);var ux=dx/length;var uy=dy/length;var cx=(a.X+b.X)*width/2;var cy=(a.Y+b.Y)*height/2;
            PipePoint At(double tangent,double normal)=>new((cx+ux*tangent+uy*normal)/width,(cy+uy*tangent-ux*normal)/height);
            result.Add([At(radius*1.5,radius),At(0,radius),At(-radius,-radius),At(radius*.5,-radius)]);
        }
        return result;
    }
}
