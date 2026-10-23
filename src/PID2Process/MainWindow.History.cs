using System.Windows;
namespace PID2Process;
public partial class MainWindow
{
    private void Undo_Click(object sender,RoutedEventArgs e)=>RestoreHistory(false);
    private void Redo_Click(object sender,RoutedEventArgs e)=>RestoreHistory(true);
    private void UpdateHistoryCommands()
    {
        var editable=_previewProject is null;
        UndoButton.IsEnabled=editable&&Current?.History.CanUndo==true;
        RedoButton.IsEnabled=editable&&Current?.History.CanRedo==true;
    }
    private async void RestoreHistory(bool redo)
    {
        if(Current is not {} doc||_previewProject is not null||OwnedWindows.Cast<Window>().Any(x=>x.IsVisible))return;
        CancelCopyDrag();CancelPlacement();var restored=doc.History.Restore(redo);if(restored is null)return;
        var scenario=(ScenarioBox.SelectedItem as Scenario)?.Id;var step=(StepBox.SelectedItem as ControlStep)?.Id;
        ClearSelection();var changedPdf=doc.Project.Pdf.FilePath!=restored.Pdf.FilePath;doc.Project=restored;doc.Dirty=true;
        if(changedPdf&&restored.BlankSheet is null){doc.Raster=new();await doc.Raster.OpenAsync(restored.Pdf.FilePath);doc.Page=0;await RenderPage();}
        SetPlanSelection(restored,scenario,step);RefreshInspector();UpdateHeader();Draw();
    }
}
