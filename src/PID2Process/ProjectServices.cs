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
        ValidateVersion(project.FormatVersion);
        ValidateRequiredValues(project,"Projekt");
        Normalize(project);project.FormatVersion="2.2";
        var destination=Path.GetFullPath(path);
        var temporary=Path.Combine(Path.GetDirectoryName(destination)!,"."+Guid.NewGuid()+".tmp");
        try
        {
            var bytes=JsonSerializer.SerializeToUtf8Bytes(project,JsonOptions);
            using(var stream=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))
            {stream.Write(bytes);stream.Flush(true);}
            if(File.Exists(destination))File.Replace(temporary,destination,destination+".bak");
            else File.Move(temporary,destination);
        }
        finally {if(File.Exists(temporary))File.Delete(temporary);}
    }

    public static EngineeringProject Load(string path)=>Load(path,out _);
    internal static EngineeringProject Load(string path,out bool normalized)
    {
        var json=File.ReadAllText(path);
        using var data=JsonDocument.Parse(json);
        if(data.RootElement.ValueKind!=JsonValueKind.Object)throw new InvalidDataException("Die Projektdatei muss ein JSON-Objekt enthalten.");
        var format="2.0";
        if(data.RootElement.TryGetProperty("formatVersion",out var version))
        {
            if(version.ValueKind!=JsonValueKind.String)throw new InvalidDataException("formatVersion muss eine Versionszeichenfolge sein, z. B. 2.2.");
            format=version.GetString()!;
        }
        ValidateVersion(format);
        var project=JsonSerializer.Deserialize<EngineeringProject>(json,JsonOptions)??throw new InvalidDataException("Ungültige FlowSketch-Projektdatei.");
        project.FormatVersion=format;
        ValidateRequiredValues(project,"Projekt");
        var before=JsonSerializer.Serialize(project,JsonOptions);
        Normalize(project);normalized=before!=JsonSerializer.Serialize(project,JsonOptions);
        if(normalized&&project.Approval.IsApproved)
        {
            project.Approval.IsApproved=false;project.Approval.IsBlocked=true;
            project.Approval.BlockReason="Projektangaben beim Laden aktualisiert. Änderungen prüfen und erneut freigeben.";
        }
        return project;
    }
    private static void ValidateVersion(string format)
    {
        if(format is not ("2.0" or "2.1" or "2.2"))
            throw new InvalidDataException($"Projektformat '{format}' wird nicht unterstützt. Unterstützt werden 2.0, 2.1 und 2.2. Bitte die passende FlowSketch-Version verwenden.");
    }
    // Reject malformed required fields before migrations access them. Optional
    // connections and assignments may still explicitly contain null.
    private static readonly System.Reflection.NullabilityInfoContext Nullability=new();
    private static void ValidateRequiredValues(object value,string path)
    {
        if(value is System.Collections.IDictionary dictionary)
        {
            foreach(System.Collections.DictionaryEntry entry in dictionary)
            {
                if(entry.Value is null)throw new InvalidDataException($"{path}[{entry.Key}] darf nicht null sein.");
                ValidateRequiredValues(entry.Value,$"{path}[{entry.Key}]");
            }
        }
        else if(value is System.Collections.IEnumerable sequence && value is not string)
        {
            var index=0;foreach(var entry in sequence)
            {
                if(entry is null)throw new InvalidDataException($"{path}[{index}] darf nicht null sein.");
                ValidateRequiredValues(entry,$"{path}[{index++}]");
            }
        }
        else if(value.GetType().Namespace==typeof(EngineeringProject).Namespace && !value.GetType().IsEnum)
        {
            foreach(var property in value.GetType().GetProperties().Where(p=>p.CanRead&&p.CanWrite))
            {
                var child=property.GetValue(value);var childPath=path+"."+JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                if(child is not null)ValidateRequiredValues(child,childPath);
                else if(Nullability.Create(property).ReadState==System.Reflection.NullabilityState.NotNull)
                    throw new InvalidDataException(childPath+" darf nicht null sein.");
            }
        }
    }
    internal static void Normalize(EngineeringProject project)
    {
        SensorMigration.Upgrade(project);
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
        if(project.Alarms.Any(a=>!a.IsDerived&&!project.Equipment.Any(x=>x.Type!=EquipmentType.Drawing&&x.Tag==a.Tag))){reason="Alarme ohne vorhandenes Betriebsmittel vor der Freigabe neu zuordnen.";return false;}
        if(project.Pipes.Any(p=>p.StartConnection?.NeedsReview==true||p.EndConnection?.NeedsReview==true)){reason="Markierte Rohranschlüsse vor der Freigabe prüfen und neu verbinden.";return false;}
        if(project.Equipment.Any(x=>SymbolCatalog.Valve(x)&&!SymbolCatalog.CheckValve(x)&&x.Actuator==ActuatorType.Unspecified)){reason="Antriebsbauart der markierten Armaturen vor der Freigabe auswählen.";return false;}
        reason="Engineering-Angaben vollständig.";return true;
    }
}
