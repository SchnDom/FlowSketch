using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;
public partial class MainWindow
{
    private void DrawGroupOutlines(EngineeringProject project)
    {
        var page=Current!.Page+1;
        var members=project.Equipment.Where(x=>x.Page==page&&x.GroupId.HasValue).Select(x=>(Group:x.GroupId!.Value,Bounds:OverlaySelection.Bounds(x)))
            .Concat(project.Pipes.Where(x=>x.Page==page&&x.GroupId.HasValue).Select(x=>(Group:x.GroupId!.Value,Bounds:OverlaySelection.Bounds(project,x))))
            .Concat(project.SignalLines.Where(x=>x.Page==page&&x.GroupId.HasValue).Select(x=>(Group:x.GroupId!.Value,Bounds:OverlaySelection.Bounds(project,x))));
        foreach(var group in members.GroupBy(x=>x.Group))
        {
            var bounds=group.Select(x=>x.Bounds).Aggregate(Rect.Union);
            var frame=new Rectangle{Tag="group-outline",Width=Math.Max(10,bounds.Width*Sheet.Width+14),Height=Math.Max(10,bounds.Height*Sheet.Height+14),Stroke=PageVisual.Color("#B7FF80"),StrokeThickness=1.5,StrokeDashArray=new DoubleCollection([5,3]),IsHitTestVisible=false};
            Canvas.SetLeft(frame,bounds.X*Sheet.Width-7);Canvas.SetTop(frame,bounds.Y*Sheet.Height-7);// Keep the frame above the PDF background but below symbols and captions.
            Sheet.Children.Insert(Sheet.Children.OfType<Image>().Any()?1:0,frame);
        }
    }
}
