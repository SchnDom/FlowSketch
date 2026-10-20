using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;

// Blank documents never create or require a substitute reference PDF.
internal sealed class DocumentBackground
{
    private readonly EngineeringProject _project;
    private readonly PdfRasterizer _raster=new();
    internal DocumentBackground(EngineeringProject project)=>_project=project;
    internal int PageCount=>_project.BlankSheet is null?_raster.PageCount:1;
    internal double AspectRatio(int page)=>_project.BlankSheet is {} b?b.HeightMm/b.WidthMm:_raster.AspectRatio(page);
    internal async Task OpenAsync()
    {
        if(!ProjectServices.ValidatePdf(_project,out var reason))throw new InvalidOperationException(reason);
        if(_project.BlankSheet is null)await _raster.OpenAsync(_project.Pdf.FilePath);
    }
    internal async Task<BitmapSource> RenderAsync(int page,int width)
    {
        if(_project.BlankSheet is null)return await _raster.RenderAsync(page,width);
        width=Math.Max(1,(int)Math.Min(width,Math.Sqrt(24_000_000/AspectRatio(page))));
        var visual=new DrawingVisual();using(var drawing=visual.RenderOpen())drawing.DrawRectangle(Brushes.White,null,new System.Windows.Rect(0,0,width,width*AspectRatio(page)));
        var bitmap=new RenderTargetBitmap(width,(int)Math.Ceiling(width*AspectRatio(page)),96,96,PixelFormats.Pbgra32);
        bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
}
