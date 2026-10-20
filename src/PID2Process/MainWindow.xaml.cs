using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
namespace PID2Process;

public partial class MainWindow : Window
{
    private sealed class Document
    {
        public EngineeringProject Project = new();
        public ProjectHistory History=new();
        public string? SavePath;
        public PdfRasterizer Raster = new();
        public BitmapSource? Image;
        public int Page;
        public double Zoom = .65;
        public bool Dirty;
        public bool FollowScenarioEditorSelection=true;
    }
    private Document? Current => (DocumentTabs.SelectedItem as TabItem)?.Tag as Document;
    private EquipmentItem? _pending, _selected;
    private FrameworkElement? _ghost;
    private bool _drawingPipe, _panning;
    internal Func<bool> MiddleButtonPressed {get;set;} = () => System.Windows.Input.Mouse.MiddleButton==MouseButtonState.Pressed;
    private Point _panStart;
    private double _panX, _panY;
    private int _renderVersion;
    private const double LogicalPageWidth = 2000;
    private readonly System.Windows.Threading.DispatcherTimer _zoomRenderTimer = new() { Interval = TimeSpan.FromMilliseconds(180) };
    private readonly string _recentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowSketch", "recent.json");
    public MainWindow()
    {
        InitializeComponent();InitializeWorkAreaBounds();StateChanged+=(_,_)=>UpdateChrome();UpdateChrome();Icon=InformationWindow.IconImage();Title=BuildInfo.Name+" · v"+BuildInfo.Version; Loaded += (_, _) => RefreshRecent();
        InitializeInspector();InitializeLibrary();Viewport.PreviewMouseLeftButtonDown+=(_,e)=>{if(e.OriginalSource is DependencyObject source&&!Sheet.IsAncestorOf(source)&&source!=Sheet)ClearSelection();};Viewport.ContextMenu=new ContextMenu();Viewport.ContextMenuOpening+=(_,_)=>Viewport.ContextMenu=PlanMenu();Viewport.ScrollChanged+=(_,_)=>UpdateOverview();Viewport.SizeChanged+=(_,_)=>UpdateOverview();
        Sheet.PreviewMouseLeftButtonDown+=(_,e)=>{if(_selectExportWindow)Sheet_Down(Sheet,e);};
        PreviewKeyDown+=Window_KeyDown;
        Viewport.AddHandler(System.Windows.Input.Mouse.PreviewMouseDownEvent,new MouseButtonEventHandler(Viewport_Down),true);
        Viewport.AddHandler(System.Windows.Input.Mouse.PreviewMouseUpEvent,new MouseButtonEventHandler(Viewport_Up),true);
        _zoomRenderTimer.Tick += async (_, _) => { _zoomRenderTimer.Stop(); await RenderPage(true); };
        Closed += (_, _) => _zoomRenderTimer.Stop();
        Deactivated += (_, _) => { if (_panning) StopPan(); };
        Closing += (_, e) => { if (DocumentTabs.Items.Cast<TabItem>().Any(t => ((Document)t.Tag).Dirty) && MessageBox.Show(this, "Ungespeicherte Änderungen verwerfen und schließen?", "FlowSketch", MessageBoxButton.YesNo) != MessageBoxResult.Yes) e.Cancel = true; };
    }
    private void Changed()
    {
        if (Current is not { } d) return; d.Dirty = true; d.Project.Approval.IsApproved = false; d.Project.Approval.IsBlocked = true; d.Project.Approval.BlockReason = "Engineering-Daten geändert. Erneute Freigabe erforderlich."; d.History.Commit(d.Project);UpdateHeader();RefreshInspector();
    }
    private void UpdateHeader()
    {
        UpdateHistoryCommands();
        if (Current is not { } d) return; ProjectTitle.Text=d.Project.Name+(d.Dirty?" *":"");Title=d.Project.Name+" · FlowSketch v"+BuildInfo.Version;var activeTab=(TabItem)DocumentTabs.SelectedItem;activeTab.Header=TabHeader(d,activeTab);
        PageLabel.Text = $"{d.Page + 1} / {Math.Max(1, d.Project.Pdf.PageCount)}"+(d.Project.BlankSheet is {} b?" · "+b.Format+(b.Format=="Formfrei"?"":b.Landscape?" quer":" hoch"):""); StatusText.Text = d.Project.Approval.IsApproved ? "Freigegeben" : "Entwurf · " + d.Project.Approval.BlockReason;
    }
    private void AddDocument(Document d)
    {
        d.History.Reset(d.Project);var tab = new TabItem { Tag = d };tab.Header=TabHeader(d,tab); DocumentTabs.Items.Add(tab); DocumentTabs.SelectedItem = tab;
    }
    internal void CreateBlankDocument() { var d=new Document{Project=ProjectServices.NewBlank(),Image=WhiteBackground()}; AddDocument(d); Draw(); }
    private void New_Click(object sender, RoutedEventArgs e) => CreateBlankDocument();
    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Filter = "PDF|*.pdf", Multiselect = true };
        if (picker.ShowDialog(this) != true) return;
        foreach (var file in picker.FileNames)
        {
            try
            {
                await ImportPdf(file);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "PDF konnte nicht geöffnet werden"); }
        }
    }
    internal async Task ImportPdf(string file)
    {
        var d = new Document(); await d.Raster.OpenAsync(file);
        d.Project.Name = Path.GetFileNameWithoutExtension(file); d.Project.Pdf = new PdfReference { FilePath = file, FileName = Path.GetFileName(file), Sha256 = ProjectServices.HashFile(file), PageCount = d.Raster.PageCount,PageAspectRatios=Enumerable.Range(0,d.Raster.PageCount).Select(d.Raster.AspectRatio).ToList() }; d.Dirty = true; AddDocument(d);
        await RenderPage();
    }
    internal EngineeringProject? ActiveProject => Current?.Project;
    internal void ClearTestDirtyFlags() { foreach (TabItem tab in DocumentTabs.Items) ((Document)tab.Tag).Dirty = false; }
    private async void Open_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Filter = "FlowSketch-Projekt|*.pid2process.json" };
        if (picker.ShowDialog(this) == true) await OpenProject(picker.FileName);
    }
    private async Task OpenProject(string file)
    {
        try
        {
            var d = new Document { Project = ProjectServices.Load(file), SavePath = file };
            if (d.Project.BlankSheet is null && !File.Exists(d.Project.Pdf.FilePath))
            {
                var locate = new Microsoft.Win32.OpenFileDialog { Title = "Referenz-PDF zuordnen", Filter = "PDF|*.pdf" };
                if (locate.ShowDialog(this) != true) return; d.Project.Pdf.FilePath = locate.FileName;
            }
            if(d.Project.BlankSheet is not null)d.Image=WhiteBackground();
            if(d.Project.BlankSheet is null){await d.Raster.OpenAsync(d.Project.Pdf.FilePath); d.Project.Pdf.PageCount = d.Raster.PageCount;} ProjectServices.ApplyApprovalStatus(d.Project); AddDocument(d); Remember(file);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Projekt öffnen"); }
    }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } d) return;
        if (d.SavePath is null) { var picker = new Microsoft.Win32.SaveFileDialog { Filter = "PID2Process|*.pid2process.json", FileName = d.Project.Name + ".pid2process.json" }; if (picker.ShowDialog(this) != true) return; d.SavePath = picker.FileName; }
        try { ProjectServices.Save(d.SavePath, d.Project); d.Dirty = false; Remember(d.SavePath); UpdateHeader(); } catch (Exception ex) { MessageBox.Show(this, ex.Message, "Speichern"); }
    }
    private void Remember(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_recentPath)!); var list = Recent().Where(x => x != file).Prepend(file).Take(10).ToArray(); File.WriteAllText(_recentPath, JsonSerializer.Serialize(list)); RefreshRecent();
    }
    private string[] Recent() { try { return File.Exists(_recentPath) ? JsonSerializer.Deserialize<string[]>(File.ReadAllText(_recentPath)) ?? [] : []; } catch { return []; } }
    private void RefreshRecent() { RecentMenu.Items.Clear(); foreach (var file in Recent()) { var entry = new MenuItem { Header = Path.GetFileName(file), ToolTip = file }; entry.Click += async (_, _) => await OpenProject(file); RecentMenu.Items.Add(entry); } RecentMenu.IsEnabled = RecentMenu.Items.Count > 0; }
    private async void DocumentTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != DocumentTabs || Current is not { } d) return; CancelPlacement(); ClearSelection(); ScenarioBox.ItemsSource = d.Project.Scenarios; ScenarioBox.SelectedIndex = -1; StepBox.ItemsSource=d.Project.ControlSteps;StepBox.SelectedIndex=-1; await RenderPage();
    }
    private async Task RenderPage(bool imageOnly = false)
    {
        var version = ++_renderVersion; var d = Current; if (d is null) return;
        if(!imageOnly)Sheet.IsEnabled=false;
        try
        {
            StatusText.Text = d.Project.BlankSheet is null?"PDF-Seite wird gerendert …":"Zeichenblatt wird vorbereitet …";
            if (d.Raster.PageCount > 0)
            {
                var dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
                var width = (int)Math.Ceiling(Math.Max(2000, LogicalPageWidth * d.Zoom * dpi * 1.25));
                var image = await d.Raster.RenderAsync(d.Page, width);
                if (version != _renderVersion || d != Current) return; d.Image = image;
            }
            if (version != _renderVersion || d != Current) return;
            if(imageOnly && Sheet.Children.OfType<Image>().FirstOrDefault() is { } background){background.Source=d.Image;QueueOverview();} else Draw();
            UpdateHeader();RefreshInspector();
        }
        catch (Exception ex) { StatusText.Text = ex.Message; }
        finally {if(version==_renderVersion)Sheet.IsEnabled=true;}
    }
    private NetworkScene? _networkScene;
    private EngineeringProject? _previewProject;
    private Scenario? _previewScenario;
    private ControlStep? _previewStep;
    private void Draw(){if(Current is {} doc&&_previewProject is not null){var saved=doc.Project;doc.Project=_previewProject;try{DrawCore();}finally{doc.Project=saved;}}else DrawCore();}
    private void DrawCore()
    {
        if(Current is {} active)DrawingGeometry.Normalize(active.Project);
        Sheet.Children.Clear(); _ghost = null;_pipePreview=null; if (Current is not { } d) return;
        Sheet.Width = (d.Project.BlankSheet is {} sheet?LogicalPageWidth*sheet.WidthMm/297:LogicalPageWidth) * d.Zoom; Sheet.Height = Sheet.Width * (d.Project.BlankSheet is {} blank?blank.HeightMm/blank.WidthMm:d.Raster.PageCount>0?d.Raster.AspectRatio(d.Page):.708);
        if (d.Image is not null) Sheet.Children.Add(new Image { Source = d.Image, Width = Sheet.Width, Height = Sheet.Height, IsHitTestVisible = false });
        var scenario = _previewProject is not null?_previewScenario:ScenarioBox.SelectedItem as Scenario;
        var step = _previewProject is not null?_previewStep:StepBox.SelectedItem as ControlStep;
        if(step is not null) { scenario = scenario is null ? new Scenario() : JsonSerializer.Deserialize<Scenario>(JsonSerializer.Serialize(scenario,ProjectServices.JsonOptions),ProjectServices.JsonOptions); foreach(var state in step.EquipmentStates)scenario!.EquipmentStates[state.Key]=state.Value;foreach(var state in step.PipeStates)scenario!.PipeStates[state.Key]=state.Value;foreach(var state in step.TextValues)scenario!.TextValues[state.Key]=state.Value;foreach(var state in step.SensorValues)scenario!.SensorValues[state.Key]=state.Value;SceneValues.MergeTanks(scenario!.TankStates,step.TankStates); }
        _networkScene=PipeNetwork.EvaluateScene(d.Project,SceneValues.From(scenario,null));DrawPipes(SceneValues.From(scenario,null));
        DrawSensorConnections();DrawSignalLines();foreach (var item in d.Project.Equipment.Where(x => x.Page == d.Page + 1)) DrawEquipment(item, scenario);
        Sheet.ContextMenu = CanvasMenu();
        DrawSelection();DrawExportWindow();QueueOverview();
    }
    private static System.Windows.Media.Brush Brush(string color) { try { return (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(color)!; } catch { return Brushes.Teal; } }
    private FrameworkElement Glyph(EquipmentItem item) => PageVisual.Symbol(item,Sheet.Width,Sheet.Height);
    private void DrawEquipment(EquipmentItem item, Scenario? scenario)
    {
        var appearance = Clone(item); var state = scenario?.EquipmentStates.GetValueOrDefault(item.Id);
        if(item.Type is EquipmentType.Text or EquipmentType.Legend&&state=="Ausgeblendet")return;
        appearance.TextContent=scenario?.TextValues.GetValueOrDefault(item.Id)??item.TextContent;
        var values=SceneValues.From(scenario,null);var network=_networkScene??PipeNetwork.EvaluateScene(Current!.Project,values);appearance=PageVisual.Appearance(appearance,Current!.Project,values);var element = PageVisual.Symbol(appearance,Sheet.Width,Sheet.Height,state,status:PageVisual.Status(item,values,network),sensorView:item.Type==EquipmentType.Instrument?SensorEvaluation.Display(item,values):null);
        Canvas.SetLeft(element, item.X * Sheet.Width); Canvas.SetTop(element, item.Y * Sheet.Height);
        var label = new TextBlock { Text = item.Tag, Background = Brushes.White, Foreground = Brushes.Black, IsHitTestVisible = false }; Canvas.SetLeft(label, item.X * Sheet.Width); Canvas.SetTop(label, item.Y * Sheet.Height + element.Height + PageVisual.CaptionOffset(item,Sheet.Width)); Sheet.Children.Add(element); if(item.Type!=EquipmentType.Drawing)Sheet.Children.Add(label);
        bool dragging=false,moved=false,copyPending=false,copyStarted=false;Point mouseStart=new();element.Tag=item.Id;label.Tag=item.Id;
        element.MouseLeftButtonDown+=(_,e)=>
        {
            if(_drawingPipe)return;e.Handled=true;Sheet.Focus();mouseStart=e.GetPosition(Sheet);
            copyPending=Keyboard.Modifiers.HasFlag(ModifierKeys.Control);copyStarted=false;
            if(!copyPending){if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)&&!_selection.Contains(item.Id))SelectObject(item.Id,true);else SelectObject(item.Id);}
            if(e.ClickCount==2&&!copyPending){if(EditEquipment(item)){Changed();Draw();}return;}
            BeginSelectionDrag(mouseStart);dragging=true;moved=false;element.CaptureMouse();
            if(!copyPending&&_selection.Count==1)ShowHandles(item);
        };
        element.MouseMove+=(_,e)=>
        {
            if(!dragging||e.LeftButton!=MouseButtonState.Pressed||_selectionDrag is null)return;
            var point=e.GetPosition(Sheet);if(!moved&&(point-mouseStart).Length<3)return;
            if(copyPending&&!copyStarted)
            {
                _dragCopy=OverlayTransforms.Copy(Current!.Project,item);ClearSelection();SelectObject(_dragCopy.Id);BeginSelectionDrag(mouseStart);
                DrawEquipment(_dragCopy,scenario);ShowHandles(_dragCopy);copyStarted=true;
            }
            UpdateSelectionDrag(point);moved=true;
        };
        element.MouseLeftButtonUp+=(_,e)=>
        {
            if(!dragging)return;dragging=false;_selectionDrag=null;_dragCopy=null;element.ReleaseMouseCapture();
            if(copyPending&&!copyStarted)SelectObject(item.Id,true);
            _dragCopy=null;if(moved)Changed();Draw();if(_selection.Count==1&&_selected is {} selected)ShowHandles(selected);e.Handled=true;
        };
        element.LostMouseCapture+=(_,_)=>{dragging=false;_selectionDrag=null;if(_dragCopy is not null){_dragCopy=null;Changed();Draw();}};
        element.ContextMenu = PlanMenu();
        element.ContextMenuOpening+=(_,_)=>{SelectObject(item.Id);element.ContextMenu=PlanMenu();};
        if(_drawingSignal){var frame=new Rectangle{Width=item.Width*Sheet.Width,Height=item.Height*Sheet.Height,Stroke=Brushes.IndianRed,StrokeThickness=1,StrokeDashArray=new DoubleCollection([2,3]),IsHitTestVisible=false};Canvas.SetLeft(frame,item.X*Sheet.Width);Canvas.SetTop(frame,item.Y*Sheet.Height);Sheet.Children.Add(frame);}
        if(_drawingPipe&&!_drawingSignal)foreach(var port in PipeNetwork.Ports(item)){var point=_drawingSignal?SignalGeometry.Port(item,port):PipeNetwork.Port(item,port);var marker=new Ellipse{Width=9,Height=9,Fill=Brushes.Orange,Stroke=Brushes.White,IsHitTestVisible=false};Canvas.SetLeft(marker,point.X*Sheet.Width-4.5);Canvas.SetTop(marker,point.Y*Sheet.Height-4.5);Sheet.Children.Add(marker);}
    }
    private void ShowHandles(EquipmentItem item)
    {
        foreach (var handle in Sheet.Children.OfType<Thumb>().ToArray()) Sheet.Children.Remove(handle);
        if(_selection.Count!=1||!_selection.Contains(item.Id))return;
        if(DrawingGeometry.IsLine(item)){ShowAnnotationEndpoints(item);return;}
        var thumb = new Thumb { Tag=item.Id, Width = 16, Height = 16, Background = Brushes.DarkOrange, Cursor = Cursors.SizeNWSE };
        void Position() { Canvas.SetLeft(thumb, (item.X + item.Width) * Sheet.Width - 8); Canvas.SetTop(thumb, (item.Y + item.Height) * Sheet.Height - 8); }
        var baseWidth=item.Width;var baseHeight=item.Height;var resizeOrigin=new Point();
        thumb.DragStarted+=(_,_)=>{baseWidth=item.Width;baseHeight=item.Height;resizeOrigin=Mouse.GetPosition(Sheet);};
        Position();thumb.DragDelta+=(_,e)=>
        {
            var movement=Mouse.GetPosition(Sheet)-resizeOrigin;
            OverlayTransforms.Resize(item,baseWidth+movement.X/Sheet.Width,baseHeight+movement.Y/Sheet.Height,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),baseWidth,baseHeight,Sheet.Width,Sheet.Height);
            Position();RefreshPipeGeometry();
            foreach(var graphic in Sheet.Children.OfType<Grid>().Where(g=>g.Tag is Guid id&&id==item.Id)){graphic.Width=item.Width*Sheet.Width;graphic.Height=item.Height*Sheet.Height;if(item.Type==EquipmentType.Drawing){var redraw=(Grid)AnnotationVisual.Symbol(item,Sheet.Width,Sheet.Height);graphic.Children.Clear();foreach(var child in redraw.Children.Cast<UIElement>().ToArray()){redraw.Children.Remove(child);graphic.Children.Add(child);}}}
        };
        thumb.DragCompleted+=(_,_)=>{Changed();Draw();ShowHandles(item);};Sheet.Children.Add(thumb);ShowSensorConnectionHandle(item);
    }
    private static EquipmentItem Clone(EquipmentItem item) => JsonSerializer.Deserialize<EquipmentItem>(JsonSerializer.Serialize(item, ProjectServices.JsonOptions), ProjectServices.JsonOptions)!;
    private static void AddMenu(ContextMenu menu, string label, Action action) { var entry = new MenuItem { Header = label }; entry.Click += (_, _) => action(); menu.Items.Add(entry); }
    private ContextMenu CanvasMenu()
    {
        return PlanMenu();
    }
    private void Delete(EquipmentItem item){if(Current is not {} d)return;OverlaySelection.Delete(d.Project,new HashSet<Guid>{item.Id});_selection.Remove(item.Id);_selected=null;Changed();Draw();}
    private (EngineeringProject Project,Guid? ScenarioId) ExportState()
    {
        var project=DocumentExport.Snapshot(Current!.Project);var scenarioId=(ScenarioBox.SelectedItem as Scenario)?.Id;
        if(StepBox.SelectedItem is ControlStep step)
        {
            var scenario=project.Scenarios.FirstOrDefault(s=>s.Id==scenarioId);
            if(scenario is null){scenario=new Scenario {Name="Aktueller Schritt"};project.Scenarios.Add(scenario);scenarioId=scenario.Id;}
            foreach(var state in step.EquipmentStates)scenario.EquipmentStates[state.Key]=state.Value;foreach(var state in step.PipeStates)scenario.PipeStates[state.Key]=state.Value;foreach(var state in step.TextValues)scenario.TextValues[state.Key]=state.Value;foreach(var state in step.SensorValues)scenario.SensorValues[state.Key]=state.Value;SceneValues.MergeTanks(scenario.TankStates,step.TankStates);
        }
        return(project,scenarioId);
    }
    private void Symbol_Click(object sender, RoutedEventArgs e)
    {
        if (Current?.Image is null) { StatusText.Text = "Zuerst ein Zeichenblatt öffnen oder eine PDF importieren."; return; }
        CancelPlacement();
        var parts = ((string)((Button)sender).Tag).Split('|'); _pending = new EquipmentItem { Type = Enum.Parse<EquipmentType>(parts[0]), Variant = parts[1], Actuator = ActuatorType.Manual, Actuation = "Manuell", Tag = parts[1] + "-" + (Current.Project.Equipment.Count + 1), States = [new() { Name = "Normal" }, new() { Name = "Störung", Color = "#C62828" }] };if(_pending.Type==EquipmentType.Instrument)_pending.Color="#000000";if(_pending.Type==EquipmentType.MediaSource){_pending.Width=.085;_pending.Height=.07;} if(_pending.Type!=EquipmentType.Text&&_pending.Type!=EquipmentType.MediaSource)_pending.Height=_pending.Width*Sheet.Width/Sheet.Height;if(_pending.Type==EquipmentType.Legend){_pending.AutomaticLegend=true;_pending.Width=.25;_pending.Height=.22;_pending.Color="#303030";_pending.Opacity=1;MediaSettings.Normalize(Current.Project);_pending.LegendEntries=Current.Project.Media.Select(m=>new LegendEntry{MediumId=m.Id,Text=m.Name,Color=m.Color}).ToList();}
        if(_pending.Type==EquipmentType.Vessel){_pending.Width=.05;_pending.Height=.09;} if(Current.Project.BlankSheet is {} sheet){var factor=297/sheet.WidthMm;_pending.Width*=factor;_pending.Height*=_pending.Type is EquipmentType.Vessel or EquipmentType.Text or EquipmentType.MediaSource or EquipmentType.Legend?210/sheet.HeightMm:factor;} if(_pending.Type==EquipmentType.Drawing){_pending.Tag="Zeichnung-"+Guid.NewGuid().ToString("N")[..8];_pending.Color="#000000";_pending.Opacity=1;_pending.TextContent="Text";ApplyDrawingPreset(_pending,string.Join("|",parts));} _drawingPipe = false; PlacementHint.Text = parts[1] + " platzieren · Esc: Abbrechen"; Sheet.Focus();
    }
    private void Pipe_Click(object sender, RoutedEventArgs e) { if (Current?.Image is null) return; CancelPlacement();ClearSelection(); _drawingPipe = true; _drawingSignal=false; PlacementHint.Text = "Rohrpfad: klicken · Doppelklick/Enter: Ende · STRG+Ziehen: Abzweig";Draw(); }
    private void EditPipe(PipeSegment pipe)
    {
        var dialog=new PipeEditor(this,Current!.Project,pipe);
        if((PipeDialogResult?.Invoke(dialog)??dialog.ShowDialog())!=true||!dialog.TryApply(out _))return;
        Changed();Draw();
    }
    internal Func<PipeEditor,bool?>? PipeDialogResult {get;set;}
    private void Step_Changed(object sender,SelectionChangedEventArgs e) {if(Sheet is not null&&!_refreshingInspector){RefreshInspector();Draw();}}
    private void Sheet_Down(object sender,MouseButtonEventArgs e)
    {
        if(Current is not { } d||_panning||d.Image is null)return;Sheet.Focus();
        if(_selectExportWindow){BeginExportWindow(e.GetPosition(Sheet));e.Handled=true;return;}
        if(_drawingPipe){AddPipePoint(e.GetPosition(Sheet),e.ClickCount>=2);e.Handled=true;return;}
        if(_pending?.Type==EquipmentType.Drawing){BeginAnnotation(e.GetPosition(Sheet));e.Handled=true;return;}
        if(_pending is null){BeginMarquee(e.GetPosition(Sheet));e.Handled=true;return;}var p=e.GetPosition(Sheet);var item=_pending;item.Page=d.Page+1;item.X=Math.Clamp(p.X/Sheet.Width,0,1-item.Width);item.Y=Math.Clamp(p.Y/Sheet.Height,0,1-item.Height);CancelPlacement();if(!EditEquipment(item))return;d.Project.Equipment.Add(item);if(item.Type==EquipmentType.Instrument)SensorConnections.BindNearest(d.Project,item);Changed();Draw();
    }
    private void Sheet_Move(object sender,System.Windows.Input.MouseEventArgs e)
    {
        if(_exportRubber is not null){MoveExportWindow(e.GetPosition(Sheet));return;}
        if(_drawingDraft is not null){UpdateAnnotation(e.GetPosition(Sheet));return;}
        if(_marquee is not null){MoveMarquee(e.GetPosition(Sheet));return;}
        if(_drawingPipe){UpdatePipePreview(e.GetPosition(Sheet));return;}
        if(_pending is null)return;if(_ghost is null){_ghost=Glyph(_pending);_ghost.IsHitTestVisible=false;_ghost.Opacity=.4;Sheet.Children.Add(_ghost);}var p=e.GetPosition(Sheet);Canvas.SetLeft(_ghost,p.X);Canvas.SetTop(_ghost,p.Y);
    }
    private void Sheet_Leave(object sender, System.Windows.Input.MouseEventArgs e) { if (_ghost is not null) Sheet.Children.Remove(_ghost); _ghost = null; }
    private void CancelPlacement() { CancelAnnotation();CancelExportSelection(); _pending = null; _drawingPipe = false; _drawingSignal=false; ClearPipeDraft(); if (_ghost is not null) Sheet.Children.Remove(_ghost); _ghost = null; PlacementHint.Text = ""; }
    private void Sheet_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) { if(e.Key==Key.Enter&&_drawingPipe)FinishPipe(); if (e.Key == Key.Escape) CancelPlacement(); if (e.Key == Key.Delete && _selected is not null) Delete(_selected); else if(e.Key==Key.Delete&&_selectedPipe is not null)DeletePipe(_selectedPipe); }
    private void Viewport_Down(object sender, MouseButtonEventArgs e) { if (e.ChangedButton != MouseButton.Middle) return; _panning = true; _panStart = e.GetPosition(Viewport); _panX = Viewport.HorizontalOffset; _panY = Viewport.VerticalOffset; Viewport.CaptureMouse(); Viewport.Cursor = Cursors.Hand; e.Handled = true; }
    private void Viewport_Move(object sender, System.Windows.Input.MouseEventArgs e) => UpdatePan(e.GetPosition(Viewport),MiddleButtonPressed());
    private void UpdatePan(Point p,bool middlePressed)
    {
        if(!_panning)return;if(!middlePressed){StopPan();return;}
        var x=_panX-p.X+_panStart.X;var y=_panY-p.Y+_panStart.Y;
        ExtendFreeCanvas(ref x,ref y);Viewport.ScrollToHorizontalOffset(x);Viewport.ScrollToVerticalOffset(y);
    }
    private void Viewport_Up(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Middle) { StopPan(); e.Handled = true; } }
    private void StopPan() { _panning = false; Viewport.ReleaseMouseCapture(); Viewport.Cursor = Cursors.Arrow; }
    private void Viewport_Wheel(object sender, MouseWheelEventArgs e)
    {
        var anchor=e.GetPosition(Viewport);var page=e.GetPosition(Sheet);var nx=page.X/Sheet.Width;var ny=page.Y/Sheet.Height;
        Zoom(e.Delta>0?1.15:1/1.15);Viewport.UpdateLayout();
        var after=Sheet.TranslatePoint(new Point(nx*Sheet.Width,ny*Sheet.Height),Viewport);
        Viewport.ScrollToHorizontalOffset(Viewport.HorizontalOffset+after.X-anchor.X);Viewport.ScrollToVerticalOffset(Viewport.VerticalOffset+after.Y-anchor.Y);e.Handled=true;
    }
    private void Zoom(double factor) { if (Current is not { } d) return; d.Zoom = Math.Clamp(d.Zoom * factor, .15, 6); ++_renderVersion; Draw(); _zoomRenderTimer.Stop(); _zoomRenderTimer.Start(); }
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => Zoom(1.2);
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => Zoom(1 / 1.2);
    private void Fit() { if (Current is not { Image: not null } d) return; d.Zoom = Math.Max(.15, Math.Min((Viewport.ActualWidth - 25) / (Sheet.Width/d.Zoom), (Viewport.ActualHeight - 25) / (Sheet.Height/d.Zoom))); ++_renderVersion; Draw(); _zoomRenderTimer.Stop(); _zoomRenderTimer.Start(); }
    private void Fit_Click(object sender, RoutedEventArgs e) => Fit();
    private async void Previous_Click(object sender, RoutedEventArgs e) { if (Current is { Page: > 0 } d) { d.Page--; ClearSelection(); CancelPlacement(); await RenderPage(); } }
    private async void Next_Click(object sender, RoutedEventArgs e) { if (Current is { } d && d.Page + 1 < d.Raster.PageCount) { d.Page++; ClearSelection(); CancelPlacement(); await RenderPage(); } }
    private void Scenario_Changed(object sender, SelectionChangedEventArgs e) { if (Sheet is not null&&!_refreshingInspector&&Current is {} doc) {var id=(ScenarioBox.SelectedItem as Scenario)?.Id;SetPlanSelection(doc.Project,id,id is null?null:ScenarioSteps.First(doc.Project,id)?.Id);RefreshInspector();Draw();} }

    private static string[] VariantOptions(EquipmentItem x)=>x.Type switch{EquipmentType.IsolationValve=>["2-Wege","Eckform","3-Wege","Kugelarmatur","Kugelarmatur in Eckform","Dreiwegekugelhahn","Rückschlagarmatur","Rückschlagklappe"],EquipmentType.ControlValve=>["Regelventil"],EquipmentType.HeatExchanger=>["Kondensator","M-Form"],EquipmentType.PumpMotor=>["Pumpe","Motor"],EquipmentType.Instrument=>["Temperatur","Druck","Durchfluss","Feuchte","Füllstand","Leitfähigkeit","pH-Wert","Drehzahl","Position","Messstelle"],_=>[x.Variant]};
    internal Func<EditorDialog,bool?>? EquipmentDialogResult {get;set;}
    private bool EditEquipment(EquipmentItem item)
    {
        if(item.Type==EquipmentType.Drawing)return EditAnnotation(item);
        if(item.Type==EquipmentType.Legend)return new LegendEditor(this,item,Current!.Project).ShowDialog()==true;
        SourceMediaFields? mediaFields=null;
        var dialog = new EditorDialog(this, "Gerätedaten · " + item.Variant); dialog.Text("Tag", item.Tag); dialog.Text("Beschreibung", item.Description, true);
        SensorEditor? sensorEditor=item.Type==EquipmentType.Instrument?new SensorEditor(dialog,item.Sensor):null;
        dialog.Text("Temperatur (°C)",item.TemperatureCelsius?.ToString()??"");dialog.Text("Druck (bar)",item.PressureBar?.ToString()??"");dialog.Text("Durchflussgeschwindigkeit (m/s)",item.FlowVelocityMetersPerSecond?.ToString()??"");
        if(item.Type!=EquipmentType.MediaSource)foreach(var field in new[]{"Temperatur (°C)","Druck (bar)","Durchflussgeschwindigkeit (m/s)"})dialog.Hide(field);
        dialog.Validation=()=>{if(string.IsNullOrWhiteSpace(dialog.Value("Tag"))||Current!.Project.Equipment.Any(x=>x.Id!=item.Id&&x.Tag==dialog.Value("Tag")))return "Ein eindeutiger, nicht leerer Tag ist erforderlich.";if(SymbolCatalog.Valve(item)&&!SymbolCatalog.CheckValve(item)&&dialog.Value("Antriebsbauart")=="Antrieb auswählen")return "Bitte eine Antriebsbauart auswählen.";if(!ColorPicker.IsValid(dialog.Value("Farbe (#RRGGBB)"))||!ColorPicker.IsValid(dialog.Value("Hintergrundfarbe (#RRGGBB)")))return "Farben als #RRGGBB eingeben.";if(dialog.Number("Breite (px)",0)<=0||dialog.Number("Höhe (px)",0)<=0)return "Breite und Höhe in px müssen positiv sein.";if(sensorEditor?.Validate(dialog) is {} error)return error;if(item.Type==EquipmentType.MediaSource)return mediaFields?.Validate();if(item.Type==EquipmentType.Vessel){if(!Current!.Project.Media.Any(m=>m.Name==dialog.Value("Name des Mediums")&&MediaUi.Types.First(t=>t.Label==dialog.Value("Mediumtyp")).Value==m.Type))return "Ein zum Typ passendes Medium unter Projekt / Medien anlegen und auswählen.";}return null;};
        dialog.Choice("Funktion",item.Function,Current!.Project.Equipment.Select(x=>x.Function).Where(x=>x.Length>0).Distinct().Order().ToArray());dialog.Text("Einbauort (+)",item.InstallationLocation);
        var tankStep=item.Type==EquipmentType.Vessel?StepBox.SelectedItem as ControlStep:null;
        var assigned=tankStep?.TankStates.GetValueOrDefault(item.Id)?.MediumId??item.MediumId;
        var initialMedium=Current!.Project.Media.FirstOrDefault(m=>m.Id==assigned)??Current.Project.Media.FirstOrDefault(m=>m.Type==item.MediumType);
        if(item.Type==EquipmentType.MediaSource){mediaFields=new SourceMediaFields(dialog,Current.Project,item);dialog.SourceMedia=mediaFields;}else MediaUi.AddSelection(dialog,Current.Project,initialMedium?.Type??item.MediumType,initialMedium?.Name??item.MediaName);
        dialog.Text("Textinhalt",item.TextContent,true);dialog.Color("Hintergrundfarbe (#RRGGBB)",item.BackgroundColor);dialog.Check("Zustandstext anzeigen",item.ShowStateText);if(!SymbolCatalog.Colored(item))dialog.Hide("Zustandstext anzeigen");
        if(item.Type is not (EquipmentType.MediaSource or EquipmentType.Vessel)){dialog.Hide("Name des Mediums");dialog.Hide("Mediumtyp");}if(item.Type!=EquipmentType.Text)dialog.Hide("Textinhalt");
        dialog.Choice("Mechanischer Aufbau / Messgröße", item.Variant, VariantOptions(item));if(item.Type==EquipmentType.MediaSource)dialog.Hide("Mechanischer Aufbau / Messgröße");
        dialog.FixedChoice("Zugelassene Flussrichtung",item.CheckValveReversed?"Rechts → links":"Links → rechts","Links → rechts","Rechts → links");if(!SymbolCatalog.CheckValve(item))dialog.Hide("Zugelassene Flussrichtung");
        dialog.FixedChoice("Antriebsbauart",item.Actuator switch{ActuatorType.Manual=>"Handbetätigt",ActuatorType.ElectricMotor=>"Elektromotor",ActuatorType.Piston=>"Kolben",ActuatorType.Diaphragm=>"Membran",_=>"Antrieb auswählen"},"Antrieb auswählen","Handbetätigt","Elektromotor","Kolben","Membran");if(!SymbolCatalog.Valve(item)||SymbolCatalog.CheckValve(item))dialog.Hide("Antriebsbauart");
        dialog.Choice("Betätigung", item.Actuation, "Pneumatisch", "Elektrisch", "Hydraulisch", "Manuell");dialog.Hide("Betätigung"); dialog.Choice("Grundstellung", item.FailPosition, "NC", "NO", "AB", "Letzte Position", "Nicht zutreffend");
        dialog.Check("Rückmeldung geöffnet", item.FeedbackOpen); dialog.Text("Tag Rückmeldung geöffnet", item.FeedbackOpenTag); dialog.Check("Rückmeldung geschlossen", item.FeedbackClosed); dialog.Text("Tag Rückmeldung geschlossen", item.FeedbackClosedTag);
        dialog.Choice("Motorantrieb", item.MotorDrive, "DOL", "SS", "FU", "Nicht zutreffend"); dialog.Color("Farbe (#RRGGBB)", item.Color); dialog.Text("Deckkraft (0 bis 1)", item.Opacity.ToString()); dialog.Text("Breite (px)", (item.Width*Sheet.Width/Current!.Zoom).ToString()); dialog.Text("Höhe (px)", (item.Height*Sheet.Height/Current!.Zoom).ToString());
        if(item.Type is not (EquipmentType.PumpMotor or EquipmentType.Compressor or EquipmentType.Fan)) dialog.Hide("Motorantrieb");
        if(!SymbolCatalog.Valve(item)||SymbolCatalog.CheckValve(item)) foreach(var field in new[]{"Betätigung","Grundstellung","Rückmeldung geöffnet","Tag Rückmeldung geöffnet","Rückmeldung geschlossen","Tag Rückmeldung geschlossen"}) dialog.Hide(field);
        if ((EquipmentDialogResult?.Invoke(dialog)??dialog.ShowDialog()) != true) return false;
        if(dialog.Validation?.Invoke() is not null)return false;
        mediaFields?.Commit(Current!.Project);
        if(item.Type==EquipmentType.MediaSource){MediaDisplay.TryOptional(dialog.Value("Temperatur (°C)"),out var t);MediaDisplay.TryOptional(dialog.Value("Druck (bar)"),out var p);MediaDisplay.TryOptional(dialog.Value("Durchflussgeschwindigkeit (m/s)"),out var v);item.TemperatureCelsius=t;item.PressureBar=p;item.FlowVelocityMetersPerSecond=v;}

        item.Function=dialog.Value("Funktion");item.InstallationLocation=dialog.Value("Einbauort (+)");if(item.Type is EquipmentType.MediaSource or EquipmentType.Vessel){var chosen=mediaFields?.Selected??Current!.Project.Media.First(m=>m.Name==dialog.Value("Name des Mediums")&&m.Type==MediaUi.Types.First(t=>t.Label==dialog.Value("Mediumtyp")).Value);if(tankStep is not null){var assignment=tankStep.TankStates.GetValueOrDefault(item.Id)??new TankAssignment();assignment.MediumId=chosen.Id;tankStep.TankStates[item.Id]=assignment;}else{item.MediumId=chosen.Id;item.MediaName=chosen.Name;item.MediumType=chosen.Type;}}if(sensorEditor is not null){var oldSide=item.Sensor.ConnectionSide;item.Sensor=sensorEditor.Result(dialog);if(oldSide!=item.Sensor.ConnectionSide&&Current is {} sensorDoc)SensorConnections.BindNearest(sensorDoc.Project,item);}item.TextContent=dialog.Value("Textinhalt");item.ShowStateText=dialog.Checked("Zustandstext anzeigen");item.BackgroundColor=dialog.Value("Hintergrundfarbe (#RRGGBB)");
        item.Actuator=dialog.Value("Antriebsbauart") switch{"Handbetätigt"=>ActuatorType.Manual,"Elektromotor"=>ActuatorType.ElectricMotor,"Kolben"=>ActuatorType.Piston,"Membran"=>ActuatorType.Diaphragm,_=>item.Actuator};
        item.Actuation=item.Actuator switch{ActuatorType.Manual=>"Manuell",ActuatorType.ElectricMotor=>"Elektrisch",ActuatorType.Piston=>"Kolben",ActuatorType.Diaphragm=>"Membran",_=>item.Actuation};
        item.CheckValveReversed=dialog.Value("Zugelassene Flussrichtung")=="Rechts → links";item.Tag = dialog.Value("Tag"); item.Description = dialog.Value("Beschreibung"); item.Variant = dialog.Value("Mechanischer Aufbau / Messgröße"); item.FailPosition = dialog.Value("Grundstellung"); item.FeedbackOpen = dialog.Checked("Rückmeldung geöffnet"); item.FeedbackClosed = dialog.Checked("Rückmeldung geschlossen"); item.FeedbackOpenTag = dialog.Value("Tag Rückmeldung geöffnet"); item.FeedbackClosedTag = dialog.Value("Tag Rückmeldung geschlossen"); item.MotorDrive = dialog.Value("Motorantrieb"); item.Color = dialog.Value("Farbe (#RRGGBB)"); item.Opacity = Math.Clamp(dialog.Number("Deckkraft (0 bis 1)", .85), 0, 1); item.Width = Math.Clamp(dialog.Number("Breite (px)",90)/(Sheet.Width/Current!.Zoom),.001,Math.Max(.001,1-item.X)); MediaSettings.Synchronize(Current!.Project);item.Height = Math.Clamp(dialog.Number("Höhe (px)",90)/(Sheet.Height/Current!.Zoom),.001,Math.Max(.001,1-item.Y)); return true;
    }
    private void Project_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } d) return; var dialog = new EditorDialog(this, "Projekt"); dialog.Text("Projektname", d.Project.Name); dialog.Text("Planrevision", d.Project.Pdf.Revision); if (dialog.ShowDialog() != true) return; d.Project.Name = dialog.Value("Projektname"); d.Project.Pdf.Revision = dialog.Value("Planrevision"); Changed();
    }
    private void Equipment_Click(object sender,RoutedEventArgs e)=>InspectorTabs.SelectedIndex=0;
    private void Manage<T>(string title, List<T> items, Func<T,string> name, Action add, Action<T> edit, Action<T> delete)
    {
        var window = new Window { Owner = this, Title = title, Width = 530, Height = Math.Min(500,Math.Max(170,120+items.Count*30)), WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var root = new DockPanel { Margin = new Thickness(15) }; var bar = new StackPanel { Orientation = Orientation.Horizontal }; var list = new ListBox();
        var empty = new TextBlock { Text = "Noch keine Einträge. Über Hinzufügen beginnen.", Foreground = Brushes.SlateGray, Margin = new Thickness(8), IsHitTestVisible = false };
        void Refresh() { list.ItemsSource = null; list.ItemsSource = items.Select(x => new ListBoxItem { Content = name(x), Tag = x }).ToArray(); window.Height = Math.Min(500,Math.Max(170,120+items.Count*30)); empty.Visibility=items.Count==0?Visibility.Visible:Visibility.Collapsed; }
        foreach (var (label, action) in new (string,Action)[] { ("Hinzufügen", () => { add(); Refresh(); }), ("Bearbeiten", () => { if (list.SelectedItem is ListBoxItem selected) edit((T)selected.Tag); Refresh(); }), ("Löschen", () => { if (list.SelectedItem is ListBoxItem selected) delete((T)selected.Tag); Refresh(); }) }) { var button = new Button { Content = label }; button.Click += (_, _) => action(); bar.Children.Add(button); }
        list.MouseDoubleClick += (_, _) => { if (list.SelectedItem is ListBoxItem selected) edit((T)selected.Tag); Refresh(); }; DockPanel.SetDock(bar, Dock.Bottom); root.Children.Add(bar);var listArea=new Grid();listArea.Children.Add(list);listArea.Children.Add(empty);root.Children.Add(listArea); window.Content = root; Refresh(); window.ShowDialog();
    }
    private void Scenarios_Click(object sender,RoutedEventArgs e)=>InspectorTabs.SelectedIndex=1;
    private void Steps_Click(object sender,RoutedEventArgs e)=>InspectorTabs.SelectedIndex=1;
    private void Alarms_Click(object sender,RoutedEventArgs e)=>InspectorTabs.SelectedIndex=2;
    private void About_Click(object sender, RoutedEventArgs e) => new InformationWindow(this,false).ShowDialog();
    private void Help_Click(object sender, RoutedEventArgs e) => new InformationWindow(this,true).ShowDialog();
    private void Validate_Click(object sender, RoutedEventArgs e) { if (Current is not { } d) return; ProjectServices.ApplyApprovalStatus(d.Project); UpdateHeader(); MessageBox.Show(this, d.Project.Approval.BlockReason, "Revision prüfen"); }
    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        if(Current is {} current&&!ProjectServices.ValidateEngineering(current.Project,out var engineeringReason)){MessageBox.Show(this,engineeringReason,"Engineering prüfen");return;}
        if (Current is not { } d) return; if (!ProjectServices.ValidatePdf(d.Project, out var reason)) { MessageBox.Show(this, reason); return; } var dialog = new EditorDialog(this, "Freigeben"); dialog.Text("Freigegeben durch", ""); if (dialog.ShowDialog() != true || dialog.Value("Freigegeben durch") == "") return; d.Project.Approval = new Approval { IsApproved = true, ApprovedBy = dialog.Value("Freigegeben durch"), ApprovedAt = DateTimeOffset.Now, ApprovedPdfHash = ProjectServices.ReferenceHash(d.Project), BlockReason = "Freigegeben" }; d.Dirty = true; UpdateHeader();
    }
    private async void Export_Click(object sender, RoutedEventArgs e) { if (Current is not { } d) return; var picker = new Microsoft.Win32.OpenFolderDialog { Title = "Review-Paket exportieren" }; if (picker.ShowDialog(this) == true) { try { await ExportService.ExportAsync(d.Project, picker.FolderName); StatusText.Text = "Review-Paket exportiert."; } catch (Exception ex) { MessageBox.Show(this, ex.Message); } } }
    private async void DocumentExport_Click(object sender,RoutedEventArgs e)
    {
        if(Current is not {Image:not null} d)return;
        var mode=(string)((MenuItem)sender).Tag;var pdf=mode.EndsWith("pdf");var all=mode.StartsWith("all");string target;
        if(all&&!pdf){var folder=new Microsoft.Win32.OpenFolderDialog {Title="Alle Seiten als JPG exportieren"};if(folder.ShowDialog(this)!=true)return;target=folder.FolderName;}
        else {var picker=new Microsoft.Win32.SaveFileDialog {Filter=pdf?"PDF|*.pdf":"JPEG|*.jpg",FileName=d.Project.Name+(all?"":"-Seite-"+(d.Page+1))+(pdf?".pdf":".jpg")};if(picker.ShowDialog(this)!=true)return;target=picker.FileName;}
        try {var state=ExportState();StatusText.Text="Export wird erstellt …";await DocumentExport.ExportAsync(state.Project,target,pdf,all?null:d.Page,state.ScenarioId);StatusText.Text="Export abgeschlossen.";}
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Export");}
    }
}
