using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace PID2Process;
public partial class MainWindow
{
    private readonly Dictionary<string,EquipmentItem> _drawingPresets=[];
    private void InitializeLibrary()
    {
        void Visit(DependencyObject root)
        {
            if(root is Button tool&&tool.Tag is null&&tool.Content is string caption&&caption is "Rohrpfad zeichnen" or "Wirklinie / Regellinie")
            {
                var row=new StackPanel{Orientation=Orientation.Horizontal};var canvas=new Canvas{Width=40,Height=28,Margin=new Thickness(0,0,4,0)};
                var line=new System.Windows.Shapes.Polyline{Stroke=caption.StartsWith("Rohr")?Brushes.DeepSkyBlue:Brushes.OrangeRed,StrokeThickness=caption.StartsWith("Rohr")?2.5:1.5,Points=new PointCollection(caption.StartsWith("Rohr")?[new Point(2,22),new Point(18,22),new Point(18,6),new Point(36,6)]:[new Point(2,15),new Point(14,15),new Point(18,7),new Point(26,7),new Point(18,23),new Point(26,23)])};canvas.Children.Add(line);row.Children.Add(canvas);row.Children.Add(new TextBlock{Text=caption,VerticalAlignment=VerticalAlignment.Center});tool.Content=row;tool.ToolTip=caption;System.Windows.Automation.AutomationProperties.SetName(tool,caption);
            }
            if(root is Button button&&button.Tag is string key&&key.Contains('|'))
            {
                var label=button.Content?.ToString()??key;var parts=key.Split('|');var item=new EquipmentItem{Type=Enum.Parse<EquipmentType>(parts[0]),Variant=parts[1],Actuator=ActuatorType.Manual,Width=.065,Height=.1,Opacity=1,Color="#D3E9FF",TextContent="Aa"};
                if(item.Type==EquipmentType.Drawing){item.Color="#D3E9FF";item.Drawing.Horizontal=true;_drawingPresets[key]=Clone(item);item.Drawing.PageWidth=600;item.Drawing.StrokeWidth=2.5;item.Drawing.ArrowSize=10;item.Drawing.FontSize=18;}
                var row=new Grid();row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(42)});row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
                var preview=new Viewbox{Width=32,Height=30,Child=PageVisual.Symbol(item,600,400),Stretch=Stretch.Uniform,IsHitTestVisible=false,Margin=new Thickness(0,0,8,0)};
                row.Children.Add(preview);var text=new TextBlock{Text=label,TextWrapping=TextWrapping.Wrap,VerticalAlignment=VerticalAlignment.Center};Grid.SetColumn(text,1);row.Children.Add(text);button.Content=row;button.ToolTip=label;System.Windows.Automation.AutomationProperties.SetName(button,label);
                if(item.Type==EquipmentType.Drawing){var menu=new ContextMenu();AddMenu(menu,"Zeicheneigenschaften…",()=>{var preset=Clone(_drawingPresets[key]);preset.Color=preset.Color=="#D3E9FF"?"#000000":preset.Color;if(EditAnnotation(preset))_drawingPresets[key]=preset;});button.ContextMenu=menu;}
            }
            foreach(var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>().ToArray())Visit(child);
        }
        Visit(SymbolLibrary);
        foreach(var text in PlanToolbar.Items.OfType<TextBlock>())text.Foreground=Brushes.AliceBlue;
        foreach(var button in PlanToolbar.Items.OfType<Button>())button.Style=(Style)FindResource("ModernButton");
    }
    private void ApplyDrawingPreset(EquipmentItem item,string key)
    {
        if(!_drawingPresets.TryGetValue(key,out var preset))return;var copy=Clone(preset);item.Drawing=copy.Drawing;item.Drawing.Start=item.Drawing.End=null;item.Color=copy.Color=="#D3E9FF"?"#000000":copy.Color;item.TextContent=copy.TextContent;
    }
}
