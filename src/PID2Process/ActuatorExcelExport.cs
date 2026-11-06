using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Xml;
namespace PID2Process;

// Standalone OOXML export: no Excel installation or process automation required.
internal static class ActuatorExcelExport
{
    internal sealed record Row(string Scenario,string Step,string Actor,string State)
    {
        internal string Function {get;init;}="";
        internal string Location {get;init;}="";
        internal string Description {get;init;}="";
        internal int Page {get;init;}
        internal string[] Texts {get;init;}=[];
    }
    private static EquipmentItem[] TextFields(EngineeringProject p)=>p.Equipment.Where(x=>x.Type==EquipmentType.Text).OrderBy(x=>x.Page).ThenBy(x=>x.Tag,NameOrder).ThenBy(x=>x.Id).ToArray();
    private static string[] StepTexts(ControlStep? step,EquipmentItem[] texts)=>texts.Select(x=>step?.TextValues.GetValueOrDefault(x.Id)??"n/a").ToArray();
    internal static bool IsActor(EquipmentItem x)=>SymbolCatalog.Valve(x)&&!SymbolCatalog.CheckValve(x)||x.Type is EquipmentType.PumpMotor or EquipmentType.Compressor or EquipmentType.Fan;
    internal static readonly IComparer<string> NameOrder=Comparer<string>.Create(CompareNames);
    private static int CompareNames(string? left,string? right)
    {
        left??="";right??="";int a=0,b=0;
        while(a<left.Length&&b<right.Length)
        {
            if(char.IsAsciiDigit(left[a])&&char.IsAsciiDigit(right[b]))
            {
                int ae=a,be=b;while(ae<left.Length&&char.IsAsciiDigit(left[ae]))ae++;while(be<right.Length&&char.IsAsciiDigit(right[be]))be++;
                var l=left[a..ae].TrimStart('0');var r=right[b..be].TrimStart('0');var c=l.Length.CompareTo(r.Length);if(c==0)c=StringComparer.Ordinal.Compare(l,r);if(c!=0)return c;a=ae;b=be;
            }
            else {var c=char.ToUpperInvariant(left[a]).CompareTo(char.ToUpperInvariant(right[b]));if(c!=0)return c;a++;b++;}
        }
        return (left.Length-a).CompareTo(right.Length-b);
    }
    internal static List<Row> Rows(EngineeringProject project)
    {
        project=DocumentExport.Snapshot(project);ScenarioSteps.Ensure(project);
        var rows=new List<Row>();var actors=project.Equipment.Where(IsActor).OrderBy(x=>x.Tag,NameOrder).ThenBy(x=>x.Page).ThenBy(x=>x.InstallationLocation,NameOrder).ThenBy(x=>x.Id).ToArray();var texts=TextFields(project);
        void Add(Scenario? scenario,ControlStep? step,string name)
        {
            var values=SceneValues.From(scenario,step);
            var stepTexts=StepTexts(step,texts);
            foreach(var actor in actors)rows.Add(new(name,step?.Name??"Schritt 1",actor.Tag,SymbolCatalog.State(actor,values)){Function=actor.Function,Location=actor.InstallationLocation,Description=actor.Description,Page=actor.Page,Texts=stepTexts});
        }
        foreach(var scenario in project.Scenarios.OrderBy(x=>x.Name,NameOrder).ThenBy(x=>x.Id))
        {
            var steps=project.ControlSteps.Where(x=>x.ScenarioId==scenario.Id).OrderBy(x=>x.Name,NameOrder).ThenBy(x=>x.Id).ToArray();
            if(steps.Length==0)Add(scenario,null,scenario.Name);else foreach(var step in steps)Add(scenario,step,scenario.Name);
        }
        foreach(var step in project.ControlSteps.Where(x=>!project.Scenarios.Any(s=>s.Id==x.ScenarioId)).OrderBy(x=>x.Name,NameOrder).ThenBy(x=>x.Id))Add(null,step,"(Ohne zugeordnetes Szenario)");
        return rows;
    }
    internal static void Export(EngineeringProject project,string path)
    {
        project=DocumentExport.Snapshot(project);ScenarioSteps.Ensure(project);
        var rows=Rows(project);if(rows.Count>1_048_575)throw new InvalidOperationException("Zu viele Zeilen für ein Excel-Arbeitsblatt.");
        var texts=TextFields(project);var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var textHeaders=texts.Select(x=>
        {
            var label=Clean($"Text: {x.Tag} (Seite {x.Page})");var unique=label;var n=2;while(!names.Add(unique))unique=label+" · "+n++;return unique;
        }).ToArray();
        string[] headers=["Szenario","Schritt","Aktor","Zustand","Funktion","Einbauort","Beschreibung","Seite",..textHeaders];
        if(headers.Length>16_384)throw new InvalidOperationException("Zu viele Textfelder für ein Excel-Arbeitsblatt.");
        XNamespace s="http://schemas.openxmlformats.org/spreadsheetml/2006/main",r="http://schemas.openxmlformats.org/officeDocument/2006/relationships",p="http://schemas.openxmlformats.org/package/2006/relationships",ct="http://schemas.openxmlformats.org/package/2006/content-types";
        var last=rows.Count+1;var range=$"A1:{Column(headers.Length-1)}{last}";var data=new XElement(s+"sheetData");
        void AddRow(int n,string[] texts)
        {
            data.Add(TextRow(s,n,texts,n==1));
        }
        AddRow(1,headers);for(int i=0;i<rows.Count;i++){var row=rows[i];AddRow(i+2,[row.Scenario,row.Step,row.Actor,row.State,row.Function,row.Location,row.Description,row.Page.ToString(System.Globalization.CultureInfo.InvariantCulture),..row.Texts]);}
        var sheet=new XElement(s+"worksheet",new XAttribute(XNamespace.Xmlns+"r",r),new XElement(s+"dimension",new XAttribute("ref",range)),new XElement(s+"sheetViews",new XElement(s+"sheetView",new XAttribute("workbookViewId",0),new XElement(s+"pane",new XAttribute("ySplit",1),new XAttribute("topLeftCell","A2"),new XAttribute("activePane","bottomLeft"),new XAttribute("state","frozen")))),new XElement(s+"cols",Enumerable.Range(1,headers.Length).Select(i=>new XElement(s+"col",new XAttribute("min",i),new XAttribute("max",i),new XAttribute("width",i==4?24:i==8?10:36),new XAttribute("customWidth",1)))),data);
        var sequence=new XElement(s+"sheetData");var line=1;
        sequence.Add(TextRow(s,line++,["Szenario","Schritt","Beschreibung","Voraussetzungen","Aktionen","Übergang",..textHeaders],true));
        void Step(Scenario? scenario,ControlStep step)=>sequence.Add(TextRow(s,line++,[scenario?.Name??"(Ohne zugeordnetes Szenario)",step.Name,step.Description,step.Preconditions,step.Actions,step.Transition,..StepTexts(step,texts)]));
        foreach(var scenario in project.Scenarios.OrderBy(x=>x.Name,NameOrder).ThenBy(x=>x.Id))
        {
            sequence.Add(TextRow(s,line++,[scenario.Name,"",scenario.Description],true));
            foreach(var step in project.ControlSteps.Where(x=>x.ScenarioId==scenario.Id).OrderBy(x=>x.Name,NameOrder).ThenBy(x=>x.Id))Step(scenario,step);
        }
        foreach(var step in project.ControlSteps.Where(x=>!project.Scenarios.Any(scenario=>scenario.Id==x.ScenarioId)).OrderBy(x=>x.Name,NameOrder).ThenBy(x=>x.Id))Step(null,step);
        if(line>1_048_577)throw new InvalidOperationException("Zu viele Ablaufschritte für ein Excel-Arbeitsblatt.");
        var sequenceSheet=new XElement(s+"worksheet",new XElement(s+"cols",Enumerable.Range(1,6+texts.Length).Select(i=>new XElement(s+"col",new XAttribute("min",i),new XAttribute("max",i),new XAttribute("width",40),new XAttribute("customWidth",1)))),sequence);
        if(rows.Count>0)sheet.Add(new XElement(s+"tableParts",new XAttribute("count",1),new XElement(s+"tablePart",new XAttribute(r+"id","rId1"))));
        var temporary=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,"."+Guid.NewGuid()+".xlsx");
        try
        {
            using(var zip=ZipFile.Open(temporary,ZipArchiveMode.Create))
            {
                void Write(string name,XElement xml){using var stream=zip.CreateEntry(name).Open();xml.Save(stream);}
                XElement Rel(string id,string type,string target)=>new(p+"Relationship",new XAttribute("Id",id),new XAttribute("Type",r.NamespaceName+"/"+type),new XAttribute("Target",target));
                XElement Type(string part,string content)=>new(ct+"Override",new XAttribute("PartName","/"+part),new XAttribute("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml."+content+"+xml"));
                var types=new XElement(ct+"Types",new XElement(ct+"Default",new XAttribute("Extension","rels"),new XAttribute("ContentType","application/vnd.openxmlformats-package.relationships+xml")),Type("xl/workbook.xml","sheet.main"),Type("xl/worksheets/sheet1.xml","worksheet"),Type("xl/worksheets/sheet2.xml","worksheet"),Type("xl/styles.xml","styles"));if(rows.Count>0)types.Add(Type("xl/tables/table1.xml","table"));Write("[Content_Types].xml",types);
                Write("_rels/.rels",new XElement(p+"Relationships",Rel("rId1","officeDocument","xl/workbook.xml")));
                Write("xl/workbook.xml",new XElement(s+"workbook",new XAttribute(XNamespace.Xmlns+"r",r),new XElement(s+"sheets",new XElement(s+"sheet",new XAttribute("name","Aktoren"),new XAttribute("sheetId",1),new XAttribute(r+"id","rId1")),new XElement(s+"sheet",new XAttribute("name","Abläufe"),new XAttribute("sheetId",2),new XAttribute(r+"id","rId3")))));
                Write("xl/_rels/workbook.xml.rels",new XElement(p+"Relationships",Rel("rId1","worksheet","worksheets/sheet1.xml"),Rel("rId2","styles","styles.xml"),Rel("rId3","worksheet","worksheets/sheet2.xml")));
                Write("xl/styles.xml",XElement.Parse($"<styleSheet xmlns='{s}'><fonts count='2'><font><sz val='11'/><name val='Calibri'/></font><font><b/><sz val='11'/><name val='Calibri'/></font></fonts><fills count='2'><fill><patternFill patternType='none'/></fill><fill><patternFill patternType='gray125'/></fill></fills><borders count='1'><border/></borders><cellStyleXfs count='1'><xf numFmtId='0' fontId='0' fillId='0' borderId='0'/></cellStyleXfs><cellXfs count='2'><xf numFmtId='0' fontId='0' fillId='0' borderId='0' xfId='0'/><xf numFmtId='0' fontId='1' fillId='0' borderId='0' xfId='0' applyFont='1'/></cellXfs><cellStyles count='1'><cellStyle name='Normal' xfId='0' builtinId='0'/></cellStyles></styleSheet>"));
                Write("xl/worksheets/sheet1.xml",sheet);
                Write("xl/worksheets/sheet2.xml",sequenceSheet);
                if(rows.Count>0)
                {
                    Write("xl/worksheets/_rels/sheet1.xml.rels",new XElement(p+"Relationships",Rel("rId1","table","../tables/table1.xml")));
                    Write("xl/tables/table1.xml",new XElement(s+"table",new XAttribute("id",1),new XAttribute("name","Aktoren"),new XAttribute("displayName","Aktoren"),new XAttribute("ref",range),new XAttribute("totalsRowShown",0),new XElement(s+"autoFilter",new XAttribute("ref",range)),new XElement(s+"tableColumns",new XAttribute("count",headers.Length),headers.Select((name,i)=>new XElement(s+"tableColumn",new XAttribute("id",i+1),new XAttribute("name",name)))),new XElement(s+"tableStyleInfo",new XAttribute("name","TableStyleMedium2"),new XAttribute("showRowStripes",1),new XAttribute("showColumnStripes",0),new XAttribute("showFirstColumn",0),new XAttribute("showLastColumn",0))));
                }
            }
            File.Move(temporary,path,true);
        }
        finally {if(File.Exists(temporary))File.Delete(temporary);}
    }
    internal static string Column(int index)
    {
        var name="";for(var n=index+1;n>0;n=(n-1)/26)name=(char)('A'+(n-1)%26)+name;return name;
    }
    private static string Clean(string text)
    {
        var result=new System.Text.StringBuilder();
        for(var i=0;i<text.Length;i++)
        {
            if(char.IsHighSurrogate(text[i])&&i+1<text.Length&&char.IsLowSurrogate(text[i+1])){result.Append(text[i]);result.Append(text[++i]);}
            else if(XmlConvert.IsXmlChar(text[i]))result.Append(text[i]);
        }
        return result.ToString();
    }
    private static XElement TextRow(XNamespace s,int number,string[] values,bool bold=false)=>new(s+"row",new XAttribute("r",number),values.Select((text,i)=>new XElement(s+"c",new XAttribute("r",$"{Column(i)}{number}"),new XAttribute("t","inlineStr"),new XAttribute("s",bold?1:0),new XElement(s+"is",new XElement(s+"t",new XAttribute(XNamespace.Xml+"space","preserve"),Clean(text))))));
}
