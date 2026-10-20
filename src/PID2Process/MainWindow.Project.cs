using System.Windows;
using System.Windows.Controls;
namespace PID2Process;
public partial class MainWindow
{
    private void Demo_Click(object sender,RoutedEventArgs e)
    {
        try
        {
            OpenDemo();
        }
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Demo öffnen");}
    }
    internal static EngineeringProject LoadEmbeddedDemo()
    {
        using var stream=typeof(MainWindow).Assembly.GetManifestResourceStream("PID2Process.Demo.json")??throw new System.IO.InvalidDataException("Die eingebettete Demo fehlt.");
        var project=System.Text.Json.JsonSerializer.Deserialize<EngineeringProject>(stream,ProjectServices.JsonOptions)??throw new System.IO.InvalidDataException("Die Demo ist ungültig.");ProjectServices.Normalize(project);return project;
    }
    internal void OpenDemo(string? path=null)
    {
        var project=path is null?LoadEmbeddedDemo():ProjectServices.Load(path);AddDocument(new Document{Project=project,Image=WhiteBackground()});Draw();
        StatusText.Text="Demo-Arbeitskopie geöffnet. Szenario und Schritt rechts auswählen; Speichern vergibt einen eigenen Dateinamen.";
    }
    private async void ActuatorExcel_Click(object sender,RoutedEventArgs e)
    {
        if(Current is not {} d)return;
        var picker=new Microsoft.Win32.SaveFileDialog{Title="Szenarien und Aktoren exportieren",Filter="Excel-Arbeitsmappe (*.xlsx)|*.xlsx",DefaultExt=".xlsx",AddExtension=true,FileName="Aktoren.xlsx"};
        if(picker.ShowDialog(this)!=true)return;
        try {var snapshot=DocumentExport.Snapshot(d.Project);StatusText.Text="Excel-Export wird erstellt …";await Task.Run(()=>ActuatorExcelExport.Export(snapshot,picker.FileName));StatusText.Text="Excel-Tabelle exportiert.";}
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Excel-Export");}
    }
    private void Media_Click(object sender,RoutedEventArgs e)
    {
        if(Current is not {} d)return;var dialog=new MediaEditor(this,d.Project);if(dialog.ShowDialog()!=true)return;
        d.Project.Media=dialog.Working.Media;d.Project.MediaColors=dialog.Working.MediaColors;foreach(var legend in d.Project.Equipment.Where(x=>x.Type==EquipmentType.Legend))legend.AutomaticLegend=true;MediaSettings.Synchronize(d.Project);Changed();Draw();
    }
    private void Legend_Click(object sender,RoutedEventArgs e)
    {
        if(Current is not {} d)return;MediaSettings.Normalize(d.Project);
        var item=d.Project.Equipment.FirstOrDefault(x=>x.Type==EquipmentType.Legend&&x.Page==d.Page+1);
        if(item is null){Symbol_Click(new Button{Tag="Legend|Medienlegende"},e);return;}
        if(EditEquipment(item)){Changed();Draw();}
    }
    private void SaveAs_Click(object sender,RoutedEventArgs e)
    {
        if(Current is not {} d)return;var picker=new Microsoft.Win32.SaveFileDialog{Title="Speichern unter",Filter="FlowSketch-Projekt|*.pid2process.json",FileName=System.IO.Path.GetFileName(d.SavePath??d.Project.Name+".pid2process.json")};
        if(picker.ShowDialog(this)!=true)return;try{ProjectServices.Save(picker.FileName,d.Project);d.SavePath=picker.FileName;d.Dirty=false;Remember(d.SavePath);UpdateHeader();}catch(Exception ex){MessageBox.Show(this,ex.Message,"Speichern unter");}
    }
    private void CloseDocument_Click(object sender,RoutedEventArgs e)=>CloseDocument(DocumentTabs.SelectedItem as TabItem);
    private void CloseDocument(TabItem? tab)
    {
        if(tab?.Tag is not Document d)return;
        if(d.Dirty){var result=MessageBox.Show(this,"Änderungen vor dem Schließen speichern?","Dokument schließen",MessageBoxButton.YesNoCancel);if(result==MessageBoxResult.Cancel)return;
            if(result==MessageBoxResult.Yes){DocumentTabs.SelectedItem=tab;Save_Click(this,new RoutedEventArgs());if(d.Dirty)return;}}
        CancelPlacement();ClearSelection();DocumentTabs.Items.Remove(tab);if(DocumentTabs.Items.Count==0)CreateBlankDocument();
    }
    private void Exit_Click(object sender,RoutedEventArgs e)=>Close();
    private StackPanel TabHeader(Document d,TabItem tab)
    {
        var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(new TextBlock{Text=d.Project.Name+(d.Dirty?" *":""),VerticalAlignment=VerticalAlignment.Center});
        var close=new Button{Content="×",Padding=new Thickness(4,0,4,0),Margin=new Thickness(8,0,0,0),ToolTip="Dokument schließen"};close.Click+=(_,e)=>{e.Handled=true;CloseDocument(tab);};header.Children.Add(close);return header;
    }
}
