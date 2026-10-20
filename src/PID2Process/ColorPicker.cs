using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
namespace PID2Process;

internal sealed class ColorPicker : DockPanel
{
    internal TextBox Hex {get;}=new(){MinWidth=100};
    public static readonly DependencyProperty ValueProperty=DependencyProperty.Register(nameof(Value),typeof(string),typeof(ColorPicker),new FrameworkPropertyMetadata("#007E87",FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public string Value {get=>(string)GetValue(ValueProperty);set=>SetValue(ValueProperty,value);}
    public ColorPicker():this(null){}
    internal ColorPicker(string? value)
    {
        var preview=new Border{Width=28,Margin=new Thickness(4),BorderBrush=Brushes.Gray,BorderThickness=new Thickness(1)};var choose=new Button{Content="Farbrad…"};DockPanel.SetDock(choose,Dock.Right);Children.Add(choose);DockPanel.SetDock(preview,Dock.Left);Children.Add(preview);Children.Add(Hex);if(value is not null)SetCurrentValue(ValueProperty,value);Hex.SetBinding(TextBox.TextProperty,new Binding(nameof(Value)){Source=this,Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});
        void Update(){try{preview.Background=new SolidColorBrush(Parse(Hex.Text));}catch{preview.Background=Brushes.Transparent;}}Hex.TextChanged+=(_,_)=>Update();Update();
        choose.Click+=(_,_)=>{var owner=Window.GetWindow(this);var dialog=new ColorWheelDialog(owner,IsValid(Value)?Value:"#007E87");if(dialog.ShowDialog()==true)Hex.Text=dialog.Hex;};
    }
    internal static DataGridTemplateColumn Column(string header,string path="Color")
    {
        var factory=new FrameworkElementFactory(typeof(ColorPicker));factory.SetBinding(ValueProperty,new Binding(path){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});
        var template=new DataTemplate{VisualTree=factory};return new DataGridTemplateColumn{Header=header,CellTemplate=template,CellEditingTemplate=template,Width=270};
    }
    internal static bool IsValid(string value)=>value.Length==7&&value[0]=='#'&&value.Skip(1).All(Uri.IsHexDigit);
    internal static Color Parse(string hex)=>IsValid(hex)?(Color)ColorConverter.ConvertFromString(hex):throw new FormatException("Farbe als #RRGGBB eingeben.");
}
internal sealed class ColorWheel : FrameworkElement
{
    internal double Hue {get;private set;}
    internal double Saturation {get;private set;}
    private double _brightness=1;
    internal double Brightness {get=>_brightness;set{_brightness=Math.Clamp(value,0,1);_image=null;InvalidateVisual();Changed?.Invoke();}}
    internal event Action? Changed;
    private BitmapSource? _image;
    internal Color Selected=>Hsv(Hue,Saturation,Brightness);
    internal ColorWheel(){Width=240;Height=240;Focusable=true;Cursor=Cursors.Cross;}
    internal void SetColor(Color c)
    {
        double r=c.R/255d,g=c.G/255d,b=c.B/255d;var max=Math.Max(r,Math.Max(g,b));var min=Math.Min(r,Math.Min(g,b));var diff=max-min;Hue=diff==0?0:max==r?60*(((g-b)/diff+6)%6):max==g?60*((b-r)/diff+2):60*((r-g)/diff+4);Saturation=max==0?0:diff/max;Brightness=max;
    }
    internal static Color Hsv(double hue,double sat,double value)
    {
        hue=((hue%360)+360)%360;sat=Math.Clamp(sat,0,1);value=Math.Clamp(value,0,1);var c=value*sat;var x=c*(1-Math.Abs(hue/60%2-1));var m=value-c;
        var rgb=hue switch{<60=>(c,x,0d),<120=>(x,c,0d),<180=>(0d,c,x),<240=>(0d,x,c),<300=>(x,0d,c),_=>(c,0d,x)};return Color.FromRgb((byte)Math.Round((rgb.Item1+m)*255),(byte)Math.Round((rgb.Item2+m)*255),(byte)Math.Round((rgb.Item3+m)*255));
    }
    internal void SelectAt(Point p)
    {
        var dx=p.X-120;var dy=p.Y-120;Hue=(Math.Atan2(dy,dx)*180/Math.PI+360)%360;Saturation=Math.Clamp(Math.Sqrt(dx*dx+dy*dy)/116,0,1);InvalidateVisual();Changed?.Invoke();
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e){SelectAt(e.GetPosition(this));CaptureMouse();e.Handled=true;}
    protected override void OnMouseMove(MouseEventArgs e){if(IsMouseCaptured&&e.LeftButton==MouseButtonState.Pressed)SelectAt(e.GetPosition(this));}
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e){if(IsMouseCaptured){ReleaseMouseCapture();e.Handled=true;}}
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);if(_image is null){var pixels=new byte[240*240*4];for(var y=0;y<240;y++)for(var x=0;x<240;x++){var dx=x-120d;var dy=y-120d;var s=Math.Sqrt(dx*dx+dy*dy)/116;if(s>1)continue;var c=Hsv(Math.Atan2(dy,dx)*180/Math.PI,s,Brightness);var i=(y*240+x)*4;pixels[i]=c.B;pixels[i+1]=c.G;pixels[i+2]=c.R;pixels[i+3]=255;}_image=BitmapSource.Create(240,240,96,96,PixelFormats.Bgra32,null,pixels,240*4);_image.Freeze();}dc.DrawImage(_image,new Rect(0,0,240,240));var p=new Point(120+Math.Cos(Hue*Math.PI/180)*116*Saturation,120+Math.Sin(Hue*Math.PI/180)*116*Saturation);dc.DrawEllipse(null,new Pen(Brushes.White,3),p,6,6);dc.DrawEllipse(null,new Pen(Brushes.Black,1),p,8,8);
    }
}
internal sealed class ColorWheelDialog : Window
{
    private readonly ColorWheel _wheel=new();
    internal string Hex=>"#"+_wheel.Selected.R.ToString("X2")+_wheel.Selected.G.ToString("X2")+_wheel.Selected.B.ToString("X2");
    internal ColorWheelDialog(Window? owner,string initial)
    {
        if(owner?.IsVisible==true)Owner=owner;Title="Farbe wählen";Width=360;SizeToContent=SizeToContent.Height;ResizeMode=ResizeMode.NoResize;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        DialogTheme.Apply(this);_wheel.SetColor(ColorPicker.Parse(initial));var stack=new StackPanel{Margin=new Thickness(16)};stack.Children.Add(new TextBlock{Text="Farbton / Sättigung",Margin=new Thickness(0,0,0,8)});_wheel.HorizontalAlignment=HorizontalAlignment.Center;stack.Children.Add(_wheel);
        stack.Children.Add(new TextBlock{Text="Helligkeit",Margin=new Thickness(0,8,0,0)});var slider=new Slider{Minimum=0,Maximum=1,Value=_wheel.Brightness};slider.ValueChanged+=(_,_)=>_wheel.Brightness=slider.Value;stack.Children.Add(slider);
        var swatch=new Border{Height=32,Margin=new Thickness(0,8,0,8)};var text=new TextBox{Text=Hex};stack.Children.Add(swatch);stack.Children.Add(new TextBlock{Text="Hexfarbe (#RRGGBB)"});stack.Children.Add(text);
        var syncing=false;void Update(){if(syncing)return;syncing=true;try{swatch.Background=new SolidColorBrush(_wheel.Selected);text.Text=Hex;slider.Value=_wheel.Brightness;}finally{syncing=false;}}_wheel.Changed+=Update;
        text.TextChanged+=(_,_)=>{if(syncing||!ColorPicker.IsValid(text.Text))return;_wheel.SetColor(ColorPicker.Parse(text.Text));Update();};Update();
        var buttons=new WrapPanel{HorizontalAlignment=HorizontalAlignment.Right};var pipette=new Button{Content="Pipette"};pipette.Click+=async(_,_)=>
        {
            var opacity=Opacity;try
            {
                Opacity=0;IsEnabled=false;
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                ScreenPipette.FlushDesktop();
                var capture=ScreenPipette.Capture();if(ScreenPipette.Pick(capture) is {} color){_wheel.SetColor(color);Update();}
            }
            catch(Exception ex){MessageBox.Show("Die Bildschirmfarbe konnte nicht aufgenommen werden: "+ex.Message,"Pipette");}
            finally{Opacity=opacity;IsEnabled=true;Activate();}
        };buttons.Children.Add(pipette);var ok=new Button{Content="Übernehmen",IsDefault=true};ok.Click+=(_,_)=>{if(!ColorPicker.IsValid(text.Text)){MessageBox.Show(this,"Farbe als #RRGGBB eingeben.");return;}DialogResult=true;};buttons.Children.Add(ok);buttons.Children.Add(new Button{Content="Abbrechen",IsCancel=true});stack.Children.Add(buttons);Content=stack;
    }
}
