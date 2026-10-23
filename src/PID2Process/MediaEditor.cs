using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace PID2Process;

internal sealed class MediumEditRow : INotifyPropertyChanged
{
    internal readonly MediumDefinition Definition;
    private readonly Func<MediumType,string> _standard;
    internal bool Automatic {get;private set;}
    public Guid Id=>Definition.Id;
    public string Name {get=>Definition.Name;set{Definition.Name=value;Changed(nameof(Name));}}
    public MediumType Type {get=>Definition.Type;set{Definition.Type=value;RefreshStandard();Changed(nameof(Type));}}
    public string Color {get=>Definition.Color;set{Automatic=false;Definition.Color=value;Changed(nameof(Color));}}
    internal MediumEditRow(MediumDefinition definition,Func<MediumType,string> standard,bool automatic)
    {Definition=definition;_standard=standard;Automatic=automatic;RefreshStandard();}
    internal void RefreshStandard(){if(!Automatic)return;Definition.Color=_standard(Type);Changed(nameof(Color));}
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed(string name)=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
}
internal sealed class MediaEditor : Window
{
    internal EngineeringProject Working;
    internal ObservableCollection<MediumEditRow> Rows {get;}
    internal readonly Dictionary<string,ColorPicker> Pickers=[];
    private readonly DataGrid _grid=new(){AutoGenerateColumns=false,CanUserAddRows=false};
    internal MediaEditor(Window? owner,EngineeringProject project)
    {
        if(owner?.IsVisible==true)Owner=owner;Title="Medien";Width=900;Height=720;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        Working=DocumentExport.Snapshot(project);MediaSettings.Normalize(Working);
        DialogTheme.Apply(this);var root=new DockPanel{Margin=new Thickness(16),Background=PageVisual.Color("#20262E")};var palette=new StackPanel();
        palette.Children.Add(new TextBlock{Text="Standardfarben je Typ; individuelle Medienfarben in der Tabelle."});
        foreach(var (label,value) in new[]{("Leer",Working.MediaColors.Empty),("Flüssig",Working.MediaColors.Liquid),("Fest",Working.MediaColors.Solid),("Gasförmig",Working.MediaColors.Gas),("Gemisch",Working.MediaColors.Mixed)})
        {var line=new DockPanel();line.Children.Add(new TextBlock{Text=label,Width=100});var picker=new ColorPicker(value);Pickers[label]=picker;line.Children.Add(picker);palette.Children.Add(line);}
        Rows=new(Working.Media.Select(m=>new MediumEditRow(m,Standard,string.Equals(m.Color,Working.MediaColors.Get(m.Type),StringComparison.OrdinalIgnoreCase))));
        foreach(var picker in Pickers.Values)picker.Hex.TextChanged+=(_,_)=>{foreach(var row in Rows)row.RefreshStandard();};
        var group=DialogTheme.Group("Standardfarben",palette);DockPanel.SetDock(group,Dock.Top);root.Children.Add(group);
        var buttons=new WrapPanel();void Button(string label,Action action){var b=new Button{Content=label};b.Click+=(_,_)=>action();buttons.Children.Add(b);}
        Button("Hinzufügen",()=>{var row=AddMedium();_grid.SelectedItem=row;_grid.ScrollIntoView(row);});
        Button("Entfernen",()=>{if(_grid.SelectedItem is not MediumEditRow row)return;if(Used(row.Id)){MessageBox.Show(this,"Medium wird verwendet.");return;}Rows.Remove(row);});
        Button("Übernehmen",()=>{var error=TryApply();if(error is not null){MessageBox.Show(this,error,"Medien prüfen");return;}DialogResult=true;});
        buttons.Children.Add(new Button{Content="Abbrechen",IsCancel=true});DockPanel.SetDock(buttons,Dock.Bottom);root.Children.Add(buttons);
        _grid.Columns.Add(new DataGridTextColumn{Header="Bezeichnung",Binding=new Binding("Name"){UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},Width=new DataGridLength(1,DataGridLengthUnitType.Star)});
        _grid.Columns.Add(new DataGridComboBoxColumn{Header="Typ",ItemsSource=MediaUi.Types,DisplayMemberPath="Label",SelectedValuePath="Value",SelectedValueBinding=new Binding("Type"){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},Width=140});
        _grid.Columns.Add(ColorPicker.Column("Farbe (#RRGGBB)"));_grid.ItemsSource=Rows;root.Children.Add(_grid);Content=root;
    }
    private string Standard(MediumType type)=>Pickers[MediaUi.Types.First(t=>t.Value==type).Label].Value;
    internal MediumEditRow AddMedium(){var row=new MediumEditRow(new(){Name="Neues Medium"},Standard,true);Rows.Add(row);return row;}
    private bool Used(Guid id)=>Working.Equipment.Any(e=>e.MediumId==id)||Working.Scenarios.Any(s=>s.TankStates.Values.Any(t=>t.MediumId==id))||Working.ControlSteps.Any(s=>s.TankStates.Values.Any(t=>t.MediumId==id));
    internal string? TryApply()
    {
        if(!_grid.CommitEdit(DataGridEditingUnit.Cell,true)||!_grid.CommitEdit(DataGridEditingUnit.Row,true))return "Tabelleneingabe abschließen.";
        if(Rows.Any(m=>string.IsNullOrWhiteSpace(m.Name)||!ColorPicker.IsValid(m.Color))||Rows.GroupBy(m=>m.Name.Trim(),StringComparer.OrdinalIgnoreCase).Any(g=>g.Count()>1)||Pickers.Values.Any(c=>!ColorPicker.IsValid(c.Value)))return "Eindeutige Bezeichnungen und gültige Farben erforderlich.";
        Working.MediaColors=new(){Empty=Pickers["Leer"].Value,Liquid=Pickers["Flüssig"].Value,Solid=Pickers["Fest"].Value,Gas=Pickers["Gasförmig"].Value,Mixed=Pickers["Gemisch"].Value};
        Working.Media=Rows.Select(row=>{row.RefreshStandard();row.Definition.Name=row.Name.Trim();return row.Definition;}).ToList();MediaSettings.Synchronize(Working);return null;
    }
}
internal static class MediaUi
{
    internal static MediumDefinition? AddMedium(EditorDialog owner,EngineeringProject project)
    {
        var dialog=new EditorDialog(owner,"Medium hinzufügen");dialog.Text("Bezeichnung","");dialog.FixedChoice("Typ",Types[0].Label,Types.Select(x=>x.Label).ToArray());dialog.Color("Farbe",project.MediaColors.Liquid);
        dialog.Validation=()=>dialog.Value("Bezeichnung").Length==0||project.Media.Any(x=>x.Name==dialog.Value("Bezeichnung"))?"Eine neue, eindeutige Bezeichnung eingeben.":!ColorPicker.IsValid(dialog.Value("Farbe"))?"Eine gültige Farbe eingeben.":null;
        if(dialog.ShowDialog()!=true)return null;var medium=new MediumDefinition{Name=dialog.Value("Bezeichnung"),Type=Types.First(x=>x.Label==dialog.Value("Typ")).Value,Color=dialog.Value("Farbe")};project.Media.Add(medium);return medium;
    }
    internal static void AddSelection(EditorDialog dialog,EngineeringProject project,MediumType type,string name)
    {
        dialog.FixedChoice("Mediumtyp",Types.First(t=>t.Value==type).Label,Types.Select(t=>t.Label).ToArray());
        dialog.FixedChoice("Name des Mediums",name,project.Media.Where(m=>m.Type==type).Select(m=>m.Name).ToArray());
        var types=(ComboBox)dialog.Control("Mediumtyp");var names=(ComboBox)dialog.Control("Name des Mediums");
        dialog.ActionBeside("Name des Mediums","+",()=>{if(AddMedium(dialog,project) is not {} medium)return;types.SelectedItem=Types.First(x=>x.Value==medium.Type).Label;names.ItemsSource=project.Media.Where(x=>x.Type==medium.Type).Select(x=>x.Name).ToArray();names.SelectedItem=medium.Name;});
        types.SelectionChanged+=(_,e)=>{if(e.Source!=types)return;var selected=Types.FirstOrDefault(t=>t.Label==types.SelectedItem?.ToString())?.Value;var old=names.SelectedItem as string;var choices=project.Media.Where(m=>m.Type==selected).Select(m=>m.Name).ToArray();names.ItemsSource=choices;names.SelectedItem=choices.Contains(old)?old:choices.FirstOrDefault();};
    }
    internal sealed record Choice(MediumType Value,string Label);
    internal static Choice[] Types=[new(MediumType.Liquid,"Flüssig"),new(MediumType.Gas,"Gasförmig"),new(MediumType.Solid,"Fest"),new(MediumType.Mixed,"Gemisch")];
}
