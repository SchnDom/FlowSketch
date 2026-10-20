using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
internal static class SensorDrawingSelfTest
{
    internal static async Task Run(string output,Action<bool,string> check)
    {
        var connectionCases=new List<object>();
        void Capture(EngineeringProject project,EquipmentItem sensor){var copy=DocumentExport.Snapshot(project);connectionCases.Add(new{project=copy,item=copy.Equipment.Single(e=>e.Id==sensor.Id),points=SensorConnections.Resolve(project,sensor)});}
        var p=ProjectServices.NewBlank();var scenario=new Scenario{Name="Alt ohne Schritt"};p.Scenarios.Add(scenario);var orphan=new ControlStep{Name="Nicht zugeordnet"};p.ControlSteps.Add(orphan);p.Approval.IsApproved=true;ProjectServices.Normalize(p);
        check(p.ControlSteps.Count(t=>t.ScenarioId==scenario.Id)==1&&orphan.ScenarioId is null&&!p.Approval.IsApproved,"Leere Altszenarien erhalten ersten Schritt ohne Zuordnung alter Einzel-Schritte; Freigabe widerrufen");
        var editor=new StateEditor(null,p,scenario,null);check(editor.Step is not null&&editor.StepPicker.Items.Count==1&&editor.StepPicker.Items.Cast<ControlStep>().All(t=>t.Id!=Guid.Empty),"Szenarioeditor hat immer ersten Schritt und keine Grundstellung");editor.DeleteStep();check(editor.Working.ControlSteps.Any(t=>t.ScenarioId==scenario.Id),"Letzter Szenarioschritt lässt sich nicht löschen");
        var item=new EquipmentItem{Type=EquipmentType.IsolationValve,X=.2,Y=.2,Width=.04,Height=.06};bool smooth=true;double? previous=null;
        for(int i=-300;i<=300;i++){OverlayTransforms.Resize(item,.04+i/2000d,.06-i/1400d,true,.04,.06,2000,1400);smooth&=double.IsFinite(item.Width)&&Math.Abs(item.Width/item.Height-2d/3)<1e-10&&item.Width>=.001&&item.X+item.Width<=1+1e-10;if(previous is double old)smooth&=Math.Abs(item.Width-old)<.002;previous=item.Width;}
        check(smooth,"SHIFT-Skalierung bleibt bei gegensinnigen Bewegungen stetig, begrenzt und unverzerrt");OverlayTransforms.Resize(item,.06,.09,true,.04,.06,2000,1400);var result=item.Width;for(int i=0;i<100;i++)OverlayTransforms.Resize(item,.06,.09,true,.04,.06,2000,1400);check(Math.Abs(item.Width-result)<1e-12,"Absolute Zeigerposition erzeugt keine sich aufsummierende Skalierung");
        var sensor=new EquipmentItem{Tag="TE-401",Type=EquipmentType.Instrument,X=.2,Y=.4,Width=.08,Height=.08,Color="#000000",Opacity=1};p.Equipment.Add(sensor);var pipe=new PipeSegment{Points=[new(.05,.65),new(.85,.65)]};p.Pipes.Add(pipe);SensorConnections.BindNearest(p,sensor);Capture(p,sensor);
        check(sensor.Sensor.Connection?.PipeId==pipe.Id&&Math.Abs(SensorConnections.Resolve(p,sensor)[^1].Y-.65)<1e-10,"Sensoranschluss entsteht unten und fängt ein Rohrsegment");
        foreach(var point in pipe.Points)point.Y=.7;sensor.X=.3;Capture(p,sensor);check(Math.Abs(SensorConnections.Resolve(p,sensor)[^1].Y-.7)<1e-10,"Sensoranschluss folgt Rohrgeometrie und Sensorbewegung");
        var emptyNetwork=PipeNetwork.EvaluateScene(p,new SceneValues());var snapshot=DocumentExport.Snapshot(p);snapshot.Equipment.Clear();check(JsonSerializer.Serialize(emptyNetwork.Pipes)==JsonSerializer.Serialize(PipeNetwork.EvaluateScene(snapshot,new SceneValues()).Pipes),"Grafischer Sensoranschluss beeinflusst Mediennetz nicht");
        var file=Path.Combine(output,"sensor-connection.pid2process.json");ProjectServices.Save(file,p);var loaded=ProjectServices.Load(file);check(loaded.Equipment.Single().Sensor.Connection?.PipeId==pipe.Id,"Sensor-Rohrbindung bleibt im JSON-Roundtrip erhalten");
        var target=SensorConnections.Resolve(p,sensor)[^1];OverlaySelection.Delete(p,new HashSet<Guid>{pipe.Id});Capture(p,sensor);check(sensor.Sensor.Connection is null&&Math.Abs(SensorConnections.Resolve(p,sensor)[^1].Y-target.Y)<1e-10,"Rohrlöschen löst Sensorbindung ohne Geometriesprung");
        foreach(var side in new[]{"Bottom","Left","Right","Top"}){sensor.Sensor.ConnectionSide=side;sensor.Sensor.ConnectionEnd=null;var points=SensorConnections.Resolve(p,sensor);var port=SignalGeometry.Port(sensor,side);Capture(p,sensor);check(points.Count>=2&&points[0].X==port.X&&points[0].Y==port.Y,"Sensoranschlussseite "+side);}
        sensor.Sensor.ConnectionSide="Hidden";Capture(p,sensor);check(SensorConnections.Resolve(p,sensor).Count==0,"Sensoranschluss ausblendbar");
        var atlas=ProjectServices.NewBlank();var view=new Scenario();atlas.Scenarios.Add(view);var cases=new List<object>();
        for(int i=0;i<5;i++)
        {
            var s=new EquipmentItem{Type=EquipmentType.Instrument,Tag=new[]{"LL","L","H","HH","Normal"}[i],X=.04+i*.19,Y=.12,Width=.11,Height=.11/.708,Color="#000000",Opacity=1};s.Sensor.Signal="TE";s.Sensor.Number="401";s.Sensor.ConnectionSide="Hidden";atlas.Equipment.Add(s);view.SensorValues[s.Id]=new[]{0d,1000d,26542d,27648d,13824d}[i];var values=SceneValues.From(view,null);var display=SensorEvaluation.Display(s,values);
            var expected=i is 0 or 3?"#FF0000":i is 1 or 2?"#FFFF00":"#FFFFFF";check((i<2?display.LowerColor:display.UpperColor)==expected&&(i==4?display.Text.Length==0:display.Text.Contains(new[]{"LL:","L:","H:","HH:"}[i])),"Sensorhalbkreis und Alarmwert: "+s.Tag);cases.Add(new{item=s,values=new{sensorValues=values.SensorValues},expected=display,project=atlas,points=SensorConnections.Resolve(atlas,s)});
        }
        Save(atlas,view,Path.Combine(output,"sensor-alarms.png"));
        var drawings=ProjectServices.NewBlank();foreach(var (variant,index) in new[]{"Linie","Kreis / Ellipse","Rechteck","Textfeld","Pfeil","Doppelpfeil"}.Select((v,i)=>(v,i)))drawings.Equipment.Add(new(){Type=EquipmentType.Drawing,Variant=variant,Tag="Intern-"+index,X=.05+index%3*.31,Y=.15+index/3*.4,Width=.2,Height=.2/.708,Color="#203544",TextContent="Nur Grafik",Opacity=1,Drawing=new(){Horizontal=variant is "Pfeil" or "Doppelpfeil"}});
        var circle=new EquipmentItem{Type=EquipmentType.Drawing,Variant="Kreis / Ellipse"};MainWindow.AnnotationBounds(circle,new Point(1950,900),new Point(2200,1400),2000,1000,true);check(Math.Abs(circle.Width*2000-circle.Height*1000)<1e-10&&circle.X+circle.Width<=1,"SHIFT-Zeichenkreis bleibt auch an Blattgrenzen kreisförmig");
        var a=drawings.Equipment[4];MainWindow.AnnotationBounds(a,new Point(300,200),new Point(100,350),2000,1000,false);check(a.Drawing.ReverseX&&AnnotationVisual.Path(a).StartsWith("M 100,0 L 0,100"),"Zeichenpfeil erhält Richtung auch bei Ziehen nach links");
        a.X=.36;a.Y=.55;a.Width=.2;a.Height=.2/.708;
        var drawingScenario=new Scenario();drawings.Scenarios.Add(drawingScenario);var drawingEditor=new StateEditor(null,drawings,drawingScenario,null);check(drawingEditor.Rows.Count==0&&ActuatorExcelExport.Rows(drawings).Count==0&&drawings.Equipment.All(x=>PipeNetwork.Ports(x).Length==0&&SignalGeometry.Ports(x).Length==0),"Zeichenelemente haben keine Zustandszeilen, Aktorenexport oder Prozessanschlüsse");
        await ExportService.ExportAsync(drawings,Path.Combine(output,"drawing-review"));
        Save(drawings,null,Path.Combine(output,"drawing-examples.png"));File.WriteAllText(Path.Combine(output,"sensor-drawing-cases.json"),JsonSerializer.Serialize(new{cases,connectionCases,drawings,paths=drawings.Equipment.Where(x=>x.Variant is "Linie" or "Pfeil" or "Doppelpfeil").ToDictionary(x=>x.Id,AnnotationVisual.Path)},ProjectServices.JsonOptions));
    }
    private static void Save(EngineeringProject p,Scenario? scenario,string file)
    {
        var white=BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{255,255,255,255},4);var bitmap=PageVisual.Render(p,1,1600,.708,scenario,white);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(file);encoder.Save(stream);
    }
}
