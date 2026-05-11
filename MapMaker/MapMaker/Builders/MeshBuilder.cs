using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Builders
{
    public static class MeshBuilder
    {
        public static MeshData Build(Brush brush)
        {
            var mesh = new MeshData();

            brush.Invalidate();
            BrushBuilder.Build(brush);

            var matrix = brush.Transform.GetMatrix();

            foreach (var face in brush.Faces)
            {
                if (face.Polygon == null)
                    continue;

                var triangles = Triangulator.Triangulate(face.Polygon);

                foreach (var tri in triangles)
                {
                    int startIndex = mesh.Vertices.Count;

                    var a = Vector3.Transform(tri.A, matrix);
                    var b = Vector3.Transform(tri.B, matrix);
                    var c = Vector3.Transform(tri.C, matrix);

                    mesh.Vertices.Add(a);
                    mesh.Vertices.Add(b);
                    mesh.Vertices.Add(c);

                    mesh.Indices.Add(startIndex + 0);
                    mesh.Indices.Add(startIndex + 1);
                    mesh.Indices.Add(startIndex + 2);

                    var normal = Vector3.Normalize(
                        Vector3.Cross(b - a, c - a));

                    mesh.Normals.Add(normal);
                    mesh.Normals.Add(normal);
                    mesh.Normals.Add(normal);
                }
            }

            return mesh;
        }
    }
}
