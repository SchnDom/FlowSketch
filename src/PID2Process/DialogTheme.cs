using System.Windows;
using System.Windows.Controls;
namespace PID2Process;
internal static class DialogTheme
{
    internal static void Apply(Window window)
    {
        window.Background=PageVisual.Color("#20262E");window.Foreground=PageVisual.Color("#E7EDF5");
        window.FontFamily=new System.Windows.Media.FontFamily("Segoe UI");window.FontSize=13;window.Icon=InformationWindow.IconImage();
        window.UseLayoutRounding=true;
        window.SourceInitialized+=(_,_)=>
        {
            var handle=new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var enabled=1;DwmSetWindowAttribute(handle,20,ref enabled,4);
            var background=0x002E2620;DwmSetWindowAttribute(handle,35,ref background,4);
            var foreground=0x00F5EDE7;DwmSetWindowAttribute(handle,36,ref foreground,4);
        };
    }
    [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint window,int attribute,ref int value,int size);
    internal static GroupBox Group(string name,UIElement content)=>new(){Header=name,Content=content,Margin=new Thickness(0,0,0,10),Padding=new Thickness(12)};
}
