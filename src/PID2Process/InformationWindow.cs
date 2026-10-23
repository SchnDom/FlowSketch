using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

// Project license and third-party notices are embedded verbatim.
internal sealed class InformationWindow : Window
{
    internal static string ResourceText(string name)
    {
        using var stream=typeof(InformationWindow).Assembly.GetManifestResourceStream("PID2Process."+name)
            ?? throw new InvalidOperationException("Informationsdatei fehlt: "+name);
        using var reader=new StreamReader(stream);
        return reader.ReadToEnd();
    }
    internal static BitmapSource IconImage(){var decoder=BitmapDecoder.Create(new Uri("pack://application:,,,/Assets/favicon.ico"),BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);return decoder.Frames.OrderByDescending(x=>x.PixelWidth).First();}
    internal InformationWindow(Window? owner,bool help)
    {
        Owner=owner;Title=help?"FlowSketch – Hilfe":"FlowSketch – Über uns";Icon=IconImage();
        Width=760;Height=650;MaxHeight=Math.Max(300,SystemParameters.WorkArea.Height-60);
        WindowStartupLocation=owner is null?WindowStartupLocation.CenterScreen:WindowStartupLocation.CenterOwner;
        DialogTheme.Apply(this);var root=new DockPanel{Margin=new Thickness(18),Background=PageVisual.Color("#20262E")};
        var header=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,16)};
        header.Children.Add(new Image{Source=IconImage(),Width=64,Height=64,Margin=new Thickness(0,0,18,0),Stretch=Stretch.Uniform});
        var heading=new StackPanel{VerticalAlignment=VerticalAlignment.Center};
        heading.Children.Add(new TextBlock{Text=BuildInfo.Name,FontSize=24,FontWeight=FontWeights.SemiBold});
        heading.Children.Add(new TextBlock{Text="Version "+BuildInfo.Version+" · Dominik Schneeberger · MIT-Lizenz",Margin=new Thickness(0,6,0,0)});
        heading.Children.Add(new TextBlock{Text="Engineering-Overlays auf unveränderten R&I-PDFs",Margin=new Thickness(0,6,0,0)});
        header.Children.Add(heading);DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        var close=new Button{Content="Schließen",IsCancel=true,IsDefault=true,HorizontalAlignment=HorizontalAlignment.Right,MinWidth=95,Margin=new Thickness(0,12,0,0)};
        close.Click+=(_,_)=>Close();DockPanel.SetDock(close,Dock.Bottom);root.Children.Add(close);
        var tabs=new TabControl();
        if(help)tabs.Items.Add(new TabItem{Header="Bedienhilfe",Content=new HelpView()});
        tabs.Items.Add(TextTab("MIT-Lizenz",ResourceText("License.txt")));
        tabs.Items.Add(TextTab("Drittanbieter",ResourceText("ThirdParty.txt")));
        root.Children.Add(tabs);Content=root;
    }
    private static TabItem TextTab(string title,string text)=>new()
    {
        Header=title,
        Content=new TextBox{Text=text,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,AcceptsReturn=true,
            VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Padding=new Thickness(14),
            BorderThickness=new Thickness(0),Background=Brushes.Transparent}
    };
}
