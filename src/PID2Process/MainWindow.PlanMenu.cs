using System.Windows;
using System.Windows.Controls;
namespace PID2Process;
public partial class MainWindow
{
    private void EditCurrentObject()
    {
        if(Current is not {} doc||_selection.Count!=1)return;
        var id=_selection.Single();
        if(doc.Project.Equipment.FirstOrDefault(x=>x.Id==id) is {} item){if(EditEquipment(item)){Changed();Draw();}}
        else if(doc.Project.Pipes.FirstOrDefault(x=>x.Id==id) is {} pipe)EditPipe(pipe);
        else if(doc.Project.SignalLines.FirstOrDefault(x=>x.Id==id) is {} signal)EditSignal(signal);
    }
    private ContextMenu PlanMenu()
    {
        var menu=new ContextMenu();
        if(_selection.Count>0)
        {
            AddMenu(menu,_selection.Count==1?"Objekt bearbeiten":"Objekte bearbeiten",_selection.Count==1?EditCurrentObject:EditSelection);
            if(_selection.Count>1){AddMenu(menu,"Gruppieren",()=>GroupSelection());AddMenu(menu,"Gruppierung aufheben",()=>GroupSelection(true));}
            menu.Items.Add(new Separator());ClipboardMenu(menu);AddMenu(menu,"Löschen",DeleteSelection);menu.Items.Add(new Separator());LineTools(menu);
        }
        else
        {
            LineTools(menu);menu.Items.Add(new Separator());ClipboardMenu(menu);AddMenu(menu,"Löschen",DeleteSelection);((MenuItem)menu.Items[^1]).IsEnabled=false;
            menu.Items.Add(new Separator());AddMenu(menu,"Gemeinsame Eigenschaften",EditSelection);((MenuItem)menu.Items[^1]).IsEnabled=false;
        }
        return menu;
    }
    private void LineTools(ContextMenu menu)
    {
        AddMenu(menu,"Rohrleitung einfügen",()=>Pipe_Click(this,new RoutedEventArgs()));
        AddMenu(menu,"Wirklinie einfügen",()=>Signal_Click(this,new RoutedEventArgs()));
    }
}
