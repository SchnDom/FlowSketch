using System.IO;
using System.Text.Json;
namespace PID2Process;

internal sealed class SymbolShape
{
    public string? Path {get;set;}
    public double[]? Ellipse {get;set;}
    public bool Dash {get;set;}
    public string? Text {get;set;}
    public string? Fill {get;set;}
    public string? Part {get;set;}
    public double X {get;set;}
    public double Y {get;set;}
}
internal sealed class SymbolDefinition
{
    public Dictionary<string,double[]> Ports {get;set;}=[];
    public List<SymbolShape> Shapes {get;set;}=[];
}
internal static class SymbolCatalog
{
    internal static readonly string Json=Read();
    internal static readonly Dictionary<string,SymbolDefinition> Definitions=JsonSerializer.Deserialize<Dictionary<string,SymbolDefinition>>(Json,ProjectServices.JsonOptions)!;
    private static string Read(){using var stream=typeof(SymbolCatalog).Assembly.GetManifestResourceStream("PID2Process.SymbolDefinitions.json")!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
    internal static SymbolDefinition Get(EquipmentItem item)=>item.Type==EquipmentType.Drawing?new SymbolDefinition():Definitions.GetValueOrDefault(item.Type+"|"+item.Variant)??Definitions.GetValueOrDefault(item.Type.ToString())??Definitions["Generic"];
    internal static bool Valve(EquipmentItem x)=>x.Type is EquipmentType.IsolationValve or EquipmentType.ControlValve;
    internal static bool CheckValve(EquipmentItem x)=>Valve(x)&&x.Variant.StartsWith("Rückschlag",StringComparison.Ordinal);
    internal static bool ThreeWay(EquipmentItem x)=>Valve(x)&&(x.Variant is "3-Wege" or "Dreiwegekugelhahn");
    internal static bool Conveyor(EquipmentItem x)=>x.Type is EquipmentType.Compressor or EquipmentType.Fan || x.Type==EquipmentType.PumpMotor&&x.Variant=="Pumpe";
    internal static bool Colored(EquipmentItem x)=>Valve(x)||Conveyor(x)||x.Type==EquipmentType.PumpMotor;
    internal static string FillColor(EquipmentItem x,SymbolShape shape,string state)
    {
        if(state is "Störung" or "Fehler")return "#FF0000";
        var active=CheckValve(x)?state=="Durchflossen":state is "Offen" or "Ein" or "Durchlässig" or "Teiloffen";
        if(ThreeWay(x)&&state is "A-B" or "A-C" or "B-C")active=shape.Part=="State"||state switch{"A-B"=>shape.Part is "Left" or "Right","A-C"=>shape.Part is "Left" or "Bottom",_=>shape.Part is "Right" or "Bottom"};
        return active?"#00FF00":"#808080";
    }
    internal static string State(EquipmentItem x,SceneValues values)
    {
        var state=values.EquipmentStates.GetValueOrDefault(x.Id)??"Normal";
        if(state!="Normal")return state;
        if(CheckValve(x)||x.Type==EquipmentType.Filter)return "Durchlässig";
        if(Valve(x))return x.FailPosition=="NO"?"Offen":x.FailPosition=="AB"&&ThreeWay(x)?"A-B":"Geschlossen";
        if(Conveyor(x)||x.Type==EquipmentType.PumpMotor)return "Aus";
        return state;
    }
    internal static string[] States(EquipmentItem x)=>x.Type==EquipmentType.Drawing?[]:x.Type==EquipmentType.Vessel?["Befüllt","Leer"]:CheckValve(x)?["Normal","Durchlässig","Gesperrt"]:ThreeWay(x)?["Normal","Offen","Geschlossen","A-B","A-C","B-C","Störung"]:Valve(x)?["Normal","Offen","Geschlossen","Störung"]:x.Type==EquipmentType.Filter?["Normal","Durchlässig","Verstopft","Störung"]:Conveyor(x)||x.Type==EquipmentType.PumpMotor?["Normal","Ein","Aus","Störung"]:x.Type==EquipmentType.MediaSource?["Aktiv","Inaktiv"]:x.Type is EquipmentType.Text or EquipmentType.Legend?["Normal","Ausgeblendet"]:["Normal","Störung"];
}
