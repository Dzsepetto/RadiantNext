using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.Rendering
{
        public static class DebugLineBuilder
        {
            public static MeshGeometry3D CreateLine(Vector3 a, Vector3 b, float thickness = 1f)
            {
                var mesh = new MeshGeometry3D();

                var dir = Vector3.Normalize(b - a);
                var up = Vector3.UnitZ;

                if (Vector3.Cross(dir, up).LengthSquared() < 0.001f)
                    up = Vector3.UnitX;

                var right = Vector3.Normalize(Vector3.Cross(dir, up)) * thickness;

                var p1 = a - right;
                var p2 = a + right;
                var p3 = b + right;
                var p4 = b - right;

                mesh.Positions.Add(new Point3D(p1.X, p1.Y, p1.Z));
                mesh.Positions.Add(new Point3D(p2.X, p2.Y, p2.Z));
                mesh.Positions.Add(new Point3D(p3.X, p3.Y, p3.Z));
                mesh.Positions.Add(new Point3D(p4.X, p4.Y, p4.Z));

                mesh.TriangleIndices = new Int32Collection
                {
                    0,1,2,
                    0,2,3
                };

                return mesh;
            }
        }
}
