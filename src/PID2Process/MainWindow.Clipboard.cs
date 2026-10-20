using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace PID2Process;
public partial class MainWindow
{
    private const string SelectionClipboardFormat="FlowSketch.Selection.1";
    private string? _selectionClipboard;
    private int _pasteSequence;
    private void CopySelection()
    {
        if(Current is not {} doc||_selection.Count==0)return;
        _selectionClipboard=JsonSerializer.Serialize(OverlayClipboard.Copy(doc.Project,_selection,doc.Page+1),ProjectServices.JsonOptions);_pasteSequence=0;
        try{Clipboard.SetData(SelectionClipboardFormat,_selectionClipboard);StatusText.Text=$"{_selection.Count} Objekte kopiert. STRG+V fügt sie im aktiven Plan ein.";}catch{StatusText.Text="Auswahl intern kopiert; Zwischenablage ist belegt.";}
    }
    private void PasteSelection()
    {
        if(Current is not {} doc)return;
        try
        {
            var json=_selectionClipboard;try{if(Clipboard.ContainsData(SelectionClipboardFormat)&&Clipboard.GetData(SelectionClipboardFormat) is string text)json=text;}catch{}
            if(json is null)return;var clip=JsonSerializer.Deserialize<OverlayClip>(json,ProjectServices.JsonOptions);if(clip is null)return;
            var point=Viewport.TranslatePoint(new Point(Viewport.ViewportWidth/2,Viewport.ViewportHeight/2),Sheet);var center=new Point(Math.Clamp(point.X/Sheet.Width+_pasteSequence*.012,0,1),Math.Clamp(point.Y/Sheet.Height+_pasteSequence*.012,0,1));
            var selected=OverlayClipboard.Paste(doc.Project,clip,doc.Page+1,Sheet.Width/doc.Zoom,Sheet.Height/doc.Zoom,center);if(selected.Count==0)return;
            CancelPlacement();ClearSelection();_selection.UnionWith(selected);_selected=doc.Project.Equipment.FirstOrDefault(x=>selected.Contains(x.Id));_pasteSequence++;Changed();Draw();
        }
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Auswahl einfügen");}
    }
    private void ClipboardMenu(ContextMenu menu){AddMenu(menu,"Kopieren",CopySelection);var copy=(MenuItem)menu.Items[^1];copy.InputGestureText="Strg+C";copy.IsEnabled=_selection.Count>0;AddMenu(menu,"Einfügen",PasteSelection);((MenuItem)menu.Items[^1]).InputGestureText="Strg+V";}
}
