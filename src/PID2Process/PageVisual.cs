using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace PID2Process;

internal static class PageVisual
{
    internal static System.Windows.Media.Brush Color(string value) { try { return (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(value)!; } catch { return Brushes.Teal; } }
    internal static string StateColor(EquipmentItem item, string? state) => SymbolCatalog.Colored(item)?"#909090":state is "Störung" or "Fehler"?"#FF0000":item.Color;
    internal static FrameworkElement Symbol(EquipmentItem item,double pageWidth,double pageHeight,string? state=null,string? textOverride=null,string? status=null,SensorAlarmView? sensorView=null)
    {
        if(item.Type==EquipmentType.Drawing)return AnnotationVisual.Symbol(item,pageWidth,pageHeight);
        if(item.Type==EquipmentType.Legend)return LegendVisual.Symbol(item,pageWidth,pageHeight);
        var values=new SceneValues();if(state is not null)values.EquipmentStates[item.Id]=state;
        state=SymbolCatalog.State(item,values);if(SymbolCatalog.CheckValve(item))state=status=="Durchflossen"?"Durchflossen":"Gesperrt";var fault=status?.StartsWith("Störung")==true;
        var color=Color(StateColor(item,fault?"Störung":state));
        var background=(fault?"Störung":state) switch{"Offen" or "Ein" or "Aktiv" or "A-B" or "A-C" or "B-C"=>"#CEF4DE","Geschlossen" or "Aus" or "Inaktiv"=>"#FFDF9A","Störung"=>"#FFC5C5",_=>item.BackgroundColor};
        if(SymbolCatalog.Colored(item))background=item.BackgroundColor=="#FFFFFF"?"#FFDF9A":item.BackgroundColor;
        var grid=new Grid{Width=Math.Max(item.Variant=="Querverweis"?2:18,item.Width*pageWidth),Height=Math.Max(item.Variant=="Querverweis"?2:18,item.Height*pageHeight),Opacity=state=="Ausgeblendet"?0:item.Opacity,ToolTip=item.Tag+" · "+item.Variant};
        // The border is an overlay background; the actual symbol is drawn from the shared catalog.
        grid.Children.Add(new Border{Background=Color(background),BorderBrush=color,BorderThickness=new Thickness(item.Type is EquipmentType.MediaSource or EquipmentType.Text?1:0)});
        var canvas=new Canvas{Width=100,Height=100};if(SymbolCatalog.CheckValve(item)&&item.CheckValveReversed)canvas.RenderTransform=new ScaleTransform(-1,1,50,50);
        void Shapes(IEnumerable<SymbolShape> shapes)
        {
            foreach(var shape in shapes)
            {
                var fill=shape.Part is not null?Color(SymbolCatalog.FillColor(item,shape,fault?"Störung":state)):shape.Fill=="stroke"?color:shape.Fill=="background"?Color(background):Brushes.Transparent;
                if(shape.Path is {} path)canvas.Children.Add(new System.Windows.Shapes.Path{Data=Geometry.Parse(path),Stroke=color,StrokeThickness=1.5,Fill=fill,StrokeDashArray=shape.Dash?new DoubleCollection([4,3]):null});
                if(shape.Ellipse is {} e){var ellipse=new Ellipse{Width=e[2],Height=e[3],Stroke=color,StrokeThickness=1.5,Fill=fill};Canvas.SetLeft(ellipse,e[0]);Canvas.SetTop(ellipse,e[1]);canvas.Children.Add(ellipse);}
                if(shape.Text is {} text){var label=new TextBlock{Text=text,FontSize=13,Foreground=color,Width=24,TextAlignment=TextAlignment.Center};Canvas.SetLeft(label,shape.X-12);Canvas.SetTop(label,shape.Y-8);canvas.Children.Add(label);}
            }
        }
        Shapes(SymbolCatalog.Get(item).Shapes);
        if(SymbolCatalog.Valve(item)&&!SymbolCatalog.CheckValve(item))Shapes(SymbolCatalog.Definitions["Actuator|"+item.Actuator].Shapes);
        if(item.Type==EquipmentType.Instrument){canvas.Children.Clear();foreach(var (sweep,fill) in new[]{(1,sensorView?.UpperColor??"#FFFFFF"),(0,sensorView?.LowerColor??"#FFFFFF")})canvas.Children.Add(new System.Windows.Shapes.Path{Data=Geometry.Parse($"M 3,50 A 47,47 0 0 {sweep} 97,50 Z"),Fill=Color(fill)});Shapes([new(){Ellipse=[3,3,94,94]},new(){Path="M 3,50 L 97,50"}]);foreach(var (value,y) in new[]{(item.Sensor.Signal,25d),(item.Sensor.Number,65d)}){var label=new TextBlock{Text=value,Foreground=Brushes.Black,FontSize=22,Width=94,TextAlignment=TextAlignment.Center};Canvas.SetLeft(label,3);Canvas.SetTop(label,y-10);canvas.Children.Add(label);}}
        grid.Children.Add(new Viewbox{Child=canvas,Stretch=Stretch.Fill,IsHitTestVisible=false});
        if(item.Type is EquipmentType.MediaSource or EquipmentType.Text)
        {
            var content=new TextBlock{Text=item.Type==EquipmentType.Text?textOverride??item.TextContent:MediaDisplay.Text(item),Foreground=color,TextWrapping=TextWrapping.Wrap,FontSize=pageWidth/140,Width=Math.Max(40,grid.Width-12),TextAlignment=TextAlignment.Center};
            grid.Children.Add(new Viewbox{Child=content,Margin=new Thickness(5),Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly});
        }
        var caption=status??(SymbolCatalog.Valve(item)||SymbolCatalog.Conveyor(item)||item.Type==EquipmentType.Filter?state:"");
        if(caption.Length>0&&(!SymbolCatalog.Colored(item)||item.ShowStateText)){var layer=new Canvas{IsHitTestVisible=false};var text=new TextBlock{Text=caption,Width=item.Variant=="Querverweis"?Math.Max(grid.Width,pageWidth*.035):item.Type==EquipmentType.Instrument?Math.Max(140,grid.Width):grid.Width,TextAlignment=TextAlignment.Center,TextWrapping=item.Type==EquipmentType.Instrument||item.Variant=="Querverweis"||SymbolCatalog.Colored(item)?TextWrapping.NoWrap:TextWrapping.Wrap,TextTrimming=TextTrimming.CharacterEllipsis,FontSize=item.Type==EquipmentType.Instrument?Math.Max(10,Math.Min(16,grid.Width/10)):SymbolCatalog.Colored(item)?Math.Max(6,pageWidth/250):Math.Max(9,pageWidth/180),Foreground=fault?Brushes.DarkRed:Brushes.Black,Background=Brushes.White};if(item.Type==EquipmentType.Instrument||item.Variant=="Querverweis")Canvas.SetLeft(text,(grid.Width-text.Width)/2);Canvas.SetTop(text,grid.Height+2);layer.Children.Add(text);grid.Children.Add(layer);}
        grid.ToolTip=item.Tag+" · "+item.Variant+" · "+caption;
        return grid;
    }
    internal static EquipmentItem Appearance(EquipmentItem item,EngineeringProject p,SceneValues v)
    {
        if(item.Type!=EquipmentType.Vessel)return item;
        var copy=System.Text.Json.JsonSerializer.Deserialize<EquipmentItem>(System.Text.Json.JsonSerializer.Serialize(item,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;
        var assignment=v.TankStates.GetValueOrDefault(item.Id);var medium=p.Media.FirstOrDefault(m=>m.Id==(assignment?.MediumId??item.MediumId));
        var filled=v.EquipmentStates.GetValueOrDefault(item.Id) switch{"Befüllt"=>true,"Leer"=>false,_=>assignment?.Filled==true||assignment?.Pressurized==true};
        copy.Color=filled?MediaSettings.Color(p,medium?.Type??assignment?.MediumType??item.MediumType,medium?.Name??assignment?.MediumName??item.MediaName):p.MediaColors.Empty;
        copy.BackgroundColor=copy.Color;return copy;
    }
    internal static double CaptionOffset(EquipmentItem x,double width)=>x.Variant=="Querverweis"?Math.Max(24,width/90):x.Type==EquipmentType.Instrument?Math.Max(26,width/100):SymbolCatalog.Colored(x)?x.ShowStateText?Math.Max(12,width/200):0:x.Type is EquipmentType.Filter or EquipmentType.Vessel or EquipmentType.HeatExchanger?Math.Max(48,width/45):0;
    internal static string? Status(EquipmentItem x,SceneValues v,NetworkScene network)
    {
        if(x.Variant=="Querverweis")return x.ReferenceId;
        if(SymbolCatalog.CheckValve(x))return network.CheckValves.GetValueOrDefault(x.Id)?"Durchflossen":"Gesperrt";
        if(x.Type==EquipmentType.Instrument)return SensorEvaluation.Display(x,v).Text;
        if(network.Errors.TryGetValue(x.Id,out var error))return error;
        if(x.Type==EquipmentType.HeatExchanger&&network.HeatExchangers.TryGetValue(x.Id,out var h))return "Primär: "+(h.Primary?"durchflossen":"kein Durchfluss")+" · Sekundär: "+(h.Secondary?"durchflossen":"kein Durchfluss");
        if(x.Type==EquipmentType.Vessel){var tank=v.TankStates.GetValueOrDefault(x.Id);return v.EquipmentStates.GetValueOrDefault(x.Id) is "Befüllt" or "Leer"?v.EquipmentStates[x.Id]:tank?.Filled==true||tank?.Pressurized==true?"Befüllt":"Leer";}
        return null;
    }
    internal static BitmapSource Render(EngineeringProject project,int page,double width,double aspect,Scenario? scenario,BitmapSource? background)
    {
        var height=width*aspect;var canvas=new Canvas { Width=width,Height=height,Background=background is null?Brushes.Transparent:Brushes.White };
        if(background is not null)canvas.Children.Add(new System.Windows.Controls.Image {Source=background,Width=width,Height=height});
        var values=SceneValues.From(scenario,null);var network=PipeNetwork.EvaluateScene(project,values);var appearances=network.Pipes;
        foreach(var pipe in project.Pipes.Where(p=>p.Page==page))
        {
            var appearance=appearances[pipe.Id];var line=new Polyline {Points=new PointCollection(PipeNetwork.Resolve(project,pipe).Select(p=>new Point(p.X*width,p.Y*height))),Stroke=Color(PipeStyleResolver.Color(project,pipe,appearance.State,appearance.MediumType,appearance.MediumName)),Opacity=pipe.Opacity,StrokeThickness=PipeStyleResolver.Width(pipe,width,width/400)};line.StrokeDashArray=PipeStyleResolver.Dash(pipe,appearance.State) is {} dash?new DoubleCollection(dash):null;canvas.Children.Add(line);
            foreach(var span in appearance.Spans.Where(x=>x.Flowing)){var mark=new Polyline{Points=new PointCollection(PipeNetwork.Slice(PipeNetwork.Resolve(project,pipe),span.Start,span.End).Select(p=>new Point(p.X*width,p.Y*height))),Stroke=Brushes.White,StrokeThickness=line.StrokeThickness/2,Opacity=pipe.Opacity*.8,StrokeDashArray=new DoubleCollection([3,3])};canvas.Children.Add(mark);}
        }
        foreach(var pipe in project.Pipes.Where(x=>x.Page==page)){var path=PipeNetwork.Resolve(project,pipe);foreach(var endpoint in new[]{(pipe.StartConnection,path[0]),(pipe.EndConnection,path[^1])}.Where(x=>x.Item1?.NeedsReview==true)){var marker=new Rectangle{Width=width/140,Height=width/140,Fill=Brushes.DarkOrange,Stroke=Brushes.DarkRed};Canvas.SetLeft(marker,endpoint.Item2.X*width-marker.Width/2);Canvas.SetTop(marker,endpoint.Item2.Y*height-marker.Height/2);canvas.Children.Add(marker);}}
        foreach(var sensor in project.Equipment.Where(x=>x.Page==page&&x.Type==EquipmentType.Instrument))canvas.Children.Add(new Polyline{Points=new PointCollection(SensorConnections.Resolve(project,sensor).Select(q=>new Point(q.X*width,q.Y*height))),Stroke=Brushes.Black,StrokeThickness=width/1800});
        foreach(var signal in project.SignalLines.Where(x=>x.Page==page)){var points=SignalGeometry.Resolve(project,signal);var paths=new[]{points}.Concat(SignalGeometry.Markers(points,width,height));foreach(var path in paths)canvas.Children.Add(new Polyline{Points=new PointCollection(path.Select(p=>new Point(p.X*width,p.Y*height))),Stroke=Color(signal.Color),StrokeThickness=width/1500,Opacity=signal.Opacity});}
        foreach(var item in project.Equipment.Where(x=>x.Page==page))
        {
            if(item.Type is EquipmentType.Text or EquipmentType.Legend&&scenario?.EquipmentStates.GetValueOrDefault(item.Id)=="Ausgeblendet")continue;
            var symbol=Symbol(Appearance(item,project,values),width,height,scenario?.EquipmentStates.GetValueOrDefault(item.Id),scenario?.TextValues.GetValueOrDefault(item.Id),Status(item,values,network),item.Type==EquipmentType.Instrument?SensorEvaluation.Display(item,values):null);Canvas.SetLeft(symbol,item.X*width);Canvas.SetTop(symbol,item.Y*height);canvas.Children.Add(symbol);if(item.Type==EquipmentType.Drawing||item.Variant=="Querverweis")continue;
            var label=new TextBlock {Text=item.Tag,Background=Brushes.White,Foreground=Brushes.Black,FontSize=width/150};Canvas.SetLeft(label,item.X*width);Canvas.SetTop(label,item.Y*height+symbol.Height+CaptionOffset(item,width));canvas.Children.Add(label);
        }
        canvas.Measure(new Size(width,height));canvas.Arrange(new Rect(0,0,width,height));canvas.UpdateLayout();var image=new RenderTargetBitmap((int)Math.Ceiling(width),(int)Math.Ceiling(height),96,96,PixelFormats.Pbgra32);image.Render(canvas);image.Freeze();return image;
    }
}
