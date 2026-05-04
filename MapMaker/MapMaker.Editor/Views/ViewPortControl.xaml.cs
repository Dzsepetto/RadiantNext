using MapMaker.Core.Builders;
using MapMaker.Core.Models;
using MapMaker.Editor.Rendering;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.Views
{
    public partial class ViewportControl : UserControl
    {
        private readonly Model3DGroup _scene = new();

        public ViewportControl()
        {
            InitializeComponent();
            SetupScene();
            StartCameraRotation();
        }

        private void SetupScene()
        {
            // Kamera
            var camera = new PerspectiveCamera
            {
                Position = new Point3D(0, -500, 200),
                LookDirection = new Vector3D(0, 500, -200),
                UpDirection = new Vector3D(0, 0, 1),
                FieldOfView = 60
            };

            Viewport3DControl.Camera = camera;

            _scene.Children.Add(new AmbientLight(Colors.White));
            var visual = new ModelVisual3D
            {
                Content = _scene
            };

            Viewport3DControl.Children.Add(visual);
        }
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
        private double _angle = 0;

        private void StartCameraRotation()
        {
            CompositionTarget.Rendering += (s, e) =>
            {
                _angle += 0.01;

                double radius = 500;

                var camera = (PerspectiveCamera)Viewport3DControl.Camera;

                camera.Position = new Point3D(
                    Math.Cos(_angle) * radius,
                    Math.Sin(_angle) * radius,
                    200);

                camera.LookDirection = new Vector3D(
                    -camera.Position.X,
                    -camera.Position.Y,
                    -camera.Position.Z);
            };
        }

        public void LoadMap(Map map)
        {
            _scene.Children.Clear();

            _scene.Children.Add(new AmbientLight(Colors.White));

            foreach (var entity in map.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    BrushBuilder.Build(brush);
                    var meshData = MeshBuilder.Build(brush);
                    var mesh = MeshConverter.ToWpfMesh(meshData);

                    var material = new DiffuseMaterial(
                        new SolidColorBrush(Color.FromRgb(180, 180, 180)));

                    var model = new GeometryModel3D
                    {
                        Geometry = mesh,
                        Material = material,
                        BackMaterial = material
                    };

                    _scene.Children.Add(model);


                    foreach (var face in brush.Faces)
                    {
                        if (face.Polygon == null)
                            continue;

                        var verts = face.Polygon.Vertices;

                        // center
                        var center = Vector3.Zero;
                        foreach (var v in verts)
                            center += v;
                        center /= verts.Count;

                        // normal
                        var normal = face.Plane.Normal;

                        var start = center;
                        var end = center + normal * 30f;

                        var lineMesh = DebugLineBuilder.CreateLine(start, end, 1f);

                        var lineModel = new GeometryModel3D
                        {
                            Geometry = lineMesh,
                            Material = new DiffuseMaterial(
                                new SolidColorBrush(Colors.Red))
                        };

                        _scene.Children.Add(lineModel);
                    }
                }
            }
        }
    }
}