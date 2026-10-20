namespace PID2Process;
internal sealed class SceneValues
{
    internal Dictionary<Guid,string> EquipmentStates {get;}=[];
    internal Dictionary<Guid,string> PipeStates {get;}=[];
    internal Dictionary<Guid,string> TextValues {get;}=[];
    internal Dictionary<Guid,double> SensorValues {get;}=[];
    internal Dictionary<Guid,TankAssignment> TankStates {get;}=[];
    internal static SceneValues From(Scenario? s,ControlStep? t)
    {
        var v=new SceneValues();if(s is not null){foreach(var x in s.EquipmentStates)v.EquipmentStates[x.Key]=x.Value;foreach(var x in s.PipeStates)v.PipeStates[x.Key]=x.Value;foreach(var x in s.TextValues)v.TextValues[x.Key]=x.Value;MergeTanks(v.TankStates,s.TankStates);foreach(var x in s.SensorValues)v.SensorValues[x.Key]=x.Value;}if(t is not null){foreach(var x in t.EquipmentStates)v.EquipmentStates[x.Key]=x.Value;foreach(var x in t.PipeStates)v.PipeStates[x.Key]=x.Value;foreach(var x in t.TextValues)v.TextValues[x.Key]=x.Value;MergeTanks(v.TankStates,t.TankStates);foreach(var x in t.SensorValues)v.SensorValues[x.Key]=x.Value;}return v;
    }
    internal static void MergeTanks(Dictionary<Guid,TankAssignment> target,Dictionary<Guid,TankAssignment> source){foreach(var (id,x) in source){var old=target.GetValueOrDefault(id);target[id]=new(){Filled=x.Filled??old?.Filled,Pressurized=x.Pressurized??old?.Pressurized,MediumId=x.MediumId??old?.MediumId,MediumType=x.MediumType??old?.MediumType,MediumName=x.MediumName??old?.MediumName};}}
}
internal sealed record PipeSpan(double Start,double End,bool Flowing,int Direction=0);
internal sealed record PipeAppearance(string State,MediumType? MediumType,string MediumName,bool Blocked)
{
    public bool Flowing {get;init;}
    public List<PipeSpan> Spans {get;init;}=[];
}
internal sealed class NetworkScene
{
    internal Dictionary<Guid,PipeAppearance> Pipes {get;}=[];
    internal Dictionary<Guid,string> Errors {get;}=[];
    internal Dictionary<Guid,bool> CheckValves {get;}=[];
    internal Dictionary<Guid,(bool Primary,bool Secondary)> HeatExchangers {get;}=[];
}
internal static class PipeNetwork
{
    internal static bool DependsOn(EngineeringProject p,PipeSegment pipe,Guid ancestor,HashSet<Guid>? visited=null){if(pipe.Id==ancestor)return true;visited??=[];if(!visited.Add(pipe.Id))return false;return new[]{pipe.StartConnection,pipe.EndConnection}.Any(c=>c?.PipeId is Guid id&&p.Pipes.FirstOrDefault(x=>x.Id==id) is {} parent&&DependsOn(p,parent,ancestor,visited));}
    internal static PipePoint Port(EquipmentItem x,string name)
    {
        if(SymbolCatalog.Get(x).Ports.TryGetValue(name,out var q))return new(x.X+q[0]*x.Width,x.Y+q[1]*x.Height);
        return name switch{"Left"=>new(x.X,x.Y+x.Height/2),"Top"=>new(x.X+x.Width/2,x.Y),"Bottom"=>new(x.X+x.Width/2,x.Y+x.Height),_=>new(x.X+x.Width,x.Y+x.Height/2)};
    }
    internal static string[] Ports(EquipmentItem x)=>SymbolCatalog.Get(x).Ports.Keys.ToArray();
    internal static List<PipePoint> Resolve(EngineeringProject p,PipeSegment pipe,HashSet<Guid>? visited=null)
    {
        visited??=[];var path=(pipe.Points.Count>=2?pipe.Points:[new(pipe.X1,pipe.Y1),new(pipe.X2,pipe.Y2)]).Select(x=>new PipePoint(x.X,x.Y)).ToList();if(!visited.Add(pipe.Id))return path;
        PipePoint End(PipeConnection? c,PipePoint fallback){if(c?.NeedsReview==true)return c.PreservedPoint??fallback;if(c?.EquipmentId is Guid id&&p.Equipment.FirstOrDefault(x=>x.Id==id&&x.Page==pipe.Page) is {} item)return Port(item,c.Port);if(c?.PipeId is Guid parent&&!visited.Contains(parent)&&p.Pipes.FirstOrDefault(x=>x.Id==parent&&x.Page==pipe.Page) is {} other){var pts=Resolve(p,other,new(visited));var i=Math.Clamp(c.Segment,0,pts.Count-2);var f=Math.Clamp(c.Fraction,0,1);return new(pts[i].X+(pts[i+1].X-pts[i].X)*f,pts[i].Y+(pts[i+1].Y-pts[i].Y)*f);}return fallback;}
        path[0]=End(pipe.StartConnection,path[0]);path[^1]=End(pipe.EndConnection,path[^1]);return path;
    }
    internal static bool Closed(EquipmentItem x,SceneValues v)=>SymbolCatalog.Valve(x)&&SymbolCatalog.State(x,v) is "Geschlossen" or "Zu" or "Gesperrt" or "Störung";
    internal static Dictionary<Guid,PipeAppearance> Evaluate(EngineeringProject p,SceneValues v)=>EvaluateScene(p,v).Pipes;
    private sealed record Edge(string A,string B,Guid? Pipe=null,double Start=0,double End=0,Guid? Conveyor=null,Guid? Heat=null,bool Primary=false,Guid? Check=null);
    internal static NetworkScene EvaluateScene(EngineeringProject p,SceneValues v)
    {
        var result=new NetworkScene();var edges=new List<Edge>();var links=new Dictionary<string,List<int>>();var targets=new Dictionary<string,Guid?>();
        string Device(Guid id,string port)=>"d:"+id+":"+port;
        string Node(Guid id,double t)=>"p:"+id+":"+t.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
        void Add(string a,string b,bool directed=false,Guid? pipe=null,double start=0,double end=0,Guid? conveyor=null,Guid? heat=null,bool primary=false,Guid? check=null){var i=edges.Count;edges.Add(new(a,b,pipe,start,end,conveyor,heat,primary,check));if(!links.ContainsKey(a))links[a]=[];links[a].Add(i);if(!directed){if(!links.ContainsKey(b))links[b]=[];links[b].Add(i);}}
        var points=p.Pipes.ToDictionary(x=>x.Id,x=>Resolve(p,x));var cuts=p.Pipes.ToDictionary(x=>x.Id,x=>new SortedSet<double>{0,points[x.Id].Count-1});
        double Position(PipeConnection c,Guid parent)=>Math.Clamp(c.Segment,0,points[parent].Count-2)+Math.Clamp(c.Fraction,0,1);
        foreach(var pipe in p.Pipes)foreach(var c in new[]{pipe.StartConnection,pipe.EndConnection})if(c?.NeedsReview!=true&&c?.PipeId is Guid parent&&p.Pipes.Any(x=>x.Id==parent&&x.Page==pipe.Page))cuts[parent].Add(Position(c,parent));
        foreach(var pipe in p.Pipes){var pos=cuts[pipe.Id].ToArray();for(var i=0;i<pos.Length-1;i++)Add(Node(pipe.Id,pos[i]),Node(pipe.Id,pos[i+1]),pipe:pipe.Id,start:pos[i],end:pos[i+1]);
            void Connect(PipeConnection? c,double t){var n=Node(pipe.Id,t);if(c is null){targets[n]=null;return;}if(c.NeedsReview)return;if(c.EquipmentId is Guid id&&p.Equipment.FirstOrDefault(x=>x.Id==id&&x.Page==pipe.Page) is {} item&&Ports(item).Contains(c.Port))Add(n,Device(id,c.Port));if(c.PipeId is Guid parent&&p.Pipes.Any(x=>x.Id==parent&&x.Page==pipe.Page))Add(n,Node(parent,Position(c,parent)));}
            Connect(pipe.StartConnection,0);Connect(pipe.EndConnection,points[pipe.Id].Count-1);
        }
        foreach(var pipe in p.Pipes)foreach(var c in new[]{pipe.StartConnection,pipe.EndConnection})if(c?.NeedsReview!=true&&c?.PipeId is Guid parent&&p.Pipes.Any(x=>x.Id==parent&&x.Page==pipe.Page))targets.Remove(Node(parent,Position(c,parent)));
        foreach(var x in p.Equipment){var ports=Ports(x);var state=SymbolCatalog.State(x,v);
            void Pair(string a,string b,bool directed=false,Guid? conveyor=null,bool? primary=null){if(ports.Contains(a)&&ports.Contains(b))Add(Device(x.Id,a),Device(x.Id,b),directed,conveyor:conveyor,heat:primary.HasValue?x.Id:null,primary:primary??false,check:SymbolCatalog.CheckValve(x)?x.Id:null);}
            if(x.Type==EquipmentType.Vessel){foreach(var port in ports)targets[Device(x.Id,port)]=x.Id;foreach(var port in ports.Skip(1))Pair(ports[0],port);continue;}if(x.Type is EquipmentType.MediaSource or EquipmentType.Text)continue;
            if(x.Type==EquipmentType.HeatExchanger){Pair("PrimaryIn","PrimaryOut",primary:true);Pair("SecondaryIn","SecondaryOut",primary:false);continue;}
            if(state=="Störung"||Closed(x,v)||x.Type==EquipmentType.Filter&&state=="Verstopft")continue;
            if(SymbolCatalog.Conveyor(x)){if(state=="Ein")Pair("Left","Right",true,x.Id);continue;}if(SymbolCatalog.CheckValve(x)){Pair(x.CheckValveReversed?"Right":"Left",x.CheckValveReversed?"Left":"Right",true);continue;}
            if(SymbolCatalog.ThreeWay(x)){if(state is "Offen" or "A-B")Pair("Left","Right");if(state is "Offen" or "A-C")Pair("Left","Bottom");if(state is "Offen" or "B-C")Pair("Right","Bottom");continue;}foreach(var port in ports.Skip(1))Pair(ports[0],port);
        }
        EquipmentItem Effective(EquipmentItem x){if(x.Type!=EquipmentType.Vessel)return x;var t=v.TankStates.GetValueOrDefault(x.Id);var m=p.Media.FirstOrDefault(m=>m.Id==(t?.MediumId??x.MediumId));return new EquipmentItem{Id=x.Id,Type=x.Type,Variant=x.Variant,Page=x.Page,MediumType=m?.Type??t?.MediumType??x.MediumType,MediaName=m?.Name??t?.MediumName??x.MediaName};}
        bool Filled(EquipmentItem x)=>v.EquipmentStates.GetValueOrDefault(x.Id) switch{"Befüllt"=>true,"Leer"=>false,_=>v.TankStates.GetValueOrDefault(x.Id)?.Filled==true||v.TankStates.GetValueOrDefault(x.Id)?.Pressurized==true};
        var sources=p.Equipment.Where(x=>x.Type==EquipmentType.MediaSource&&v.EquipmentStates.GetValueOrDefault(x.Id) is not ("Inaktiv" or "Aus" or "Störung")||x.Type==EquipmentType.Vessel&&Filled(x)).Select(Effective).ToDictionary(x=>x.Id);
        var conveyors=p.Equipment.Where(SymbolCatalog.Conveyor).ToDictionary(x=>x.Id);var disabled=new HashSet<Guid>();var media=new Dictionary<string,HashSet<Guid>>();var seenBySource=new Dictionary<Guid,HashSet<string>>();
        bool Allowed(Edge e,EquipmentItem source)=>e.Conveyor is not Guid id||!disabled.Contains(id)&&source.MediumType==(conveyors[id].Type==EquipmentType.PumpMotor?MediumType.Liquid:MediumType.Gas);
        string Other(Edge e,string n)=>e.A==n?e.B:e.A;
        // Incompatible conveyors are removed monotonically; cycles do not prevent convergence.
        while(true){media.Clear();seenBySource.Clear();foreach(var source in sources.Values){var seen=new HashSet<string>();var queue=new Queue<string>(Ports(source).Select(port=>Device(source.Id,port)));while(queue.Count>0){var n=queue.Dequeue();if(!seen.Add(n))continue;if(!media.ContainsKey(n))media[n]=[];media[n].Add(source.Id);if(links.TryGetValue(n,out var adjacent))foreach(var i in adjacent)if(Allowed(edges[i],source))queue.Enqueue(Other(edges[i],n));}seenBySource[source.Id]=seen;}
            var suppliedTanks=sources.Values.Where(x=>x.Type==EquipmentType.Vessel&&Ports(x).Any(port=>media.GetValueOrDefault(Device(x.Id,port))?.Any(id=>sources[id].Type==EquipmentType.MediaSource)==true)).Select(x=>x.Id).ToArray();
            if(suppliedTanks.Length>0){foreach(var id in suppliedTanks)sources.Remove(id);continue;}
            var changed=false;foreach(var (id,x) in conveyors){if(SymbolCatalog.State(x,v)!="Ein")continue;var supply=media.GetValueOrDefault(Device(id,"Left"));var expected=x.Type==EquipmentType.PumpMotor?MediumType.Liquid:MediumType.Gas;if(supply is not null&&supply.Any(s=>sources[s].MediumType!=expected)&&disabled.Add(id))changed=true;}if(!changed)break;
        }
        foreach(var (id,x) in conveyors)if(SymbolCatalog.State(x,v)=="Ein"){var supply=media.GetValueOrDefault(Device(id,"Left"));if(supply is null||supply.Count==0)result.Errors[id]="Störung: Medium fehlt";else if(disabled.Contains(id))result.Errors[id]="Störung: Medium ungeeignet";}
        var flowing=new HashSet<int>();var directions=new Dictionary<int,int>();
        foreach(var source in sources.Values){var reachable=seenBySource[source.Id];bool Target(string n)=>targets.TryGetValue(n,out var tank)&&tank!=source.Id&&(source.Type!=EquipmentType.Vessel||tank is null);if(!reachable.Any(Target))continue;
            // Each edge must belong to a simple source-to-target path: no dead-end backtracking.
            foreach(var required in Enumerable.Range(0,edges.Count).Where(i=>!flowing.Contains(i)&&reachable.Contains(edges[i].A)&&reachable.Contains(edges[i].B)&&Allowed(edges[i],source))){var visited=new HashSet<string>();var path=new List<(int Index,string From)>();
                bool Search(string n,bool included){if(!visited.Add(n))return false;if(Target(n)&&included){foreach(var part in path){flowing.Add(part.Index);directions.TryAdd(part.Index,edges[part.Index].A==part.From?1:-1);}visited.Remove(n);return true;}if(source.Type==EquipmentType.Vessel&&targets.TryGetValue(n,out var otherTank)&&otherTank is not null&&otherTank!=source.Id){visited.Remove(n);return false;}if(links.TryGetValue(n,out var adjacent))foreach(var i in adjacent){var edge=edges[i];var next=Other(edge,n);if(visited.Contains(next)||!Allowed(edge,source))continue;path.Add((i,n));if(Search(next,included||i==required)){visited.Remove(n);return true;}path.RemoveAt(path.Count-1);}visited.Remove(n);return false;}
                foreach(var port in Ports(source))if(Search(Device(source.Id,port),false))break;
            }
        }
        foreach(var pipe in p.Pipes){var indices=Enumerable.Range(0,edges.Count).Where(i=>edges[i].Pipe==pipe.Id).ToArray();var supply=indices.SelectMany(i=>media.GetValueOrDefault(edges[i].A)??[]).Distinct().Select(id=>sources[id]).ToArray();var blocked=new[]{pipe.StartConnection,pipe.EndConnection}.Any(c=>c?.NeedsReview==true||c?.EquipmentId is Guid id&&p.Equipment.FirstOrDefault(x=>x.Id==id) is {} x&&(Closed(x,v)||x.Type==EquipmentType.Filter&&SymbolCatalog.State(x,v)=="Verstopft"||SymbolCatalog.Conveyor(x)&&(SymbolCatalog.State(x,v)!="Ein"||result.Errors.ContainsKey(id))));
            var type=supply.Length==0?(MediumType?)null:supply.Select(x=>x.MediumType).Distinct().Count()==1?supply[0].MediumType:MediumType.Mixed;var state=type switch{MediumType.Gas=>"Druckbeschlagen",MediumType.Liquid=>"Befüllt",MediumType.Solid=>"Materialführung",MediumType.Mixed=>"Gemisch",_=>pipe.StartConnection is null&&pipe.EndConnection is null?v.PipeStates.GetValueOrDefault(pipe.Id)??"Leer":"Leer"};result.Pipes[pipe.Id]=new(state,type,string.Join(" / ",supply.Select(x=>x.MediaName).Distinct()),blocked){Flowing=indices.Any(flowing.Contains),Spans=indices.Select(i=>new PipeSpan(edges[i].Start,edges[i].End,flowing.Contains(i),directions.GetValueOrDefault(i))).ToList()};
        }
        foreach(var x in p.Equipment.Where(SymbolCatalog.CheckValve))result.CheckValves[x.Id]=flowing.Any(i=>edges[i].Check==x.Id);
        foreach(var x in p.Equipment.Where(x=>x.Type==EquipmentType.HeatExchanger))result.HeatExchangers[x.Id]=(flowing.Any(i=>edges[i].Heat==x.Id&&edges[i].Primary),flowing.Any(i=>edges[i].Heat==x.Id&&!edges[i].Primary));return result;
    }
    internal static List<PipePoint> Slice(List<PipePoint> points,double start,double end){PipePoint At(double t){var i=Math.Min((int)t,points.Count-2);var f=t-i;return new(points[i].X+(points[i+1].X-points[i].X)*f,points[i].Y+(points[i+1].Y-points[i].Y)*f);}var result=new List<PipePoint>{At(start)};for(var i=(int)Math.Floor(start)+1;i<end;i++)result.Add(points[i]);result.Add(At(end));return result;}
}
