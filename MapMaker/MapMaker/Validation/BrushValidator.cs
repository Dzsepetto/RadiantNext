using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Core.Validation
{
    public enum BrushValidationSeverity
    {
        Warning,
        Error
    }

    public sealed class BrushValidationIssue
    {
        public BrushValidationSeverity Severity { get; }
        public string Message { get; }

        public BrushValidationIssue(BrushValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Severity}: {Message}";
        }
    }

    public static class BrushValidator
    {
        private const float Epsilon = 0.01f;
        private const float NormalEpsilon = 0.999f;

        public static List<BrushValidationIssue> Validate(Brush brush)
        {
            var issues = new List<BrushValidationIssue>();

            if (brush.Faces.Count < 4)
            {
                issues.Add(new BrushValidationIssue(
                    BrushValidationSeverity.Error,
                    "Brush must have at least 4 faces."
                ));

                return issues;
            }

            ValidateDegenerateFaces(brush, issues);
            ValidateDuplicatePlanes(brush, issues);

            // Fontos: előbb Build(), mert ez javítja a face normálokat.
            ValidateFacePolygons(brush, issues);

            // Utána polygonokból nézzük a vertexeket, nem raw brush.GetVertices()-ből.
            ValidateBrushVerticesAfterBuild(brush, issues);

            return issues;
        }

        private static void ValidateDegenerateFaces(
            Brush brush,
            List<BrushValidationIssue> issues)
        {
            for (int i = 0; i < brush.Faces.Count; i++)
            {
                var face = brush.Faces[i];

                var a = face.P2 - face.P1;
                var b = face.P3 - face.P1;
                var cross = Vector3.Cross(a, b);

                if (cross.LengthSquared() < Epsilon * Epsilon)
                {
                    issues.Add(new BrushValidationIssue(
                        BrushValidationSeverity.Error,
                        $"Face {i} is degenerate. Its three points are collinear or too close."
                    ));
                }
            }
        }

        private static void ValidateDuplicatePlanes(
            Brush brush,
            List<BrushValidationIssue> issues)
        {
            for (int i = 0; i < brush.Faces.Count - 1; i++)
            {
                var planeA = brush.Faces[i].Plane;

                for (int j = i + 1; j < brush.Faces.Count; j++)
                {
                    var planeB = brush.Faces[j].Plane;

                    float normalDot = Vector3.Dot(planeA.Normal, planeB.Normal);
                    float distanceDelta = MathF.Abs(planeA.D - planeB.D);

                    if (normalDot > NormalEpsilon && distanceDelta < Epsilon)
                    {
                        issues.Add(new BrushValidationIssue(
                            BrushValidationSeverity.Warning,
                            $"Faces {i} and {j} are on the same plane."
                        ));
                    }
                }
            }
        }

        private static void ValidateFacePolygons(
            Brush brush,
            List<BrushValidationIssue> issues)
        {
            try
            {
                MapMaker.Core.Builders.BrushBuilder.Build(brush);
            }
            catch (Exception ex)
            {
                issues.Add(new BrushValidationIssue(
                    BrushValidationSeverity.Error,
                    $"Brush polygons could not be built: {ex.Message}"
                ));

                return;
            }

            for (int i = 0; i < brush.Faces.Count; i++)
            {
                var face = brush.Faces[i];

                if (face.Polygon == null)
                {
                    issues.Add(new BrushValidationIssue(
                        BrushValidationSeverity.Warning,
                        $"Face {i} did not generate a visible polygon. It may be a bevel, redundant, or non-contributing plane."
                    ));

                    continue;
                }

                if (face.Polygon.Vertices.Count < 3)
                {
                    issues.Add(new BrushValidationIssue(
                        BrushValidationSeverity.Error,
                        $"Face {i} generated fewer than 3 polygon vertices."
                    ));
                }
            }
        }

        private static void ValidateBrushVerticesAfterBuild(
            Brush brush,
            List<BrushValidationIssue> issues)
        {
            var vertices = new List<Vector3>();

            foreach (var face in brush.Faces)
            {
                if (face.Polygon == null)
                    continue;

                foreach (var vertex in face.Polygon.Vertices)
                {
                    bool duplicate = vertices.Any(existing =>
                        Vector3.DistanceSquared(existing, vertex) < Epsilon * Epsilon);

                    if (!duplicate)
                    {
                        vertices.Add(vertex);
                    }
                }
            }

            if (vertices.Count < 4)
            {
                issues.Add(new BrushValidationIssue(
                    BrushValidationSeverity.Error,
                    $"Brush generated only {vertices.Count} vertices after polygon build. A valid convex brush needs at least 4 vertices."
                ));
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];

                if (
                    float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z) ||
                    float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.Z)
                )
                {
                    issues.Add(new BrushValidationIssue(
                        BrushValidationSeverity.Error,
                        $"Vertex {i} contains invalid numeric values."
                    ));
                }
            }
        }
    }
}