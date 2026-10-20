using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using CheckBox = System.Windows.Controls.CheckBox;
using Button = System.Windows.Controls.Button;
namespace PID2Process;

// Edits are committed by the caller only after a successful dialog result.
public sealed class EditorDialog : Window
{
    private readonly StackPanel _fields = new() { Margin = new Thickness(20) };
    private readonly Dictionary<string, FrameworkElement> _controls = [];
    private readonly Dictionary<string,FrameworkElement> _rows=[];
    private readonly Dictionary<string,TextBlock> _labels=[];
    private readonly Dictionary<string,(GroupBox Box,Grid Grid)> _groups=[];
    private bool _device;
    internal Func<string?>? Validation {get;set;}
    public EditorDialog(Window? owner, string title)
    {
        DialogTheme.Apply(this);_device=title.StartsWith("Gerätedaten",StringComparison.Ordinal);if(owner?.IsVisible==true)Owner = owner; Title = title; Width = _device?740:570; SizeToContent = SizeToContent.Height; MaxHeight = Math.Min(850, SystemParameters.WorkArea.Height - 60); WindowStartupLocation = WindowStartupLocation.CenterOwner;
        if(_device)foreach(var name in new[]{"Kennzeichnung","Objekteigenschaften","Farben und Größen"}){var grid=new Grid();grid.ColumnDefinitions.Add(new());grid.ColumnDefinitions.Add(new());var group=DialogTheme.Group(name,grid);_groups.Add(name,(group,grid));_fields.Children.Add(group);}
        var root = new DockPanel{Background=PageVisual.Color("#20262E")}; var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = "Übernehmen", IsDefault = true }; ok.Click += (_, _) => {var error=Validation?.Invoke();if(error is not null){MessageBox.Show(this,error,"Eingaben prüfen");return;} DialogResult = true; };
        var cancel = new Button { Content = "Abbrechen", IsCancel = true }; buttons.Children.Add(ok); buttons.Children.Add(cancel);
        DockPanel.SetDock(buttons, Dock.Bottom); root.Children.Add(buttons); root.Children.Add(new ScrollViewer { Content = _fields, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }); Content = root;
    }
    public void Text(string label, string value, bool multiline = false)
    {
        Add(label, new TextBox { Text = value, AcceptsReturn = multiline, TextWrapping = TextWrapping.Wrap, MinHeight = multiline ? 45 : 26 });
    }
    public void Choice(string label, string value, params string[] options)
    {
        var box = new ComboBox { ItemsSource = options, IsEditable = true, Text = value }; Add(label, box);
    }
    public void Check(string label, bool value) => Add(label, new CheckBox { IsChecked = value });
    public void Color(string label,string value)=>Add(label,new ColorPicker(value));
    public void FixedChoice(string label,string value,params string[] options) => Add(label,new ComboBox{ItemsSource=options,SelectedItem=value,IsEditable=false});
    internal FrameworkElement Control(string label)=>_controls[label];
    internal void Custom(string label,FrameworkElement control)=>Add(label,control,true);
    private void Add(string label, FrameworkElement control,bool custom=false)
    {
        var row=new StackPanel{Tag=label,Margin=new Thickness(4,4,4,6)};var heading=new TextBlock{Text=label,FontWeight=FontWeights.SemiBold,Foreground=PageVisual.Color("#BFD0E2"),Margin=new Thickness(0,0,0,4)};
        if(control is CheckBox toggle)toggle.Content=label;else if(!custom)row.Children.Add(heading);row.Children.Add(control);_rows.Add(label,row);_labels.Add(label,heading);control.Tag=label;_controls.Add(label,control);
        System.Windows.Automation.AutomationProperties.SetLabeledBy(control,heading);
        if(!_device){_fields.Children.Add(row);return;}
        var key=label is "Tag" or "Beschreibung" or "Funktion" or "Einbauort (+)"?"Kennzeichnung":new[]{"Farbe","Hintergrundfarbe","Deckkraft","Breite","Höhe"}.Any(label.StartsWith)?"Farben und Größen":"Objekteigenschaften";
        _groups[key].Box.Visibility=Visibility.Visible;var grid=_groups[key].Grid;var wide=custom||control is TextBox{AcceptsReturn:true};var column=0;var index=grid.Children.Count==0?0:grid.Children.Cast<UIElement>().Max(Grid.GetRow);
        if(grid.Children.Count>0){var previous=(FrameworkElement)grid.Children[^1];if(wide||Grid.GetColumnSpan(previous)==2||Grid.GetColumn(previous)==1)index++;else column=1;}
        while(grid.RowDefinitions.Count<=index)grid.RowDefinitions.Add(new(){Height=GridLength.Auto});Grid.SetRow(row,index);Grid.SetColumn(row,column);if(wide)Grid.SetColumnSpan(row,2);grid.Children.Add(row);ReflowGroups();
    }
    public string Value(string label) => _controls[label] switch { TextBox t => t.Text.Trim(), ComboBox c => (c.IsEditable?c.Text:c.SelectedItem?.ToString()??"").Trim(), ColorPicker c=>c.Value, _ => "" };
    public bool Checked(string label) => ((CheckBox)_controls[label]).IsChecked == true;
    public void Hide(string label)
    {
        _controls[label].Visibility=Visibility.Collapsed;_rows[label].Visibility=Visibility.Collapsed;
        ReflowGroups();
    }
    public void Label(string key, string caption)
    {
        _labels[key].Text=caption;if(_controls[key] is CheckBox toggle)toggle.Content=caption;
    }
    private void ReflowGroups()
    {
        foreach(var (name,group) in _groups)
        {
            var rows=group.Grid.Children.Cast<FrameworkElement>().Where(x=>x.Visibility==Visibility.Visible);
            if(name=="Kennzeichnung"){var order=new[]{"Tag","Funktion","Einbauort (+)","Beschreibung"};rows=rows.OrderBy(x=>Array.IndexOf(order,x.Tag?.ToString()));}
            var index=0;var column=0;group.Grid.RowDefinitions.Clear();
            foreach(var row in rows){var wide=Grid.GetColumnSpan(row)==2;if(wide&&column==1){index++;column=0;}while(group.Grid.RowDefinitions.Count<=index)group.Grid.RowDefinitions.Add(new(){Height=GridLength.Auto});Grid.SetRow(row,index);Grid.SetColumn(row,column);column+=wide?2:1;if(column==2){index++;column=0;}}
            group.Box.Visibility=group.Grid.Children.Cast<FrameworkElement>().Any(x=>x.Visibility==Visibility.Visible)?Visibility.Visible:Visibility.Collapsed;
        }
    }
    public double Number(string label, double fallback) => double.TryParse(Value(label).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) && double.IsFinite(n) ? n : fallback;
}
