using MapMaker.Core.Builders;
using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Editor.Editor;
using MapMaker.Editor.Input;
using MapMaker.Editor.Models;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.Rendering
{
    public partial class ViewPortControl : UserControl
    {
        private readonly Model3DGroup _scene = new();

        private readonly Dictionary<GeometryModel3D, Face> _modelToFace = new();
        private readonly Dictionary<Face, GeometryModel3D> _faceToModel = new();
        private readonly Dictionary<GeometryModel3D, MapMaker.Core.Models.Brush> _modelToBrush = new();
        private readonly Dictionary<MapMaker.Core.Models.Brush, List<GeometryModel3D>> _brushToModels = new();

        private EditorState? _state;

        private Face? _selectedFace;
        private InputController? _input;

        private Map? _currentMap;
        private float _gridSize = 16f;

        public ViewPortControl()
        {
            InitializeComponent();
            SetupScene();

            this.PreviewMouseDown += OnMouseDown;
            this.PreviewMouseUp += OnMouseUp;
            this.PreviewMouseMove += OnMouseMove;
        }

        public void SetInput(InputController input, EditorState state)
        {
            _input = input;
            _state = state;

            _input.SetViewport(this);
        }

        #region Mouse Handling

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _input?.HandleMouseDown(e);

            if (e.Handled)
                return;

            if (_state == null)
                return;

            if (e.ChangedButton == MouseButton.Left &&
                _state.CurrentTool == EditorTool.Select)
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
            if (_state == null)
                return;

            var pos = e.GetPosition(Viewport3DControl);

            VisualTreeHelper.HitTest(
                Viewport3DControl,
                null,
                result =>
                {
                    if (result is RayMeshGeometry3DHitTestResult meshResult)
                    {
                        var model = meshResult.ModelHit as GeometryModel3D;

                        if (model == null)
                            return HitTestResultBehavior.Continue;

                        if (_state.SelectionMode == Editor.SelectionMode.Face &&
                            _modelToFace.TryGetValue(model, out var face))
                        {
                            SelectFace(face);
                            return HitTestResultBehavior.Stop;
                        }

                        if (_state.SelectionMode == Editor.SelectionMode.Object &&
                            _modelToBrush.TryGetValue(model, out var brush))
                        {
                            SelectBrush(brush);
                            return HitTestResultBehavior.Stop;
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                new PointHitTestParameters(pos));
        }

        private void SelectFace(Face face)
        {
            ClearSelectionVisuals();

            _state?.ClearSelection();
            if (_state != null)
                _state.SelectedFace = face;

            _selectedFace = face;

            if (_faceToModel.TryGetValue(face, out var model))
            {
                model.Material = new DiffuseMaterial(
                    new SolidColorBrush(Colors.Yellow));
            }
        }
        private void SelectBrush(MapMaker.Core.Models.Brush brush)
        {
            ClearSelectionVisuals();

            _state?.ClearSelection();
            if (_state != null)
                _state.SelectedBrush = brush;

            if (_brushToModels.TryGetValue(brush, out var models))
            {
                foreach (var model in models)
                {
                    model.Material = new DiffuseMaterial(
                        new SolidColorBrush(Colors.Orange));
                }
            }
        }
        private void ClearSelectionVisuals()
        {
            foreach (var model in _modelToFace.Keys)
            {
                model.Material = new DiffuseMaterial(
                    new SolidColorBrush(Color.FromRgb(180, 180, 180)));
            }

            _selectedFace = null;
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

            GridRenderer.AddGrid(_scene, _gridSize, 64);

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


        #region Scene Rebuild

        public void SetGridSize(float gridSize)
        {
            _gridSize = gridSize;
            RebuildScene();
        }

        public void LoadMap(Map map)
        {
            _currentMap = map;
            RebuildScene();
        }
        public void Refresh()
        {
            RebuildScene();
        }
        private void RebuildScene()
        {
            _scene.Children.Clear();

            _modelToFace.Clear();
            _faceToModel.Clear();
            _modelToBrush.Clear();
            _brushToModels.Clear();

            _selectedFace = null;

            _scene.Children.Add(new AmbientLight(Colors.White));

            GridRenderer.AddGrid(_scene, _gridSize, 64);

            if (_currentMap != null)
            {
                MapRenderer.AddMap(_scene, _currentMap, _modelToFace, _faceToModel, _modelToBrush, _brushToModels);
            }

            RestoreSelectionVisuals();
        }

        private void RestoreSelectionVisuals()
        {
            if (_state == null)
                return;

            if (_state.SelectedFace != null &&
                _faceToModel.TryGetValue(_state.SelectedFace, out var faceModel))
            {
                faceModel.Material = new DiffuseMaterial(
                    new SolidColorBrush(Colors.Yellow));

                _selectedFace = _state.SelectedFace;
            }

            if (_state.SelectedBrush != null &&
                _brushToModels.TryGetValue(_state.SelectedBrush, out var brushModels))
            {
                foreach (var model in brushModels)
                {
                    model.Material = new DiffuseMaterial(
                        new SolidColorBrush(Colors.Orange));
                }
            }
        }
        #endregion
    }
}