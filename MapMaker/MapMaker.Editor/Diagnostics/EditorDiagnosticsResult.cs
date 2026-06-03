namespace MapMaker.Editor.Diagnostics;

public sealed class EditorDiagnosticsResult
{
    public int EntityCount { get; set; }
    public int BrushCount { get; set; }
    public int FaceCount { get; set; }
    public int PolygonNullCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }

    public List<string> Messages { get; } = new();

    public bool HasIssues =>
        ErrorCount > 0 ||
        WarningCount > 0 ||
        PolygonNullCount > 0;
}