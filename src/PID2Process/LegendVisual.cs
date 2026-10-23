using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace PID2Process;

internal static class LegendVisual
{
    // Labels and RGB line colors from the legends in Wolkenkammer V18 / Vakuumsystem V12.
    internal static List<LegendEntry> PidEntries()=>[
        new(){Text="Luft",Color="#939598"},new(){Text="Druckluft",Color="#139B48"},
        new(){Text="Dampf (Wasserdampf)",Color="#FF00FF"},new(){Text="Wasser",Color="#0000FF"},
        new(){Text="Silikonöl",Color="#F26722"},new(){Text="Wasser-Glykol Mischung",Color="#00FFFF"}];
    internal static FrameworkElement Symbol(EquipmentItem item,double width,double height)
    {
        var grid=new Grid{Width=Math.Max(18,item.Width*width),Height=Math.Max(18,item.Height*height),Opacity=item.Opacity};
        var canvas=new Canvas{Width=300,Height=40+item.LegendEntries.Count*25};
        canvas.Children.Add(new Rectangle{Width=300,Height=canvas.Height,Fill=item.BackgroundTransparent?Brushes.Transparent:PageVisual.Color(item.BackgroundColor),Stroke=item.LinesTransparent?Brushes.Transparent:PageVisual.Color(item.Color),StrokeThickness=1});
        var title=new TextBlock{Text=item.LegendTitle,FontSize=14,FontWeight=FontWeights.SemiBold,Width=276,TextTrimming=TextTrimming.CharacterEllipsis};Canvas.SetLeft(title,12);Canvas.SetTop(title,8);canvas.Children.Add(title);
        for(var i=0;i<item.LegendEntries.Count;i++)
        {
            var entry=item.LegendEntries[i];var y=40+i*25;
            var text=new TextBlock{Text=entry.Text,FontSize=11,Width=170,Height=23,TextWrapping=TextWrapping.Wrap,ToolTip=entry.Text};Canvas.SetLeft(text,12);Canvas.SetTop(text,y-10);canvas.Children.Add(text);
            canvas.Children.Add(new Line{X1=190,Y1=y,X2=285,Y2=y,Stroke=PageVisual.Color(entry.Color),StrokeThickness=1.2});
            canvas.Children.Add(new Polyline{Points=new PointCollection([new(280,y-5),new(285,y),new(280,y+5)]),Stroke=PageVisual.Color(item.Color),StrokeThickness=1,Fill=Brushes.Transparent});
        }
        grid.Children.Add(new Viewbox{Child=canvas,Stretch=Stretch.Fill});return grid;
    }
}
