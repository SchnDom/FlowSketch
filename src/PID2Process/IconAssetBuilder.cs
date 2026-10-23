using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PID2Process;
// Neutral FlowSketch mark matching _media/FlowSketch.svg, rendered at each icon size.
internal static class IconAssetBuilder
{
    internal static void Write(string folder)
    {
        var artwork=new DrawingVisual();using(var c=artwork.RenderOpen())
        {
            var background=PageVisual.Color("#202A35");var ink=PageVisual.Color("#51C7EF");c.DrawRoundedRectangle(background,null,new Rect(4,4,248,248),40,40);
            c.DrawGeometry(null,new Pen(ink,16){LineJoin=PenLineJoin.Round},Geometry.Parse("M40,60 H112 V132 H216 M112,132 V216 H40"));
            foreach(var point in new[]{new Point(40,60),new Point(216,132),new Point(40,216)})c.DrawEllipse(background,new Pen(ink,12),point,18,18);
        }
        var source=new RenderTargetBitmap(256,256,96,96,PixelFormats.Pbgra32);source.Render(artwork);source.Freeze();
        var frames=new List<(int Size,byte[] Png)>();
        foreach(var size in new[]{16,24,32,48,64,96,128,256})
        {
            var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.HighQuality);using(var c=visual.RenderOpen())c.DrawImage(source,new Rect(0,0,size,size));
            var bitmap=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var png=new MemoryStream();encoder.Save(png);frames.Add((size,png.ToArray()));
            if(size is 16 or 32 or 96 or 256)File.WriteAllBytes(Path.Combine(folder,$"favicon-{size}x{size}.png"),png.ToArray());
            if(size==256)File.WriteAllBytes(Path.Combine(folder,"FlowSketch.png"),png.ToArray());
        }
        using var file=new MemoryStream();using var writer=new BinaryWriter(file);writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)frames.Count);var offset=6+16*frames.Count;
        foreach(var (size,png) in frames){writer.Write((byte)(size==256?0:size));writer.Write((byte)(size==256?0:size));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(png.Length);writer.Write(offset);offset+=png.Length;}
        foreach(var frame in frames)writer.Write(frame.Png);
        writer.Flush();File.WriteAllBytes(Path.Combine(folder,"FlowSketch.ico"),file.ToArray());File.WriteAllBytes(Path.Combine(folder,"favicon.ico"),file.ToArray());
    }
}
