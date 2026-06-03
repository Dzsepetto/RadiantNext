using MapMaker.Core.Models;
using MapMaker.Core.Validation;
using System.Diagnostics;

namespace MapMaker.Editor.Diagnostics;

public sealed class EditorDiagnosticsService
{
    public EditorDiagnosticsResult Analyze(Map? map)
    {
        var result = new EditorDiagnosticsResult();

        if (map == null)
        {
            result.Messages.Add("No map loaded.");
            return result;
        }

        result.EntityCount = map.Entities.Count;

        for (int entityIndex = 0; entityIndex < map.Entities.Count; entityIndex++)
        {
            var entity = map.Entities[entityIndex];

            for (int brushIndex = 0; brushIndex < entity.Brushes.Count; brushIndex++)
            {
                var brush = entity.Brushes[brushIndex];

                result.BrushCount++;
                result.FaceCount += brush.Faces.Count;

                AnalyzeBrush(result, brush, entityIndex, brushIndex);
            }
        }

        return result;
    }

    private static void AnalyzeBrush(
        EditorDiagnosticsResult result,
        Brush brush,
        int entityIndex,
        int brushIndex)
    {
        var issues = BrushValidator.Validate(brush);

        foreach (var issue in issues)
        {
            if (issue.Severity == BrushValidationSeverity.Error)
                result.ErrorCount++;

            if (issue.Severity == BrushValidationSeverity.Warning)
                result.WarningCount++;

            result.Messages.Add(
                $"Entity {entityIndex}, Brush {brushIndex}: {issue}");
        }

        for (int faceIndex = 0; faceIndex < brush.Faces.Count; faceIndex++)
        {
            var face = brush.Faces[faceIndex];

            if (face.Polygon == null)
            {
                result.PolygonNullCount++;

                result.Messages.Add(
                    $"Entity {entityIndex}, Brush {brushIndex}, Face {faceIndex}: Polygon is NULL.");
            }
            else
            {
                result.Messages.Add(
                    $"Entity {entityIndex}, Brush {brushIndex}, Face {faceIndex}: Polygon vertices={face.Polygon.Vertices.Count}.");
            }
        }
    }
}