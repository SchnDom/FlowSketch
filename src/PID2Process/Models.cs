using System.Text.Json.Serialization;

namespace PID2Process;

public sealed class EngineeringProject
{
    public string FormatVersion { get; set; } = "2.1";
    public string Name { get; set; } = "Neues FlowSketch-Projekt";
    public PdfReference Pdf { get; set; } = new();
    public BlankSheet? BlankSheet {get;set;}
    public List<MediumDefinition> Media {get;set;}=[];
    public MediaPalette MediaColors {get;set;}=new();
    public List<EquipmentItem> Equipment { get; set; } = [];
    public List<PipeSegment> Pipes { get; set; } = [];
    public List<SignalLine> SignalLines {get;set;}=[];
    public List<Scenario> Scenarios { get; set; } = [];
    public List<ControlStep> ControlSteps { get; set; } = [];
    public List<Alarm> Alarms { get; set; } = [];
    public Approval Approval { get; set; } = new();
}

public sealed class BlankSheet
{
    public string Format {get;set;}="A4";
    public bool Landscape {get;set;}=true;
    public ExportWindow? ExportWindow {get;set;}
    public double WidthMm {get;set;}=297;
    public double HeightMm {get;set;}=210;
}

public sealed class ExportWindow
{
    public double XMm {get;set;}
    public double YMm {get;set;}
    public double WidthMm {get;set;}
    public double HeightMm {get;set;}
}

public sealed class PdfReference
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public int PageCount { get; set; }
    public string Revision { get; set; } = "";
    public List<double> PageAspectRatios {get;set;}=[];
}

public sealed class EquipmentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tag { get; set; } = "";
    public EquipmentType Type { get; set; } = EquipmentType.Generic;
    public string Description { get; set; } = "";
    public int Page { get; set; } = 1;
    public double X { get; set; } = .5;
    public double Y { get; set; } = .5;
    public List<EquipmentState> States { get; set; } = [];
    public string Variant { get; set; } = "2-Wege";
    public string Actuation { get; set; } = "Pneumatisch";
    public ActuatorType Actuator { get; set; } = ActuatorType.Unspecified;
    public string FailPosition { get; set; } = "NC";
    public bool FeedbackOpen { get; set; }
    public bool FeedbackClosed { get; set; }
    public string FeedbackOpenTag { get; set; } = "";
    public string FeedbackClosedTag { get; set; } = "";
    public string MotorDrive { get; set; } = "DOL";
    public double Width { get; set; } = .045;
    public double Height { get; set; } = .045;
    public string Color { get; set; } = "#007E87";
    public double Opacity { get; set; } = .85;
    public string BackgroundColor {get;set;} = "#FFFFFF";
    public bool ShowStateText {get;set;}
    public bool CheckValveReversed {get;set;}
    public bool AutomaticLegend {get;set;}
    public Guid? MediumId {get;set;}
    public DrawingSettings Drawing {get;set;}=new();
    public SensorSettings Sensor {get;set;}=new();
    public string LegendTitle {get;set;}="Legende";
    public List<LegendEntry> LegendEntries {get;set;}=[];
    public string Function {get;set;} = "";
    public string InstallationLocation {get;set;} = "";
    public string MediaName {get;set;} = "";
    public MediumType MediumType {get;set;} = MediumType.Liquid;
    public string TextContent {get;set;} = "Text";
    public Guid? GroupId {get;set;}
    public double? TemperatureCelsius {get;set;}
    public double? PressureBar {get;set;}
    public double? FlowVelocityMetersPerSecond {get;set;}
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EquipmentType { IsolationValve, ControlValve, PumpMotor, Instrument, Vessel, Generic, MediaSource, Text, Filter, HeatExchanger, Compressor, Fan, Legend, Drawing }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActuatorType { Unspecified, Manual, ElectricMotor, Piston, Diaphragm }

public sealed class LegendEntry
{
    public Guid? MediumId {get;set;}
    public string Text {get;set;}="";
    public string Color {get;set;}="#939598";
}

