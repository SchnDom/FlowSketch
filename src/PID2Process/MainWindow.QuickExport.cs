using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
namespace PID2Process;
public partial class MainWindow
{
    private ContextMenu QuickExportMenu()
    {
        var menu=new ContextMenu();
        AddMenu(menu,"Aktoren als Excel-Tabelle…",()=>ActuatorExcel_Click(this,new RoutedEventArgs()));
        AddMenu(menu,"Review-Paket…",()=>Export_Click(this,new RoutedEventArgs()));
        menu.Items.Add(new Separator());
        foreach(var (label,tag) in new[]{("Gesamtexport als JPG…","all-jpg"),("Gesamtexport als PDF…","all-pdf"),("Einzelexport als JPG…","page-jpg"),("Einzelexport als PDF…","page-pdf")})
            AddMenu(menu,label,()=>DocumentExport_Click(new MenuItem{Tag=tag},new RoutedEventArgs()));
        return menu;
    }
    private void QuickExport_Click(object sender,RoutedEventArgs e)
    {
        var menu=QuickExportMenu();menu.PlacementTarget=QuickExportButton;menu.Placement=PlacementMode.Bottom;menu.IsOpen=true;
    }
}
