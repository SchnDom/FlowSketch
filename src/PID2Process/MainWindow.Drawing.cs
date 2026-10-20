using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace PID2Process;
public partial class MainWindow
{
    private EquipmentItem? _drawingDraft;
    private Point _drawingOrigin;
    private void BeginAnnotation(Point point)
    {
        _drawingDraft=_pending;_pending=null;_drawingOrigin=point;if(_ghost is not null)Sheet.Children.Remove(_ghost);_ghost=null;Sheet.CaptureMouse();UpdateAnnotation(point);
    }
    internal static void AnnotationBounds(EquipmentItem item,Point start,Point end,double width,double height,bool shift)
    {
        var dx=end.X-start.X;var dy=end.Y-start.Y;
        if(shift&&item.Variant is "Rechteck" or "Kreis / Ellipse"){var size=Math.Max(0,Math.Min(Math.Max(Math.Abs(dx),Math.Abs(dy)),Math.Min(dx<0?start.X:width-start.X,dy<0?start.Y:height-start.Y)));dx=Math.CopySign(size,dx);dy=Math.CopySign(size,dy);}
        item.X=Math.Clamp(Math.Min(start.X,start.X+dx)/width,0,.999);item.Y=Math.Clamp(Math.Min(start.Y,start.Y+dy)/height,0,.999);
        item.Width=Math.Clamp(Math.Abs(dx)/width,.001,1-item.X);item.Height=Math.Clamp(Math.Abs(dy)/height,.001,1-item.Y);item.Drawing.ReverseX=dx<0;item.Drawing.ReverseY=dy<0;item.Drawing.Horizontal=Math.Abs(dy)<2;item.Drawing.Vertical=Math.Abs(dx)<2;
        if(DrawingGeometry.IsLine(item)){item.Drawing.PageWidth=width;DrawingGeometry.SetEnds(item,new(start.X/width,start.Y/height),new((start.X+dx)/width,(start.Y+dy)/height),width,height);}
    }
    private void UpdateAnnotation(Point point)
    {
        if(_drawingDraft is not {} item)return;AnnotationBounds(item,_drawingOrigin,point,Sheet.Width,Sheet.Height,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        if(_ghost is not null)Sheet.Children.Remove(_ghost);_ghost=AnnotationVisual.Symbol(item,Sheet.Width,Sheet.Height);_ghost.IsHitTestVisible=false;Canvas.SetLeft(_ghost,item.X*Sheet.Width);Canvas.SetTop(_ghost,item.Y*Sheet.Height);Sheet.Children.Add(_ghost);
    }
    private void FinishAnnotation(Point point)
    {
        if(_drawingDraft is not {} item||Current is not {} doc)return;
        if((point-_drawingOrigin).Length<3)point=_drawingOrigin+new Vector(item.Variant=="Textfeld"?220:120,item.Variant=="Textfeld"?45:item.Variant is "Linie" or "Pfeil" or "Doppelpfeil"?0:90);
        AnnotationBounds(item,_drawingOrigin,point,Sheet.Width,Sheet.Height,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));item.Page=doc.Page+1;item.Drawing.PageWidth=Sheet.Width/doc.Zoom;if(DrawingGeometry.IsLine(item))DrawingGeometry.UpdateBounds(item,Sheet.Width/doc.Zoom,Sheet.Height/doc.Zoom);CancelAnnotation();
        if(item.Variant=="Textfeld"&&!EditAnnotation(item))return;doc.Project.Equipment.Add(item);ClearSelection();Changed();Draw();SelectObject(item.Id);ShowHandles(item);
    }
    private void CancelAnnotation()
    {
        if(_drawingDraft is null)return;_drawingDraft=null;if(_ghost is not null)Sheet.Children.Remove(_ghost);_ghost=null;Sheet.ReleaseMouseCapture();
    }
    private bool EditAnnotation(EquipmentItem item)
    {
        var dialog=new EditorDialog(this,"Zeichnung · "+item.Variant){Width=480};if(item.Variant is "Pfeil" or "Doppelpfeil")dialog.Text("Pfeilspitze (px)",item.Drawing.ArrowSize.ToString());if(item.Variant=="Textfeld"){dialog.Text("Text",item.TextContent,true);dialog.Text("Schriftgröße (px)",item.Drawing.FontSize.ToString());}
        dialog.Color("Linien-/Textfarbe",item.Color);var palette=new WrapPanel{Margin=new Thickness(0,8,0,0)};foreach(var color in new[]{"#000000","#808080","#FFFFFF","#FF0000","#FFAA33","#FFFF00","#00A650","#00A0DC","#0000FF","#FF00FF"}){var swatch=new Button{Width=28,Height=28,Padding=new Thickness(0),Margin=new Thickness(2),Background=PageVisual.Color(color),ToolTip=color};swatch.Click+=(_,_)=>((ColorPicker)dialog.Control("Linien-/Textfarbe")).Hex.Text=color;palette.Children.Add(swatch);}dialog.Custom("Schnellfarben",palette);dialog.Choice("Linienstärke (px)",item.Drawing.StrokeWidth.ToString(),"1","2","3","5","8","12");dialog.Check("Fläche füllen",item.Drawing.Filled);dialog.Color("Füllfarbe",item.Drawing.FillColor);if(DrawingGeometry.IsLine(item)){dialog.Hide("Fläche füllen");dialog.Hide("Füllfarbe");}
        dialog.Validation=()=>item.Variant is "Pfeil" or "Doppelpfeil"&&dialog.Number("Pfeilspitze (px)",0) is <2 or >200?"Pfeilspitze zwischen 2 und 200 px eingeben.":!ColorPicker.IsValid(dialog.Value("Linien-/Textfarbe"))||!ColorPicker.IsValid(dialog.Value("Füllfarbe"))?"Gültige Farben eingeben.":dialog.Number("Linienstärke (px)",0) is <.1 or >20?"Linienstärke zwischen 0,1 und 20 px eingeben.":item.Variant=="Textfeld"&&dialog.Number("Schriftgröße (px)",0) is <4 or >300?"Schriftgröße zwischen 4 und 300 px eingeben.":null;
        if(dialog.ShowDialog()!=true)return false;if(item.Variant is "Pfeil" or "Doppelpfeil")item.Drawing.ArrowSize=Math.Clamp(dialog.Number("Pfeilspitze (px)",14),2,200);item.Color=dialog.Value("Linien-/Textfarbe");item.Drawing.StrokeWidth=dialog.Number("Linienstärke (px)",1.5);item.Drawing.Filled=dialog.Checked("Fläche füllen");item.Drawing.FillColor=dialog.Value("Füllfarbe");if(item.Variant=="Textfeld"){item.TextContent=dialog.Value("Text");item.Drawing.FontSize=dialog.Number("Schriftgröße (px)",16);}return true;
    }
}
