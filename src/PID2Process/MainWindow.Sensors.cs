using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;
public partial class MainWindow
{
    private readonly Dictionary<Guid,Polyline> _sensorConnections=[];
    private void DrawSensorConnections()
    {
        _sensorConnections.Clear();if(Current is not {} doc)return;
        foreach(var sensor in doc.Project.Equipment.Where(x=>x.Page==doc.Page+1&&x.Type==EquipmentType.Instrument))
        {
            var path=SensorConnections.Resolve(doc.Project,sensor);var line=new Polyline{Points=new PointCollection(path.Select(Pixel)),Stroke=Brushes.Black,StrokeThickness=1,IsHitTestVisible=false};_sensorConnections[sensor.Id]=line;Sheet.Children.Add(line);
        }
    }
    private void RefreshSensorConnections()
    {
        if(Current is not {} doc)return;foreach(var sensor in doc.Project.Equipment.Where(x=>x.Type==EquipmentType.Instrument))if(_sensorConnections.TryGetValue(sensor.Id,out var line))line.Points=new PointCollection(SensorConnections.Resolve(doc.Project,sensor).Select(Pixel));
    }
    private void ShowSensorConnectionHandle(EquipmentItem sensor)
    {
        if(sensor.Type!=EquipmentType.Instrument||sensor.Sensor.ConnectionSide=="Hidden"||Current is not {} doc)return;var path=SensorConnections.Resolve(doc.Project,sensor);if(path.Count<2)return;
        var handle=new Thumb{Tag=sensor.Id,Width=12,Height=12,Background=Brushes.SteelBlue,Cursor=Cursors.Cross,ToolTip="Prozessanschluss an Rohrstück ziehen"};
        void Position(PipePoint q){Canvas.SetLeft(handle,q.X*Sheet.Width-6);Canvas.SetTop(handle,q.Y*Sheet.Height-6);}Position(path[^1]);
        handle.DragDelta+=(_,_)=>{var point=Mouse.GetPosition(Sheet);sensor.Sensor.Connection=null;sensor.Sensor.ConnectionEnd=Normal(point,Sheet.Width,Sheet.Height);Position(sensor.Sensor.ConnectionEnd);RefreshSensorConnections();};
        handle.DragCompleted+=(_,_)=>{var snap=SensorConnections.Snap(doc.Project,sensor.Page,sensor.Sensor.ConnectionEnd??path[^1]);sensor.Sensor.Connection=snap.Connection;sensor.Sensor.ConnectionEnd=snap.Point;Changed();Draw();ShowHandles(sensor);};Sheet.Children.Add(handle);
    }
}
