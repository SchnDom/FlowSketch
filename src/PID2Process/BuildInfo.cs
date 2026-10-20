namespace PID2Process;

internal static class BuildInfo
{
    internal const string Name="FlowSketch";
    internal static string Version=>typeof(BuildInfo).Assembly.GetName().Version!.ToString(3);
}
