namespace PID2Process;

internal static class BlankLayout
{
    internal static bool Free(EngineeringProject p)=>p.BlankSheet?.Format=="Formfrei";
    internal static void Set(EngineeringProject p,string format,bool landscape)
    {
        var b=p.BlankSheet??throw new InvalidOperationException("Blattlayout ist nur ohne PDF verfügbar.");
        if(format is not ("A4" or "A3" or "A2" or "Formfrei"))throw new InvalidOperationException("Unbekanntes Blattformat.");
        if(format!="Formfrei")
        {
            var (shortSide,longSide)=format switch{"A3"=>(297d,420d),"A2"=>(420d,594d),_=>(210d,297d)};
            Resize(p,landscape?longSide:shortSide,landscape?shortSide:longSide,0,0);
            b.ExportWindow=null;
        }
        b.Format=format;b.Landscape=landscape;
    }
    // Preserve physical geometry and endpoint identities when extending the work area.
    internal static void Resize(EngineeringProject p,double width,double height,double left,double top)
    {
        var b=p.BlankSheet!;
        var oldWidth=b.WidthMm;var oldHeight=b.HeightMm;
        void Point(PipePoint q){q.X=(q.X*oldWidth+left)/width;q.Y=(q.Y*oldHeight+top)/height;}
        foreach(var e in p.Equipment){if(e.Sensor.ConnectionEnd is {} endpoint)Point(endpoint);if(e.Drawing.Start is {} a)Point(a);if(e.Drawing.End is {} z)Point(z);e.Drawing.PageWidth=2000*width/297;e.X=(e.X*oldWidth+left)/width;e.Y=(e.Y*oldHeight+top)/height;e.Width*=oldWidth/width;e.Height*=oldHeight/height;}
        foreach(var pipe in p.Pipes)
        {
            foreach(var q in pipe.Points)Point(q);
            var start=new PipePoint(pipe.X1,pipe.Y1);var end=new PipePoint(pipe.X2,pipe.Y2);Point(start);Point(end);
            pipe.X1=start.X;pipe.Y1=start.Y;pipe.X2=end.X;pipe.Y2=end.Y;
            foreach(var c in new[]{pipe.StartConnection,pipe.EndConnection})if(c?.PreservedPoint is {} q)Point(q);
        }
        foreach(var line in p.SignalLines)foreach(var q in line.Points)Point(q);
        if(b.ExportWindow is {} f){f.XMm+=left;f.YMm+=top;}
        b.WidthMm=width;b.HeightMm=height;p.Pdf.PageAspectRatios=[height/width];
    }
    internal static void ValidateWindow(ExportWindow f)
    {
        if(!double.IsFinite(f.XMm)||!double.IsFinite(f.YMm)||!double.IsFinite(f.WidthMm)||!double.IsFinite(f.HeightMm)||f.WidthMm<1||f.HeightMm<1)
            throw new InvalidOperationException("Ein gültiges Druck-/Exportfenster aufziehen.");
    }
    internal static EngineeringProject ExportSnapshot(EngineeringProject p)
    {
        var copy=DocumentExport.Snapshot(p);DrawingGeometry.Normalize(copy);
        if(!Free(copy))return copy;
        var f=copy.BlankSheet!.ExportWindow??throw new InvalidOperationException("Für Formfrei zuerst ein Druck-/Exportfenster aufziehen (Datei → Blattlayout).");
        ValidateWindow(f);
        Resize(copy,f.WidthMm,f.HeightMm,-f.XMm,-f.YMm);
        copy.BlankSheet.Format="Ausschnitt";copy.BlankSheet.ExportWindow=null;
        return copy;
    }
}
