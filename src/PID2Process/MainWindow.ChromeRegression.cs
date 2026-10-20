using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace PID2Process;
public partial class MainWindow
{
    internal void CheckChromeAndHistory(Action<bool,string> check,string output)
    {
        var tab=DocumentTabs.SelectedItem;var doc=Current!;doc.History.Reset(doc.Project);var initialName=doc.Project.Name;
        doc.Project.Name="FlowSketch – Engineering-Demo";Changed();Draw();
        check(UndoButton.IsEnabled&&!RedoButton.IsEnabled&&ProjectTitle.Text.Contains("Engineering-Demo"),"Titelleiste zeigt Projektname und aktiven Undo-Befehl");
        UndoButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));check(Current!.Project.Name==initialName&&RedoButton.IsEnabled,"Titelleisten-Undo stellt tatsächlichen Projektstand wieder her");
        RedoButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));check(Current!.Project.Name.Contains("Engineering-Demo")&&!Current.Project.Approval.IsApproved,"Titelleisten-Redo stellt Änderung ohne Freigabe wieder her");
        var a=new EquipmentItem{Type=EquipmentType.IsolationValve,Variant="2-Wege",Tag="XV-101",X=.2,Y=.3,Width=.08,Height=.1};var b=new EquipmentItem{Type=EquipmentType.IsolationValve,Variant="3-Wege",Tag="XV-102",X=.4,Y=.3,Width=.08,Height=.1};Current.Project.Equipment.AddRange([a,b]);Changed();Draw();SelectObject(a.Id);SelectObject(b.Id,true);GroupSelection();ClearSelection();
        var frames=Sheet.Children.OfType<Rectangle>().Where(x=>Equals(x.Tag,"group-outline")).ToArray();
        check(frames.Length==1&&frames[0].StrokeDashArray.Count>0&&frames[0].Width>=(b.X+b.Width-a.X)*Sheet.Width&&frames[0].Stroke.ToString()=="#FFB7FF80","Gruppenrahmen hellgrün gestrichelt umfasst alle Mitglieder auch ohne Auswahl");
        check(frames.Length==1&&Sheet.Children.OfType<TextBlock>().Where(x=>x.Tag is Guid).All(text=>Sheet.Children.IndexOf(frames[0])<Sheet.Children.IndexOf(text)),"Gruppenrahmen liegt hinter sämtlichen Objektbeschriftungen");
        RestoreHistory(false);check(Current.Project.Equipment.All(x=>x.GroupId is null),"Undo löst gerade erstellte Gruppe vollständig auf");RestoreHistory(true);check(Current.Project.Equipment.Select(x=>x.GroupId).Distinct().Count()==1&&Current.Project.Equipment[0].GroupId is not null,"Redo erhält Gruppenzuordnung und Objekt-IDs");
        CreateBlankDocument();check(!UndoButton.IsEnabled&&!RedoButton.IsEnabled,"Neuer Tab erbt keinen Verlauf eines anderen Dokuments");DocumentTabs.SelectedItem=tab;check(UndoButton.IsEnabled,"Tabwechsel aktiviert den eigenen Dokumentverlauf");
        var chrome=System.Windows.Shell.WindowChrome.GetWindowChrome(this);check(chrome is {CaptionHeight:44,UseAeroCaptionButtons:false}&&System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(MainMenu),"Eigene Titelleiste behält Resize-/Caption-Zonen mit bedienbarem Menü");
        foreach(var monitor in new[]{new NativeRect{Left=0,Top=0,Right=1920,Bottom=1080},new NativeRect{Left=-2560,Top=-300,Right=0,Bottom=1140}})
        foreach(var edge in new[]{"unten","oben","links","rechts"})
        {
            var work=monitor;
            if(edge=="unten")work.Bottom-=48;else if(edge=="oben")work.Top+=48;else if(edge=="links")work.Left+=48;else work.Right-=48;
            var bounds=WorkAreaBounds(monitor,work);
            check(bounds.Position.X+monitor.Left==work.Left&&bounds.Position.Y+monitor.Top==work.Top&&bounds.Size.X==work.Right-work.Left&&bounds.Size.Y==work.Bottom-work.Top,"Maximierter Rahmen berücksichtigt Taskleiste "+edge+" auf Monitor "+monitor.Left);
        }
        check(MainMenu.Items.OfType<MenuItem>().Select(x=>x.Header?.ToString()).SequenceEqual(new[]{"_Datei","_Projekt","_Hilfe"}),"Titelleistenmenüs entsprechen Logo, Datei, Projekt, Hilfe und Schnellaktionen");
        check(QuickExportMenu().Items.OfType<MenuItem>().Count()==6,"Export-Schnellaktion erhält Excel, Review sowie JPG-/PDF-Export");
        var root=(FrameworkElement)Content;Fit();root.UpdateLayout();ClearSelection();
        var menu=PlanMenu();menu.ApplyTemplate();menu.Measure(new Size(320,double.PositiveInfinity));menu.Arrange(new Rect(menu.DesiredSize));menu.UpdateLayout();
        IEnumerable<DependencyObject> Descendants(DependencyObject parent){for(var i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);yield return child;foreach(var grandchild in Descendants(child))yield return grandchild;}}
        check(!Descendants(menu).OfType<ScrollViewer>().Any()&&menu.ActualHeight>200,"Kontextmenü zeigt vollständigen Inhalt ohne ScrollViewer");
        var fileMenu=(MenuItem)MainMenu.Items[0];fileMenu.ApplyTemplate();var popup=(System.Windows.Controls.Primitives.Popup)fileMenu.Template.FindName("PART_Popup",fileMenu);check(!Descendants(popup.Child).OfType<ScrollViewer>().Any(),"Auch Menü-Unterlisten besitzen keinen ScrollViewer");
        check(MainMenu.Items.OfType<MenuItem>().All(item=>item.Background is SolidColorBrush brush&&brush.Color.A==0)&&menu.Items.OfType<MenuItem>().All(item=>item.Background is SolidColorBrush brush&&brush.Color.A==0),"Titel- und Kontextmenüs haben keine dauerhaften Buttonflächen");
        Save(menu,"context-menu-"+BuildInfo.Version+".png");Save(root,"desktop-chrome-"+BuildInfo.Version+".png");
        void Save(FrameworkElement visual,string name){var bitmap=new RenderTargetBitmap((int)Math.Ceiling(visual.ActualWidth),(int)Math.Ceiling(visual.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var file=System.IO.File.Create(System.IO.Path.Combine(output,name));encoder.Save(file);}
    }
}
