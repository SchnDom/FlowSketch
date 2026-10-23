using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;

public partial class MainWindow
{
    private readonly HashSet<Guid> _selection=[];
    private Rectangle? _marquee;
    private Point _marqueeStart;
    private bool _extendMarquee;
    private Point? _selectionDrag;
    private Point _dragOrigin;
    private Vector _dragApplied;
    private int _dragAxis;
    private EquipmentItem? _dragCopy;
    private void BeginSelectionDrag(Point point){_selectionDrag=point;_dragOrigin=point;_dragApplied=new();_dragAxis=0;}
    private void CancelCopyDrag()
    {
        if(_dragCopy is null||Current is not {} d)return;OverlaySelection.Delete(d.Project,new HashSet<Guid>{_dragCopy.Id});_dragCopy=null;_selectionDrag=null;
        if(Mouse.Captured is UIElement captured&&IsAncestorOf(captured))captured.ReleaseMouseCapture();
        ClearSelection();
    }
    private void ClearSelection(){_selection.Clear();_selected=null;_selectedPipe=null;_selectionDrag=null;if(Sheet is not null)DrawSelection();}
    private void SelectObject(Guid id,bool extend=false)
    {
        if(Current is not {} d)return;if(!extend&&!_selection.Contains(id))_selection.Clear();
        var members=OverlaySelection.ExpandGroups(d.Project,[id],d.Page+1);if(extend&&_selection.Contains(id))_selection.ExceptWith(members);else _selection.UnionWith(members);
        _selected=d.Project.Equipment.FirstOrDefault(x=>x.Id==id);_selectedPipe=d.Project.Pipes.FirstOrDefault(x=>x.Id==id);DrawSelection();
    }
    private void DrawSelection()
    {
        foreach(var thumb in Sheet.Children.OfType<Thumb>().ToArray())if(_selection.Count!=1||!(thumb.Tag is Guid id&&_selection.Contains(id)||thumb.Tag is ValueTuple<Guid,bool> endpoint&&_selection.Contains(endpoint.Item1)))Sheet.Children.Remove(thumb);
        foreach(var old in Sheet.Children.OfType<Rectangle>().Where(r=>Equals(r.Tag,"selection-outline")||Equals(r.Tag,"group-outline")).ToArray())Sheet.Children.Remove(old);
        if(Current is not {} d)return;
        _selection.IntersectWith(d.Project.Equipment.Where(x=>x.Page==d.Page+1).Select(x=>x.Id).Concat(d.Project.Pipes.Where(x=>x.Page==d.Page+1).Select(x=>x.Id)).Concat(d.Project.SignalLines.Where(x=>x.Page==d.Page+1).Select(x=>x.Id)));
        DrawGroupOutlines(d.Project);
        foreach(var rect in d.Project.Equipment.Where(x=>_selection.Contains(x.Id)).Select(OverlaySelection.Bounds).Concat(d.Project.Pipes.Where(x=>_selection.Contains(x.Id)).Select(x=>OverlaySelection.Bounds(d.Project,x))).Concat(d.Project.SignalLines.Where(x=>_selection.Contains(x.Id)).Select(x=>OverlaySelection.Bounds(d.Project,x))))
        {
            var outline=new Rectangle{Tag="selection-outline",Width=Math.Max(8,rect.Width*Sheet.Width+8),Height=Math.Max(8,rect.Height*Sheet.Height+8),Stroke=Brushes.DodgerBlue,StrokeThickness=1.5,StrokeDashArray=new DoubleCollection([3,2]),IsHitTestVisible=false};Canvas.SetLeft(outline,rect.X*Sheet.Width-4);Canvas.SetTop(outline,rect.Y*Sheet.Height-4);Sheet.Children.Add(outline);
        }
        Sheet.ContextMenu=CanvasMenu();
        if(_selection.Count==0&&PlacementHint.Text.Contains("ausgewählt"))PlacementHint.Text="";
        if(_selection.Count>0)PlacementHint.Text=$"{_selection.Count} ausgewählt · ENTF: löschen · Rechtsklick: Eigenschaften";
    }
    private void BeginMarquee(Point point)
    {
        _extendMarquee=Keyboard.Modifiers.HasFlag(ModifierKeys.Control)||Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);if(!_extendMarquee)ClearSelection();_marqueeStart=point;
        _marquee=new Rectangle{Stroke=Brushes.DodgerBlue,Fill=new SolidColorBrush(Color.FromArgb(35,30,144,255)),StrokeThickness=1,IsHitTestVisible=false};Canvas.SetLeft(_marquee,point.X);Canvas.SetTop(_marquee,point.Y);Sheet.Children.Add(_marquee);Sheet.CaptureMouse();
    }
    private void MoveMarquee(Point p)
    {
        if(_marquee is null)return;var rect=new Rect(_marqueeStart,p);Canvas.SetLeft(_marquee,rect.X);Canvas.SetTop(_marquee,rect.Y);_marquee.Width=rect.Width;_marquee.Height=rect.Height;
    }
    private void Sheet_Up(object sender,MouseButtonEventArgs e)
    {
        if(_drawingDraft is not null){FinishAnnotation(e.GetPosition(Sheet));e.Handled=true;return;}
        if(_exportRubber is not null){FinishExportWindow(e.GetPosition(Sheet));e.Handled=true;return;}
        if(_marquee is null||Current is not {} d)return;var box=new Rect(_marqueeStart,e.GetPosition(Sheet));Sheet.Children.Remove(_marquee);_marquee=null;Sheet.ReleaseMouseCapture();
        var normalized=new Rect(box.X/Sheet.Width,box.Y/Sheet.Height,box.Width/Sheet.Width,box.Height/Sheet.Height);_selection.UnionWith(OverlaySelection.ExpandGroups(d.Project,OverlaySelection.InRectangle(d.Project,d.Page+1,normalized),d.Page+1));DrawSelection();e.Handled=true;
    }
    private void DeleteSelection()
    {
        if(Current is not {} d||_selection.Count==0)return;OverlaySelection.Delete(d.Project,_selection);ClearSelection();Changed();Draw();PlacementHint.Text="";
    }
    private void GroupSelection(bool ungroup=false)
    {
        if(Current is not {} d||_selection.Count==0||(!ungroup&&_selection.Count<2))return;if(ungroup)OverlaySelection.Ungroup(d.Project,_selection);else OverlaySelection.Group(d.Project,_selection);Changed();Draw();
    }
    private void EditSelection()
    {
        if(Current is not {} d||_selection.Count==0)return;var dialog=new BatchPropertiesEditor(this,d.Project,_selection,Sheet.Width/d.Zoom,Sheet.Height/d.Zoom);if(dialog.ShowDialog()!=true)return;OverlaySelection.Apply(d.Project,_selection,dialog.Patch!);Changed();Draw();
    }
    private ContextMenu SelectionMenu()
    {
        return PlanMenu();
    }
    private void UpdateSelectionDrag(Point p)
    {
        if(_selectionDrag is null||Current is not {} d)return;var total=OverlayTransforms.Direction(p-_dragOrigin,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),ref _dragAxis);var delta=total-_dragApplied;OverlaySelection.Move(d.Project,_selection,delta.X/Sheet.Width,delta.Y/Sheet.Height);_dragApplied=total;_selectionDrag=p;RefreshPipeGeometry();
        foreach(var element in Sheet.Children.OfType<FrameworkElement>())if(element.Tag is Guid id&&d.Project.Equipment.FirstOrDefault(x=>x.Id==id) is {} item){if(element is TextBlock label)PositionTag(item,label);else{Canvas.SetLeft(element,item.X*Sheet.Width);Canvas.SetTop(element,item.Y*Sheet.Height);}}foreach(var handle in Sheet.Children.OfType<Thumb>())if(handle.Tag is ValueTuple<Guid,bool> tag&&d.Project.Equipment.FirstOrDefault(x=>x.Id==tag.Item1) is {} line){var ends=DrawingGeometry.Ends(line);var point=tag.Item2?ends.Start:ends.End;Canvas.SetLeft(handle,point.X*Sheet.Width-7);Canvas.SetTop(handle,point.Y*Sheet.Height-7);}DrawSelection();
    }
    private static bool EditingText(DependencyObject? node)
    {
        while(node is not null){if(node is TextBoxBase or PasswordBox or ComboBox)return true;node=node is Visual?VisualTreeHelper.GetParent(node):LogicalTreeHelper.GetParent(node);}return false;
    }
    private void Window_KeyDown(object sender,KeyEventArgs e)
    {
        if(EditingText(Keyboard.FocusedElement as DependencyObject)||OwnedWindows.Cast<Window>().Any(w=>w.IsVisible))return;
        if((Keyboard.Modifiers==ModifierKeys.Control&&e.Key==Key.Z)){RestoreHistory(false);e.Handled=true;}
        else if((Keyboard.Modifiers==ModifierKeys.Control&&e.Key==Key.Y)||(Keyboard.Modifiers==(ModifierKeys.Control|ModifierKeys.Shift)&&e.Key==Key.Z)){RestoreHistory(true);e.Handled=true;}
        else if(Keyboard.Modifiers==ModifierKeys.Control&&e.Key==Key.C){CopySelection();e.Handled=true;}
        else if(Keyboard.Modifiers==ModifierKeys.Control&&e.Key==Key.V){PasteSelection();e.Handled=true;}
        else if(Keyboard.Modifiers==ModifierKeys.Control&&e.Key==Key.S){Save_Click(this,new RoutedEventArgs());e.Handled=true;}
        else if(Keyboard.Modifiers==(ModifierKeys.Control|ModifierKeys.Shift)&&e.Key==Key.S){SaveAs_Click(this,new RoutedEventArgs());e.Handled=true;}
        else if(e.Key==Key.Delete&&_selection.Count>0){DeleteSelection();e.Handled=true;}
        else if(e.Key==Key.R&&Keyboard.Modifiers==ModifierKeys.None&&Current?.Image is not null){Pipe_Click(this,new RoutedEventArgs());Sheet.Focus();e.Handled=true;}
        else if(e.Key==Key.Escape){CancelCopyDrag();CancelPlacement();if(_marquee is not null){Sheet.Children.Remove(_marquee);_marquee=null;Sheet.ReleaseMouseCapture();}ClearSelection();Draw();e.Handled=true;}
    }
}
