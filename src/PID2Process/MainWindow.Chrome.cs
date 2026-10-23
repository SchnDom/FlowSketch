using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
namespace PID2Process;
public partial class MainWindow
{
    private void InitializeWorkAreaBounds()
    {
        SourceInitialized += (_, _) => HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(WorkAreaHook);
        // WindowChrome also handles WM_GETMINMAXINFO. Reconcile after its processing,
        // and after DPI/display changes, using physical bounds from the actual monitor.
        StateChanged+=(_,_)=>QueueWorkArea();
        Loaded+=(_,_)=>QueueWorkArea();
    }

    private bool _workAreaQueued;
    private void QueueWorkArea()
    {
        if(_workAreaQueued)return;_workAreaQueued=true;
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,new Action(()=>
        {
            _workAreaQueued=false;if(WindowState!=WindowState.Maximized||!IsVisible)return;
            var hwnd=new WindowInteropHelper(this).Handle;var info=new MonitorInfo{Size=Marshal.SizeOf<MonitorInfo>()};
            if(!GetMonitorInfo(MonitorFromWindow(hwnd,2),ref info))return;
            var dpi=System.Windows.Media.VisualTreeHelper.GetDpi(this);
            MinWidth=Math.Min(900,(info.Work.Right-info.Work.Left)/dpi.DpiScaleX);
            MinHeight=Math.Min(600,(info.Work.Bottom-info.Work.Top)/dpi.DpiScaleY);
            if(GetWindowRect(hwnd,out var current)&&!current.Equals(info.Work))
                SetWindowPos(hwnd,IntPtr.Zero,info.Work.Left,info.Work.Top,info.Work.Right-info.Work.Left,info.Work.Bottom-info.Work.Top,0x0014);
        }));
    }

    private IntPtr WorkAreaHook(IntPtr hwnd,int message,IntPtr wParam,IntPtr lParam,ref bool handled)
    {
        if(message is 0x02E0 or 0x007E or 0x001A or 0x0047)QueueWorkArea(); // DPI, display, work area or monitor position changed.
        if(message!=0x0024)return IntPtr.Zero; // WM_GETMINMAXINFO, physical screen pixels (also at mixed DPI).
        var monitor=MonitorFromWindow(hwnd,2); // MONITOR_DEFAULTTONEAREST
        var info=new MonitorInfo{Size=Marshal.SizeOf<MonitorInfo>()};
        if(monitor==IntPtr.Zero||!GetMonitorInfo(monitor,ref info))return IntPtr.Zero;
        var bounds=Marshal.PtrToStructure<MinMaxInfo>(lParam);
        (bounds.MaxPosition,bounds.MaxSize)=WorkAreaBounds(info.Monitor,info.Work);
        bounds.MaxTrackSize.X=Math.Max(bounds.MaxTrackSize.X,bounds.MaxSize.X);
        bounds.MaxTrackSize.Y=Math.Max(bounds.MaxTrackSize.Y,bounds.MaxSize.Y);
        Marshal.StructureToPtr(bounds,lParam,false);
        handled=true;
        return IntPtr.Zero;
    }

    // Monitor-relative origin preserves taskbars on any edge and negative desktop coordinates.
    internal static (NativePoint Position,NativePoint Size) WorkAreaBounds(NativeRect monitor,NativeRect work)
        =>(new NativePoint{X=work.Left-monitor.Left,Y=work.Top-monitor.Top},
           new NativePoint{X=work.Right-work.Left,Y=work.Bottom-work.Top});

    [StructLayout(LayoutKind.Sequential)] internal struct NativePoint { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] internal struct NativeRect { public int Left,Top,Right,Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct MinMaxInfo { public NativePoint Reserved,MaxSize,MaxPosition,MinTrackSize,MaxTrackSize; }
    [StructLayout(LayoutKind.Sequential)] private struct MonitorInfo { public int Size; public NativeRect Monitor,Work; public uint Flags; }
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr hwnd,uint flags);
    [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(IntPtr hwnd,out NativeRect rect);
    [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(IntPtr hwnd,IntPtr after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] [return:MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor,ref MonitorInfo info);

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
        // Native bounds already exclude the taskbar; no guessed resize-border padding.
        ShellRoot.Margin=new Thickness(0);
        // WindowChrome otherwise reports HTTOP for the first six pixels, even when
        // maximized. With no resize border Windows receives HTCAPTION at the screen
        // edge and performs its native restore-and-drag gesture.
        if(System.Windows.Shell.WindowChrome.GetWindowChrome(this) is {} chrome)
            chrome.ResizeBorderThickness=new Thickness(WindowState==WindowState.Maximized?0:6);
        MaximizeButton.Content=WindowState==WindowState.Maximized?"\uE923":"\uE922";
        MaximizeButton.ToolTip=WindowState==WindowState.Maximized?"Wiederherstellen":"Maximieren";
    }
}
