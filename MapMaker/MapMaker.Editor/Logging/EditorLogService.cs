using MapMaker.Editor.Diagnostics;
using System.Diagnostics;

namespace MapMaker.Editor.Logging;

public sealed class EditorLogService
{
    public void LogDiagnostics(EditorDiagnosticsResult result)
    {
        Debug.WriteLine("---- EDITOR DIAGNOSTICS ----");
        Debug.WriteLine($"Entities: {result.EntityCount}");
        Debug.WriteLine($"Brushes: {result.BrushCount}");
        Debug.WriteLine($"Faces: {result.FaceCount}");
        Debug.WriteLine($"Polygon NULL: {result.PolygonNullCount}");
        Debug.WriteLine($"Warnings: {result.WarningCount}");
        Debug.WriteLine($"Errors: {result.ErrorCount}");

        foreach (var message in result.Messages)
            Debug.WriteLine(message);

        Debug.WriteLine("----------------------------");
    }
}