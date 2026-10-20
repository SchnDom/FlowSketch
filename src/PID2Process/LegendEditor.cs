using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Text.RegularExpressions;
namespace PID2Process;

internal sealed class LegendEditor : Window
{
    private bool _createSources;
    private readonly EquipmentItem _item;
    private readonly EngineeringProject _project;
    internal readonly ObservableCollection<LegendEntry> Rows;
    internal readonly TextBox TagBox=new(),TitleBox=new(),FunctionBox=new(),LocationBox=new(),DescriptionBox=new();
    private readonly DataGrid _grid=new(){AutoGenerateColumns=false,CanUserAddRows=false,SelectionMode=DataGridSelectionMode.Single};
    internal LegendEditor(Window? owner,EquipmentItem item,EngineeringProject project)
    {
        if(owner is not null)Owner=owner;_item=item;_project=project;
        Title="Legende bearbeiten";Width=720;Height=720;MaxHeight=Math.Max(400,SystemParameters.WorkArea.Height-60);WindowStartupLocation=WindowStartupLocation.CenterOwner;
        Rows=new(item.LegendEntries.Select(x=>new LegendEntry{MediumId=x.MediumId,Text=x.Text,Color=x.Color}));
        TagBox.Text=item.Tag;TitleBox.Text=item.LegendTitle;FunctionBox.Text=item.Function;LocationBox.Text=item.InstallationLocation;DescriptionBox.Text=item.Description;
        DialogTheme.Apply(this);var root=new DockPanel{Margin=new Thickness(18),Background=PageVisual.Color("#20262E")};
        var fields=new StackPanel();void Field(string label,TextBox box){fields.Children.Add(new TextBlock{Text=label,Margin=new Thickness(0,5,0,2)});fields.Children.Add(box);}
        Field("Tag",TagBox);Field("Legendentitel",TitleBox);
        fields.Children.Add(new TextBlock{Text="PID-Vorlage: Wolkenkammer V18 / Vakuumsystem V12. Zeilentexte und Hex-Farben sind editierbar.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,10)});
        var fieldGroup=DialogTheme.Group("Legendenangaben",fields);DockPanel.SetDock(fieldGroup,Dock.Top);root.Children.Add(fieldGroup);
        var bottom=new StackPanel();var rowsButtons=new StackPanel{Orientation=Orientation.Horizontal};
        void Button(string label,Action action){var button=new Button{Content=label};button.Click+=(_,_)=>action();rowsButtons.Children.Add(button);}
        Button("Zeile hinzufügen",()=>Rows.Add(new(){Text="Neuer Eintrag"}));
        Button("Zeile entfernen",()=>{if(_grid.SelectedItem is LegendEntry row)Rows.Remove(row);});
        Button("Projektmedien laden",()=>{Rows.Clear();foreach(var m in project.Media)Rows.Add(new(){MediumId=m.Id,Text=m.Name,Color=m.Color});});
        Button("Medienquellen erstellen",()=>{_createSources=true;Title="Legende bearbeiten · Medienquellen werden beim Übernehmen erstellt";});
        bottom.Children.Add(rowsButtons);
        var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,10,0,0)};
        var ok=new Button{Content="Übernehmen",IsDefault=true};ok.Click+=(_,_)=>{_grid.CommitEdit(DataGridEditingUnit.Cell,true);_grid.CommitEdit(DataGridEditingUnit.Row,true);var reason=TryApply();if(reason is not null){MessageBox.Show(this,reason,"Legende prüfen");return;}if(_createSources)MediaSettings.CreateSources(_project,_item.Page);MediaSettings.Synchronize(_project);DialogResult=true;};
        buttons.Children.Add(ok);buttons.Children.Add(new Button{Content="Abbrechen",IsCancel=true});bottom.Children.Add(buttons);DockPanel.SetDock(bottom,Dock.Bottom);root.Children.Add(bottom);
        _grid.Columns.Add(new DataGridTextColumn{Header="Textfenster / Bezeichnung",Binding=new Binding("Text"),MinWidth=300,Width=new DataGridLength(1,DataGridLengthUnitType.Star)});
        _grid.Columns.Add(ColorPicker.Column("Linienfarbe (#RRGGBB)"));
        _grid.ItemsSource=Rows;root.Children.Add(_grid);Content=root;
    }
    internal string? TryApply()
    {
        if(string.IsNullOrWhiteSpace(TagBox.Text)||_project.Equipment.Any(x=>x.Id!=_item.Id&&x.Tag.Equals(TagBox.Text.Trim(),StringComparison.OrdinalIgnoreCase)))return "Einen eindeutigen Tag eintragen.";
        if(string.IsNullOrWhiteSpace(TitleBox.Text))return "Einen Legendentitel eintragen.";
        if(Rows.Any(x=>string.IsNullOrWhiteSpace(x.Text)||!Regex.IsMatch(x.Color??"", "^#[0-9a-fA-F]{6}$")))return "Bezeichnungen und gültige Hex-Farben (#RRGGBB) eintragen.";
        _item.Tag=TagBox.Text.Trim();_item.LegendTitle=TitleBox.Text.Trim();
        foreach(var row in Rows){var medium=_project.Media.FirstOrDefault(m=>m.Id==row.MediumId)??_project.Media.FirstOrDefault(m=>m.Name==row.Text.Trim());if(medium is null){medium=new(){Name=row.Text.Trim(),Color=row.Color,Type=MediumType.Liquid};_project.Media.Add(medium);}else{medium.Name=row.Text.Trim();medium.Color=row.Color;}row.MediumId=medium.Id;}
        _item.LegendEntries=Rows.Select(x=>new LegendEntry{MediumId=x.MediumId,Text=x.Text.Trim(),Color=x.Color.ToUpperInvariant()}).ToList();
        return null;
    }
}
