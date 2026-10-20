using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace PID2Process;

public partial class MainWindow
{
    private bool _selectExportWindow;
    private Rectangle? _exportRubber;
    private Point _exportStart;
    private static BitmapSource WhiteBackground()
    {
        var b=BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{255,255,255,255},4);b.Freeze();return b;
    }
    private void Layout_Click(object sender,RoutedEventArgs e)
    {
        if(Current?.Project.BlankSheet is not {} sheet){StatusText.Text="Blattlayout ist für leere Zeichenblätter verfügbar.";return;}
        var dialog=new EditorDialog(this,"Blattlayout");dialog.FixedChoice("Blattformat",sheet.Format,"A4","A3","A2","Formfrei");dialog.Check("Querformat",sheet.Landscape);
        if(dialog.ShowDialog()!=true)return;
        CancelPlacement();BlankLayout.Set(Current!.Project,dialog.Value("Blattformat"),dialog.Checked("Querformat"));Changed();Draw();Fit();
        if(BlankLayout.Free(Current.Project))PlacementHint.Text="Formfrei: Mittlere Maustaste erweitert die Fläche am Rand · Druck-/Exportfenster aufziehen";
    }
    private void ExportWindow_Click(object sender,RoutedEventArgs e)
    {
        if(Current is not {} d||!BlankLayout.Free(d.Project)){StatusText.Text="Druck-/Exportfenster ist im Blattformat Formfrei verfügbar.";return;}
        CancelPlacement();ClearSelection();_selectExportWindow=true;
        PlacementHint.Text="Druck-/Exportfenster mit linker Maustaste aufziehen · Esc: Abbrechen";Sheet.Focus();
    }
    private void BeginExportWindow(Point point)
    {
        _exportStart=point;_exportRubber=new Rectangle{Stroke=Brushes.DarkOrange,StrokeThickness=2,StrokeDashArray=new DoubleCollection([5,3]),Fill=new SolidColorBrush(Color.FromArgb(20,255,140,0)),IsHitTestVisible=false};
        Sheet.Children.Add(_exportRubber);Sheet.CaptureMouse();
    }
    private void MoveExportWindow(Point p)
    {
        if(_exportRubber is null)return;var r=new Rect(_exportStart,p);Canvas.SetLeft(_exportRubber,r.X);Canvas.SetTop(_exportRubber,r.Y);_exportRubber.Width=r.Width;_exportRubber.Height=r.Height;
    }
    private void FinishExportWindow(Point p)
    {
        if(Current?.Project.BlankSheet is not {} b)return;
        var r=new Rect(_exportStart,p);if(r.Width<5||r.Height<5){CancelExportSelection();Draw();return;}
        var f=new ExportWindow{XMm=r.X/Sheet.Width*b.WidthMm,YMm=r.Y/Sheet.Height*b.HeightMm,WidthMm=r.Width/Sheet.Width*b.WidthMm,HeightMm=r.Height/Sheet.Height*b.HeightMm};
        try{BlankLayout.ValidateWindow(f);}catch{CancelExportSelection();Draw();return;}
        b.ExportWindow=f;CancelExportSelection();Changed();Draw();PlacementHint.Text=$"Druck-/Exportfenster: {f.WidthMm:F1} × {f.HeightMm:F1} mm";
    }
    private void CancelExportSelection()
    {
        if(_exportRubber is not null){Sheet.Children.Remove(_exportRubber);_exportRubber=null;Sheet.ReleaseMouseCapture();}
        _selectExportWindow=false;
    }
    private void DrawExportWindow()
    {
        if(Current?.Project.BlankSheet is not {ExportWindow:{} f} b||!BlankLayout.Free(Current.Project))return;
        var rectangle=new Rectangle{Width=f.WidthMm/b.WidthMm*Sheet.Width,Height=f.HeightMm/b.HeightMm*Sheet.Height,Stroke=Brushes.DarkOrange,StrokeThickness=2,StrokeDashArray=new DoubleCollection([5,3]),IsHitTestVisible=false};
        Canvas.SetLeft(rectangle,f.XMm/b.WidthMm*Sheet.Width);Canvas.SetTop(rectangle,f.YMm/b.HeightMm*Sheet.Height);Sheet.Children.Add(rectangle);
    }
    private void ExtendFreeCanvas(ref double x,ref double y)
    {
        if(Current is not {} d||!BlankLayout.Free(d.Project))return;
        var b=d.Project.BlankSheet!;var unit=LogicalPageWidth/297*d.Zoom;
        var left=x<0?297d:0;var top=y<0?210d:0;
        var right=x>Viewport.ScrollableWidth?297d:0;var bottom=y>Viewport.ScrollableHeight?210d:0;
        if(left+right+top+bottom==0)return;
        // Only pan extends the work area; active drawing coordinates are not altered.
        CancelPlacement();BlankLayout.Resize(d.Project,b.WidthMm+left+right,b.HeightMm+top+bottom,left,top);
        x+=left*unit;y+=top*unit;_panX+=left*unit;_panY+=top*unit;Changed();Draw();Viewport.UpdateLayout();
    }
}
