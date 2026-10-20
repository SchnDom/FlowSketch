using System.Globalization;
using System.Windows;
namespace PID2Process;

internal static class DrawingGeometry
{
    internal static bool IsLine(EquipmentItem x)=>x.Type==EquipmentType.Drawing&&x.Variant is "Linie" or "Pfeil" or "Doppelpfeil";
    internal static (PipePoint Start,PipePoint End) Ends(EquipmentItem x)
    {
        var d=x.Drawing;
        return (d.Start??new(x.X+x.Width*(d.Vertical?.5:d.ReverseX?1:0),x.Y+x.Height*(d.Horizontal?.5:d.ReverseY?1:0)),
            d.End??new(x.X+x.Width*(d.Vertical?.5:d.ReverseX?0:1),x.Y+x.Height*(d.Horizontal?.5:d.ReverseY?0:1)));
    }
    internal static List<List<Point>> Strokes(EquipmentItem x,double width,double height)
    {
        var (a,b)=Ends(x);var start=new Point(a.X*width,a.Y*height);var end=new Point(b.X*width,b.Y*height);var strokes=new List<List<Point>>{new(){start,end}};
        void Head(Point tip,Point tail)
        {
            var direction=tip-tail;var length=direction.Length;if(length<.001)return;direction.Normalize();var size=Math.Min(x.Drawing.ArrowSize*width/x.Drawing.PageWidth,length*.45);var side=new Vector(-direction.Y,direction.X)*size*.45;var back=tip-direction*size;
            strokes.Add([back+side,tip,back-side]);
        }
        if(x.Variant is "Pfeil" or "Doppelpfeil")Head(end,start);if(x.Variant=="Doppelpfeil")Head(start,end);return strokes;
    }
    internal static string Path(EquipmentItem x,double width,double height)
    {
        string N(double n)=>n.ToString("0.######",CultureInfo.InvariantCulture);
        return string.Join(" ",Strokes(x,width,height).Select(points=>"M "+string.Join(" L ",points.Select(q=>N(q.X-x.X*width)+","+N(q.Y-x.Y*height)))));
    }
    internal static void SetEnds(EquipmentItem x,PipePoint start,PipePoint end,double width,double height)
    {
        x.Drawing.Start=new(start.X,start.Y);x.Drawing.End=new(end.X,end.Y);UpdateBounds(x,width,height);
    }
    internal static void UpdateBounds(EquipmentItem x,double width,double height)
    {
        if(!IsLine(x))return;var points=Strokes(x,width,height).SelectMany(p=>p).ToArray();var padding=Math.Max(.25,x.Drawing.StrokeWidth/2);
        var left=points.Min(p=>p.X)-padding;var top=points.Min(p=>p.Y)-padding;
        x.X=left/width;x.Y=top/height;x.Width=Math.Max(.25,points.Max(p=>p.X)+padding-left)/width;x.Height=Math.Max(.25,points.Max(p=>p.Y)+padding-top)/height;
    }
    internal static void Normalize(EngineeringProject p)
    {
        foreach(var x in p.Equipment.Where(x=>x.Type==EquipmentType.Drawing))
        {
            var width=p.BlankSheet is {} b?2000*b.WidthMm/297:2000;var aspect=p.BlankSheet is {} blank?blank.HeightMm/blank.WidthMm:p.Pdf.PageAspectRatios.ElementAtOrDefault(x.Page-1);if(aspect<=0)aspect=.708;
            x.Drawing.PageWidth=width;if(x.Variant is "Rechteck" or "Kreis / Ellipse"){x.Width=Math.Max(x.Width,x.Drawing.StrokeWidth/width);x.Height=Math.Max(x.Height,x.Drawing.StrokeWidth/(width*aspect));}if(IsLine(x)){var(a,z)=Ends(x);SetEnds(x,a,z,width,width*aspect);}
        }
    }
}
