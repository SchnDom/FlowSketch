using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

internal sealed class HelpView : DockPanel
{
    internal sealed record Chapter(string Title,List<string> Lines);
    internal static string GuideText=>InformationWindow.ResourceText("HelpGuide.md");
    internal static List<Chapter> Chapters()
    {
        var result=new List<Chapter>();Chapter? current=null;
        foreach(var line in GuideText.Replace("\r","").Split('\n'))
        {
            if(line.StartsWith("# ")){current=new(line[2..],[]);result.Add(current);}
            else if(line.StartsWith("## ")){current=new(line[3..],[]);result.Add(current);}
            else current?.Lines.Add(line);
        }
        return result;
    }
    internal HelpView()
    {
        Background=PageVisual.Color("#202A35");var chapters=Chapters();
        var selector=new ComboBox{ItemsSource=chapters.Select(x=>x.Title).ToList(),Margin=new Thickness(12),ToolTip="Hilfekapitel auswählen"};
        SetDock(selector,Dock.Top);Children.Add(selector);
        var body=new StackPanel{Margin=new Thickness(16)};
        var scroll=new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
        Children.Add(scroll);
        selector.SelectionChanged+=(_,_)=>{
            body.Children.Clear();if(selector.SelectedIndex<0)return;var chapter=chapters[selector.SelectedIndex];
            body.Children.Add(new TextBlock{Text=chapter.Title,FontSize=20,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,16)});
            foreach(var line in chapter.Lines.Where(x=>x.Length>0))
            {
                var match=Regex.Match(line,@"^!\[([^\]]+)\]\(images/([a-z0-9-]+\.png)\)$");
                if(match.Success)
                {
                    var image=new Image{Source=new BitmapImage(new Uri("pack://application:,,,/Help/"+match.Groups[2].Value)),Stretch=Stretch.Uniform,MaxHeight=470,Margin=new Thickness(0,4,0,6)};
                    body.Children.Add(image);body.Children.Add(new TextBlock{Text=match.Groups[1].Value,TextWrapping=TextWrapping.Wrap,Foreground=PageVisual.Color("#A9BCD0"),Margin=new Thickness(0,0,0,18)});
                }
                else if(line.StartsWith("### "))body.Children.Add(new TextBlock{Text=line[4..],TextWrapping=TextWrapping.Wrap,FontSize=16,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,8,0,12)});
                else body.Children.Add(new TextBlock{Text=line,TextWrapping=TextWrapping.Wrap,FontSize=14,Margin=new Thickness(0,0,0,12)});
            }
            scroll.ScrollToTop();
        };
        selector.SelectedIndex=1;
    }
}
