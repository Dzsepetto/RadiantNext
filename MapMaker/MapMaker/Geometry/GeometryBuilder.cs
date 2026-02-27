using System.Numerics;
using MapMaker.Core.Logger;
using MapMaker.Core.Models;

namespace MapMaker.Core.Geometry
{
    public static class GeometryBuilder
    {
        public static void BuildBrushPolygons(Brush brush, ILogger? logger = null)
        {
            if (brush.Faces.Count < 4)
            {
                logger?.Warning($"Brush has only {brush.Faces.Count} faces. A valid convex brush needs at least 4.");
            }

            logger?.Info($"Building polygons for brush with {brush.Faces.Count} faces.");

            int faceIndex = 0;

            foreach (var face in brush.Faces)
            {
                logger?.Info($"  Processing face #{faceIndex}");

                var vertices = new List<Vector3>();
                int intersectionTests = 0;
                int validIntersections = 0;

                for (int i = 0; i < brush.Faces.Count; i++)
                {
                    for (int j = i + 1; j < brush.Faces.Count; j++)
                    {
                        if (brush.Faces[i] == face ||
                            brush.Faces[j] == face)
                            continue;

                        intersectionTests++;

                        var intersection = Plane3D.Intersect(
                            face.Plane,
                            brush.Faces[i].Plane,
                            brush.Faces[j].Plane);

                        if (intersection == null)
                            continue;

                        var point = intersection.Value;

                        if (IsPointInsideBrush(point, brush))
                        {
                            vertices.Add(point);
                            validIntersections++;
                        }
                        else
                        {
                            logger?.Warning(
                                $"    Intersection outside brush on face #{faceIndex}");
                        }
                    }
                }

                logger?.Info(
                    $"    Intersection tests: {intersectionTests}, valid: {validIntersections}");

                if (vertices.Count < 3)
                {
                    logger?.Error(
                        $"    Face #{faceIndex} generated only {vertices.Count} vertices. Polygon invalid.");

                    face.Polygon = null;
                    faceIndex++;
                    continue;
                }

                var sorted = SortVerticesOnPlane(vertices, face.Plane).ToList();

                face.Polygon = new Polygon3D(sorted);

                logger?.Info(
                    $"    Face #{faceIndex} polygon built with {sorted.Count} vertices.");

                faceIndex++;
            }

            logger?.Info("Brush polygon generation completed.");
        }

        private static bool IsPointInsideBrush(Vector3 point, Brush brush)
        {
            const float epsilon = 0.01f;

            foreach (var f in brush.Faces)
            {
                if (f.Plane.DistanceToPoint(point) < -epsilon)
                    return false;
            }

            return true;
        }

        private static IEnumerable<Vector3> SortVerticesOnPlane(
            List<Vector3> vertices,
            Plane3D plane)
        {
            if (vertices.Count < 3)
                return vertices;

            var center = Vector3.Zero;
            foreach (var v in vertices)
                center += v;
            center /= vertices.Count;

            var right = Vector3.Normalize(
                Vector3.Cross(plane.Normal, Vector3.UnitY));

            if (right.LengthSquared() < 0.0001f)
            {
                right = Vector3.Normalize(
                    Vector3.Cross(plane.Normal, Vector3.UnitX));
            }

            var up = Vector3.Cross(right, plane.Normal);

            return vertices.OrderBy(v =>
            {
                var dir = v - center;
                var x = Vector3.Dot(dir, right);
                var y = Vector3.Dot(dir, up);
                return MathF.Atan2(y, x);
            }).ToList();
        }
    }
}