using System.Numerics;
using MapMaker.Core.Models;

namespace MapMaker.Core.Geometry
{
    public static class GeometryBuilder
    {
        public static void BuildBrushPolygons(Brush brush)
        {
            foreach (var face in brush.Faces)
            {
                var vertices = new List<Vector3>();

                // Minden 3 plane kombináció metszése
                for (int i = 0; i < brush.Faces.Count; i++)
                {
                    for (int j = i + 1; j < brush.Faces.Count; j++)
                    {
                        if (brush.Faces[i] == face ||
                            brush.Faces[j] == face)
                            continue;

                        var intersection = Plane3D.Intersect(face.Plane, brush.Faces[i].Plane, brush.Faces[j].Plane);

                        if (intersection != null)
                        {
                            var point = intersection.Value;

                            if (IsPointInsideBrush(point, brush))
                            {
                                vertices.Add(point);
                            }
                        }
                    }
                }

                face.Polygon = new Polygon3D(
                    SortVerticesOnPlane(vertices, face.Plane));
            }
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