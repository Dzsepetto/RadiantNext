using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Core.Builders
{
    public static class BrushBuilder
    {
        const float EPSILON = 0.01f;

        public static void Build(Brush brush)
        {
            var buildFaces = CreateBuildFacesWithFixedNormals(brush);

            var vertices = GenerateVertices(buildFaces);

            GenerateFacePolygons(brush, buildFaces, vertices);
        }

        private static List<Face> CreateBuildFacesWithFixedNormals(Brush brush)
        {
            var buildFaces = brush.Faces.ToList();

            var points = GenerateVerticesRaw(buildFaces);

            if (points.Count == 0)
                return buildFaces;

            var center = Vector3.Zero;

            foreach (var v in points)
                center += v;

            center /= points.Count;

            for (int i = 0; i < buildFaces.Count; i++)
            {
                var face = buildFaces[i];
                float d = face.Plane.DistanceToPoint(center);

                if (d > 0)
                    buildFaces[i] = face.Flip();
            }

            return buildFaces;
        }

        public static List<Vector3> GenerateVertices(List<Face> faces)
        {
            var vertices = new List<Vector3>();
            int count = faces.Count;

            for (int i = 0; i < count - 2; i++)
            {
                for (int j = i + 1; j < count - 1; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        var p = Plane3D.Intersect(
                            faces[i].Plane,
                            faces[j].Plane,
                            faces[k].Plane);

                        if (p == null)
                            continue;

                        var point = p.Value;

                        bool inside = true;

                        foreach (var face in faces)
                        {
                            float d = face.Plane.DistanceToPoint(point);

                            if (d > EPSILON)
                            {
                                inside = false;
                                break;
                            }
                        }

                        if (!inside)
                            continue;

                        bool duplicate = vertices.Any(v =>
                            Vector3.DistanceSquared(v, point) < EPSILON * EPSILON);

                        if (!duplicate)
                            vertices.Add(point);
                    }
                }
            }

            return vertices;
        }

        public static void GenerateFacePolygons(
            Brush originalBrush,
            List<Face> buildFaces,
            List<Vector3> allVertices)
        {
            for (int i = 0; i < originalBrush.Faces.Count; i++)
            {
                var originalFace = originalBrush.Faces[i];
                var buildFace = buildFaces[i];

                var faceVerts = new List<Vector3>();

                foreach (var v in allVertices)
                {
                    if (MathF.Abs(buildFace.Plane.DistanceToPoint(v)) < EPSILON)
                        faceVerts.Add(v);
                }

                if (faceVerts.Count < 3)
                {
                    originalFace.Polygon = null;
                    continue;
                }

                originalFace.Polygon = new Polygon3D(
                    SortVertices(faceVerts, buildFace.Plane));
            }
        }

        private static List<Vector3> SortVertices(List<Vector3> verts, Plane3D plane)
        {
            var center = Vector3.Zero;

            foreach (var v in verts)
                center += v;

            center /= verts.Count;

            Vector3 axis = MathF.Abs(plane.Normal.Z) > 0.9f
                ? Vector3.UnitX
                : Vector3.UnitZ;

            Vector3 right = Vector3.Normalize(Vector3.Cross(axis, plane.Normal));
            Vector3 up = Vector3.Cross(plane.Normal, right);

            var sorted = verts
                .Select(v =>
                {
                    var d = v - center;
                    float x = Vector3.Dot(d, right);
                    float y = Vector3.Dot(d, up);
                    float angle = MathF.Atan2(y, x);

                    return (v, angle);
                })
                .OrderBy(t => t.angle)
                .Select(t => t.v)
                .ToList();

            if (Vector3.Dot(
                Vector3.Cross(sorted[1] - sorted[0], sorted[2] - sorted[0]),
                plane.Normal) < 0)
            {
                sorted.Reverse();
            }

            return sorted;
        }

        private static List<Vector3> GenerateVerticesRaw(List<Face> faces)
        {
            var vertices = new List<Vector3>();
            int count = faces.Count;

            for (int i = 0; i < count - 2; i++)
            {
                for (int j = i + 1; j < count - 1; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        var p = Plane3D.Intersect(
                            faces[i].Plane,
                            faces[j].Plane,
                            faces[k].Plane);

                        if (p != null)
                            vertices.Add(p.Value);
                    }
                }
            }

            return vertices;
        }
    }
}