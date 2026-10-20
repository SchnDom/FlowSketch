namespace PID2Process;
public partial class App : System.Windows.Application
{
    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        if(e.Args.Contains("--brand-assets")){ShutdownMode=System.Windows.ShutdownMode.OnExplicitShutdown;IconAssetBuilder.Write(System.IO.Path.Combine(Environment.CurrentDirectory,"_media"));Shutdown();return;}
        if(e.Args.Contains("--model-test")){ShutdownMode=System.Windows.ShutdownMode.OnExplicitShutdown;Shutdown(await HeadlessSelfTest.Run());return;}
        if (e.Args.Contains("--self-test")) { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown; var result = await EditorSelfTest.Run(); Shutdown(result); return; }
        var window = new MainWindow(); window.CreateBlankDocument(); window.Show();
        foreach (var file in e.Args.Where(x => x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)))
        {
            try { await window.ImportPdf(System.IO.Path.GetFullPath(file)); }
            catch (Exception ex) { System.Windows.MessageBox.Show(window, ex.Message, "PDF öffnen"); }
        }
    }
}
