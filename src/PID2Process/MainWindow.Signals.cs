using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;

public partial class MainWindow
{
    private bool _drawingSignal;
    private readonly Dictionary<Guid,List<Polyline>> _signalGraphics=[];
    private void Signal_Click(object sender,RoutedEventArgs e)
    {
        Pipe_Click(sender,e);if(!_drawingPipe)return;_drawingSignal=true;Draw();PlacementHint.Text="Wirklinie: Sensor → Gerät · Eckpunkte klicken · Doppelklick/Enter: Ende · keine Rohrverbindung";
    }
    private void DrawSignalLines()
    {
        _signalGraphics.Clear();if(Current is not {} d)return;
        foreach(var signal in d.Project.SignalLines.Where(x=>x.Page==d.Page+1&&x.Points.Count>=2))
        {
            var path=SignalGeometry.Resolve(d.Project,signal);var graphics=new List<Polyline>();_signalGraphics[signal.Id]=graphics;
            string Tag(SignalConnection? c)=>d.Project.Equipment.FirstOrDefault(x=>x.Id==c?.EquipmentId)?.Tag??"frei";
            var line=new Polyline{Points=new PointCollection(path.Select(Pixel)),Stroke=Brush(signal.Color),StrokeThickness=1.3,Opacity=signal.Opacity,ToolTip=signal.Tag+" · "+Tag(signal.StartConnection)+" → "+Tag(signal.EndConnection)+" · grafische Zuordnung"};graphics.Add(line);
            var hit=new Polyline{Points=line.Points.Clone(),Stroke=Brushes.Transparent,StrokeThickness=14,ToolTip=line.ToolTip};graphics.Add(hit);
            foreach(var marker in SignalGeometry.Markers(path,Sheet.Width,Sheet.Height))graphics.Add(new Polyline{Points=new PointCollection(marker.Select(Pixel)),Stroke=line.Stroke,StrokeThickness=1.3,Opacity=signal.Opacity,IsHitTestVisible=false});
            bool moved=false;
            void Down(object sender,MouseButtonEventArgs e){if(_drawingPipe)return;e.Handled=true;SelectObject(signal.Id);Sheet.Focus();if(e.ClickCount==2){EditSignal(signal);return;}BeginSelectionDrag(e.GetPosition(Sheet));moved=false;line.CaptureMouse();if(_selection.Count==1)ShowSignalHandles(signal);}
            void Move(object sender,MouseEventArgs e){if(_selectionDrag is null||e.LeftButton!=MouseButtonState.Pressed)return;UpdateSelectionDrag(e.GetPosition(Sheet));moved=true;}
            void Up(object sender,MouseButtonEventArgs e){if(_selectionDrag is null)return;_selectionDrag=null;line.ReleaseMouseCapture();if(moved)Changed();Draw();if(_selection.Count==1)ShowSignalHandles(signal);e.Handled=true;}
            var menu=PlanMenu();
            foreach(var target in new[]{line,hit}){target.MouseLeftButtonDown+=Down;target.MouseMove+=Move;target.MouseLeftButtonUp+=Up;target.ContextMenu=menu;target.ContextMenuOpening+=(_,_)=>{SelectObject(signal.Id);target.ContextMenu=PlanMenu();};}
            Sheet.Children.Add(hit);Sheet.Children.Add(line);foreach(var marker in graphics.Skip(2))Sheet.Children.Add(marker);
        }
    }
    private void RefreshSignalGeometry()
    {
        if(Current is not {} d)return;foreach(var signal in d.Project.SignalLines)if(_signalGraphics.TryGetValue(signal.Id,out var graphics)){var path=SignalGeometry.Resolve(d.Project,signal);graphics[0].Points=new PointCollection(path.Select(Pixel));graphics[1].Points=graphics[0].Points.Clone();var markers=SignalGeometry.Markers(path,Sheet.Width,Sheet.Height);for(var i=0;i<Math.Min(markers.Count,graphics.Count-2);i++)graphics[i+2].Points=new PointCollection(markers[i].Select(Pixel));}
    }
    private void EditSignal(SignalLine signal)
    {
        var dialog=new EditorDialog(this,"Wirklinie · grafische Zuordnung");dialog.Text("Bezeichnung",signal.Tag);dialog.Color("Farbe (#RRGGBB)",signal.Color);dialog.Validation=()=>string.IsNullOrWhiteSpace(dialog.Value("Bezeichnung"))?"Bezeichnung eingeben.":!ColorPicker.IsValid(dialog.Value("Farbe (#RRGGBB)"))?"Gültige Farbe eingeben.":null;if(dialog.ShowDialog()!=true)return;signal.Tag=dialog.Value("Bezeichnung");signal.Color=dialog.Value("Farbe (#RRGGBB)");Changed();Draw();
    }
    private void ShowSignalHandles(SignalLine signal)
    {
        if(Current is not {} d)return;foreach(var old in Sheet.Children.OfType<Thumb>().ToArray())Sheet.Children.Remove(old);signal.Points=SignalGeometry.Resolve(d.Project,signal);
        for(var i=0;i<signal.Points.Count;i++)
        {
            var index=i;var handle=new Thumb{Width=10,Height=10,Background=Brushes.OrangeRed,Cursor=Cursors.SizeAll};Canvas.SetLeft(handle,signal.Points[i].X*Sheet.Width-5);Canvas.SetTop(handle,signal.Points[i].Y*Sheet.Height-5);var origin=new Point();var total=new Vector();var axis=0;
            handle.DragStarted+=(_,_)=>{origin=Pixel(signal.Points[index]);total=new();axis=0;};
            handle.DragDelta+=(_,e)=>{if(index==0)signal.StartConnection=null;if(index==signal.Points.Count-1)signal.EndConnection=null;total+=new Vector(e.HorizontalChange,e.VerticalChange);var delta=OverlayTransforms.Direction(total,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),ref axis);signal.Points[index]=Normal(origin+delta,Sheet.Width,Sheet.Height);Canvas.SetLeft(handle,signal.Points[index].X*Sheet.Width-5);Canvas.SetTop(handle,signal.Points[index].Y*Sheet.Height-5);RefreshSignalGeometry();};
            handle.DragCompleted+=(_,_)=>{var snap=Snap(signal.Points[index],equipmentOnly:true);SignalConnection? connection=snap.Connection?.EquipmentId is Guid id?new(){EquipmentId=id,Port=snap.Connection.Port,Fraction=snap.Connection.Fraction}:null;if(index==0)signal.StartConnection=connection;if(index==signal.Points.Count-1)signal.EndConnection=connection;Changed();Draw();ShowSignalHandles(signal);};Sheet.Children.Add(handle);
        }
    }
}
