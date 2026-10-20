using System.Windows;
using System.Windows.Controls;
namespace PID2Process;

internal sealed class BatchPropertiesEditor : Window
{
    private readonly double _pageWidth,_pageHeight;
    private readonly Dictionary<string,(CheckBox Toggle,FrameworkElement Input)> _fields=[];
    internal SelectionPatch? Patch {get;private set;}
    internal BatchPropertiesEditor(Window? owner,EngineeringProject project,ISet<Guid> ids,double pageWidth=2000,double pageHeight=1416)
    {
        _pageWidth=pageWidth;_pageHeight=pageHeight;
        if(owner is not null)Owner=owner;Title=$"Gemeinsame Eigenschaften · {ids.Count} Objekte";Width=500;MaxHeight=SystemParameters.WorkArea.Height-60;SizeToContent=SizeToContent.Height;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        DialogTheme.Apply(this);var root=new DockPanel();var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};var ok=new Button{Content="Übernehmen",IsDefault=true};ok.Click+=(_,_)=>{if(TryBuildPatch(out var error))DialogResult=true;else MessageBox.Show(this,error,"Eigenschaften prüfen");};buttons.Children.Add(ok);buttons.Children.Add(new Button{Content="Abbrechen",IsCancel=true});DockPanel.SetDock(buttons,Dock.Bottom);root.Children.Add(buttons);
        var stack=new StackPanel{Margin=new Thickness(16)};stack.Children.Add(new TextBlock{Text="Nur angehakte Eigenschaften werden geändert. Leere Textfelder löschen den bisherigen Text.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
        var identity=new StackPanel();var appearance=new StackPanel();stack.Children.Add(DialogTheme.Group("Zuordnung",identity));stack.Children.Add(DialogTheme.Group("Farben und Größen",appearance));
        var devices=project.Equipment.Where(x=>ids.Contains(x.Id)).ToArray();var pipes=project.Pipes.Where(x=>ids.Contains(x.Id)).ToArray();var signals=project.SignalLines.Where(x=>ids.Contains(x.Id)).ToArray();
        string Common(IEnumerable<string> values){var a=values.Distinct().ToArray();return a.Length==1?a[0]:"";}
        void Field(string name,FrameworkElement input,bool enabled=true){var toggle=new CheckBox{Content=name,IsEnabled=enabled,Margin=new Thickness(0,6,0,4)};input.IsEnabled=false;input.Tag=name;toggle.Checked+=(_,_)=>input.IsEnabled=true;toggle.Unchecked+=(_,_)=>input.IsEnabled=false;var target=name is "Funktion" or "Einbauort (+)"?identity:appearance;target.Children.Add(toggle);target.Children.Add(input);_fields.Add(name,(toggle,input));}
        Field("Funktion",new TextBox{Text=Common(devices.Select(x=>x.Function).Concat(pipes.Select(x=>x.Function)).Concat(signals.Select(x=>x.Function)))});Field("Einbauort (+)",new TextBox{Text=Common(devices.Select(x=>x.InstallationLocation).Concat(pipes.Select(x=>x.InstallationLocation)).Concat(signals.Select(x=>x.InstallationLocation)))});
        var initial=Common(devices.Select(x=>x.Color).Concat(pipes.Select(x=>x.Color??"#007E87")).Concat(signals.Select(x=>x.Color)));Field("Farbe",new ColorPicker(ColorPicker.IsValid(initial)?initial:"#007E87"));
        Field("Deckkraft (0 bis 1)",new TextBox{Text="1"});Field("Breite je Symbol (px)",new TextBox{Text=devices.FirstOrDefault()?.Width is double w?(w*_pageWidth).ToString():""},devices.Length>0);Field("Höhe je Symbol (px)",new TextBox{Text=devices.FirstOrDefault()?.Height is double h?(h*_pageHeight).ToString():""},devices.Length>0);
        stack.Children.Add(new TextBlock{Text="Zustandsfarben der Symbole bleiben vorrangig. Bei Rohren gilt die gewählte Farbe statt der Medienfarbe. Breite/Höhe ändern nur Symbole, nicht Rohrpfade oder Wirklinien.",TextWrapping=TextWrapping.Wrap,Foreground=PageVisual.Color("#A9BCD0"),Margin=new Thickness(0,12,0,0)});root.Children.Add(new ScrollViewer{Content=stack,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});Content=root;
    }
    internal void SetField(string name,string value){var f=_fields[name];f.Toggle.IsChecked=true;if(f.Input is ColorPicker c)c.Hex.Text=value;else ((TextBox)f.Input).Text=value;}
    internal bool TryBuildPatch(out string error)
    {
        string? Value(string name){var f=_fields[name];return f.Toggle.IsChecked==true?(f.Input is ColorPicker c?c.Value:((TextBox)f.Input).Text.Trim()):null;}
        error="";var color=Value("Farbe");if(color is not null&&!ColorPicker.IsValid(color)){error="Farbe als #RRGGBB eingeben oder über das Farbrad wählen.";return false;}
        bool Numeric(string name,double max,out double? number){var text=Value(name);number=null;if(text is null)return true;if(!MediaDisplay.TryOptional(text,out number)||number is null||number<0||number>max||(max==100&&number==0))return false;return true;}
        if(!Numeric("Deckkraft (0 bis 1)",1,out var opacity)||!Numeric("Breite je Symbol (px)",_pageWidth,out var width)||!Numeric("Höhe je Symbol (px)",_pageHeight,out var height)){error="Deckkraft: 0–1; Breite/Höhe: positive Pixelmaße innerhalb der Seite.";return false;}
        Patch=new SelectionPatch(Value("Funktion"),Value("Einbauort (+)"),color,opacity,width/_pageWidth,height/_pageHeight);return true;
    }
}
