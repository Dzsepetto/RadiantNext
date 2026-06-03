using MapMaker.Core.Editing;
using MapMaker.Editor.Commands;
using MapMaker.Editor.Editor;
using MapMaker.Editor.Tools;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace MapMaker.Editor.Input
{
    public class InputController
    {
        private readonly EditorState _state;

        private bool _w, _s, _a, _d, _q, _e;
        private bool _rightMouseDown;

        private Point _lastMousePos;

        private const float MoveSpeed = 5f;
        private const float MouseSensitivity = 0.003f;

        private IInputElement? _viewport;

        private readonly MoveTool _moveTool;
        private readonly RotateTool _rotateTool;
        private IEditorTool? _activeTool;

        public event Action? SceneChanged;

        public InputController(EditorState state)
        {
            _state = state;

            _moveTool = new MoveTool(_state);
            _rotateTool = new RotateTool(_state);

            _moveTool.SceneChanged += () => SceneChanged?.Invoke();
            _rotateTool.SceneChanged += () => SceneChanged?.Invoke();

            _moveTool.Committed += () => SceneChanged?.Invoke();
            _rotateTool.Committed += () => SceneChanged?.Invoke();
        }

        public void SetViewport(IInputElement element)
        {
            _viewport = element;

            _moveTool.SetViewport(element);
            _rotateTool.SetViewport(element);
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _activeTool != null)
            {
                _activeTool.Cancel();
                _activeTool = null;
                _state.CurrentTool = EditorTool.Select;

                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Z)
            {
                _state.History.Undo();
                _state.IsDirty = true;

                SceneChanged?.Invoke();

                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Y)
            {
                _state.History.Redo();
                _state.IsDirty = true;

                SceneChanged?.Invoke();

                e.Handled = true;
                return;
            }

            if (_activeTool != null)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.R)
            {
                _state.CurrentTool = EditorTool.Rotate;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.G)
            {
                _state.CurrentTool = EditorTool.Move;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.W) _w = true;
            if (e.Key == Key.S) _s = true;
            if (e.Key == Key.A) _a = true;
            if (e.Key == Key.D) _d = true;
            if (e.Key == Key.Q) _q = true;
            if (e.Key == Key.E) _e = true;

            HandleMoveKeys(e);
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.W) _w = false;
            if (e.Key == Key.S) _s = false;
            if (e.Key == Key.A) _a = false;
            if (e.Key == Key.D) _d = false;
            if (e.Key == Key.Q) _q = false;
            if (e.Key == Key.E) _e = false;
        }

        public void HandleMouseDown(MouseButtonEventArgs e)
        {
            if (_viewport == null)
                return;

            if (_activeTool != null && e.ChangedButton == MouseButton.Right)
            {
                _activeTool.Cancel();
                _activeTool = null;

                _state.CurrentTool = EditorTool.Select;

                e.Handled = true;
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (_state.CurrentTool == EditorTool.Move &&
                    _state.SelectedBrush != null)
                {
                    _activeTool = _moveTool;
                    _activeTool.OnMouseDown(e);
                    return;
                }

                if (_state.CurrentTool == EditorTool.Rotate &&
                    _state.SelectedBrush != null)
                {
                    _activeTool = _rotateTool;
                    _activeTool.OnMouseDown(e);
                    return;
                }
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                _rightMouseDown = true;
                _lastMousePos = e.GetPosition(_viewport);

                Mouse.Capture(_viewport);

                e.Handled = true;
            }
        }

        public void HandleMouseUp(MouseButtonEventArgs e)
        {
            if (_activeTool != null)
            {
                _activeTool.OnMouseUp(e);

                if (e.Handled)
                {
                    _activeTool = null;
                    return;
                }
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                _rightMouseDown = false;

                Mouse.Capture(null);

                e.Handled = true;
            }
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            if (_viewport == null)
                return;

            if (_activeTool != null)
            {
                _activeTool.OnMouseMove(e);

                if (e.Handled)
                    return;
            }

            if (!_rightMouseDown)
                return;

            var pos = e.GetPosition(_viewport);

            var deltaX = (float)(pos.X - _lastMousePos.X);
            var deltaY = (float)(pos.Y - _lastMousePos.Y);

            _lastMousePos = pos;

            var camera = _state.Camera;

            camera.Yaw -= deltaX * MouseSensitivity;
            camera.Pitch -= deltaY * MouseSensitivity;
            camera.Pitch = Math.Clamp(camera.Pitch, -1.5f, 1.5f);

            camera.UpdateVectors();

            e.Handled = true;
        }

        public void Update()
        {
            if (_activeTool != null)
                return;

            var camera = _state.Camera;

            if (_w) camera.Position += camera.Forward * MoveSpeed;
            if (_s) camera.Position -= camera.Forward * MoveSpeed;
            if (_a) camera.Position -= camera.Right * MoveSpeed;
            if (_d) camera.Position += camera.Right * MoveSpeed;
            if (_q) camera.Position += Vector3.UnitZ * MoveSpeed;
            if (_e) camera.Position -= Vector3.UnitZ * MoveSpeed;
        }

        private void HandleMoveKeys(KeyEventArgs e)
        {
            if (_state.CurrentTool != EditorTool.Move)
                return;

            float step = _state.Grid.Size;

            Vector3 delta = Vector3.Zero;

            switch (e.Key)
            {
                case Key.Left:
                    delta = new Vector3(-step, 0, 0);
                    break;

                case Key.Right:
                    delta = new Vector3(step, 0, 0);
                    break;

                case Key.Up:
                    delta = new Vector3(0, step, 0);
                    break;

                case Key.Down:
                    delta = new Vector3(0, -step, 0);
                    break;

                case Key.PageUp:
                    delta = new Vector3(0, 0, step);
                    break;

                case Key.PageDown:
                    delta = new Vector3(0, 0, -step);
                    break;
            }

            if (delta == Vector3.Zero)
                return;

            if (_state.SelectedBrush != null)
            {
                var brush = _state.SelectedBrush;

                var before = brush.Faces
                    .Select(face => new BrushFaceSnapshot(face))
                    .ToList();

                BrushMover.Move(brush, delta, step);

                var after = brush.Faces
                    .Select(face => new BrushFaceSnapshot(face))
                    .ToList();

                _state.History.PushExecuted(
                    new MoveBrushCommand(brush, before, after));
            }
            else if (_state.SelectedFace != null)
            {
                FaceMover.Move(_state.SelectedFace, delta, step);
            }
            else
            {
                return;
            }

            _state.IsDirty = true;

            SceneChanged?.Invoke();

            e.Handled = true;
        }
    }
}