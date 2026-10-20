using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;
public partial class MainWindow
{
    private Rectangle? _overviewWindow;
    private System.Windows.Media.Imaging.BitmapSource? _overviewThumbnail;
    private bool _overviewRenderQueued;
    private void QueueOverview()
    {
        if(_overviewRenderQueued)return;_overviewRenderQueued=true;Dispatcher.BeginInvoke(new Action(()=>{_overviewRenderQueued=false;if(Current is not {} doc)return;var project=_previewProject??doc.Project;var values=SceneValues.From(_previewProject is null?ScenarioBox.SelectedItem as Scenario:_previewScenario,_previewProject is null?StepBox.SelectedItem as ControlStep:_previewStep);var scenario=new Scenario{EquipmentStates=values.EquipmentStates,PipeStates=values.PipeStates,TextValues=values.TextValues,SensorValues=values.SensorValues,TankStates=values.TankStates};var aspect=project.BlankSheet is {} blank?blank.HeightMm/blank.WidthMm:doc.Raster.PageCount>0?doc.Raster.AspectRatio(doc.Page):.708;_overviewThumbnail=PageVisual.Render(project,doc.Page+1,480,aspect,scenario,doc.Image);UpdateOverview();}),System.Windows.Threading.DispatcherPriority.Background);
    }
    private Rect _overviewSheet;
    private bool _overviewDragging;
    private Vector _overviewGrab;
    private void UpdateOverview()
    {
        if(Overview is null||Sheet.Width<=0||Sheet.Height<=0||Overview.ActualWidth<1||Overview.ActualHeight<1)return;
        // Fit the complete document independently of the main zoom and viewport.
        var aspect=_overviewThumbnail is {} thumbnail?(double)thumbnail.PixelHeight/thumbnail.PixelWidth:Sheet.Height/Sheet.Width;
        var width=Math.Min(Overview.ActualWidth,Overview.ActualHeight/aspect);var height=width*aspect;
        _overviewSheet=new((Overview.ActualWidth-width)/2,(Overview.ActualHeight-height)/2,width,height);
        Overview.Children.Clear();
        var image=new Rectangle{Width=_overviewSheet.Width,Height=_overviewSheet.Height,IsHitTestVisible=false,Fill=_overviewThumbnail is null?Brushes.White:new ImageBrush(_overviewThumbnail){Stretch=Stretch.Fill}};
        Canvas.SetLeft(image,_overviewSheet.Left);Canvas.SetTop(image,_overviewSheet.Top);Overview.Children.Add(image);
        var origin=Sheet.TranslatePoint(new Point(),Viewport);var left=Math.Clamp(-origin.X/Sheet.Width,0,1);var top=Math.Clamp(-origin.Y/Sheet.Height,0,1);var right=Math.Clamp((Viewport.ViewportWidth-origin.X)/Sheet.Width,0,1);var bottom=Math.Clamp((Viewport.ViewportHeight-origin.Y)/Sheet.Height,0,1);
        _overviewWindow=new Rectangle{Width=Math.Max(2,(right-left)*_overviewSheet.Width),Height=Math.Max(2,(bottom-top)*_overviewSheet.Height),Stroke=Brushes.DeepSkyBlue,StrokeThickness=2,Fill=new SolidColorBrush(Color.FromArgb(40,50,170,255)),IsHitTestVisible=false};
        Canvas.SetLeft(_overviewWindow,_overviewSheet.Left+left*_overviewSheet.Width);Canvas.SetTop(_overviewWindow,_overviewSheet.Top+top*_overviewSheet.Height);Overview.Children.Add(_overviewWindow);
    }
    private void Overview_SizeChanged(object sender,SizeChangedEventArgs e)=>UpdateOverview();
    private void Overview_Down(object sender,MouseButtonEventArgs e)
    {
        if(Current is null||_overviewWindow is null)return;var point=e.GetPosition(Overview);var rect=new Rect(Canvas.GetLeft(_overviewWindow),Canvas.GetTop(_overviewWindow),_overviewWindow.Width,_overviewWindow.Height);_overviewGrab=rect.Contains(point)?point-new Point(rect.X+rect.Width/2,rect.Y+rect.Height/2):new Vector();_overviewDragging=true;Overview.CaptureMouse();PanOverview(point);e.Handled=true;
    }
    private void Overview_Move(object sender,MouseEventArgs e){if(_overviewDragging&&e.LeftButton==MouseButtonState.Pressed)PanOverview(e.GetPosition(Overview));}
    private void Overview_Up(object sender,MouseButtonEventArgs e){_overviewDragging=false;Overview.ReleaseMouseCapture();e.Handled=true;}
    private void PanOverview(Point point)
    {
        if(_overviewSheet.Width<=0)return;var target=point-_overviewGrab;var nx=Math.Clamp((target.X-_overviewSheet.Left)/_overviewSheet.Width,0,1);var ny=Math.Clamp((target.Y-_overviewSheet.Top)/_overviewSheet.Height,0,1);var origin=Sheet.TranslatePoint(new Point(),Viewport);
        Viewport.ScrollToHorizontalOffset(Viewport.HorizontalOffset+origin.X+nx*Sheet.Width-Viewport.ViewportWidth/2);Viewport.ScrollToVerticalOffset(Viewport.VerticalOffset+origin.Y+ny*Sheet.Height-Viewport.ViewportHeight/2);Viewport.UpdateLayout();UpdateOverview();
    }
}
