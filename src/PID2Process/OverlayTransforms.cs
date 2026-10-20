using System.Text.Json;
using System.Windows;
namespace PID2Process;

internal static class OverlayTransforms
{
    // Displacements are display pixels: the dominant axis is independent of PDF aspect/zoom.
    internal static Vector Direction(Vector displacement,bool shift,ref int axis)
    {
        if(!shift){axis=0;return displacement;}
        if(axis==0&&displacement.Length>=2)axis=Math.Abs(displacement.X)>=Math.Abs(displacement.Y)?1:2;
        return axis==1?new(displacement.X,0):axis==2?new(0,displacement.Y):new();
    }
    internal static void Resize(EquipmentItem item,double width,double height,bool proportional,double baseWidth,double baseHeight,double pageWidth=1,double pageHeight=1)
    {
        if(proportional)
        {
            var a=baseWidth*pageWidth;var b=baseHeight*pageHeight;var scale=(width*pageWidth*a+height*pageHeight*b)/(a*a+b*b);
            var maximum=Math.Min((1-item.X)/baseWidth,(1-item.Y)/baseHeight);var minimum=Math.Min(maximum,Math.Max(.001/baseWidth,.001/baseHeight));scale=Math.Clamp(scale,minimum,maximum);
            item.Width=baseWidth*scale;item.Height=baseHeight*scale;
        }
        else{item.Width=Math.Clamp(width,Math.Min(.001,1-item.X),1-item.X);item.Height=Math.Clamp(height,Math.Min(.001,1-item.Y),1-item.Y);}
    }
    internal static EquipmentItem Copy(EngineeringProject project,EquipmentItem source)
    {
        var copy=JsonSerializer.Deserialize<EquipmentItem>(JsonSerializer.Serialize(source,ProjectServices.JsonOptions),ProjectServices.JsonOptions)!;
        copy.Id=Guid.NewGuid();copy.GroupId=null;var root=source.Tag+"-Kopie";copy.Tag=root;var suffix=2;while(project.Equipment.Any(x=>x.Tag==copy.Tag))copy.Tag=root+"-"+suffix++;
        project.Equipment.Add(copy);
        void Assign(Dictionary<Guid,string> states,Dictionary<Guid,string> texts,Dictionary<Guid,TankAssignment> tanks)
        {
            if(states.TryGetValue(source.Id,out var state))states[copy.Id]=state;
            if(texts.TryGetValue(source.Id,out var text))texts[copy.Id]=text;
            if(tanks.TryGetValue(source.Id,out var tank))tanks[copy.Id]=new(){Filled=tank.Filled,Pressurized=tank.Pressurized,MediumId=tank.MediumId,MediumType=tank.MediumType,MediumName=tank.MediumName};
        }
        foreach(var s in project.Scenarios){Assign(s.EquipmentStates,s.TextValues,s.TankStates);if(s.SensorValues.TryGetValue(source.Id,out var value))s.SensorValues[copy.Id]=value;}
        foreach(var s in project.ControlSteps){Assign(s.EquipmentStates,s.TextValues,s.TankStates);if(s.SensorValues.TryGetValue(source.Id,out var value))s.SensorValues[copy.Id]=value;}
        return copy;
    }
}
