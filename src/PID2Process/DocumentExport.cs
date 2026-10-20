using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
namespace PID2Process;

internal static class DocumentExport
{
    internal static EngineeringProject Snapshot(EngineeringProject project) => JsonSerializer.Deserialize<EngineeringProject>(JsonSerializer.Serialize(project,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;
    internal static async Task ExportAsync(EngineeringProject project,string target,bool pdf,int? pageIndex,Guid? scenarioId)
    {
        if(project.BlankSheet is null && Path.GetFullPath(target).Equals(Path.GetFullPath(project.Pdf.FilePath),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Die Referenz-PDF darf nicht überschrieben werden.");
        var snapshot=BlankLayout.ExportSnapshot(project);var scenario=snapshot.Scenarios.FirstOrDefault(s=>s.Id==scenarioId);var raster=new DocumentBackground(snapshot);await raster.OpenAsync();
        var pages=pageIndex.HasValue?new[]{pageIndex.Value}:Enumerable.Range(0,raster.PageCount).ToArray();
        if(pdf)
        {
            using var input=snapshot.BlankSheet is null?PdfReader.Open(snapshot.Pdf.FilePath,PdfDocumentOpenMode.Import):null;using var output=new PdfDocument();output.Info.Title=snapshot.Name;
            foreach(var index in pages)
            {
                var page=input is null?output.AddPage():output.AddPage(input.Pages[index]);if(snapshot.BlankSheet is {} blank){page.Width=XUnit.FromMillimeter(blank.WidthMm);page.Height=XUnit.FromMillimeter(blank.HeightMm);}var aspect=raster.AspectRatio(index);var width=Math.Min(4000,Math.Sqrt(24_000_000/aspect));var overlay=PageVisual.Render(snapshot,index+1,width,aspect,scenario,null);
                var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(overlay));using var stream=new MemoryStream();encoder.Save(stream);stream.Position=0;
                using var image=XImage.FromStream(stream);using var graphics=XGraphics.FromPdfPage(page,XGraphicsPdfPageOptions.Append);
                // PDF /Rotate rotates the page contents, including appended graphics.
                // Map upright display coordinates back into the raw page coordinate space.
                var rawWidth=page.Width.Point;var rawHeight=page.Height.Point;var rotation=((page.Rotate%360)+360)%360;
                switch(rotation)
                {
                    case 90:graphics.TranslateTransform(0,rawHeight);graphics.RotateTransform(-90);break;
                    case 180:graphics.TranslateTransform(rawWidth,rawHeight);graphics.RotateTransform(180);break;
                    case 270:graphics.TranslateTransform(rawWidth,0);graphics.RotateTransform(90);break;
                }
                graphics.DrawImage(image,0,0,rotation is 90 or 270?rawHeight:rawWidth,rotation is 90 or 270?rawWidth:rawHeight);
            }
            output.Save(target);
        }
        else
        {
            if(!pageIndex.HasValue)Directory.CreateDirectory(target);
            foreach(var index in pages)
            {
                var background=await raster.RenderAsync(index,6000);var image=PageVisual.Render(snapshot,index+1,background.PixelWidth,raster.AspectRatio(index),scenario,background);
                var encoder=new JpegBitmapEncoder {QualityLevel=95};encoder.Frames.Add(BitmapFrame.Create(image));var file=pageIndex.HasValue?target:Path.Combine(target,$"Seite-{index+1:000}.jpg");using var stream=File.Create(file);encoder.Save(stream);
            }
        }
    }
}
