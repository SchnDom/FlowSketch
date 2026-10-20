using System.IO;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
namespace PID2Process;
public sealed class PdfRasterizer
{
    private PdfDocument? _document;
    private readonly SemaphoreSlim _renderLock = new(1,1);
    public int PageCount => (int)(_document?.PageCount ?? 0);
    public double AspectRatio(int index)
    {
        if (_document is null) return .708;
        using var page = _document.GetPage((uint)index); return page.Size.Height / page.Size.Width;
    }
    public async Task OpenAsync(string path) => _document = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path)));
    public async Task<BitmapSource> RenderAsync(int index, int requestedWidth = 2000)
    {
        if (_document is null) throw new InvalidOperationException("Keine PDF geöffnet.");
        await _renderLock.WaitAsync();
        try
        {
        using var page = _document.GetPage((uint)index);
        // Bound memory for large portrait pages while rendering from the original
        // PDF at the resolution required by the current display zoom.
        var aspect = page.Size.Height / page.Size.Width;
        var width = (uint)Math.Clamp(requestedWidth, 500, Math.Min(12000, (int)Math.Sqrt(48_000_000 / aspect)));
        using var stream = new InMemoryRandomAccessStream();
        await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = width });
        stream.Seek(0); using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)stream.Size); var bytes = new byte[(int)stream.Size]; reader.ReadBytes(bytes);
        using var memory = new MemoryStream(bytes);
        var image = BitmapFrame.Create(memory, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        image.Freeze(); return image;
        }
        finally { _renderLock.Release(); }
    }
}
