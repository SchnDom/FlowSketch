using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
namespace PID2Process;

public static class ExportService
{
    public static async Task ExportAsync(EngineeringProject project, string targetDirectory)
    {
        ProjectServices.ApplyApprovalStatus(project);
        project=BlankLayout.ExportSnapshot(project);
        if (project.BlankSheet is null && !File.Exists(project.Pdf.FilePath)) throw new FileNotFoundException("Referenz-PDF fehlt.");
        Directory.CreateDirectory(targetDirectory);
        var raster = new DocumentBackground(project); await raster.OpenAsync();
        project.Pdf.PageAspectRatios=Enumerable.Range(0,raster.PageCount).Select(raster.AspectRatio).ToList();
        for (var i = 0; i < raster.PageCount; i++)
        {
            var image = await raster.RenderAsync(i,6000); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
            using var stream = File.Create(Path.Combine(targetDirectory, $"page-{i+1}.png")); encoder.Save(stream);
        }
        var json = JsonSerializer.Serialize(project, ProjectServices.JsonOptions);
        File.WriteAllText(Path.Combine(targetDirectory,"project-data.json"),json);
        var destination = Path.Combine(targetDirectory,project.Pdf.FileName);
        if (project.BlankSheet is null && !Path.GetFullPath(destination).Equals(Path.GetFullPath(project.Pdf.FilePath),StringComparison.OrdinalIgnoreCase)) File.Copy(project.Pdf.FilePath,destination,true);
        File.WriteAllText(Path.Combine(targetDirectory,"viewer.html"),Html.Replace("__DATA__",json.Replace("</","<\\/")).Replace("__SYMBOLS__",SymbolCatalog.Json).Replace("__SCENE_SCRIPT__",ReviewScript));
    }
    private static string ReviewScript { get { using var stream=typeof(ExportService).Assembly.GetManifestResourceStream("PID2Process.ReviewScene.js")!; using var reader=new StreamReader(stream);using var zoomStream=typeof(ExportService).Assembly.GetManifestResourceStream("PID2Process.ReviewZoom.js")!;using var zoomReader=new StreamReader(zoomStream);return zoomReader.ReadToEnd()+"\n"+reader.ReadToEnd(); } }
    private const string Html = """
<!doctype html><html lang="de"><meta charset="utf-8"><title>FlowSketch Review</title>
<style>
*{box-sizing:border-box}body{margin:0;font:14px Segoe UI,sans-serif;background:#53616b;color:#203544}header{padding:12px;background:#e9eef2;display:flex;gap:18px;align-items:center}select{padding:6px}#warning{background:#ffe0a8;padding:10px}.layout{display:grid;grid-template-columns:minmax(0,1fr) 320px;min-height:0}.viewport{overflow:auto;padding:15px;min-width:0;min-height:0;touch-action:none;cursor:grab}.viewport.panning{cursor:grabbing}#paper-size{position:relative}#stage{position:relative;width:100%;background:white;transform-origin:0 0}html,body{height:100%}body{display:grid;grid-template-rows:auto auto minmax(0,1fr)}header{flex-wrap:wrap;gap:10px}button{padding:6px;cursor:pointer}.zoom-tools{display:flex;align-items:center;gap:5px}#zoom-level{min-width:48px;text-align:center}button:focus-visible,.viewport:focus-visible{outline:2px solid #007e87}#background{display:block;width:100%;height:auto}.device{position:absolute;display:grid;place-items:center;border:2px solid;border-radius:4px;background:white;cursor:pointer;font-size:20px}.device span{position:absolute;top:100%;font-size:12px;white-space:nowrap;background:white;color:#203544}aside{background:#fff;padding:18px;overflow:auto}article{background:#f3f5f7;padding:10px;margin:8px 0}.device .caption{top:100%;font-size:10px;white-space:normal;width:100%;text-align:center}.device .tag{top:calc(100% + 45px)}.device .caption.compact{font-size:9px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.device .tag.compact{top:calc(100% + 14px)}svg{position:absolute;inset:0;width:100%;height:100%;pointer-events:none}.flow{stroke-dasharray:15 7;animation:flow 1s linear infinite}.pressure{animation:pulse 1s ease-in-out infinite alternate}@keyframes flow{to{stroke-dashoffset:-22}}@keyframes pulse{to{opacity:.4}}h3{margin:16px 0 8px}
</style>
<header><b>FlowSketch</b><label>Seite <select id="page"></select></label><label>Szenario <select id="scenario"></select></label><label>Schritt <select id="step"></select></label><span class="zoom-tools" role="group" aria-label="Planzoom"><button id="zoom-out" title="Verkleinern" aria-label="Plan verkleinern">−</button><output id="zoom-level" aria-live="polite">100 %</output><button id="zoom-in" title="Vergrößern" aria-label="Plan vergrößern">+</button><button id="zoom-fit">Einpassen</button></span><span id="revision"></span></header><div id="warning"></div><div class="layout"><div class="viewport" id="viewport" tabindex="0" aria-label="Planansicht: Mausrad zum Zoomen, Ziehen zum Verschieben"><div id="paper-size"><div id="stage"><img id="background" alt="Zeichenblatt oder PDF-Seite" draggable="false"></div></div></div><aside><h2 id="name"></h2><p id="description"></p><div id="details"></div><h3>Schritte</h3><div id="steps"></div><h3>Alarme</h3><div id="alarms"></div></aside></div>
<script>
const PROJECT=__DATA__;
const SYMBOLS=__SYMBOLS__;
__SCENE_SCRIPT__
</script></html>
""";
}
