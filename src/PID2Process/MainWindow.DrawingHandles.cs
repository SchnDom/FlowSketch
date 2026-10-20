using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
namespace PID2Process;
public partial class MainWindow
{
    private void ShowAnnotationEndpoints(EquipmentItem item)
    {
        if(Current is not {} doc)return;
        foreach(var start in new[]{true,false})
        {
            var handle=new Thumb{Width=14,Height=14,Background=Brushes.White,BorderBrush=Brushes.DodgerBlue,BorderThickness=new Thickness(2),Cursor=Cursors.Cross,ToolTip=start?"Startpunkt verschieben":"Endpunkt verschieben",Tag=(item.Id,start)};
            void Position(){var point=start?DrawingGeometry.Ends(item).Start:DrawingGeometry.Ends(item).End;Canvas.SetLeft(handle,point.X*Sheet.Width-7);Canvas.SetTop(handle,point.Y*Sheet.Height-7);}
            Position();var origin=new Point();var saved=new PipePoint();
            handle.DragStarted+=(_,_)=>{origin=Mouse.GetPosition(Sheet);var point=start?DrawingGeometry.Ends(item).Start:DrawingGeometry.Ends(item).End;saved=new(point.X,point.Y);};
            handle.DragDelta+=(_,_)=>
            {
                var delta=Mouse.GetPosition(Sheet)-origin;var point=new PipePoint(Math.Clamp(saved.X+delta.X/Sheet.Width,.002,.998),Math.Clamp(saved.Y+delta.Y/Sheet.Height,.002,.998));
                var ends=DrawingGeometry.Ends(item);DrawingGeometry.SetEnds(item,start?point:ends.Start,start?ends.End:point,Sheet.Width/doc.Zoom,Sheet.Height/doc.Zoom);
                foreach(var grid in Sheet.Children.OfType<Grid>().Where(g=>Equals(g.Tag,item.Id)))
                {
                    var replacement=(Grid)AnnotationVisual.Symbol(item,Sheet.Width,Sheet.Height);grid.Children.Clear();grid.Width=replacement.Width;grid.Height=replacement.Height;
                    foreach(var child in replacement.Children.Cast<UIElement>().ToArray()){replacement.Children.Remove(child);grid.Children.Add(child);}Canvas.SetLeft(grid,item.X*Sheet.Width);Canvas.SetTop(grid,item.Y*Sheet.Height);
                }
                Position();DrawSelection();
            };
            handle.DragCompleted+=(_,_)=>{Changed();Draw();ShowHandles(item);};Sheet.Children.Add(handle);
        }
    }
}
