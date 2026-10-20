using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
internal static class FlowSketchSelfTest
{
    internal static void Run(string output,Action<bool,string> check)
    {
        var cases=new List<object>();var p=ProjectServices.NewBlank();var v=new SceneValues();
        var water=new MediumDefinition{Name="Wasser",Type=MediumType.Liquid,Color="#0000FF"};var gas=new MediumDefinition{Name="Gas",Type=MediumType.Gas,Color="#FFAA33"};p.Media.AddRange([water,gas]);
        EquipmentItem Item(EquipmentType type,string tag,double x,double y)=>new(){Type=type,Variant=type==EquipmentType.IsolationValve?"3-Wege":"",Tag=tag,Actuator=ActuatorType.Manual,MediumId=water.Id,MediaName="Wasser",MediumType=MediumType.Liquid,X=x,Y=y,Width=.07,Height=.08};
        var source=Item(EquipmentType.MediaSource,"Quelle",.05,.18);var splitter=Item(EquipmentType.IsolationValve,"Dreiwege",.3,.18);var tank=Item(EquipmentType.Vessel,"Tank",.6,.18);var outlet=Item(EquipmentType.IsolationValve,"Ausgang",.78,.18);outlet.Variant="2-Wege";p.Equipment.AddRange([source,splitter,tank,outlet]);v.EquipmentStates[splitter.Id]="A-B";v.EquipmentStates[outlet.Id]="Offen";v.EquipmentStates[tank.Id]="Leer";
        PipeSegment Pipe(EquipmentItem? a,string ap,EquipmentItem? b,string bp){var pipe=new PipeSegment{Points=[new(.1,.2),new(.5,.2)],StartConnection=a is null?null:new(){EquipmentId=a.Id,Port=ap},EndConnection=b is null?null:new(){EquipmentId=b.Id,Port=bp}};p.Pipes.Add(pipe);return pipe;}
        var reversed=Pipe(splitter,"Left",source,"Right");var input=Pipe(splitter,"Right",tank,"Left1");var outputPipe=Pipe(tank,"Right1",outlet,"Left");var end=Pipe(outlet,"Right",null,"");end.Points[^1]=new(.94,.22);
        NetworkScene Capture(string name){var n=PipeNetwork.EvaluateScene(p,v);cases.Add(new{name,project=DocumentExport.Snapshot(p),scenario=new Scenario{EquipmentStates=new(v.EquipmentStates),TankStates=new(v.TankStates),SensorValues=new(v.SensorValues)},expected=n.Pipes,alarms=SensorEvaluation.Alarms(p,v).Count});return n;}
        var scene=Capture("Quelle durch leeren Tank zum Ausgang");
        check(scene.Pipes[reversed.Id].Spans.All(s=>s.Direction==-1),"Animation eines rückwärts gezeichneten Rohrs folgt der Quelle");
        check(scene.Pipes[input.Id].Flowing&&scene.Pipes[outputPipe.Id].Flowing&&scene.Pipes[end.Id].Flowing,"Quelle versorgt Tank und dessen offenen Ausgang");
        check(v.EquipmentStates[tank.Id]=="Leer"&&!v.TankStates.ContainsKey(tank.Id),"Zufluss ändert den vorgegebenen Tankzustand nicht");
        v.EquipmentStates[source.Id]="Inaktiv";v.EquipmentStates[tank.Id]="Befüllt";scene=Capture("Befüllter Tank ohne Quelle");
        check(scene.Pipes[outputPipe.Id].Flowing&&scene.Pipes[end.Id].MediumType==MediumType.Liquid&&!scene.Pipes[input.Id].Flowing,"Getrennter befüllter Tank versorgt Ausgang, nicht Sackleitung zur Quelle");
        v.TankStates[tank.Id]=new(){MediumId=gas.Id};scene=Capture("Tankmedium Gas im Schritt");
        check(scene.Pipes[end.Id].MediumType==MediumType.Gas,"Behältermedium übernimmt Typ aus der Schrittzuweisung");
        v.EquipmentStates[outlet.Id]="Geschlossen";scene=Capture("Ausgang geschlossen");check(!scene.Pipes.Values.Any(a=>a.Flowing),"Geschlossener Ausgang ohne erreichbares Ziel bleibt statisch");
        var other=Item(EquipmentType.Vessel,"Tank 2",.6,.6);p.Equipment.Add(other);var link=Pipe(tank,"Bottom1",other,"Top1");v.EquipmentStates[other.Id]="Befüllt";scene=Capture("Zwei Tanks ohne Quelle");check(!scene.Pipes[link.Id].Flowing,"Direkter Weg zwischen Behältern ohne Quelle animiert nicht");
        v.EquipmentStates[source.Id]="Aktiv";v.EquipmentStates[outlet.Id]="Offen";scene=Capture("Quelle gewinnt vor Tankversorgung");check(scene.Pipes[end.Id].MediumType==MediumType.Liquid,"Erreichbare Quelle bestimmt das Durchflussmedium statt eigener Tankversorgung");
        var sensor=Item(EquipmentType.Instrument,"PE1234",.25,.55);sensor.Height=sensor.Width*297/210;sensor.Color="#000000";sensor.Sensor.Signal="PE";sensor.Sensor.Number="1234";p.Equipment.Add(sensor);
        check(sensor.Sensor.RawMax==27648&&sensor.Sensor.Unit=="%"&&sensor.Sensor.Limits.All(l=>l.Enabled),"Sensor-Standardwerte und aktive Alarmgrenzen");
        v.SensorValues[sensor.Id]=27648;Capture("Sensor High-High");check(SensorEvaluation.Alarms(p,v).Count==2,"High und High-High sind bei 100 Prozent aktiv");
        v.SensorValues[sensor.Id]=0;Capture("Sensor Low-Low");check(SensorEvaluation.Alarms(p,v).Count==2,"Low und Low-Low sind bei 0 Prozent aktiv");
        v.SensorValues.Remove(sensor.Id);Capture("Sensor ohne Vorgabe");check(SensorEvaluation.Alarms(p,v).Count==0,"Ohne vorgegebenen Rohwert keine erfundenen Sensoralarme");
        var d=new EditorDialog(null,"Sensor");new SensorEditor(d,sensor.Sensor);check(d.Control("Skalierung") is Expander{IsExpanded:false}&&d.Control("Alarmwerte") is Expander{IsExpanded:false},"Sensor-Skalierung und Alarme sind standardmäßig eingeklappt");
        var navProject=ProjectServices.NewBlank();var navValve=new EquipmentItem{Type=EquipmentType.IsolationValve,Actuator=ActuatorType.Manual,Tag="V"};navProject.Equipment.Add(navValve);var navScenario=new Scenario{Name="Vorschau"};navProject.Scenarios.Add(navScenario);var first=new ControlStep{Name="Eins",ScenarioId=navScenario.Id};var second=new ControlStep{Name="Zwei",ScenarioId=navScenario.Id};navProject.ControlSteps.AddRange([first,second]);
        var stateEditor=new StateEditor(null,navProject,navScenario,null);
        IEnumerable<DependencyObject> Children(DependencyObject node){yield return node;for(var i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)foreach(var child in Children(VisualTreeHelper.GetChild(node,i)))yield return child;}
        var navigation=Children(stateEditor).OfType<ComboBox>().FirstOrDefault(c=>c.Items.OfType<ControlStep>().Any());
        if(navigation is null){var content=(FrameworkElement)stateEditor.Content;content.Measure(new Size(1100,700));content.Arrange(new Rect(0,0,1100,700));content.UpdateLayout();navigation=Children(content).OfType<ComboBox>().First(c=>c.Items.OfType<ControlStep>().Any());}
        navigation.SelectedItem=stateEditor.Working.ControlSteps.First(t=>t.Id==first.Id);stateEditor.Rows.Single().State="Geschlossen";navigation.SelectedItem=stateEditor.Working.ControlSteps.First(t=>t.Id==second.Id);navigation.SelectedItem=stateEditor.Working.ControlSteps.First(t=>t.Id==first.Id);
        check(stateEditor.Rows.Single().State=="Geschlossen"&&!first.EquipmentStates.ContainsKey(navValve.Id),"Schrittwechsel erhält Arbeitskopie und lässt Original bis Übernehmen unverändert");
        var addButton=Children((DependencyObject)stateEditor.Content).OfType<Button>().First(b=>b.Content?.ToString()=="Neuer Schritt");addButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));check(stateEditor.Working.ControlSteps.Count==3,"Neuer Schritt wird im geöffneten Editor angelegt");
        var removeButton=Children((DependencyObject)stateEditor.Content).OfType<Button>().First(b=>b.Content?.ToString()=="Schritt löschen");removeButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));check(stateEditor.Working.ControlSteps.Count==2&&navProject.ControlSteps.Count==2,"Schritt entfernen bleibt bis Übernehmen in der Arbeitskopie");
        var legend=Item(EquipmentType.Legend,"Legende",.05,.55);legend.Width=.3;legend.Height=.2;legend.Y=.7;legend.AutomaticLegend=true;p.Equipment.Add(legend);MediaSettings.Synchronize(p);check(legend.LegendEntries.Count==2,"Neue Legende verwendet definierte Projektmedien");
        MediaSettings.CreateSources(p,1);var count=p.Equipment.Count(x=>x.Type==EquipmentType.MediaSource);MediaSettings.CreateSources(p,1);check(count==2&&p.Equipment.Count(x=>x.Type==EquipmentType.MediaSource)==2,"Medienquellen-Erstellung ist ohne Duplikate wiederholbar");
        water.Color="#112233";MediaSettings.Synchronize(p);check(source.Color=="#112233"&&legend.LegendEntries[0].Color=="#112233"&&MediaSettings.Color(p,MediumType.Liquid,"Wasser")=="#112233","Medienfarbe aktualisiert Quelle, Rohrpalette und Legende");
        var scenario=new Scenario{Name="Medien"};scenario.TankStates[tank.Id]=new(){MediumId=water.Id};var step=new ControlStep{ScenarioId=scenario.Id,Name="Gas"};step.TankStates[tank.Id]=new(){MediumId=gas.Id};check(SceneValues.From(scenario,step).TankStates[tank.Id].MediumId==gas.Id,"Behältermedium wird im Schritt einzeln überschrieben");
        var file=Path.Combine(output,"flowsketch-fixture.pid2process.json");ProjectServices.Save(file,p);var loaded=ProjectServices.Load(file);check(loaded.Media.Count==2&&loaded.Equipment.Single(e=>e.Id==sensor.Id).Sensor.Limits.Count==4,"Neue Medien- und Sensordaten im JSON-Roundtrip");
        File.WriteAllText(Path.Combine(output,"flowsketch-cases.json"),JsonSerializer.Serialize(cases,ProjectServices.JsonOptions));
        var white=BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{255,255,255,255},4);var image=PageVisual.Render(p,1,1600,210d/297,new Scenario{EquipmentStates=new(v.EquipmentStates),TankStates=new(v.TankStates)},white);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using(var stream=File.Create(Path.Combine(output,"flowsketch-media-sensor.png")))encoder.Save(stream);
    }
}
