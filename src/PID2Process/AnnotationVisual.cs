using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;
public sealed class DrawingSettings
{
    public double StrokeWidth {get;set;}=1.5;
    public string FillColor {get;set;}="#FFFFFF";
    public bool Filled {get;set;}
    public double FontSize {get;set;}=16;
    public bool ReverseX {get;set;}
    public bool ReverseY {get;set;}
    public bool Horizontal {get;set;}
    public bool Vertical {get;set;}
    public double ArrowSize {get;set;}=14;
    public double PageWidth {get;set;}=2000;
    public PipePoint? Start {get;set;}
    public PipePoint? End {get;set;}
}
internal static class AnnotationVisual
{
    internal static string Path(EquipmentItem item)
    {
        var d=item.Drawing;double x1=d.Vertical?50:d.ReverseX?100:0,x2=d.Vertical?50:100-x1,y1=d.Horizontal?50:d.ReverseY?100:0,y2=d.Horizontal?50:100-y1;
        string N(double n)=>n.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
        var path=$"M {N(x1)},{N(y1)} L {N(x2)},{N(y2)}";
        void Head(double x,double y,double ax,double ay){var dx=x-ax;var dy=y-ay;var l=Math.Sqrt(dx*dx+dy*dy);if(l<1)return;dx/=l;dy/=l;path+=$" M {N(x-dx*15+dy*7)},{N(y-dy*15-dx*7)} L {N(x)},{N(y)} L {N(x-dx*15-dy*7)},{N(y-dy*15+dx*7)}";}
        if(item.Variant is "Pfeil" or "Doppelpfeil")Head(x2,y2,x1,y1);if(item.Variant=="Doppelpfeil")Head(x1,y1,x2,y2);return path;
    }
    internal static FrameworkElement Symbol(EquipmentItem item,double width,double height)
    {
        var grid=new Grid{Width=Math.Max(2,item.Width*width),Height=Math.Max(2,item.Height*height),Background=Brushes.Transparent,Opacity=item.Opacity};
        if(item.Variant=="Textfeld"){grid.Children.Add(new TextBlock{Text=item.TextContent,Foreground=PageVisual.Color(item.Color),Background=item.Drawing.Filled?PageVisual.Color(item.Drawing.FillColor):Brushes.Transparent,FontSize=item.Drawing.FontSize*width/item.Drawing.PageWidth,TextWrapping=TextWrapping.Wrap});return grid;}
        var stroke=item.Drawing.StrokeWidth*width/item.Drawing.PageWidth;var canvas=new Canvas{Width=grid.Width,Height=grid.Height};
        Shape shape=item.Variant switch{"Kreis / Ellipse"=>new Ellipse{Width=Math.Max(.1,grid.Width-stroke),Height=Math.Max(.1,grid.Height-stroke)},"Rechteck"=>new Rectangle{Width=Math.Max(.1,grid.Width-stroke),Height=Math.Max(.1,grid.Height-stroke)},_=>new System.Windows.Shapes.Path{Data=Geometry.Parse(DrawingGeometry.Path(item,width,height))}};
        if(!DrawingGeometry.IsLine(item)){Canvas.SetLeft(shape,stroke/2);Canvas.SetTop(shape,stroke/2);}
        shape.Stroke=PageVisual.Color(item.Color);shape.StrokeThickness=stroke;shape.StrokeStartLineCap=shape.StrokeEndLineCap=PenLineCap.Round;shape.StrokeLineJoin=PenLineJoin.Round;
        shape.Fill=item.Drawing.Filled&&item.Variant is "Kreis / Ellipse" or "Rechteck"?PageVisual.Color(item.Drawing.FillColor):Brushes.Transparent;
        if(DrawingGeometry.IsLine(item))canvas.Children.Add(new System.Windows.Shapes.Path{Data=shape is System.Windows.Shapes.Path path?path.Data:null,Stroke=Brushes.Transparent,StrokeThickness=Math.Max(10,stroke),StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round});
        canvas.Children.Add(shape);grid.Children.Add(canvas);if(DrawingGeometry.IsLine(item))grid.Background=null;return grid;
    }
}
