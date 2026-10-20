using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

internal static class BrandingSelfTest
{
    internal static void Run(string output,Action<bool,string> check)
    {
        var company=InformationWindow.ResourceText("CompanyInfo.txt");
        check(company==File.ReadAllText(Path.Combine(Environment.CurrentDirectory,"_media","companyInfo.txt")),"Firmeninformationen unverändert eingebettet");
        var logo=new BitmapImage(new Uri("pack://application:,,,/Assets/logo.png"));check(logo.PixelWidth==2803&&logo.PixelHeight==776,"Neue Firmenwortmarke vollständig und unverzerrt eingebettet");
        check(InformationWindow.IconImage().PixelWidth>0,"Eingebettetes Fenstericon lesbar");
        foreach(var help in new[]{false,true})
        {
            var window=new InformationWindow(null,help);
            var root=(DockPanel)window.Content;var tabs=root.Children.OfType<TabControl>().Single();
            check(!window.IsVisible&&tabs.Items.Count==(help?3:2),"Hilfe/Über uns ohne Bildschirmanzeige mit Firmen- und Kontakttabs");
            check(tabs.Items.OfType<TabItem>().Any(t=>t.Content is TextBox text&&text.Text==company&&text.IsReadOnly),"Firmenangaben vollständig und nur lesbar");
            if(help){check(tabs.Items.OfType<TabItem>().Any(t=>t.Content is HelpView)&&HelpView.GuideText.Contains(BuildInfo.Version)&&HelpView.Chapters().Count>=20,"Bebilderte Bedienhilfe enthält aktuelle Version und vollständige Kapitel");foreach(var fileName in new[]{"blank-editor.png","free-frame-editor.png","batch-properties.png","signal-line.png","valve-colors.png","scenario-editor.png","scenario-editor-steps.png","scenario-editor-alarms.png","media-filter.png","sensor-alarms.png","drawing-examples.png","blank-export.png","legend.png","legend-editor.png"})check(new BitmapImage(new Uri("pack://application:,,,/Help/"+fileName)).PixelWidth>0,"Hilfebild eingebettet: "+fileName);}
            root.Width=720;root.Height=570;root.Measure(new Size(720,570));root.Arrange(new Rect(0,0,720,570));root.UpdateLayout();
            var image=new RenderTargetBitmap(720,570,96,96,PixelFormats.Pbgra32);image.Render(root);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));
            using var file=File.Create(Path.Combine(output,help?"help-information.png":"about-information.png"));encoder.Save(file);
        }
    }
}
