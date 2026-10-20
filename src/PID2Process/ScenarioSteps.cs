namespace PID2Process;
internal static class ScenarioSteps
{
    internal static ControlStep? First(EngineeringProject p,Guid? scenarioId)=>p.ControlSteps.Where(t=>t.ScenarioId==scenarioId).OrderBy(t=>t.Name,ActuatorExcelExport.NameOrder).ThenBy(t=>t.Id).FirstOrDefault();
    internal static bool Ensure(EngineeringProject p)
    {
        var changed=false;foreach(var s in p.Scenarios)if(!p.ControlSteps.Any(t=>t.ScenarioId==s.Id)){p.ControlSteps.Add(new(){ScenarioId=s.Id,Name="Schritt 1"});changed=true;}return changed;
    }
}
