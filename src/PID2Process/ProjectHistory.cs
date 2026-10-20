using System.Text.Json;
namespace PID2Process;
internal sealed class ProjectHistory
{
    private const int Limit=100;
    private readonly List<string> _undo=[];
    private readonly List<string> _redo=[];
    private string _current="";
    internal bool CanUndo=>_undo.Count>0;
    internal bool CanRedo=>_redo.Count>0;
    private static string Capture(EngineeringProject project)
    {
        var copy=DocumentExport.Snapshot(project);copy.Approval=new();
        return JsonSerializer.Serialize(copy,ProjectServices.JsonOptions);
    }
    internal void Reset(EngineeringProject project){_undo.Clear();_redo.Clear();_current=Capture(project);}
    internal void Commit(EngineeringProject project)
    {
        var next=Capture(project);if(next==_current)return;
        if(_current.Length>0)_undo.Add(_current);if(_undo.Count>Limit)_undo.RemoveAt(0);
        _redo.Clear();_current=next;
    }
    internal EngineeringProject? Restore(bool redo)
    {
        var from=redo?_redo:_undo;var to=redo?_undo:_redo;if(from.Count==0)return null;
        to.Add(_current);_current=from[^1];from.RemoveAt(from.Count-1);
        var project=JsonSerializer.Deserialize<EngineeringProject>(_current,ProjectServices.JsonOptions)!;
        project.Approval.IsApproved=false;project.Approval.IsBlocked=true;project.Approval.BlockReason="Engineering-Änderung rückgängig gemacht oder wiederhergestellt. Erneute Freigabe erforderlich.";
        return project;
    }
}