public sealed class TankAssignment
{
    public Guid? MediumId {get;set;}
    public MediumType? MediumType {get;set;}
    public string? MediumName {get;set;}
    public bool? Filled { get; set; }
    public bool? Pressurized { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediumType { Gas, Liquid, Solid, Mixed }

public sealed class EquipmentState
{
    public string Name { get; set; } = "Normal";
    public string Color { get; set; } = "#5B9BD5";
    public string Description { get; set; } = "";
}

public sealed class PipeSegment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tag { get; set; } = "Rohrleitung";
    public int Page { get; set; } = 1;
    public double X1 { get; set; } = .35;
    public double Y1 { get; set; } = .5;
    public double X2 { get; set; } = .65;
    public double Y2 { get; set; } = .5;
    public List<PipeState> States { get; set; } = [];
    public List<PipePoint> Points {get;set;} = [];
    public PipeConnection? StartConnection {get;set;}
    public PipeConnection? EndConnection {get;set;}
    public Guid? GroupId {get;set;}
    public string Function {get;set;} = "";
    public string InstallationLocation {get;set;} = "";
    public string? Color {get;set;}
    public double? LineWidth {get;set;}
    public string? LineStyle {get;set;}
    public string? EmptyColor {get;set;}
    public string? FilledColor {get;set;}
    public double Opacity {get;set;} = 1;
}

public sealed class PipePoint
{
    public double X {get;set;}
    public double Y {get;set;}
    public PipePoint() { }
    public PipePoint(double x,double y) { X=x;Y=y; }
}
public sealed class SignalConnection
{
    public Guid EquipmentId {get;set;}
    public string Port {get;set;}="Right";
    public double Fraction {get;set;}=.5;
}
public sealed class SignalLine
{
    public Guid Id {get;set;}=Guid.NewGuid();
    public string Tag {get;set;}="Wirklinie";
    public int Page {get;set;}=1;
    public List<PipePoint> Points {get;set;}=[];
    public SignalConnection? StartConnection {get;set;}
    public SignalConnection? EndConnection {get;set;}
    public string Color {get;set;}="#FF0000";
    public double Opacity {get;set;}=1;
    public Guid? GroupId {get;set;}
    public string Function {get;set;}="";
    public string InstallationLocation {get;set;}="";
}
public sealed class PipeConnection
{
    public Guid? EquipmentId {get;set;}
    public string Port {get;set;} = "Right";
    public Guid? PipeId {get;set;}
    public int Segment {get;set;}
    public double Fraction {get;set;}
    public bool NeedsReview {get;set;}
    public PipePoint? PreservedPoint {get;set;}
}

public sealed class PipeState
{
    public string Name { get; set; } = "Leer";
    public string Color { get; set; } = "#7D8790";
    public string Description { get; set; } = "";
}

public sealed class Scenario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Neues Szenario";
    public string Description { get; set; } = "";
    public Dictionary<Guid, string> EquipmentStates { get; set; } = [];
    public Dictionary<Guid, string> PipeStates { get; set; } = [];
    public Dictionary<Guid,string> TextValues {get;set;} = [];
    public Dictionary<Guid,TankAssignment> TankStates {get;set;} = [];
    public Dictionary<Guid,double> SensorValues {get;set;}=[];
}

public sealed class ControlStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Neuer Regelschritt";
    public string Description {get;set;}="";
    public string Preconditions { get; set; } = "";
    public string Actions { get; set; } = "";
    public string Transition { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public Dictionary<Guid,string> EquipmentStates { get; set; } = [];
    public Dictionary<Guid,string> PipeStates { get; set; } = [];
    public Dictionary<Guid,string> TextValues {get;set;} = [];
    public Guid? ScenarioId {get;set;}
    public Dictionary<Guid,TankAssignment> TankStates {get;set;} = [];
    public Dictionary<Guid,double> SensorValues {get;set;}=[];
}

public sealed class Alarm
{
    [JsonIgnore] public bool IsDerived {get;set;}
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tag { get; set; } = "";
    public string Name { get; set; } = "Neuer Alarm";
    public AlarmPriority Priority { get; set; } = AlarmPriority.Warning;
    public string Trigger { get; set; } = "";
    public string Impact { get; set; } = "";
    public string OperatorAction { get; set; } = "";
    public List<Guid> ScenarioIds { get; set; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AlarmPriority { Information, Warning, Critical }

public sealed class Approval
{
    public bool IsApproved { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string ApprovedBy { get; set; } = "";
    public string? ApprovedPdfHash { get; set; }
    public bool IsBlocked { get; set; }
    public string BlockReason { get; set; } = "Noch nicht freigegeben.";
}
