using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace PID2Process;

internal static class ProcessSelfTest
{
    internal static async Task Run(MainWindow window,EngineeringProject project,EquipmentItem valve,string output,Action<bool,string> check)
    {
        object? Invoke(string name,params object?[] args)=>typeof(MainWindow).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,args);
        valve.Function="Druckluftversorgung";valve.InstallationLocation="+A";valve.X=.35;valve.Y=.3;valve.Width=.08;valve.Height=.07;
        var source=new EquipmentItem{Type=EquipmentType.MediaSource,Tag="MED-101",Variant="Medienquelle",MediaName="Druckluft",MediumType=MediumType.Gas,Function=valve.Function,InstallationLocation="+A",X=.08,Y=.3,Width=.12,Height=.07};
        var text=new EquipmentItem{Type=EquipmentType.Text,Tag="TXT-101",Variant="Textfeld",TextContent="Grundtext",Function=valve.Function,X=.35,Y=.12,Width=.25,Height=.07};project.Equipment.AddRange([source,text]);
        while(project.Equipment.Count<100)project.Equipment.Add(new EquipmentItem{Type=EquipmentType.Instrument,Tag=$"PT-{project.Equipment.Count:000}",Variant="Druck",Function="Temperatur-Regelung ABC",InstallationLocation="+CUSTOMER",Page=2});
        var upstream=new PipeSegment{Tag="L-VOR",StartConnection=new(){EquipmentId=source.Id,Port="Right"},EndConnection=new(){EquipmentId=valve.Id,Port="Left"},Points=[new(.2,.335),new(.35,.335)]};
        var downstream=new PipeSegment{Tag="L-NACH",StartConnection=new(){EquipmentId=valve.Id,Port="Right"},Points=[new(.43,.335),new(.65,.335),new(.65,.6)]};
        var branch=new PipeSegment{Tag="L-ABZ",StartConnection=new(){PipeId=downstream.Id,Segment=1,Fraction=.5},Points=[new(.65,.45),new(.8,.45)]};project.Pipes.AddRange([upstream,downstream,branch]);
        var scenario=new Scenario{Name="Befüllen",Description="Anlage befüllen"};scenario.EquipmentStates[valve.Id]="Geschlossen";scenario.TextValues[text.Id]="Bereit";project.Scenarios.Add(scenario);
        foreach(var name in new[]{"Stillstand","Regelbetrieb","Störung"})project.Scenarios.Add(new Scenario{Name=name});
        var open=new ControlStep{Name="Füllen starten",ScenarioId=scenario.Id,Actions="XV-101 öffnen",Tags=[valve.Tag]};open.EquipmentStates[valve.Id]="Offen";open.TextValues[text.Id]="Befüllung läuft";
        var shut=new ControlStep{Name="Füllen beenden",ScenarioId=scenario.Id};shut.EquipmentStates[valve.Id]="Geschlossen";
        var other=new ControlStep{Name="Regeln",ScenarioId=project.Scenarios[2].Id};project.ControlSteps.AddRange([open,shut,other]);
        var closed=SceneValues.From(scenario,null);var opened=SceneValues.From(scenario,open);
        check(PipeNetwork.Evaluate(project,closed)[upstream.Id].State=="Druckbeschlagen"&&PipeNetwork.Evaluate(project,closed)[downstream.Id].State=="Leer","Geschlossenes Ventil sperrt Medienweitergabe");
        check(PipeNetwork.Evaluate(project,opened)[branch.Id].State=="Druckbeschlagen","Offenes Ventil gibt Gas an mehrteiligen Rohrpfad und Abzweig weiter");
        foreach(var (type,state)in new[]{(MediumType.Liquid,"Befüllt"),(MediumType.Solid,"Materialführung"),(MediumType.Mixed,"Gemisch")}){source.MediumType=type;check(PipeNetwork.Evaluate(project,opened)[downstream.Id].State==state,"Rohrzustand aus Mediumtyp: "+type);}source.MediumType=MediumType.Gas;
        check(opened.TextValues[text.Id]=="Befüllung läuft"&&closed.TextValues[text.Id]=="Bereit","Schritttext überschreibt Szenariotext");
        var before=PipeNetwork.Resolve(project,branch)[0];downstream.Points[2].Y+=.1;var after=PipeNetwork.Resolve(project,branch)[0];check(Math.Abs(after.Y-before.Y-.05)<.00001,"Mittiger Abzweig folgt verschobenem Rohr-Endpunkt");
        check(PipeNetwork.Resolve(project,new PipeSegment{X1=.1,Y1=.2,X2=.3,Y2=.4})[1].Y==.4,"Alte Zweipunkt-Rohre bleiben lesbar");
        Invoke("RefreshInspector");var inspector=(TabControl)window.FindName("InspectorTabs");foreach(var (method,index)in new[]{("Equipment_Click",0),("Scenarios_Click",1),("Steps_Click",1),("Alarms_Click",2)}){Invoke(method,window,new RoutedEventArgs());check(inspector.SelectedIndex==index,"Rechter Tab: "+method);}
        ((ComboBox)window.FindName("ScenarioBox")).SelectedItem=scenario;check(((ComboBox)window.FindName("StepBox")).Items.Count==2,"Schrittauswahl auf Szenario begrenzt");
        async Task ShowEditor(StateEditor editor,Action<StateEditor> action,bool accept,string name)
        {
            Exception? error=null;var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(200)};timer.Tick+=(_,_)=>{timer.Stop();try{action(editor);editor.UpdateLayout();Capture(editor,Path.Combine(output,name+".png"));if(accept)Walk(editor).OfType<Button>().Single(b=>Equals(b.Content,"Übernehmen")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));else editor.Close();}catch(Exception ex){error=ex;editor.Close();}};timer.Start();editor.ShowDialog();if(error is not null)throw error;await Task.Delay(20);
        }
        var editor=new StateEditor(window,project,scenario,null);
        await ShowEditor(editor,e=>{
            check(e.Rows.Count==100&&e.StateGrid.EnableRowVirtualization,"100 Symbole in virtualisiertem Szenarioeditor");
            e.FunctionFilter.SelectedItem=valve.Function;check(e.StateGrid.Items.Count==3,"Funktionsfilter reduziert 100 Symbole auf drei");
            e.Rows.Single(r=>r.Id==valve.Id).State="Offen";e.Search.Text="TXT-101";check(e.StateGrid.Items.Count==1,"Suche kombiniert mit Funktionsfilter");
            e.Rows.Single(r=>r.Id==text.Id).TextValue="Geprüfter Szenariotext";e.Rows.Single(r=>r.Id==text.Id).OverrideText=true;
        },true,"scenario-100-filtered");
        check(editor.Scenario!.EquipmentStates[valve.Id]=="Offen"&&editor.Scenario.TextValues[text.Id]=="Geprüfter Szenariotext","Übernehmen bewahrt auch ausgefilterte Änderungen");
        check(scenario.EquipmentStates[valve.Id]=="Geschlossen","Editor bearbeitet transaktionale Kopie");
        var stepEditor=new StateEditor(window,project,null,open);
        await ShowEditor(stepEditor,e=>{e.Rows.Single(r=>r.Id==valve.Id).State="Szenario übernehmen";e.Rows.Single(r=>r.Id==text.Id).OverrideText=false;},true,"step-100");
        check(!stepEditor.Step!.EquipmentStates.ContainsKey(valve.Id)&&!stepEditor.Step.TextValues.ContainsKey(text.Id),"Schritt kann Zustände und Text wieder vom Szenario übernehmen");
        var cancelled=new StateEditor(window,project,scenario,null);await ShowEditor(cancelled,e=>e.Rows.Single(r=>r.Id==valve.Id).State="Störung",false,"scenario-cancel");check(scenario.EquipmentStates[valve.Id]=="Geschlossen","Abbrechen verändert weder Szenario noch Schritte");
        // Exercise the real sidebar modal workflow and commit the resulting copy.
        Exception? modalError=null;var modalTimer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(150)};modalTimer.Tick+=(_,_)=>{modalTimer.Stop();try{var dialog=Application.Current.Windows.OfType<StateEditor>().Single();Walk(dialog).OfType<TextBox>().Single(t=>Equals(t.Tag,"Beschreibung")).Text="Über rechten Tab bearbeitet";Walk(dialog).OfType<Button>().Single(b=>Equals(b.Content,"Übernehmen")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}catch(Exception ex){modalError=ex;foreach(var d in Application.Current.Windows.OfType<StateEditor>().ToArray())d.Close();}};modalTimer.Start();Invoke("EditScenario",scenario);if(modalError is not null)throw modalError;
        scenario=project.Scenarios.Single(s=>s.Id==scenario.Id);check(scenario.Description=="Über rechten Tab bearbeitet"&&project.ControlSteps.Count(s=>s.ScenarioId==scenario.Id)==2,"Rechter Szenarioeditor übernimmt und erhält mehrere Schritte");
        Invoke("Pipe_Click",window,new RoutedEventArgs());var sheet=(Canvas)window.FindName("Sheet");Invoke("AddPipePoint",new Point(sheet.Width*.1,sheet.Height*.8),false);Invoke("AddPipePoint",new Point(sheet.Width*.25,sheet.Height*.8),false);Invoke("UpdatePipePreview",new Point(sheet.Width*.25,sheet.Height*.9));check(sheet.Children.OfType<System.Windows.Shapes.Polyline>().Any(p=>!p.IsHitTestVisible&&p.Opacity<1&&p.Points.Count==3),"Rohrpfad zeigt mehrteilige Vorschau am Mauszeiger");Invoke("AddPipePoint",new Point(sheet.Width*.25,sheet.Height*.9),true);check(project.Pipes.Last().Points.Count==3,"Mehrteiliger Rohrpfad wird gespeichert");
        Invoke("ShowPipeHandles",downstream);var handle=sheet.Children.OfType<Thumb>().Last();var oldPoint=PipeNetwork.Resolve(project,branch)[0];handle.RaiseEvent(new DragDeltaEventArgs(0,30){RoutedEvent=Thumb.DragDeltaEvent});check(PipeNetwork.Resolve(project,branch)[0].Y>oldPoint.Y,"Native Endpunkt-Griffe führen Abzweig nach");handle.RaiseEvent(new DragCompletedEventArgs(0,30,false){RoutedEvent=Thumb.DragCompletedEvent});
        var shape=PageVisual.Symbol(valve,1000,800,"Geschlossen");check(((Grid)shape).Children.OfType<Border>().Single().Background.ToString()=="#FFFFDF9A","Geschlossenes Ventil mit kontrastierender Hintergrundfarbe");
        var cases=new[]{("scenario",scenario,(ControlStep?)null),("open",scenario,project.ControlSteps.Single(s=>s.Id==open.Id)),("closed",scenario,project.ControlSteps.Single(s=>s.Id==shut.Id))}.Select(c=>new{name=c.Item1,scenarioId=c.Item2.Id,stepId=c.Item3?.Id,expected=PipeNetwork.Evaluate(project,SceneValues.From(c.Item2,c.Item3))}).ToArray();
        File.WriteAllText(Path.Combine(output,"scene-cases.json"),JsonSerializer.Serialize(cases,ProjectServices.JsonOptions));ProjectServices.Save(Path.Combine(output,"process-fixture.pid2process.json"),project);
        Invoke("Fit");await Task.Delay(250);Invoke("Draw");Capture(window,Path.Combine(output,"process-editor.png"));
    }
    private static IEnumerable<DependencyObject> Walk(DependencyObject root){for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var c=VisualTreeHelper.GetChild(root,i);yield return c;foreach(var d in Walk(c))yield return d;}}
    private static void Capture(Window w,string path){w.UpdateLayout();var image=new RenderTargetBitmap((int)w.ActualWidth,(int)w.ActualHeight,96,96,PixelFormats.Pbgra32);image.Render(w);var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(image));using var file=File.Create(path);e.Save(file);}
}
