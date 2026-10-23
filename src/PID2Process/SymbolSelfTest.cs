using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

internal static class SymbolSelfTest
{
    internal static async Task Run(string output,Action<bool,string> check)
    {
        var cases=new List<object>();
        EquipmentItem Item(EngineeringProject p,EquipmentType type,string variant="",MediumType medium=MediumType.Gas){var x=new EquipmentItem{Type=type,Variant=variant,Tag="T-"+(p.Equipment.Count+1),MediumType=medium,MediaName=medium.ToString(),Actuator=ActuatorType.Manual};p.Equipment.Add(x);return x;}
        PipeSegment Pipe(EngineeringProject p,EquipmentItem? a,string ap,EquipmentItem? b,string bp){var pipe=new PipeSegment{Points=[new(.1,.1),new(.5,.1)],StartConnection=a is null?null:new(){EquipmentId=a.Id,Port=ap},EndConnection=b is null?null:new(){EquipmentId=b.Id,Port=bp}};p.Pipes.Add(pipe);return pipe;}
        NetworkScene Capture(string name,EngineeringProject p,SceneValues v){var n=PipeNetwork.EvaluateScene(p,v);cases.Add(new{name,project=DocumentExport.Snapshot(p),scenario=new Scenario{EquipmentStates=new(v.EquipmentStates),TankStates=new(v.TankStates)},expected=new{pipes=n.Pipes,errors=n.Errors,checkValves=n.CheckValves,heatExchangers=n.HeatExchangers.ToDictionary(x=>x.Key,x=>new{primary=x.Value.Primary,secondary=x.Value.Secondary})}});return n;}
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource);var v1=Item(p,EquipmentType.IsolationValve,"2-Wege");var v2=Item(p,EquipmentType.IsolationValve,"2-Wege");var main=Pipe(p,s,"Right",v1,"Left");main.Points=[new(.1,.1),new(.4,.1),new(.7,.1)];var branch=Pipe(p,null,"",v2,"Left");branch.StartConnection=new(){PipeId=main.Id,Segment=1,Fraction=.5};var values=new SceneValues();values.EquipmentStates[v1.Id]="Geschlossen";values.EquipmentStates[v2.Id]="Geschlossen";
            var n=Capture("Zwei geschlossene Ventile",p,values);check(n.Pipes.Values.All(x=>!x.Flowing)&&n.Pipes[main.Id].State=="Druckbeschlagen","Sacknetz vor zwei geschlossenen Ventilen: verfügbar, kein Durchfluss");
            var outlet=Pipe(p,v1,"Right",null,"");values.EquipmentStates[v1.Id]="Offen";n=Capture("Hauptweg offen, Ast geschlossen",p,values);check(n.Pipes[main.Id].Flowing&&n.Pipes[outlet.Id].Flowing&&!n.Pipes[branch.Id].Flowing,"Offener Hauptweg animiert, gesperrter Abzweig statisch");
            v1.Page=2;n=Capture("Seiten getrennt",p,values);check(!n.Pipes[main.Id].Flowing,"Keine Medienweitergabe über PDF-Seitengrenze");
        }
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource);var main=Pipe(p,s,"Right",null,"");main.Points=[new(.1,.1),new(.4,.1),new(.8,.1)];var valve=Item(p,EquipmentType.IsolationValve,"2-Wege");var branch=Pipe(p,null,"",valve,"Left");branch.StartConnection=new(){PipeId=main.Id,Segment=0,Fraction=.5};var values=new SceneValues();var n=Capture("Freies Ende und geschlossener Ast",p,values);check(n.Pipes[main.Id].Flowing&&!n.Pipes[branch.Id].Flowing,"Freies Ende ist Ziel, Sackast bleibt ohne Durchfluss");
            var extension=Pipe(p,null,"",valve,"Left");extension.StartConnection=new(){PipeId=main.Id,Segment=1,Fraction=1};n=Capture("Abzweig ersetzt freies Ende",p,values);check(n.Pipes.Values.All(x=>!x.Flowing),"Angebundenes Elternrohr-Ende zählt nicht mehr als freies Ziel");
        }
        foreach(var state in new[]{"Offen","Geschlossen","A-B","A-C","B-C"})foreach(var inlet in new[]{"Left","Right","Bottom"})
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource);var valve=Item(p,EquipmentType.IsolationValve,"3-Wege");Pipe(p,s,"Right",valve,inlet);var outputs=new Dictionary<string,PipeSegment>();foreach(var port in new[]{"Left","Right","Bottom"}.Where(x=>x!=inlet))outputs[port]=Pipe(p,valve,port,null,"");var values=new SceneValues();values.EquipmentStates[valve.Id]=state;var n=Capture("Dreiwege "+state+" ab "+inlet,p,values);var pair=state switch{"A-B"=>new[]{"Left","Right"},"A-C"=>new[]{"Left","Bottom"},"B-C"=>new[]{"Right","Bottom"},_=>Array.Empty<string>()};check(outputs.All(x=>n.Pipes[x.Value.Id].Flowing==(state=="Offen"||pair.Contains(inlet)&&pair.Contains(x.Key))),"Dreiwege "+state+" mit Eingang "+inlet);
        }
        foreach(var variant in new[]{"Rückschlagarmatur","Rückschlagklappe"})foreach(var inlet in new[]{"Left","Right"})
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource);var valve=Item(p,EquipmentType.IsolationValve,variant);Pipe(p,s,"Right",valve,inlet);var pipe=Pipe(p,valve,inlet=="Left"?"Right":"Left",null,"");var n=Capture(variant+" ab "+inlet,p,new());check(n.Pipes[pipe.Id].Flowing==(inlet=="Left"),variant+": Kugelseite bestimmt Richtung");
        }
        foreach(var state in new[]{"Durchlässig","Verstopft"})
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource);var f=Item(p,EquipmentType.Filter);Pipe(p,s,"Right",f,"Top");var pipe=Pipe(p,f,"Bottom",null,"");var values=new SceneValues();values.EquipmentStates[f.Id]=state;var n=Capture("Filter "+state,p,values);check(n.Pipes[pipe.Id].Flowing==(state=="Durchlässig"),"Filter "+state+": Weitergabe");
        }
        foreach(var type in new[]{EquipmentType.PumpMotor,EquipmentType.Compressor,EquipmentType.Fan})foreach(var medium in new[]{MediumType.Gas,MediumType.Liquid,MediumType.Solid,MediumType.Mixed})foreach(var state in new[]{"Ein","Aus"})
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource,medium:medium);var pump=Item(p,type,type==EquipmentType.PumpMotor?"Pumpe":"");Pipe(p,s,"Right",pump,"Left");var pipe=Pipe(p,pump,"Right",null,"");var values=new SceneValues();values.EquipmentStates[pump.Id]=state;var n=Capture(type+" "+medium+" "+state,p,values);var suitable=medium==(type==EquipmentType.PumpMotor?MediumType.Liquid:MediumType.Gas);check(n.Pipes[pipe.Id].Flowing==(state=="Ein"&&suitable)&&n.Errors.ContainsKey(pump.Id)==(state=="Ein"&&!suitable),type+" "+medium+" "+state+": Typprüfung und Sperre");
        }
        {
            var p=new EngineeringProject();var pump=Item(p,EquipmentType.PumpMotor,"Pumpe");Pipe(p,pump,"Right",null,"");var values=new SceneValues();values.EquipmentStates[pump.Id]="Ein";var n=Capture("Pumpenmedium fehlt",p,values);check(n.Errors[pump.Id]=="Störung: Medium fehlt","Eingeschaltete Pumpe ohne Medium meldet Fehler");
            var s=Item(p,EquipmentType.MediaSource,medium:MediumType.Liquid);Pipe(p,s,"Right",pump,"Right");n=Capture("Pumpenversorgung am Ausgang",p,values);check(n.Errors.ContainsKey(pump.Id),"Ausgangsversorgung verhindert keinen Eingangsfehler");
        }
        foreach(var medium in new[]{MediumType.Liquid,MediumType.Gas})
        {
            var p=new EngineeringProject();var tank=Item(p,EquipmentType.Vessel,medium:medium);var pump=Item(p,medium==MediumType.Liquid?EquipmentType.PumpMotor:EquipmentType.Compressor,medium==MediumType.Liquid?"Pumpe":"");Pipe(p,tank,"Right1",pump,"Left");var outlet=Pipe(p,pump,"Right",null,"");var values=new SceneValues();values.EquipmentStates[pump.Id]="Ein";values.TankStates[tank.Id]=new(){Filled=true,Pressurized=true};var n=Capture("Behälterversorgung "+medium,p,values);check(n.Pipes[outlet.Id].Flowing&&!n.Errors.ContainsKey(pump.Id),"Behälter versorgt "+medium+" ohne Fehler");check(PipeNetwork.Ports(tank).Length==8,"Behälter besitzt acht Anschlüsse");
            p.Pipes.Remove(outlet);n=Capture("Behälter ohne externes Ziel "+medium,p,values);check(n.Pipes.Values.All(x=>!x.Flowing),"Eigener Behälter zählt nicht als eigenes Durchflussziel");
            values.TankStates[tank.Id]=new(){Filled=false,Pressurized=false};n=Capture("Leerer Behälter "+medium,p,values);check(n.Errors.ContainsKey(pump.Id),"Behälter ohne Versorgungsvorgabe liefert kein Medium");
        }
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource,medium:MediumType.Liquid);var tank=Item(p,EquipmentType.Vessel,medium:MediumType.Liquid);var pipe=Pipe(p,s,"Right",tank,"Left1");var n=Capture("Zufluss zum leeren Tank",p,new());check(n.Pipes[pipe.Id].Flowing,"Behälter zählt als Zuflussziel ohne automatische Zustandsänderung");
            var scenario=new Scenario();scenario.TankStates[tank.Id]=new(){Filled=true,Pressurized=true};var step=new ControlStep{ScenarioId=scenario.Id};step.TankStates[tank.Id]=new(){Filled=false};var v=SceneValues.From(scenario,step);check(v.TankStates[tank.Id].Filled==false&&v.TankStates[tank.Id].Pressurized==true,"Behälter-Schritt überschreibt Füllung und erbt Druck einzeln");
            var se=new StateEditor(null,p,scenario,null);var row=se.Rows.Single(x=>x.Id==tank.Id);row.State="Leer";check(se.SaveValues()&&SceneValues.From(se.Working.Scenarios.Single(s=>s.Id==scenario.Id),se.Step).TankStates[tank.Id].Filled==false&&SceneValues.From(se.Working.Scenarios.Single(s=>s.Id==scenario.Id),se.Step).TankStates[tank.Id].Pressurized==true,"Behälterwerte im transaktionalen Zustandseditor");
        }
        foreach(var variant in new[]{"Kondensator","M-Form"})foreach(var primary in new[]{false,true})foreach(var secondary in new[]{false,true})
        {
            var p=new EngineeringProject();var hx=Item(p,EquipmentType.HeatExchanger,variant);var a=Item(p,EquipmentType.MediaSource,medium:MediumType.Gas);var b=Item(p,EquipmentType.MediaSource,medium:MediumType.Liquid);Pipe(p,a,"Right",hx,"PrimaryIn");var po=Pipe(p,hx,"PrimaryOut",null,"");Pipe(p,b,"Right",hx,"SecondaryIn");var so=Pipe(p,hx,"SecondaryOut",null,"");var v=new SceneValues();v.EquipmentStates[a.Id]=primary?"Aktiv":"Inaktiv";v.EquipmentStates[b.Id]=secondary?"Aktiv":"Inaktiv";var n=Capture("Wärmetauscher "+variant+" "+primary+"/"+secondary,p,v);check(n.HeatExchangers[hx.Id]==(primary,secondary)&&n.Pipes[po.Id].MediumType==(primary?MediumType.Gas:null)&&n.Pipes[so.Id].MediumType==(secondary?MediumType.Liquid:null),"Wärmetauscher "+variant+": unabhängige Kreise "+primary+"/"+secondary);
        }
        {
            var p=new EngineeringProject();var s=Item(p,EquipmentType.MediaSource);var a=Pipe(p,s,"Right",null,"");var b=Pipe(p,null,"",null,"");b.StartConnection=new(){PipeId=a.Id,Segment=0,Fraction=.3};b.EndConnection=new(){PipeId=a.Id,Segment=0,Fraction=.7};var n=Capture("Ring mit freiem Auslass",p,new());check(n.Pipes[a.Id].Flowing&&n.Pipes[b.Id].Flowing,"Ring mit Versorgung und freiem Auslass durchflossen");a.EndConnection=new(){EquipmentId=Item(p,EquipmentType.IsolationValve,"2-Wege").Id,Port="Left"};n=Capture("Ring ohne Ziel",p,new());check(n.Pipes.Values.All(x=>!x.Flowing),"Ring ohne erreichbares Ziel bleibt statisch");
        }
        {
            var p=new EngineeringProject{FormatVersion="2.0"};var tank=Item(p,EquipmentType.Vessel);var valve=Item(p,EquipmentType.IsolationValve,"2-Wege");valve.Actuator=ActuatorType.Unspecified;valve.Actuation="Elektrisch";var pipe=Pipe(p,tank,"Left",valve,"Top");var id=pipe.Id;var old=PipeNetwork.Port(tank,"Left");var file=Path.Combine(output,"legacy-symbols.pid2process.json");File.WriteAllText(file,JsonSerializer.Serialize(p,ProjectServices.JsonOptions));var loaded=ProjectServices.Load(file);check(loaded.Pipes[0].StartConnection!.NeedsReview&&PipeNetwork.Resolve(loaded,loaded.Pipes[0])[0].X==old.X&&loaded.Pipes[0].Id==id,"Altanschlüsse werden geometrisch erhalten und markiert");check(loaded.Equipment[1].Actuator==ActuatorType.ElectricMotor,"Alter elektrischer Antrieb wird eindeutig übernommen");check(!ProjectServices.ValidateEngineering(loaded,out _)&&!loaded.Approval.IsApproved,"Ungeklärte Altanschlüsse sperren Freigabe");ProjectServices.Save(file,loaded);check(ProjectServices.Load(file).FormatVersion=="2.2","Format 2.0 lesen, Format 2.2 speichern");
        }
        File.WriteAllText(Path.Combine(output,"symbol-network-cases.json"),JsonSerializer.Serialize(cases,ProjectServices.JsonOptions));
        var window=new MainWindow();foreach(var method in new[]{"CanvasMenu","SelectionMenu"}){var menu=(ContextMenu)typeof(MainWindow).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,null)!;check(menu.Items.OfType<MenuItem>().Any(x=>x.Header?.ToString()=="Rohrleitung einfügen"),"Kontextmenü "+method+" enthält Rohrleitung einfügen");}
        await window.ImportPdf(Path.Combine(output,"model-pages.pdf"));var active=window.ActiveProject!;var parent=new PipeSegment{Points=[new(.2,.2),new(.6,.2)]};active.Pipes.Add(parent);
        object? Invoke(string name,params object?[] args)=>typeof(MainWindow).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,args);
        var sheet=(Canvas)window.FindName("Sheet");
        void StartBranch()=>Invoke("BeginPipeBranch",new PipeConnection{PipeId=parent.Id,Segment=0,Fraction=.25},new PipePoint(.3,.2));
        StartBranch();Invoke("ContinuePipeBranch",new PipePoint(.3,.4),null);
        check(active.Pipes.Count==1&&((TextBlock)window.FindName("PlacementHint")).Text.Contains("weitere Eckpunkte"),"Abzweig-Loslassen lässt Entwurf aktiv und speichert noch kein Rohr");
        Invoke("AddPipePoint",new Point(.5*sheet.Width,.4*sheet.Height),false);Invoke("FinishPipe",new object?[]{null});
        check(active.Pipes.Count==2&&active.Pipes[1].Points.Count==3&&active.Pipes[1].StartConnection!.PipeId==parent.Id,"Abzweig übernimmt mehrere Segmente und Elternbindung");
        StartBranch();Invoke("ContinuePipeBranch",new PipePoint(.3,.5),null);Invoke("CancelPlacement");check(active.Pipes.Count==2,"Esc-Abbruch verwirft gesamten Abzweigentwurf");
        var target=new EquipmentItem{Type=EquipmentType.IsolationValve,Variant="2-Wege",X=.8,Y=.6,Actuator=ActuatorType.Manual};active.Equipment.Add(target);StartBranch();Invoke("ContinuePipeBranch",new PipePoint(.3,.5),null);var endpoint=PipeNetwork.Port(target,"Left");Invoke("AddPipePoint",new Point(endpoint.X*sheet.Width,endpoint.Y*sheet.Height),false);check(active.Pipes.Count==3&&active.Pipes[2].EndConnection?.EquipmentId==target.Id,"Anschlussfang beendet mehrteiligen Abzweig");
        Invoke("Draw");check(sheet.Children.OfType<FrameworkElement>().Where(x=>x is not TextBlock&&x.ContextMenu is not null).All(x=>x.ContextMenu!.Items.OfType<MenuItem>().Any(m=>m.Header?.ToString()=="Rohrleitung einfügen")),"Symbol- und Rohr-Kontextmenüs enthalten Rohrleitung einfügen");
        var supply=new EquipmentItem{Type=EquipmentType.MediaSource,X=.05,Y=.1,MediumType=MediumType.Liquid};active.Equipment.Add(supply);active.Pipes.Add(new PipeSegment{Points=[new(.1,.1),new(.2,.2)],StartConnection=new(){EquipmentId=supply.Id,Port="Right"},EndConnection=new(){PipeId=parent.Id,Segment=0,Fraction=0}});Invoke("Draw");
        var flowGraphics=(Dictionary<Guid,List<(System.Windows.Shapes.Polyline Graphic,PipeSpan Span)>>)typeof(MainWindow).GetField("_pipeFlowGraphics",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(window)!;
        check(flowGraphics[parent.Id].Count>0&&window.RunningFlowClocks==0,"Durchflossene Abschnitte sind im unsichtbaren Fenster ohne aktive Animationsuhren vorbereitet");
        window.SetFlowAnimation(true);check(window.RunningFlowClocks>0&&flowGraphics[parent.Id].All(x=>x.Graphic.HasAnimatedProperties),"Aktivierung startet die vorbereiteten Flussanimationen");window.SetFlowAnimation(false);check(window.RunningFlowClocks==0&&flowGraphics[parent.Id].All(x=>!x.Graphic.HasAnimatedProperties),"Deaktivierung entfernt sämtliche Animationsuhren");
        var last=flowGraphics[parent.Id][^1].Graphic;var beforeY=last.Points[^1].Y;parent.Points[^1].Y+=.08;Invoke("RefreshPipeGeometry");check(Math.Abs(last.Points[^1].Y-beforeY-.08*sheet.Height)<.001,"Animierte Abschnitte folgen Rohr-Geometrieänderungen sofort");
        window.ClearTestDirtyFlags();
        var atlas=new EngineeringProject{Name="Symbolprüfung"};var variants=new[]{(EquipmentType.Vessel,"Behälter"),(EquipmentType.Filter,"Filter"),(EquipmentType.HeatExchanger,"Kondensator"),(EquipmentType.HeatExchanger,"M-Form"),(EquipmentType.PumpMotor,"Pumpe"),(EquipmentType.Compressor,"Kompressor"),(EquipmentType.Fan,"Ventilator"),(EquipmentType.IsolationValve,"2-Wege"),(EquipmentType.IsolationValve,"Eckform"),(EquipmentType.IsolationValve,"3-Wege"),(EquipmentType.IsolationValve,"Kugelarmatur"),(EquipmentType.IsolationValve,"Kugelarmatur in Eckform"),(EquipmentType.IsolationValve,"Dreiwegekugelhahn"),(EquipmentType.IsolationValve,"Rückschlagarmatur"),(EquipmentType.IsolationValve,"Rückschlagklappe")};
        foreach(var (type,variant) in variants){var i=atlas.Equipment.Count;var x=Item(atlas,type,variant);x.X=.04+(i%4)*.24;x.Y=.04+(i/4)*.18;x.Width=.12;x.Height=.095;x.Tag=variant;x.BackgroundColor="#FFFFFF";}
        foreach(var actuator in new[]{ActuatorType.Manual,ActuatorType.ElectricMotor,ActuatorType.Piston,ActuatorType.Diaphragm}){var i=atlas.Equipment.Count;var x=Item(atlas,EquipmentType.IsolationValve,"2-Wege");x.Actuator=actuator;x.X=.04+(i%4)*.24;x.Y=.04+(i/4)*.18;x.Width=.12;x.Height=.095;x.Tag=actuator.ToString();}
        var sc=new Scenario{Name="Offen"};foreach(var x in atlas.Equipment)sc.EquipmentStates[x.Id]=SymbolCatalog.Valve(x)&&!SymbolCatalog.CheckValve(x)?"Offen":SymbolCatalog.Conveyor(x)?"Aus":"Normal";atlas.Scenarios.Add(sc);
        // Match normalized heights to the actual PDF aspect so circular symbols remain circular.
        var raster=new PdfRasterizer();await raster.OpenAsync(Path.Combine(output,"model-pages.pdf"));var aspect=raster.AspectRatio(0);foreach(var x in atlas.Equipment){x.Width=.09;x.Height=x.Width/aspect;if(x.Type==EquipmentType.Vessel)x.Width=.06;}
        var white=BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{255,255,255,255},4);var image=PageVisual.Render(atlas,1,1600,aspect,sc,white);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using(var file=File.Create(Path.Combine(output,"symbols-atlas.png")))png.Save(file);
        atlas.Pdf=new(){FilePath=Path.Combine(output,"model-pages.pdf"),FileName="model-pages.pdf",Sha256=ProjectServices.HashFile(Path.Combine(output,"model-pages.pdf")),PageCount=2};ProjectServices.Save(Path.Combine(output,"symbols-atlas.pid2process.json"),atlas);await ExportService.ExportAsync(atlas,Path.Combine(output,"symbols-review"));await DocumentExport.ExportAsync(atlas,Path.Combine(output,"symbols-atlas.pdf"),true,0,sc.Id);check(true,"Symbolatlas, Review-Paket und PDF-Export erzeugt");
    }
}
