namespace PID2Process;

// Graphical process tapping. It never adds an edge to the medium network.
internal static class SensorConnections
{
    internal static double Aspect(EngineeringProject p,int page)=>p.BlankSheet is {} b?b.HeightMm/b.WidthMm:p.Pdf.PageAspectRatios.ElementAtOrDefault(page-1) is >0 and var a?a:.708;
    internal static List<PipePoint> Resolve(EngineeringProject p,EquipmentItem sensor)
    {
        var side=sensor.Sensor.ConnectionSide;if(side=="Hidden")return [];
        var start=SignalGeometry.Port(sensor,side);var aspect=Aspect(p,sensor.Page);
        var end=sensor.Sensor.ConnectionEnd??new(start.X+(side=="Left"?-.025:side=="Right"?.025:0),start.Y+(side=="Top"?-.025/aspect:side=="Bottom"?.025/aspect:0));
        if(sensor.Sensor.Connection is {PipeId:Guid id} c&&p.Pipes.FirstOrDefault(x=>x.Id==id&&x.Page==sensor.Page) is {} pipe)
        {
            var path=PipeNetwork.Resolve(p,pipe);var i=Math.Clamp(c.Segment,0,path.Count-2);var f=Math.Clamp(c.Fraction,0,1);end=new(path[i].X+(path[i+1].X-path[i].X)*f,path[i].Y+(path[i+1].Y-path[i].Y)*f);
        }
        var bend=side is "Top" or "Bottom"?new PipePoint(start.X,end.Y):new PipePoint(end.X,start.Y);
        return new[]{start,bend,end}.Where((q,i)=>i==0||Math.Abs(q.X-(i==1?start:bend).X)+Math.Abs(q.Y-(i==1?start:bend).Y)>1e-10).ToList();
    }
    internal static (PipeConnection? Connection,PipePoint Point) Snap(EngineeringProject p,int page,PipePoint point,double tolerance=.012)
    {
        var best=tolerance;PipeConnection? connection=null;var result=new PipePoint(point.X,point.Y);var aspect=Aspect(p,page);
        foreach(var pipe in p.Pipes.Where(x=>x.Page==page))
        {
            var path=PipeNetwork.Resolve(p,pipe);for(int i=0;i<path.Count-1;i++)
            {
                var a=path[i];var b=path[i+1];var dx=b.X-a.X;var dy=(b.Y-a.Y)*aspect;var length=dx*dx+dy*dy;if(length<1e-15)continue;
                var f=Math.Clamp(((point.X-a.X)*dx+(point.Y-a.Y)*aspect*dy)/length,0,1);var q=new PipePoint(a.X+f*dx,a.Y+f*(b.Y-a.Y));var distance=Math.Sqrt(Math.Pow(q.X-point.X,2)+Math.Pow((q.Y-point.Y)*aspect,2));
                if(distance<best){best=distance;connection=new(){PipeId=pipe.Id,Segment=i,Fraction=f};result=q;}
            }
        }
        return(connection,result);
    }
    internal static void BindNearest(EngineeringProject p,EquipmentItem sensor)
    {
        var side=sensor.Sensor.ConnectionSide;if(side=="Hidden")return;var start=SignalGeometry.Port(sensor,side);var best=.18;PipePoint? target=null;var aspect=Aspect(p,sensor.Page);
        foreach(var pipe in p.Pipes.Where(x=>x.Page==sensor.Page))
        {
            var path=PipeNetwork.Resolve(p,pipe);for(int i=0;i<path.Count-1;i++)
            {
                var a=path[i];var b=path[i+1];var vertical=side is "Top" or "Bottom";var denominator=vertical?b.X-a.X:b.Y-a.Y;if(Math.Abs(denominator)<1e-12)continue;
                var f=vertical?(start.X-a.X)/denominator:(start.Y-a.Y)/denominator;if(f<0||f>1)continue;
                var q=new PipePoint(a.X+f*(b.X-a.X),a.Y+f*(b.Y-a.Y));var delta=vertical?(q.Y-start.Y)*aspect:q.X-start.X;var forward=side is "Top" or "Left"?-delta:delta;
                if(forward>0&&forward<best){best=forward;target=q;}
            }
        }
        if(target is not null){var snap=Snap(p,sensor.Page,target);sensor.Sensor.Connection=snap.Connection;sensor.Sensor.ConnectionEnd=snap.Point;}
    }
}
