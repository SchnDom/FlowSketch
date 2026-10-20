using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace PID2Process;

internal static class EditorSelfTest
{
    internal static async Task<int> Run()
    {
        var report = new List<string>(); var output = Path.Combine(Environment.CurrentDirectory, "test-results"); Directory.CreateDirectory(output);
        MainWindow? window = null;
        void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException(name); report.Add("PASS: " + name); }
        try
        {
            var testPdf = Path.Combine(output, "two-pages.pdf"); MakePdf(testPdf);
            var renderer = new PdfRasterizer(); await renderer.OpenAsync(testPdf); Check(renderer.PageCount == 2, "Echte PDF-Seitenzahl: 2");
            var first = await renderer.RenderAsync(0); var second = await renderer.RenderAsync(1); Check(first.PixelWidth == 2000 && second.PixelWidth == 2000, "Beide Seiten als Rasterbilder gerendert");
            window = new MainWindow { WindowState = WindowState.Normal, Width = 1400, Height = 900 }; window.Show(); await window.ImportPdf(testPdf);
            var tabs = (TabControl)window.FindName("DocumentTabs"); var original = window.ActiveProject!;
            foreach (var pdf in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "_PIDs"), "*.pdf")) { await window.ImportPdf(pdf); Check(window.ActiveProject!.Pdf.PageCount > 0, "Test-PID importiert: " + Path.GetFileName(pdf)); }
            Check(tabs.Items.Count == 3, "Drei unabhängige PDF-Tabs"); tabs.SelectedIndex = 0; await Task.Delay(150); Check(ReferenceEquals(window.ActiveProject, original), "Tabwechsel erhält Projektzuordnung");
            var actualPdf=Directory.GetFiles(Path.Combine(Environment.CurrentDirectory,"_PIDs"),"*.pdf").First();var detailRenderer=new PdfRasterizer();await detailRenderer.OpenAsync(actualPdf);var detailed=await detailRenderer.RenderAsync(0,6000);Check(detailed.PixelWidth==6000,"PDF-Details direkt aus der Originaldatei mit 6000 Pixeln neu gerendert");
            var detailEncoder=new PngBitmapEncoder();detailEncoder.Frames.Add(BitmapFrame.Create(new CroppedBitmap(detailed,new Int32Rect(2400,1200,1500,1000))));using(var detailFile=File.Create(Path.Combine(output,"pdf-detail-6000.png")))detailEncoder.Save(detailFile);
            var sheet = (Canvas)window.FindName("Sheet"); Check(sheet.Children.OfType<System.Windows.Controls.Image>().Any(x => x.Source is BitmapSource), "Zeichenfläche enthält Rasterbild ohne PDF-Reader");
            void Invoke(string name) => typeof(MainWindow).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(window, [window, new RoutedEventArgs()]);
            Invoke("Next_Click"); await Task.Delay(200); Check(((TextBlock)window.FindName("PageLabel")).Text == "2 / 2", "Seitennavigation vorwärts"); Invoke("Previous_Click"); await Task.Delay(200); Check(((TextBlock)window.FindName("PageLabel")).Text == "1 / 2", "Seitennavigation zurück");
            var item = new EquipmentItem { Type = EquipmentType.IsolationValve, Tag = "XV-101", Variant = "3-Wege", Actuation = "Pneumatisch", FailPosition = "NC", FeedbackOpen = true, FeedbackClosed = true, MotorDrive = "FU", X = .3, Y = .4, Width = .08, Height = .06, Opacity = .6 }; original.Equipment.Add(item);
            typeof(MainWindow).GetMethod("Zoom",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[3.0]);await Task.Delay(250);await (Task)typeof(MainWindow).GetMethod("RenderPage",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[true])!;
            var zoomCanvas=(Canvas)window.FindName("Sheet");var zoomImage=zoomCanvas.Children.OfType<System.Windows.Controls.Image>().First();Check(((BitmapSource)zoomImage.Source).PixelWidth>2000&&Math.Abs(zoomCanvas.Width-3900)<1&&item.X==.3&&item.Width==.08,"Zoom erhöht Rasterauflösung, ohne Seitengröße oder Overlay-Koordinaten zu verändern");
            var saved = Path.Combine(output, "roundtrip.pid2process.json"); ProjectServices.Save(saved, original); var loaded = ProjectServices.Load(saved); Check(loaded.Equipment[0].FeedbackOpen && loaded.Equipment[0].MotorDrive == "FU" && loaded.Equipment[0].Width == .08, "Gerätedaten und Grafik persistent");
            await ProcessSelfTest.Run(window,original,item,output,Check);
            var editTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) }; bool deviceDialog = false;
            editTimer.Tick += (_, _) => { editTimer.Stop(); var d = System.Windows.Application.Current.Windows.OfType<EditorDialog>().First(); deviceDialog = d.Title.Contains("Gerätedaten"); Capture(d, Path.Combine(output,"device-dialog.png")); d.Close(); };
            editTimer.Start(); typeof(MainWindow).GetMethod("EditEquipment",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[item]); Check(deviceDialog, "Vollständiger Gerätedaten-Dialog verfügbar");
            void DeviceFields(EquipmentItem device, string visibleKey, string hiddenKey)
            {
                bool visible=false,hidden=false; var timer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(100) };
                timer.Tick+=(_,_)=>{timer.Stop();var dialog=System.Windows.Application.Current.Windows.OfType<EditorDialog>().Single();var controls=Descendants(dialog).OfType<FrameworkElement>().ToArray();visible=controls.Any(c=>Equals(c.Tag,visibleKey)&&c.IsVisible);hidden=!controls.Any(c=>Equals(c.Tag,hiddenKey)&&c.IsVisible);Capture(dialog,Path.Combine(output,device.Type+"-dialog.png"));dialog.Close();};
                timer.Start();typeof(MainWindow).GetMethod("EditEquipment",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[device]);Check(visible&&hidden,"Passende Felder ohne unnötigen Dialograum: "+device.Type);
            }
            DeviceFields(item,"Grundstellung","Motorantrieb");
            DeviceFields(new EquipmentItem {Type=EquipmentType.PumpMotor,Variant="Motor",Tag="M-101"},"Motorantrieb","Nicht vorhanden");
            DeviceFields(new EquipmentItem {Type=EquipmentType.Instrument,Variant="Druck",Tag="P-101"},"Farbe (#RRGGBB)","Grundstellung");
            var priorPage=((TextBlock)window.FindName("PageLabel")).Text;
            var rightDown=new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice,0,System.Windows.Input.MouseButton.Right) { RoutedEvent=System.Windows.Input.Mouse.PreviewMouseDownEvent };
            ((ScrollViewer)window.FindName("Viewport")).RaiseEvent(rightDown);Check(!(bool)typeof(MainWindow).GetField("_panning",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(window)!,"Rechte Taste aktiviert niemals Pan");
            typeof(MainWindow).GetMethod("Zoom",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[4.0]);
            var viewport=(ScrollViewer)window.FindName("Viewport");window.Activate();viewport.Focus();await Task.Delay(100);window.UpdateLayout();viewport.ScrollToHorizontalOffset(0);viewport.ScrollToVerticalOffset(0);window.UpdateLayout();
            // Routed WPF events don't change the physical button state. Model the
            // held button while exercising actual capture and scrolling controls.
            window.MiddleButtonPressed=()=>true;
            viewport.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice,0,System.Windows.Input.MouseButton.Middle){RoutedEvent=System.Windows.Input.Mouse.PreviewMouseDownEvent});
            Check((bool)typeof(MainWindow).GetField("_panning",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(window)!&&viewport.IsMouseCaptured,"Mittlere Taste erfasst die Zeichenansicht");
            var panStart=(Point)typeof(MainWindow).GetField("_panStart",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(window)!;
            typeof(MainWindow).GetMethod("UpdatePan",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[new Point(panStart.X-60,panStart.Y-40),true]);window.UpdateLayout();Check(viewport.HorizontalOffset>=59&&viewport.VerticalOffset>=39,"Greifen verschiebt die native Zeichenansicht um den Mausweg");
            window.MiddleButtonPressed=()=>false;typeof(MainWindow).GetMethod("UpdatePan",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[panStart,false]);Check(!viewport.IsMouseCaptured&&!(bool)typeof(MainWindow).GetField("_panning",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(window)!,"Loslassen beendet Pan vor einer weiteren Rechtsklickbewegung");
            typeof(MainWindow).GetMethod("Draw",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[]);
            typeof(MainWindow).GetMethod("ShowHandles",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[item]); var thumb=sheet.Children.OfType<System.Windows.Controls.Primitives.Thumb>().Single(); var previousWidth=item.Width;
            thumb.RaiseEvent(new System.Windows.Controls.Primitives.DragDeltaEventArgs(30,20){RoutedEvent=System.Windows.Controls.Primitives.Thumb.DragDeltaEvent}); thumb.RaiseEvent(new System.Windows.Controls.Primitives.DragCompletedEventArgs(30,20,false){RoutedEvent=System.Windows.Controls.Primitives.Thumb.DragCompletedEvent}); Check(item.Width>previousWidth,"Größenänderung über tatsächlichen Resize-Handler");
            await ExportService.ExportAsync(original,Path.Combine(output,"review")); Check(File.Exists(Path.Combine(output,"review","page-2.png"))&&ProjectServices.Load(Path.Combine(output,"review","project-data.json")).ControlSteps.Any(s=>s.Name=="Füllen starten")&&!File.ReadAllText(Path.Combine(output,"review","viewer.html")).Contains("__DATA__"),"Offline-Review mit Rasterseiten, Schritten und Gerätedaten exportiert");
            var sourceHash=ProjectServices.HashFile(testPdf);var allPdf=Path.Combine(output,"all-pages.pdf");var singlePdf=Path.Combine(output,"single-page.pdf");
            await DocumentExport.ExportAsync(original,allPdf,true,null,null);await DocumentExport.ExportAsync(original,singlePdf,true,1,null);
            var exported=new PdfRasterizer();await exported.OpenAsync(allPdf);Check(exported.PageCount==2,"Gesamtexport PDF enthält beide Originalseiten");await exported.OpenAsync(singlePdf);Check(exported.PageCount==1,"Einzelexport PDF enthält nur aktuelle Seite");Check(ProjectServices.HashFile(testPdf)==sourceHash,"PDF-Export verändert die Referenzdatei nicht");
            var jpgFolder=Path.Combine(output,"jpg-all");await DocumentExport.ExportAsync(original,jpgFolder,false,null,null);Check(Directory.GetFiles(jpgFolder,"*.jpg").Length==2,"JPG-Gesamtexport erstellt ein Bild je Seite");
            var singleJpg=Path.Combine(output,"single-page.jpg");await DocumentExport.ExportAsync(original,singleJpg,false,1,null);using(var jpeg=File.OpenRead(singleJpg)){var bitmap=BitmapFrame.Create(jpeg,BitmapCreateOptions.None,BitmapCacheOption.OnLoad);Check(bitmap.PixelWidth>4000&&bitmap.PixelHeight>bitmap.PixelWidth,"JPG-Einzelexport enthält hochauflösend nur die gewählte Hochformatseite");}
            var actualProject=DocumentExport.Snapshot(window.ActiveProject!);actualProject.Pdf.FilePath=actualPdf;actualProject.Pdf.FileName=Path.GetFileName(actualPdf);actualProject.Equipment[0].Page=1;
            await DocumentExport.ExportAsync(actualProject,Path.Combine(output,"test-pid-overlay.pdf"),true,0,null);await exported.OpenAsync(Path.Combine(output,"test-pid-overlay.pdf"));var renderedExport=await exported.RenderAsync(0,4000);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(renderedExport));using(var file=File.Create(Path.Combine(output,"pdf-export-rendered.png")))png.Save(file);
            original.Pipes.Add(new PipeSegment {Tag="L-101",Page=1});var pipe=original.Pipes.Last();original.ControlSteps[0].PipeStates[pipe.Id]="Befüllt";((ComboBox)window.FindName("StepBox")).SelectedItem=original.ControlSteps[0];typeof(MainWindow).GetMethod("Draw",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,[]);var graphics=(Dictionary<Guid,System.Windows.Shapes.Polyline>)typeof(MainWindow).GetField("_pipeGraphics",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(window)!;Check(!graphics[pipe.Id].HasAnimatedProperties,"Unverbundenes manuell befülltes Rohr behauptet keinen Durchfluss");
            original.Approval.IsApproved = true; original.Approval.ApprovedPdfHash = original.Pdf.Sha256; File.AppendAllText(testPdf, "\n%revision"); Check(!ProjectServices.ValidatePdf(original,out _), "PDF-Änderung wird erkannt");
            tabs.SelectedIndex=1;await Task.Delay(250);Capture(window, Path.Combine(output,"editor.png")); window.ClearTestDirtyFlags(); window.Close(); File.WriteAllLines(Path.Combine(output,"results.txt"),report); return 0;
        }
        catch (Exception ex) { report.Add("FAIL: " + ex); if (window is not null) { window.ClearTestDirtyFlags(); window.Close(); } File.WriteAllLines(Path.Combine(output,"results.txt"),report); return 1; }
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++) { var child=VisualTreeHelper.GetChild(root,i); yield return child;foreach(var descendant in Descendants(child)) yield return descendant; }
    }
    private static void Capture(Window window, string path)
    {
        window.UpdateLayout(); var image = new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32); image.Render(window); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image)); using var file = File.Create(path); encoder.Save(file);
    }
    internal static void MakePdf(string path)
    {
        string[] objects = ["<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R 4 0 R] /Count 2 >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 800 600] /Contents 5 0 R >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 600 800] /Contents 6 0 R >>", "<< /Length 4 >>\nstream\nq\nQ\nendstream", "<< /Length 4 >>\nstream\nq\nQ\nendstream"];
        var data = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>(); for(var i=0;i<objects.Length;i++) { offsets.Add(data.Length); data.Append($"{i+1} 0 obj\n{objects[i]}\nendobj\n"); } var xref = data.Length; data.Append($"xref\n0 7\n0000000000 65535 f \n"); foreach(var offset in offsets) data.Append($"{offset:0000000000} 00000 n \n"); data.Append($"trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n"); File.WriteAllText(path,data.ToString(),Encoding.ASCII);
    }
}
