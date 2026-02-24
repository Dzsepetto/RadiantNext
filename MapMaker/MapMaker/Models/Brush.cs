using MapMaker.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Models
{
    public class Brush
    {
        public List<Face> Faces { get; } = new();
        public Transform3D Transform { get; } = new();
        public List<Vector3> GenerateVertices()
        {
            var vertices = new List<Vector3>();

            const float epsilon = 0.001f;

            int count = Faces.Count;

            for (int i = 0; i < count - 2; i++)
            {
                for (int j = i + 1; j < count - 1; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        var p1 = Faces[i].Plane;
                        var p2 = Faces[j].Plane;
                        var p3 = Faces[k].Plane;

                        var intersection = Plane3D.Intersect(p1, p2, p3);

                        if (intersection == null)
                            continue;

                        var point = intersection.Value;

                        bool inside = true;

                        foreach (var face in Faces)
                        {
                            float distance = face.Plane.DistanceToPoint(point);

                            if (distance < -epsilon) 
                            {
                                inside = false;
                                break;
                            }
                        }

                        if (!inside)
                            continue;

                        // Duplikátum szűrés
                        bool duplicate = vertices.Any(v =>
                            Vector3.DistanceSquared(v, point) < epsilon * epsilon);

                        if (!duplicate)
                            vertices.Add(point);
                    }
                }
            }

            return vertices;
        }
        private IEnumerable<Vector3> SortVerticesOnPlane(List<Vector3> vertices, Plane3D plane)
        {
            var center = Vector3.Zero;

            foreach (var v in vertices)
                center += v;

            center /= vertices.Count;

            Vector3 right = Vector3.Normalize(
                Vector3.Cross(plane.Normal, Vector3.UnitY));

            if (right.LengthSquared() < 0.0001f)
                right = Vector3.Normalize(
                    Vector3.Cross(plane.Normal, Vector3.UnitX));

            Vector3 up = Vector3.Cross(right, plane.Normal);

            return vertices
                .OrderBy(v =>
                {
                    var dir = v - center;
                    float x = Vector3.Dot(dir, right);
                    float y = Vector3.Dot(dir, up);
                    return MathF.Atan2(y, x);
                })
                .ToList();
        }
        public Vector3 GetCenter()
        {
            var allPoints = Faces
                .SelectMany(f => new[] { f.P1, f.P2, f.P3 });

            return new Vector3(
                allPoints.Average(p => p.X),
                allPoints.Average(p => p.Y),
                allPoints.Average(p => p.Z)
            );
        }
        public bool AreNormalsFacingInward()
        {
            if (Faces.Count < 4)
                return false;

            var referencePoint = Faces[0].P1;

            foreach (var face in Faces)
            {
                var distance = face.Plane.DistanceToPoint(referencePoint);

                if (distance > 0.001f)
                    return false;
            }

            return true;
        }
        public void GenerateFacePolygons()
        {
            var allVertices = GenerateVertices();
            const float epsilon = 0.01f;

            foreach (var face in Faces)
            {
                var faceVertices = new List<Vector3>();

                foreach (var v in allVertices)
                {
                    if (MathF.Abs(face.Plane.DistanceToPoint(v)) < epsilon)
                    {
                        faceVertices.Add(v);
                    }
                }

                if (faceVertices.Count < 3)
                    continue;

                face.Polygon = new Polygon3D(
                    SortVerticesOnPlane(faceVertices, face.Plane));
            }
        }
        public List<Vector3> GetWorldVertices()
        {
            var localVertices = GenerateVertices();
            var matrix = Transform.GetMatrix();

            var world = new List<Vector3>();

            foreach (var v in localVertices)
            {
                world.Add(Vector3.Transform(v, matrix));
            }

            return world;
        }
    }

}
