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
using MapMaker.Editor.Selection;

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

        private InputController? _input;

        private Map? _currentMap;
        private float _gridSize = 16f;

        private ViewportPicker? _picker;
        private SelectionService? _selectionService;
        private SelectionVisualService? _selectionVisualService;

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

            _picker = new ViewportPicker(
                state,
                Viewport3DControl,
                _modelToFace,
                _modelToBrush);

            _selectionService = new SelectionService(state);

            _selectionVisualService = new SelectionVisualService(
                state,
                _modelToFace,
                _faceToModel,
                _brushToModels);
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
            if (_state == null ||
                _picker == null ||
                _selectionService == null ||
                _selectionVisualService == null)
            {
                return;
            }

            var pos = e.GetPosition(Viewport3DControl);
            var result = _picker.Pick(pos);

            if (result.HasHit)
                _selectionService.Select(result);
            else
                _selectionService.ClearSelection();

            _selectionVisualService.ApplySelection();
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


            _scene.Children.Add(new AmbientLight(Colors.White));

            GridRenderer.AddGrid(_scene, _gridSize, 64);

            if (_currentMap != null)
            {
                MapRenderer.AddMap(_scene, _currentMap, _modelToFace, _faceToModel, _modelToBrush, _brushToModels);
            }

            _selectionVisualService?.ApplySelection();
        }
        #endregion
    }
}