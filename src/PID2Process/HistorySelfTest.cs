using System.Text.Json;
namespace PID2Process;
internal static class HistorySelfTest
{
    internal static void Run(Action<bool,string> check)
    {
        var p=ProjectServices.NewBlank();var history=new ProjectHistory();history.Reset(p);
        check(!history.CanUndo&&!history.CanRedo,"Neuer Dokumentverlauf beginnt leer");
        var tank=new EquipmentItem{Type=EquipmentType.Vessel,Tag="B-1"};p.Equipment.Add(tank);history.Commit(p);var id=tank.Id;
        var scenario=new Scenario{Name="Betrieb"};p.Scenarios.Add(scenario);var step=new ControlStep{Name="Schritt 1",ScenarioId=scenario.Id};step.EquipmentStates[id]="Befüllt";step.TankStates[id]=new(){MediumType=MediumType.Liquid,MediumName="Wasser"};p.ControlSteps.Add(step);history.Commit(p);
        var json=JsonSerializer.Serialize(p,ProjectServices.JsonOptions);history.Commit(p);var undo=history.Restore(false)!;
        check(undo.Equipment.Single().Id==id&&undo.Scenarios.Count==0&&undo.ControlSteps.Count==0,"Undo fasst Szenario und Schritt als Aktion zusammen; keine zusätzlichen No-op-Einträge");
        var redo=history.Restore(true)!;check(redo.Scenarios.Single().Id==scenario.Id&&redo.ControlSteps.Single().EquipmentStates[id]=="Befüllt"&&redo.ControlSteps[0].TankStates[id].MediumName=="Wasser","Redo erhält IDs, Schrittzustand und Behältermedium");
        check(!redo.Approval.IsApproved&&redo.Approval.IsBlocked,"Undo/Redo stellt keine frühere Engineering-Freigabe wieder her");
        undo=history.Restore(false)!;undo.Name="Neue Richtung";history.Commit(undo);check(!history.CanRedo,"Neue Änderung nach Undo verwirft den Redo-Zweig");
        var isolated=new ProjectHistory();isolated.Reset(ProjectServices.NewBlank());check(!isolated.CanUndo&&history.CanUndo,"Dokumentverläufe sind voneinander unabhängig");
        var bounded=new ProjectHistory();bounded.Reset(p);for(var i=0;i<110;i++){p.Name="Änderung "+i;bounded.Commit(p);}var count=0;while(bounded.Restore(false) is not null)count++;check(count==100,"Undo-Verlauf ist auf 100 abgeschlossene Änderungen begrenzt");
    }
}
