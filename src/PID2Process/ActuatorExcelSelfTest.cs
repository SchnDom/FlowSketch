using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;
namespace PID2Process;
internal static class ActuatorExcelSelfTest
{
    internal static void Run(string output,Action<bool,string> check)
    {
        var p=ProjectServices.NewBlank();
        var v10=new EquipmentItem{Type=EquipmentType.IsolationValve,Tag="Ventil 10"};var v2=new EquipmentItem{Type=EquipmentType.IsolationValve,Tag="Ventil 2"};var motor=new EquipmentItem{Type=EquipmentType.PumpMotor,Tag="=Motor & <Ä>"};
        p.Equipment.AddRange([v10,new(){Type=EquipmentType.MediaSource,Tag="Keine Medien"},v2,motor,new(){Type=EquipmentType.Instrument},new(){Type=EquipmentType.IsolationValve,Variant="Rückschlagarmatur"}]);
        var s10=new Scenario{Name="Szenario 10"};var s2=new Scenario{Name="Szenario 2"};s2.EquipmentStates[v2.Id]="Offen";p.Scenarios.AddRange([s10,s2]);
        var t10=new ControlStep{Name="Schritt 10",ScenarioId=s2.Id};var t2=new ControlStep{Name="Schritt 2",ScenarioId=s2.Id};t2.EquipmentStates[v2.Id]="Geschlossen";
        var orphan=new ControlStep{Name="Alt",ScenarioId=Guid.NewGuid()};p.ControlSteps.AddRange([t10,orphan,t2]);var before=JsonSerializer.Serialize(p,ProjectServices.JsonOptions);
        var rows=ActuatorExcelExport.Rows(p);
        check(rows.Count==12&&rows.All(x=>x.Actor is "Ventil 2" or "Ventil 10" or "=Motor & <Ä>"),"Excel: ausschließlich Aktoren, alle Aktoren je Schritt");
        check(rows[0].Scenario=="Szenario 2"&&rows[0].Step=="Schritt 2"&&rows[1].Actor=="Ventil 2"&&rows[2].Actor=="Ventil 10"&&rows[3].Step=="Schritt 10"&&rows[6].Scenario=="Szenario 10","Excel: natürliche Sortierung Szenario, Schritt und Aktor");
        check(rows[1].State=="Geschlossen"&&rows[4].State=="Offen"&&rows[6].State=="Aus","Excel: Schritt überschreibt Szenario, fehlende Werte verwenden Gerätegrundzustand");
        check(rows[6].Step=="Schritt 1"&&rows[9].Scenario=="(Ohne zugeordnetes Szenario)","Excel: Szenario ohne Schritte und verwaiste Schritte bleiben sichtbar");
        var file=Path.Combine(output,"aktoren-export.xlsx");ActuatorExcelExport.Export(p,file);
        using(var zip=ZipFile.OpenRead(file))
        {
            XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XDocument Read(string part){using var stream=zip.GetEntry(part)!.Open();return XDocument.Load(stream);}
            foreach(var entry in zip.Entries){using var stream=entry.Open();XDocument.Load(stream);}
            var sheet=Read("xl/worksheets/sheet1.xml");
            check(sheet.Descendants(ns+"row").Count()==13&&sheet.Descendants(ns+"t").First().Value=="Szenario"&&sheet.Descendants(ns+"t").Any(x=>x.Value==motor.Tag)&&!sheet.Descendants(ns+"f").Any(),"Excel: gültiges XML, vier Textspalten und sichere Sonderzeichen ohne Formeln");
            check(Read("xl/tables/table1.xml").Root!.Attribute("ref")!.Value=="A1:D13"&&sheet.Descendants(ns+"pane").Single().Attribute("state")!.Value=="frozen","Excel: filterbare Tabelle und fixierte Kopfzeile");
        }
        ActuatorExcelExport.Export(new EngineeringProject(),Path.Combine(output,"aktoren-leer.xlsx"));
        using(var zip=ZipFile.OpenRead(Path.Combine(output,"aktoren-leer.xlsx")))check(zip.GetEntry("xl/tables/table1.xml") is null,"Excel: leeres Projekt exportiert nur Kopfzeile ohne ungültige Tabelle");
        check(before==JsonSerializer.Serialize(p,ProjectServices.JsonOptions),"Excel-Export verändert weder Projektzustände noch Freigabe");
    }
}
