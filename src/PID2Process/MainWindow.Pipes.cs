using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
namespace PID2Process;

public partial class MainWindow
{
    private readonly List<PipePoint> _pipeDraft=[];
    private Polyline? _pipePreview;
    private PipeConnection? _draftStart;
    private readonly Dictionary<Guid,Polyline> _pipeGraphics=[];
    private readonly Dictionary<Guid,Polyline> _pipeHitGraphics=[];
    private readonly Dictionary<Guid,List<(Polyline Graphic,PipeSpan Span)>> _pipeFlowGraphics=[];
    private PipeSegment? _selectedPipe;
    private static PipePoint Normal(Point p,double w,double h)=>new(Math.Clamp(p.X/w,0,1),Math.Clamp(p.Y/h,0,1));
    private Point Pixel(PipePoint p)=>new(p.X*Sheet.Width,p.Y*Sheet.Height);
    private void ClearPipeDraft(){_pipeDraft.Clear();_draftStart=null;if(_pipePreview is not null)Sheet.Children.Remove(_pipePreview);_pipePreview=null;}
    private void BeginPipeBranch(PipeConnection connection,PipePoint point)
    {
        ClearPipeDraft();_draftStart=connection;_pipeDraft.Add(new(point.X,point.Y));
    }
    private void ContinuePipeBranch(PipePoint point,PipeConnection? connection)
    {
        _drawingPipe=true;
        if(_pipeDraft.Count>0&&(Pixel(point)-Pixel(_pipeDraft[0])).Length>5)_pipeDraft.AddRange(OrthogonalPath.Continue(_pipeDraft[^1],point,Sheet.Width,Sheet.Height,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),connection is not null));
        if(_pipeDraft.Count>=2&&connection is not null)FinishPipe(connection);
        else{PlacementHint.Text="Abzweig: weitere Eckpunkte klicken · Doppelklick/Enter: Ende · Esc: Abbrechen";Draw();UpdatePipePreview(Pixel(point));}
    }
    private (PipeConnection? Connection,PipePoint Point) Snap(PipePoint p,Guid? excluded=null,bool equipmentOnly=false)
    {
        if(Current is not {} d)return(null,p);double best=18*18;PipeConnection? result=null;var snapped=p;
        foreach(var item in d.Project.Equipment.Where(x=>x.Page==d.Page+1))foreach(var port in equipmentOnly?SignalGeometry.Ports(item):PipeNetwork.Ports(item))
        {
            var fraction=equipmentOnly?Math.Clamp(port is "Top" or "Bottom"?(p.X-item.X)/Math.Max(item.Width,1e-9):(p.Y-item.Y)/Math.Max(item.Height,1e-9),0,1):.5;var q=equipmentOnly?SignalGeometry.Port(item,port,fraction):PipeNetwork.Port(item,port);var distance=Math.Pow((p.X-q.X)*Sheet.Width,2)+Math.Pow((p.Y-q.Y)*Sheet.Height,2);if(distance>=best)continue;best=distance;result=new PipeConnection{EquipmentId=item.Id,Port=port,Fraction=fraction};snapped=q;
        }
        if(result is not null)return(result,snapped);if(equipmentOnly)return(null,p);
        foreach(var pipe in d.Project.Pipes.Where(x=>x.Page==d.Page+1&&(excluded is null||!PipeNetwork.DependsOn(d.Project,x,excluded.Value))))
        {
            var points=PipeNetwork.Resolve(d.Project,pipe);for(var i=0;i<points.Count-1;i++)
            {
                var a=Pixel(points[i]);var b=Pixel(points[i+1]);var target=Pixel(p);var dx=b.X-a.X;var dy=b.Y-a.Y;var length=dx*dx+dy*dy;var t=length==0?0:Math.Clamp(((target.X-a.X)*dx+(target.Y-a.Y)*dy)/length,0,1);var q=new Point(a.X+t*dx,a.Y+t*dy);var distance=(target-q).LengthSquared;if(distance>=best)continue;best=distance;snapped=Normal(q,Sheet.Width,Sheet.Height);result=new PipeConnection{PipeId=pipe.Id,Segment=i,Fraction=t};
            }
        }
        return(result,snapped);
    }
    private void AddPipePoint(Point p,bool finish)
    {
        var snap=Snap(Normal(p,Sheet.Width,Sheet.Height),equipmentOnly:_drawingSignal);if(_pipeDraft.Count==0)_draftStart=snap.Connection;
        if(_pipeDraft.Count==0)_pipeDraft.Add(snap.Point);
        else if((Pixel(_pipeDraft[^1])-Pixel(snap.Point)).Length>2)_pipeDraft.AddRange(OrthogonalPath.Continue(_pipeDraft[^1],snap.Point,Sheet.Width,Sheet.Height,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),snap.Connection is not null));
        if(_pipeDraft.Count>=2&&(finish||snap.Connection is not null)){FinishPipe(snap.Connection);return;}UpdatePipePreview(p);
    }
    private void FinishPipe(PipeConnection? end=null)
    {
        if(Current is not {} d||_pipeDraft.Count<2)return;
        if(_drawingSignal)
        {
            SignalConnection? Convert(PipeConnection? c)=>c?.EquipmentId is Guid id?new(){EquipmentId=id,Port=c.Port,Fraction=c.Fraction}:null;
            var connection=end;
            d.Project.SignalLines.Add(new(){Tag="Wirklinie-"+(d.Project.SignalLines.Count+1),Page=d.Page+1,Points=_pipeDraft.Select(p=>new PipePoint(p.X,p.Y)).ToList(),StartConnection=Convert(_draftStart),EndConnection=Convert(connection)});CancelPlacement();Changed();Draw();return;
        }
        d.Project.Pipes.Add(new PipeSegment{Tag="Rohr-"+(d.Project.Pipes.Count+1),Page=d.Page+1,Points=_pipeDraft.Select(p=>new PipePoint(p.X,p.Y)).ToList(),StartConnection=_draftStart,EndConnection=end});CancelPlacement();Changed();Draw();
    }
    private void UpdatePipePreview(Point cursor)
    {
        if(_pipePreview is null){_pipePreview=new Polyline{Stroke=_drawingSignal?Brushes.Red:Brushes.DeepSkyBlue,StrokeThickness=_drawingSignal?1.3:4,Opacity=.5,StrokeDashArray=_drawingSignal?null:new DoubleCollection([2,2]),IsHitTestVisible=false};Sheet.Children.Add(_pipePreview);}
        var snap=Snap(Normal(cursor,Sheet.Width,Sheet.Height),equipmentOnly:_drawingSignal);var points=_pipeDraft.Select(Pixel).ToList();
        if(_pipeDraft.Count>0)points.AddRange(OrthogonalPath.Continue(_pipeDraft[^1],snap.Point,Sheet.Width,Sheet.Height,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),snap.Connection is not null).Select(Pixel));else points.Add(Pixel(snap.Point));_pipePreview.Points=new PointCollection(points);
    }
    private void DrawPipes(SceneValues values)
    {
        _pipeGraphics.Clear();_pipeHitGraphics.Clear();_pipeFlowGraphics.Clear();if(Current is not {} d)return;var appearances=(_networkScene??PipeNetwork.EvaluateScene(d.Project,values)).Pipes;
        foreach(var pipe in d.Project.Pipes.Where(p=>p.Page==d.Page+1))
        {
            var appearance=appearances[pipe.Id];var state=appearance.State;var line=new Polyline{Points=new PointCollection(PipeNetwork.Resolve(d.Project,pipe).Select(Pixel)),Stroke=Brush(MediaSettings.Color(d.Project,appearance.MediumType,appearance.MediumName)),StrokeThickness=6,StrokeLineJoin=PenLineJoin.Round,ToolTip=$"{pipe.Tag} · {appearance.MediumName} · {state}"+(appearance.Blocked?" · Verbindung gesperrt oder zu prüfen":"")};
            line.Stroke=Brush(PipeStyleResolver.Color(d.Project,pipe,state,appearance.MediumType,appearance.MediumName));
            line.StrokeThickness=PipeStyleResolver.Width(pipe,Sheet.Width,6);
            line.StrokeDashArray=PipeStyleResolver.Dash(pipe,state) is {} dash?new DoubleCollection(dash):null;
            var flows=new List<(Polyline Graphic,PipeSpan Span)>();_pipeFlowGraphics[pipe.Id]=flows;
            // Only the source-to-target sections animate; availability remains a static base line.
            foreach(var span in appearance.Spans.Where(x=>x.Flowing))
            {
                var flow=new Polyline{Points=new PointCollection(PipeNetwork.Slice(PipeNetwork.Resolve(d.Project,pipe),span.Start,span.End).Select(Pixel)),Stroke=Brushes.White,StrokeThickness=line.StrokeThickness/2,StrokeDashArray=new DoubleCollection([3,2]),IsHitTestVisible=false,Opacity=pipe.Opacity*.8};

                AnimateFlow(flow,span.Direction);
                flows.Add((flow,span));
            }
            line.Opacity=pipe.Opacity;
            var resolved=PipeNetwork.Resolve(d.Project,pipe);
            foreach(var endpoint in new[]{(pipe.StartConnection,resolved[0]),(pipe.EndConnection,resolved[^1])}.Where(x=>x.Item1?.NeedsReview==true))
            {
                var marker=new Rectangle{Width=10,Height=10,Fill=Brushes.DarkOrange,Stroke=Brushes.DarkRed,IsHitTestVisible=false};Canvas.SetLeft(marker,endpoint.Item2.X*Sheet.Width-5);Canvas.SetTop(marker,endpoint.Item2.Y*Sheet.Height-5);Panel.SetZIndex(marker,2);Sheet.Children.Add(marker);
            }
            PipeSegment? branch=null;bool groupMoved=false;
            var hitLine=new Polyline{Points=line.Points.Clone(),Stroke=Brushes.Transparent,StrokeThickness=Math.Max(18,line.StrokeThickness+8),ToolTip=line.ToolTip};
            void Down(object sender,MouseButtonEventArgs e)
            {
                if(_drawingPipe)return;e.Handled=true;SelectObject(pipe.Id);Sheet.Focus();
                if(e.ClickCount==2){EditPipe(pipe);return;}
                if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    var hit=Snap(Normal(e.GetPosition(Sheet),Sheet.Width,Sheet.Height));if(hit.Connection?.PipeId!=pipe.Id)return;
                    branch=new PipeSegment{Tag="Abzweig-"+(d.Project.Pipes.Count+1),Page=pipe.Page,StartConnection=hit.Connection,Points=[hit.Point,new(hit.Point.X,hit.Point.Y)]};line.CaptureMouse();BeginPipeBranch(hit.Connection,hit.Point);UpdatePipePreview(e.GetPosition(Sheet));
                }
                else{BeginSelectionDrag(e.GetPosition(Sheet));groupMoved=false;line.CaptureMouse();if(_selection.Count==1)ShowPipeHandles(pipe);}
            }
            void Move(object sender,MouseEventArgs e){if(e.LeftButton!=MouseButtonState.Pressed)return;if(branch is not null)UpdatePipePreview(e.GetPosition(Sheet));else if(_selectionDrag is not null){UpdateSelectionDrag(e.GetPosition(Sheet));groupMoved=true;}}
            void Up(object sender,MouseButtonEventArgs e)
            {
                if(branch is null&&_selectionDrag is not null){line.ReleaseMouseCapture();_selectionDrag=null;if(groupMoved)Changed();Draw();e.Handled=true;return;}
                if(branch is null)return;if(_pipeDraft.Count==0){branch=null;line.ReleaseMouseCapture();return;}
                var snap=Snap(Normal(e.GetPosition(Sheet),Sheet.Width,Sheet.Height),pipe.Id);line.ReleaseMouseCapture();branch=null;ContinuePipeBranch(snap.Point,snap.Connection);e.Handled=true;
            }
            var menu=PlanMenu();foreach(var target in new[]{line,hitLine}){target.MouseLeftButtonDown+=Down;target.MouseMove+=Move;target.MouseLeftButtonUp+=Up;target.ContextMenu=menu;target.ContextMenuOpening+=(_,_)=>{SelectObject(pipe.Id);target.ContextMenu=PlanMenu();};}_pipeGraphics.Add(pipe.Id,line);_pipeHitGraphics.Add(pipe.Id,hitLine);Sheet.Children.Add(hitLine);Sheet.Children.Add(line);foreach(var flow in flows)Sheet.Children.Add(flow.Graphic);
        }
    }
    private void RefreshPipeGeometry()
    {
        RefreshSignalGeometry();RefreshSensorConnections();
        if(Current is not {} d)return;foreach(var p in d.Project.Pipes)if(_pipeGraphics.TryGetValue(p.Id,out var graphic)){var path=PipeNetwork.Resolve(d.Project,p);graphic.Points=new PointCollection(path.Select(Pixel));if(_pipeHitGraphics.TryGetValue(p.Id,out var hit))hit.Points=graphic.Points.Clone();if(_pipeFlowGraphics.TryGetValue(p.Id,out var flows))foreach(var flow in flows)flow.Graphic.Points=new PointCollection(PipeNetwork.Slice(path,flow.Span.Start,flow.Span.End).Select(Pixel));}
    }
    private void DeletePipe(PipeSegment pipe)
    {
        if(Current is not {} d)return;
        OverlaySelection.Delete(d.Project,new HashSet<Guid>{pipe.Id});_selection.Remove(pipe.Id);_selectedPipe=null;Changed();Draw();
    }
    private void ShowPipeHandles(PipeSegment pipe)
    {
        if(Current is not {} d)return;foreach(var handle in Sheet.Children.OfType<Thumb>().ToArray())Sheet.Children.Remove(handle);
        var points=PipeNetwork.Resolve(d.Project,pipe);pipe.Points=points;
        for(var i=0;i<points.Count;i++)
        {
            var index=i;var handle=new Thumb{Width=12,Height=12,Background=Brushes.DarkOrange,Cursor=Cursors.SizeAll};Canvas.SetLeft(handle,points[i].X*Sheet.Width-6);Canvas.SetTop(handle,points[i].Y*Sheet.Height-6);
            var origin=new Point(points[i].X*Sheet.Width,points[i].Y*Sheet.Height);var total=new Vector();var axis=0;
            handle.DragStarted+=(_,_)=>{origin=new(pipe.Points[index].X*Sheet.Width,pipe.Points[index].Y*Sheet.Height);total=new();axis=0;};
            handle.DragDelta+=(_,e)=>{if(index==0)pipe.StartConnection=null;if(index==points.Count-1)pipe.EndConnection=null;total+=new Vector(e.HorizontalChange,e.VerticalChange);var constrained=OverlayTransforms.Direction(total,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),ref axis);pipe.Points[index].X=Math.Clamp((origin.X+constrained.X)/Sheet.Width,0,1);pipe.Points[index].Y=Math.Clamp((origin.Y+constrained.Y)/Sheet.Height,0,1);Canvas.SetLeft(handle,pipe.Points[index].X*Sheet.Width-6);Canvas.SetTop(handle,pipe.Points[index].Y*Sheet.Height-6);RefreshPipeGeometry();};
            handle.DragCompleted+=(_,_)=>{var snap=Snap(pipe.Points[index],pipe.Id);if(index==0)pipe.StartConnection=snap.Connection;if(index==points.Count-1)pipe.EndConnection=snap.Connection;Changed();Draw();ShowPipeHandles(pipe);};Sheet.Children.Add(handle);
        }
    }
}
