using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;

namespace PID2Process;

public static class ProjectServices
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static EngineeringProject NewBlank()=>new(){Name="Leeres Blatt",BlankSheet=new(),Pdf=new(){PageCount=1,PageAspectRatios=[210d/297]}};
    internal static string ReferenceHash(EngineeringProject p)=>p.BlankSheet is {} b
        ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(FormattableString.Invariant($"Blank:{b.Format}:{b.WidthMm:R}:{b.HeightMm:R}"))))
        : p.Pdf.Sha256;
    public static string HashFile(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    // A lightweight count is sufficient for revision metadata; rendering remains the browser's responsibility.
    public static int CountPdfPages(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return Math.Max(1, Encoding.Latin1.GetString(bytes).Split("/Type /Page", StringSplitOptions.None).Length - 1);
    }

    public static bool ValidatePdf(EngineeringProject project, out string reason)
    {
        if(project.BlankSheet is {} sheet)
        {
            var valid=double.IsFinite(sheet.WidthMm)&&double.IsFinite(sheet.HeightMm)&&sheet.WidthMm>=1&&sheet.HeightMm>=1;
            reason=valid?"Leeres Zeichenblatt ist verfügbar.":"Ungültige Zeichenblattgröße.";
            return valid;
        }
        var pdf = project.Pdf;
        if (string.IsNullOrWhiteSpace(pdf.FilePath) || !File.Exists(pdf.FilePath))
        {
            reason = "Die referenzierte PDF-Unterlage ist nicht verfügbar.";
            return false;
        }

        var currentHash = HashFile(pdf.FilePath);
        if (!string.Equals(currentHash, pdf.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Die PDF-Unterlage wurde seit dem Import geändert. Overlay prüfen und erneut freigeben.";
            return false;
        }

        reason = "PDF-Unterlage entspricht der importierten Revision.";
        return true;
    }

    public static void ApplyApprovalStatus(EngineeringProject project)
    {
        var valid = ValidatePdf(project, out var reason);
        var engineeringValid=ValidateEngineering(project,out var engineeringReason);
        project.Approval.IsBlocked = !valid || !engineeringValid || !project.Approval.IsApproved ||
            !string.Equals(project.Approval.ApprovedPdfHash, ReferenceHash(project), StringComparison.OrdinalIgnoreCase);
        project.Approval.BlockReason = project.Approval.IsBlocked
            ? (!valid ? reason : !engineeringValid?engineeringReason:"Projekt wurde für diese Dokumentrevision noch nicht freigegeben.")
            : "Freigegeben.";
    }

    public static void Save(string path, EngineeringProject project)
    {
        Normalize(project);project.FormatVersion="2.1";
        File.WriteAllText(path, JsonSerializer.Serialize(project, JsonOptions));
    }

    public static EngineeringProject Load(string path)
    {
        var project=JsonSerializer.Deserialize<EngineeringProject>(File.ReadAllText(path),JsonOptions)??throw new InvalidDataException("Ungültige PID2Process-Projektdatei.");Normalize(project);return project;
    }
    internal static void Normalize(EngineeringProject project)
    {
        if(CrossReferences.Normalize(project)){project.Approval.IsApproved=false;project.Approval.IsBlocked=true;project.Approval.BlockReason="Querverweise kompakt dargestellt und lange IDs gruppenerhaltend gekürzt. Erneute Freigabe erforderlich.";}
        if(project.BlankSheet is {} blank){project.Pdf.PageCount=1;project.Pdf.PageAspectRatios=[blank.HeightMm/blank.WidthMm];}
        if(ScenarioSteps.Ensure(project)){project.Approval.IsApproved=false;project.Approval.IsBlocked=true;project.Approval.BlockReason="Ersten Szenarioschritt ergänzt. Erneute Freigabe erforderlich.";}
        var upgradedDrawing=project.Equipment.Any(DrawingGeometry.IsLine)&&project.Equipment.Where(DrawingGeometry.IsLine).Any(x=>x.Drawing.Start is null||x.Drawing.End is null);DrawingGeometry.Normalize(project);
        if(upgradedDrawing){project.Approval.IsApproved=false;project.Approval.IsBlocked=true;project.Approval.BlockReason="Grafikgeometrie aktualisiert. Erneute Freigabe erforderlich.";}
        MediaSettings.Normalize(project);
        foreach(var item in project.Equipment)if(item.Actuator==ActuatorType.Unspecified)item.Actuator=item.Actuation switch{"Manuell"=>ActuatorType.Manual,"Elektrisch"=>ActuatorType.ElectricMotor,_=>ActuatorType.Unspecified};
        foreach(var pipe in project.Pipes)foreach(var c in new[]{pipe.StartConnection,pipe.EndConnection})if(c?.EquipmentId is Guid id&&project.Equipment.FirstOrDefault(x=>x.Id==id) is {} item&&!PipeNetwork.Ports(item).Contains(c.Port)&&!c.NeedsReview){c.PreservedPoint=PipeNetwork.Port(item,c.Port);c.NeedsReview=true;}
        if(!ValidateEngineering(project,out var reason)){project.Approval.IsApproved=false;project.Approval.IsBlocked=true;project.Approval.BlockReason=reason;}
    }
    internal static bool ValidateEngineering(EngineeringProject project,out string reason)
    {
        if(project.Pipes.Any(p=>p.StartConnection?.NeedsReview==true||p.EndConnection?.NeedsReview==true)){reason="Markierte Rohranschlüsse vor der Freigabe prüfen und neu verbinden.";return false;}
        if(project.Equipment.Any(x=>SymbolCatalog.Valve(x)&&!SymbolCatalog.CheckValve(x)&&x.Actuator==ActuatorType.Unspecified)){reason="Antriebsbauart der markierten Armaturen vor der Freigabe auswählen.";return false;}
        reason="Engineering-Angaben vollständig.";return true;
    }
}
