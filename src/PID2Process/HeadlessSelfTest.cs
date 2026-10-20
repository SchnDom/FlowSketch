using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

// No Show/ShowDialog, foreground activation, mouse capture or injected input.
internal static class HeadlessSelfTest
{
    internal static async Task<int> Run()
    {
        var report=new List<string>();var output=Path.Combine(Environment.CurrentDirectory,"test-results");Directory.CreateDirectory(output);
        void Check(bool condition,string name){if(!condition)throw new InvalidOperationException(name);report.Add("PASS: "+name);}
        try
        {
            IconAssetBuilder.Write(Path.Combine(Environment.CurrentDirectory,"_media"));
            var fixture=Path.Combine(output,"process-fixture.pid2process.json");
            var pdf=Path.Combine(output,"model-pages.pdf");EditorSelfTest.MakePdf(pdf);var project=Fixture(pdf);ProjectServices.Save(fixture,project);var scenario=project.Scenarios.Single(s=>s.Name=="Befüllen");var step=project.ControlSteps.Single(s=>s.Name=="Füllen starten");
            var valve=project.Equipment.Single(x=>x.Tag=="XV-101");var source=project.Equipment.Single(x=>x.Type==EquipmentType.MediaSource);var text=project.Equipment.Single(x=>x.Type==EquipmentType.Text);
            var upstream=project.Pipes.Single(x=>x.Tag=="L-VOR");var downstream=project.Pipes.Single(x=>x.Tag=="L-NACH");var branch=project.Pipes.Single(x=>x.Tag=="L-ABZ");
            await SymbolSelfTest.Run(output,Check);
            SelectionSelfTest.Run(project,output,Check);
            InteractionSelfTest.Run(output,Check);
            ScenarioEditorSelfTest.Run(output,Check);
            DemoSelfTest.Run(Check);
            await SensorDrawingSelfTest.Run(output,Check);
            await ModernUiSelfTest.Run(output,Check);
            BrandingSelfTest.Run(output,Check);
            await BlankSheetSelfTest.Run(output,Check);
            await LegendSelfTest.Run(output,Check);
            FlowSketchSelfTest.Run(output,Check);
            ActuatorExcelSelfTest.Run(output,Check);
            ScenarioViewSelfTest.Run(Check);
            await SignalSelfTest.Run(output,Check);
            var closed=PipeNetwork.Evaluate(project,SceneValues.From(scenario,null));Check(closed[upstream.Id].State=="Druckbeschlagen"&&closed[downstream.Id].State=="Leer","Ventil geschlossen: Medium vor Ventil, kein Medium dahinter");
            foreach(var (type,state)in new[]{(MediumType.Gas,"Druckbeschlagen"),(MediumType.Liquid,"Befüllt"),(MediumType.Solid,"Materialführung"),(MediumType.Mixed,"Gemisch")}){source.MediumType=type;Check(PipeNetwork.Evaluate(project,SceneValues.From(scenario,step))[branch.Id].State==state,"Weitergabe an Pfad und Abzweig: "+type);}source.MediumType=MediumType.Gas;
            var inactive=SceneValues.From(scenario,step);inactive.EquipmentStates[source.Id]="Inaktiv";Check(PipeNetwork.Evaluate(project,inactive)[branch.Id].State=="Leer","Inaktive Medienquelle versorgt kein Rohr");
            var before=PipeNetwork.Resolve(project,branch)[0];downstream.Points[^1].Y+=.1;Check(Math.Abs(PipeNetwork.Resolve(project,branch)[0].Y-before.Y-.05)<.000001,"Mittiger Anschluss folgt bewegtem Endpunkt");downstream.Points[^1].Y-=.1;
            Check(PipeNetwork.DependsOn(project,branch,downstream.Id)&&!PipeNetwork.DependsOn(project,downstream,branch.Id),"Abhängigkeitserkennung verhindert zyklische Fangpunkte");
            var start=PipeNetwork.Resolve(project,upstream)[0];source.X+=.02;Check(Math.Abs(PipeNetwork.Resolve(project,upstream)[0].X-start.X-.02)<.000001,"Rohranschluss folgt bewegtem Symbol");source.X-=.02;
            Check(SceneValues.From(scenario,step).TextValues[text.Id]=="Befüllung läuft","Textinhalt wird pro Schritt überschrieben");
            var owner=new Window();var editor=new StateEditor(null,project,scenario,null);Check(!owner.IsVisible&&!editor.IsVisible,"Prüfung öffnet und bedient keine Bildschirmfenster");
            Check(editor.Rows.Count>=100&&editor.StateGrid.EnableRowVirtualization,"100 Symbole mit aktivierter Zeilenvirtualisierung");
            editor.FunctionFilter.SelectedItem="Druckluftversorgung";Check(editor.StateGrid.Items.Count==3,"Funktionsfilter bei 100 Symbolen");
            editor.Rows.Single(r=>r.Id==valve.Id).State="Offen";editor.Search.Text="TXT-101";Check(editor.StateGrid.Items.Count==1,"Suche und Funktionsfilter kombiniert");
            var textRow=editor.Rows.Single(r=>r.Id==text.Id);textRow.TextValue="Geprüft";textRow.OverrideText=true;Check(editor.SaveValues()&&editor.Step!.EquipmentStates[valve.Id]=="Offen"&&editor.Step.TextValues[text.Id]=="Geprüft","Ausgefilterte Zeilen und Text werden übernommen");
            Check(scenario.EquipmentStates[valve.Id]=="Geschlossen"&&project.ControlSteps.Count(s=>s.ScenarioId==scenario.Id)==2,"Transaktionale Kopie erhält Szenario und seine Schritte");
            var stepEditor=new StateEditor(null,project,null,step);stepEditor.Rows.Single(r=>r.Id==valve.Id).State="Szenario übernehmen";stepEditor.Rows.Single(r=>r.Id==text.Id).OverrideText=false;Check(stepEditor.SaveValues()&&!stepEditor.Step!.EquipmentStates.ContainsKey(valve.Id)&&!stepEditor.Step.TextValues.ContainsKey(text.Id),"Schrittvererbung für Symbolzustand und Text");
            var border=((Grid)PageVisual.Symbol(valve,1000,800,"Geschlossen")).Children.OfType<Border>().Single();Check(border.Background.ToString()=="#FFFFDF9A"&&border.BorderBrush.ToString()!=border.Background.ToString(),"Ventilhintergrund unterscheidet sich von Linienfarbe");
            // Render the content only, not a desktop window; useful for layout review.
            var panel=(FrameworkElement)editor.Content;editor.Content=null;var whiteBackground=new Border{Background=Brushes.White,Child=panel};whiteBackground.Measure(new Size(1070,680));whiteBackground.Arrange(new Rect(0,0,1070,680));whiteBackground.UpdateLayout();
            var bitmap=new RenderTargetBitmap(1070,680,96,96,PixelFormats.Pbgra32);bitmap.Render(whiteBackground);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(Path.Combine(output,"headless-state-editor.png")))encoder.Save(file);
            var saved=Path.Combine(output,"headless-roundtrip.pid2process.json");ProjectServices.Save(saved,project);var loaded=ProjectServices.Load(saved);Check(loaded.FormatVersion=="2.1"&&loaded.Pipes.Single(p=>p.Id==branch.Id).StartConnection!.PipeId==downstream.Id&&loaded.Equipment.Single(x=>x.Id==source.Id).Function==source.Function&&loaded.ControlSteps.Single(s=>s.Id==step.Id).TextValues[text.Id]=="Befüllung läuft","JSON-Roundtrip: Pfade, Anschlüsse, Funktion, Medium und Schritttext");
            var cases=new[]{("scenario",scenario,(ControlStep?)null),("open",scenario,step),("closed",scenario,project.ControlSteps.Single(s=>s.Name=="Füllen beenden"))}.Select(c=>new{name=c.Item1,scenarioId=c.Item2.Id,stepId=c.Item3?.Id,expected=PipeNetwork.Evaluate(project,SceneValues.From(c.Item2,c.Item3))}).ToArray();
            File.WriteAllText(Path.Combine(output,"scene-cases.json"),JsonSerializer.Serialize(cases,ProjectServices.JsonOptions));
            await ExportService.ExportAsync(project,Path.Combine(output,"review"));Check(File.ReadAllText(Path.Combine(output,"review","viewer.html")).Contains("const ReviewScene")&&!File.ReadAllText(Path.Combine(output,"review","viewer.html")).Contains("__SCENE_SCRIPT__"),"Review-Paket bettet aktuelle Netzwerkdarstellung ein");
            var exportScenario=DocumentExport.Snapshot(project).Scenarios.Single(s=>s.Id==scenario.Id);foreach(var pair in step.EquipmentStates)exportScenario.EquipmentStates[pair.Key]=pair.Value;foreach(var pair in step.TextValues)exportScenario.TextValues[pair.Key]=pair.Value;
            var exportProject=DocumentExport.Snapshot(project);exportProject.Scenarios.RemoveAll(s=>s.Id==scenario.Id);exportProject.Scenarios.Add(exportScenario);
            var hash=ProjectServices.HashFile(project.Pdf.FilePath);await DocumentExport.ExportAsync(exportProject,Path.Combine(output,"headless-process.pdf"),true,0,scenario.Id);await DocumentExport.ExportAsync(exportProject,Path.Combine(output,"headless-process.jpg"),false,0,scenario.Id);Check(ProjectServices.HashFile(project.Pdf.FilePath)==hash,"PDF-/JPG-Export belässt Referenz-PDF unverändert");
            var renderer=new PdfRasterizer();await renderer.OpenAsync(Path.Combine(output,"headless-process.pdf"));Check(renderer.PageCount==1,"Exportiertes Prozess-PDF ist lesbar");
            var raster=await renderer.RenderAsync(0,2000);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(raster));using(var file=File.Create(Path.Combine(output,"headless-process.png")))png.Save(file);
            var revision=DocumentExport.Snapshot(project);revision.Pdf.Sha256="absichtlich abweichend";revision.Approval.IsApproved=true;ProjectServices.ApplyApprovalStatus(revision);Check(revision.Approval.IsBlocked,"Abweichender PDF-Hash sperrt Freigabe");
            var pidFolder=Path.Combine(Environment.CurrentDirectory,"_PIDs");if(Directory.Exists(pidFolder))foreach(var userPdf in Directory.GetFiles(pidFolder,"*.pdf")){var userHash=ProjectServices.HashFile(userPdf);var reader=new PdfRasterizer();await reader.OpenAsync(userPdf);var preview=await reader.RenderAsync(0,2000);var detail=await reader.RenderAsync(0,6000);Check(reader.PageCount>0&&detail.PixelWidth>preview.PixelWidth&&ProjectServices.HashFile(userPdf)==userHash,"Test-PID bildschirmfrei in höherer Auflösung gerendert: "+Path.GetFileName(userPdf));}
            File.WriteAllLines(Path.Combine(output,"headless-results.txt"),report);return 0;
        }
        catch(Exception ex){report.Add("FAIL: "+ex);File.WriteAllLines(Path.Combine(output,"headless-results.txt"),report);return 1;}
    }
    private static EngineeringProject Fixture(string pdf)
    {
        var project=new EngineeringProject{Name="Prozess-Test",Pdf=new PdfReference{FilePath=pdf,FileName=Path.GetFileName(pdf),Sha256=ProjectServices.HashFile(pdf),PageCount=2}};
        var valve=new EquipmentItem{Tag="XV-101",Type=EquipmentType.IsolationValve,Variant="2-Wege",FailPosition="NC",Function="Druckluftversorgung",InstallationLocation="+A",X=.35,Y=.3,Width=.08,Height=.07};
        var source=new EquipmentItem{Tag="MED-101",Type=EquipmentType.MediaSource,Variant="Medienquelle",MediaName="Druckluft",Description="Netzversorgung",TemperatureCelsius=20,PressureBar=6,FlowVelocityMetersPerSecond=2.5,MediumType=MediumType.Gas,Function=valve.Function,InstallationLocation="+A",X=.08,Y=.3,Width=.17,Height=.14};
        var text=new EquipmentItem{Tag="TXT-101",Type=EquipmentType.Text,Variant="Textfeld",TextContent="Grundtext",Function=valve.Function,X=.35,Y=.12,Width=.25,Height=.07};project.Equipment.AddRange([valve,source,text]);
        while(project.Equipment.Count<100)project.Equipment.Add(new EquipmentItem{Tag=$"PT-{project.Equipment.Count:000}",Type=EquipmentType.Instrument,Variant="Druck",Function="Temperatur-Regelung ABC",InstallationLocation="+CUSTOMER",Page=2});
        var upstream=new PipeSegment{Tag="L-VOR",StartConnection=new(){EquipmentId=source.Id,Port="Right"},EndConnection=new(){EquipmentId=valve.Id,Port="Left"},Points=[new(.2,.335),new(.35,.335)]};
        var downstream=new PipeSegment{Tag="L-NACH",StartConnection=new(){EquipmentId=valve.Id,Port="Right"},Points=[new(.43,.335),new(.65,.335),new(.65,.6)]};
        var branch=new PipeSegment{Tag="L-ABZ",StartConnection=new(){PipeId=downstream.Id,Segment=1,Fraction=.5},Points=[new(.65,.45),new(.8,.45)]};project.Pipes.AddRange([upstream,downstream,branch]);
        var scenario=new Scenario{Name="Befüllen"};scenario.EquipmentStates[valve.Id]="Geschlossen";scenario.TextValues[text.Id]="Bereit";project.Scenarios.Add(scenario);foreach(var name in new[]{"Stillstand","Regelbetrieb","Störung"})project.Scenarios.Add(new Scenario{Name=name});
        var open=new ControlStep{Name="Füllen starten",ScenarioId=scenario.Id,Actions="XV-101 öffnen"};open.EquipmentStates[valve.Id]="Offen";open.TextValues[text.Id]="Befüllung läuft";var shut=new ControlStep{Name="Füllen beenden",ScenarioId=scenario.Id};shut.EquipmentStates[valve.Id]="Geschlossen";project.ControlSteps.AddRange([open,shut,new ControlStep{Name="Regeln",ScenarioId=project.Scenarios[2].Id}]);return project;
    }
}
