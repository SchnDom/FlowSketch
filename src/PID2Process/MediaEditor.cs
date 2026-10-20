using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace PID2Process;
internal sealed class MediaEditor : Window
{
    internal EngineeringProject Working;
    private readonly DataGrid _grid=new(){AutoGenerateColumns=false,CanUserAddRows=false};
    internal MediaEditor(Window? owner,EngineeringProject p)
    {
        if(owner?.IsVisible==true)Owner=owner;Title="Medien";Width=800;Height=720;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        Working=DocumentExport.Snapshot(p);MediaSettings.Normalize(Working);var rows=new ObservableCollection<MediumDefinition>(Working.Media);
        var oldTypes=Working.Media.ToDictionary(m=>m.Id,m=>m.Type);
        DialogTheme.Apply(this);var root=new DockPanel{Margin=new Thickness(16),Background=PageVisual.Color("#20262E")};var palette=new StackPanel();
        palette.Children.Add(new TextBlock{Text="Standardfarben je Typ; individuelle Medienfarben in der Tabelle."});
        var pickers=new Dictionary<string,ColorPicker>();
        foreach(var (label,value) in new[]{("Leer",Working.MediaColors.Empty),("Flüssig",Working.MediaColors.Liquid),("Fest",Working.MediaColors.Solid),("Gasförmig",Working.MediaColors.Gas),("Gemisch",Working.MediaColors.Mixed)}){var line=new DockPanel();line.Children.Add(new TextBlock{Text=label,Width=100});var picker=new ColorPicker(value);pickers[label]=picker;line.Children.Add(picker);palette.Children.Add(line);}
        var paletteGroup=DialogTheme.Group("Standardfarben",palette);DockPanel.SetDock(paletteGroup,Dock.Top);root.Children.Add(paletteGroup);var buttons=new WrapPanel();
        void Button(string label,Action action){var b=new Button{Content=label};b.Click+=(_,_)=>action();buttons.Children.Add(b);}
        Button("Hinzufügen",()=>rows.Add(new(){Name="Neues Medium",Color=Working.MediaColors.Liquid}));
        Button("Entfernen",()=>{if(_grid.SelectedItem is MediumDefinition m){if(p.Equipment.Any(e=>e.MediumId==m.Id)||p.Scenarios.Any(s=>s.TankStates.Values.Any(t=>t.MediumId==m.Id))||p.ControlSteps.Any(s=>s.TankStates.Values.Any(t=>t.MediumId==m.Id))){MessageBox.Show(this,"Medium wird verwendet.");return;}rows.Remove(m);}});
        Button("Übernehmen",()=>{_grid.CommitEdit(DataGridEditingUnit.Cell,true);_grid.CommitEdit(DataGridEditingUnit.Row,true);
            if(rows.Any(m=>string.IsNullOrWhiteSpace(m.Name)||!ColorPicker.IsValid(m.Color))||rows.GroupBy(m=>m.Name.Trim(),StringComparer.OrdinalIgnoreCase).Any(g=>g.Count()>1)||pickers.Values.Any(c=>!ColorPicker.IsValid(c.Value))){MessageBox.Show(this,"Eindeutige Bezeichnungen und gültige Farben erforderlich.");return;}
            var next=new MediaPalette{Empty=pickers["Leer"].Value,Liquid=pickers["Flüssig"].Value,Solid=pickers["Fest"].Value,Gas=pickers["Gasförmig"].Value,Mixed=pickers["Gemisch"].Value};
            foreach(var m in rows)if(m.Color==Working.MediaColors.Get(oldTypes.GetValueOrDefault(m.Id,MediumType.Liquid)))m.Color=next.Get(m.Type);
            Working.MediaColors=next;Working.Media=rows.ToList();foreach(var m in Working.Media)m.Name=m.Name.Trim();DialogResult=true;
        });
        buttons.Children.Add(new Button{Content="Abbrechen",IsCancel=true});DockPanel.SetDock(buttons,Dock.Bottom);root.Children.Add(buttons);
        _grid.Columns.Add(new DataGridTextColumn{Header="Bezeichnung",Binding=new Binding("Name"),Width=new DataGridLength(1,DataGridLengthUnitType.Star)});
        _grid.Columns.Add(new DataGridComboBoxColumn{Header="Typ",ItemsSource=MediaUi.Types,DisplayMemberPath="Label",SelectedValuePath="Value",SelectedValueBinding=new Binding("Type"){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},Width=140});
        _grid.Columns.Add(new DataGridTextColumn{Header="Farbe (#RRGGBB)",Binding=new Binding("Color"),Width=180});_grid.ItemsSource=rows;root.Children.Add(_grid);Content=root;
    }
}
internal static class MediaUi
{
    internal static void AddSelection(EditorDialog dialog,EngineeringProject project,MediumType type,string name)
    {
        dialog.FixedChoice("Mediumtyp",Types.First(t=>t.Value==type).Label,Types.Select(t=>t.Label).ToArray());
        dialog.FixedChoice("Name des Mediums",name,project.Media.Where(m=>m.Type==type).Select(m=>m.Name).ToArray());
        var types=(ComboBox)dialog.Control("Mediumtyp");var names=(ComboBox)dialog.Control("Name des Mediums");
        types.SelectionChanged+=(_,e)=>{if(e.Source!=types)return;var selected=Types.FirstOrDefault(t=>t.Label==types.SelectedItem?.ToString())?.Value;var old=names.SelectedItem as string;var choices=project.Media.Where(m=>m.Type==selected).Select(m=>m.Name).ToArray();names.ItemsSource=choices;names.SelectedItem=choices.Contains(old)?old:choices.FirstOrDefault();};
    }
    internal sealed record Choice(MediumType Value,string Label);
    internal static Choice[] Types=[new(MediumType.Liquid,"Flüssig"),new(MediumType.Gas,"Gasförmig"),new(MediumType.Solid,"Fest"),new(MediumType.Mixed,"Gemisch")];
}
