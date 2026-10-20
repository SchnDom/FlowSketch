using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
// Multi-resolution ICO derived from the supplied PNG; source artwork and alpha are preserved.
// Only the generated ICO is written; original PNG files remain unchanged.
internal static class IconAssetBuilder
{
    internal static void Write(string folder)
    {
        var source=new BitmapImage();source.BeginInit();source.CacheOption=BitmapCacheOption.OnLoad;source.UriSource=new Uri(Path.GetFullPath(Path.Combine(folder,"CS26001-1-Icon-4c.png")));source.EndInit();source.Freeze();
        var frames=new List<(int Size,byte[] Png)>();
        foreach(var size in new[]{16,24,32,48,64,128,256})
        {
            var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.HighQuality);using(var c=visual.RenderOpen())c.DrawImage(source,new Rect(0,0,size,size));
            var bitmap=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var png=new MemoryStream();encoder.Save(png);frames.Add((size,png.ToArray()));
        }
        using var file=File.Create(Path.Combine(folder,"FlowSketch.ico"));using var writer=new BinaryWriter(file);writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)frames.Count);var offset=6+16*frames.Count;
        foreach(var (size,png) in frames){writer.Write((byte)(size==256?0:size));writer.Write((byte)(size==256?0:size));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(png.Length);writer.Write(offset);offset+=png.Length;}
        foreach(var frame in frames)writer.Write(frame.Png);
    }
}
