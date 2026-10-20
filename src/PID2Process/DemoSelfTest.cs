using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
internal static class DemoSelfTest
{
    internal static void Run(Action<bool,string> check)
    {
        var directory=Path.Combine(Environment.CurrentDirectory,"examples");var p=ProjectServices.Load(Path.Combine(directory,"FlowSketch-Demo.pid2process.json"));
        check(p.BlankSheet is {Format:"A2"}&&p.Equipment.Count>=30&&p.Scenarios.Count==7&&p.ControlSteps.Count==26,"Demo enthält A2-Blatt, 37 Symbole, sieben Szenarien und 26 Schritte");
        var segments=p.Pipes.SelectMany(pipe=>{var points=PipeNetwork.Resolve(p,pipe);return points.Zip(points.Skip(1),(a,b)=>(a,b,pipe.Id));}).ToArray();
        const double eps=1e-8;
        check(segments.All(s=>Math.Abs(s.a.X-s.b.X)<eps||Math.Abs(s.a.Y-s.b.Y)<eps),"Demo: sämtliche aufgelösten Rohrsegmente sind horizontal oder vertikal");
        var crossing=false;
        foreach(var a in segments)foreach(var b in segments.Where(b=>b.Id!=a.Id))
        {
            if(Math.Abs(a.a.X-a.b.X)<eps&&Math.Abs(b.a.Y-b.b.Y)<eps&&a.a.X>Math.Min(b.a.X,b.b.X)+eps&&a.a.X<Math.Max(b.a.X,b.b.X)-eps&&b.a.Y>Math.Min(a.a.Y,a.b.Y)+eps&&b.a.Y<Math.Max(a.a.Y,a.b.Y)-eps)crossing=true;
        }
        check(!crossing,"Demo: keine Rohrkreuzungen zwischen den getrennten Anlagenwegen");
        check(p.ControlSteps.All(t=>p.Scenarios.Any(s=>s.Id==t.ScenarioId))&&p.Pipes.All(pipe=>pipe.StartConnection?.NeedsReview!=true&&pipe.EndConnection?.NeedsReview!=true),"Demo: gültige Szenariozuordnung und normierte Geräteanschlüsse");
        SceneValues Values(int scenario,int step)=>SceneValues.From(p.Scenarios[scenario-1],p.ControlSteps.Where(t=>t.ScenarioId==p.Scenarios[scenario-1].Id).ElementAt(step-1));
        var pump=p.Equipment.Single(e=>e.Tag=="P-101");var tank=p.Equipment.Single(e=>e.Tag=="B-101");var fan=p.Equipment.Single(e=>e.Tag=="V-201");var heat=p.Equipment.Single(e=>e.Tag=="WT-401");
        var filling=PipeNetwork.EvaluateScene(p,Values(2,2));check(filling.Pipes.Values.Any(a=>a.Flowing)&&!filling.Errors.ContainsKey(pump.Id),"Demo: Wasserfüllung läuft mit versorgter Pumpe");
        var mixed=PipeNetwork.EvaluateScene(p,Values(3,4));check(mixed.Pipes.Values.Any(a=>a.Flowing&&a.MediumType==MediumType.Mixed),"Demo: Behälter versorgt Ausgang mit Schrittmedium Emulsion");
        check(PipeNetwork.EvaluateScene(p,Values(4,4)).Errors.GetValueOrDefault(fan.Id)=="Störung: Medium ungeeignet","Demo: ungeeignetes Ventilatormedium wird als Störung gezeigt");
        check(PipeNetwork.EvaluateScene(p,Values(6,1)).HeatExchangers[heat.Id]==(true,false)&&PipeNetwork.EvaluateScene(p,Values(6,2)).HeatExchangers[heat.Id]==(true,true)&&PipeNetwork.EvaluateScene(p,Values(6,3)).HeatExchangers[heat.Id]==(false,true),"Demo: Wärmetauscher zeigt primären, beide und sekundären Durchfluss unabhängig");
        check(SensorEvaluation.Alarms(p,Values(7,2)).Count==0&&SensorEvaluation.Alarms(p,Values(7,4)).Count==4,"Demo: Normalwert und High-/High-High-Sensoralarme reproduzierbar");
        var file=Path.Combine(directory,"FlowSketch-Demo.pid2process.json");var before=ProjectServices.HashFile(file);var window=new MainWindow();window.OpenDemo(file);
        var tab=(System.Windows.Controls.TabItem)((System.Windows.Controls.TabControl)window.FindName("DocumentTabs")).SelectedItem;
        window.ActiveProject!.Name="Bearbeitete Arbeitskopie";
        check(window.ActiveProject.Equipment.Count==p.Equipment.Count&&tab.Tag.GetType().GetField("SavePath")!.GetValue(tab.Tag) is null&&ProjectServices.HashFile(file)==before,"Demo-Menü öffnet unabhängige Arbeitskopie ohne Speicherpfad zur Vorlage");
        ActuatorExcelExport.Export(p,Path.Combine(directory,"FlowSketch-Demo-Aktoren.xlsx"));
        var scene=p.Scenarios[1];var selected=p.ControlSteps.Where(t=>t.ScenarioId==scene.Id).ElementAt(1);var snapshot=DocumentExport.Snapshot(p);var combined=SceneValues.From(scene,selected);var state=new Scenario{Name=scene.Name,EquipmentStates=combined.EquipmentStates,TextValues=combined.TextValues,TankStates=combined.TankStates,SensorValues=combined.SensorValues};
        var white=BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{255,255,255,255},4);var image=PageVisual.Render(snapshot,1,2600,420d/594,state,white);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var stream=File.Create(Path.Combine(directory,"FlowSketch-Demo.png"));encoder.Save(stream);
    }
}
