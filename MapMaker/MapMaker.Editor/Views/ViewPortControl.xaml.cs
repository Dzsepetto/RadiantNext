using MapMaker.Core.Builders;
using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Editor.Input;
using MapMaker.Editor.Models;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.Views
{
    public partial class ViewportControl : UserControl
    {
        private readonly Model3DGroup _scene = new();

        private readonly Dictionary<GeometryModel3D, Face> _modelToFace = new();
        private readonly Dictionary<Face, GeometryModel3D> _faceToModel = new();

        private Face? _selectedFace;
        private InputController? _input;

        public ViewportControl()
        {
            InitializeComponent();
            SetupScene();

            // 🔥 FONTOS: a UserControl-ra tesszük, nem a Viewport3D-re
            this.PreviewMouseDown += OnMouseDown;
            this.PreviewMouseUp += OnMouseUp;
            this.PreviewMouseMove += OnMouseMove;
        }

        public void SetInput(InputController input)
        {
            _input = input;

            // 🔥 EZ IS FONTOS: nem Viewport3DControl!
            _input.SetViewport(this);
        }

        #region Mouse Handling

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _input?.HandleMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                HandleSelection(e);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _input?.HandleMouseUp(e);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _input?.HandleMouseMove(e);
        }

        #endregion

        #region Selection

        private void HandleSelection(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Viewport3DControl);

            VisualTreeHelper.HitTest(
                Viewport3DControl,
                null,
                result =>
                {
                    if (result is RayMeshGeometry3DHitTestResult meshResult)
                    {
                        var model = meshResult.ModelHit as GeometryModel3D;

                        if (model != null && _modelToFace.TryGetValue(model, out var face))
                        {
                            SelectFace(face);
                            return HitTestResultBehavior.Stop;
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                new PointHitTestParameters(pos));
        }

        private void SelectFace(Face face)
        {
            if (_selectedFace != null &&
                _faceToModel.TryGetValue(_selectedFace, out var oldModel))
            {
                oldModel.Material = new DiffuseMaterial(
                    new SolidColorBrush(Color.FromRgb(180, 180, 180)));
            }

            _selectedFace = face;

            if (_faceToModel.TryGetValue(face, out var model))
            {
                model.Material = new DiffuseMaterial(
                    new SolidColorBrush(Colors.Yellow));
            }
        }

        #endregion

        #region Scene

        private void SetupScene()
        {
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

        public void ApplyCamera(Camera3D cam)
        {
            var camera = (PerspectiveCamera)Viewport3DControl.Camera;

            camera.Position = new Point3D(cam.Position.X, cam.Position.Y, cam.Position.Z);
            camera.LookDirection = new Vector3D(cam.Forward.X, cam.Forward.Y, cam.Forward.Z);
            camera.UpDirection = new Vector3D(0, 0, 1);
        }

        #endregion

        #region Debug Line

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

        #endregion

        #region LoadMap

        public void LoadMap(Map map)
        {
            _scene.Children.Clear();

            _modelToFace.Clear();
            _faceToModel.Clear();

            _scene.Children.Add(new AmbientLight(Colors.White));

            foreach (var entity in map.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    BrushBuilder.Build(brush);

                    foreach (var face in brush.Faces)
                    {
                        if (face.Polygon == null)
                            continue;

                        var triangles = Triangulator.Triangulate(face.Polygon);

                        var mesh = new MeshGeometry3D();

                        foreach (var tri in triangles)
                        {
                            int start = mesh.Positions.Count;

                            mesh.Positions.Add(new Point3D(tri.A.X, tri.A.Y, tri.A.Z));
                            mesh.Positions.Add(new Point3D(tri.B.X, tri.B.Y, tri.B.Z));
                            mesh.Positions.Add(new Point3D(tri.C.X, tri.C.Y, tri.C.Z));

                            mesh.TriangleIndices.Add(start + 0);
                            mesh.TriangleIndices.Add(start + 1);
                            mesh.TriangleIndices.Add(start + 2);
                        }

                        var material = new DiffuseMaterial(
                            new SolidColorBrush(Color.FromRgb(180, 180, 180)));

                        var model = new GeometryModel3D
                        {
                            Geometry = mesh,
                            Material = material,
                            BackMaterial = material
                        };

                        _modelToFace[model] = face;
                        _faceToModel[face] = model;

                        _scene.Children.Add(model);

                        // debug normal
                        var verts = face.Polygon.Vertices;

                        var center = Vector3.Zero;
                        foreach (var v in verts)
                            center += v;
                        center /= verts.Count;

                        var normal = face.Plane.Normal;

                        var lineMesh = DebugLineBuilder.CreateLine(
                            center,
                            center + normal * 30f,
                            1f);

                        _scene.Children.Add(new GeometryModel3D
                        {
                            Geometry = lineMesh,
                            Material = new DiffuseMaterial(
                                new SolidColorBrush(Colors.Red))
                        });
                    }
                }
            }
        }

        #endregion
    }
}