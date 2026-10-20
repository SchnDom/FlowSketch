using System.Windows;
namespace PID2Process;
public partial class MainWindow
{
    private void TitleBar_SizeChanged(object sender,SizeChangedEventArgs e)
    {
        ProjectTitle.MaxWidth=Math.Max(0,TitleBar.ActualWidth-2*Math.Max(TitleActions.ActualWidth,CaptionActions.ActualWidth)-20);
    }
    private void Minimize_Click(object sender,RoutedEventArgs e)=>SystemCommands.MinimizeWindow(this);
    private void Maximize_Click(object sender,RoutedEventArgs e)
    {
        if(WindowState==WindowState.Maximized)SystemCommands.RestoreWindow(this);else SystemCommands.MaximizeWindow(this);
    }
    private void CloseWindow_Click(object sender,RoutedEventArgs e)=>SystemCommands.CloseWindow(this);
    private void Logo_Click(object sender,RoutedEventArgs e)
    {
        SystemCommands.ShowSystemMenu(this,TitleBar.PointToScreen(new Point(0,TitleBar.ActualHeight)));
    }
    private void UpdateChrome()
    {
        // Keep content within the work area when Windows maximizes the custom frame.
        ShellRoot.Margin=WindowState==WindowState.Maximized?SystemParameters.WindowResizeBorderThickness:new Thickness(0);
        MaximizeButton.Content=WindowState==WindowState.Maximized?"\uE923":"\uE922";
        MaximizeButton.ToolTip=WindowState==WindowState.Maximized?"Wiederherstellen":"Maximieren";
    }
}
