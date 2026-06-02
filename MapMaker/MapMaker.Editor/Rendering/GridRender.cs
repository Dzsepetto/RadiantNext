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
    internal static class GridRenderer
    {
        private const float GridExtent = 8192f;
        public static void AddGrid(Model3DGroup scene, float gridSize = 16f, int lineCount = 64)
        {
            float extent = GridExtent;
            int actualLineCount = (int)(extent / gridSize);

            var minorMaterial = new DiffuseMaterial(
                new SolidColorBrush(Color.FromRgb(55, 55, 55)));

            var majorMaterial = new DiffuseMaterial(
                new SolidColorBrush(Color.FromRgb(90, 90, 90)));

            var xAxisMaterial = new DiffuseMaterial(
                new SolidColorBrush(Colors.Red));

            var yAxisMaterial = new DiffuseMaterial(
                new SolidColorBrush(Colors.Green));

            for (int i = -actualLineCount; i <= actualLineCount; i++)
            {
                float p = i * gridSize;

                bool isMajor = i % 8 == 0;
                var material = isMajor ? majorMaterial : minorMaterial;

                scene.Children.Add(CreateLineModel(
                    new Vector3(-extent, p, 0),
                    new Vector3(extent, p, 0),
                    i == 0 ? xAxisMaterial : material,
                    i == 0 ? 2f : 0.5f));

                scene.Children.Add(CreateLineModel(
                    new Vector3(p, -extent, 0),
                    new Vector3(p, extent, 0),
                    i == 0 ? yAxisMaterial : material,
                    i == 0 ? 2f : 0.5f));
            }

            scene.Children.Add(CreateLineModel(
                new Vector3(0, 0, 0),
                new Vector3(0, 0, extent * 0.25f),
                new DiffuseMaterial(new SolidColorBrush(Colors.Blue)),
                2f));
        }

        private static GeometryModel3D CreateLineModel(
            Vector3 a,
            Vector3 b,
            Material material,
            float thickness)
        {
            return new GeometryModel3D
            {
                Geometry = CreateLine(a, b, thickness),
                Material = material,
                BackMaterial = material
            };
        }

        private static MeshGeometry3D CreateLine(Vector3 a, Vector3 b, float thickness)
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

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            return mesh;
        }
    }
}
