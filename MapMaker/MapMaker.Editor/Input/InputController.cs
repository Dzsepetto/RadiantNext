using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using MapMaker.Core.Editing;
using MapMaker.Core.Models;
using MapMaker.Editor.State;

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

        private const float RotateMouseSensitivity = 0.5f;
        private const float MouseMoveSensitivity = 1.0f;

        private IInputElement? _viewport;

        public event Action? SceneChanged;
        public event Action<Brush>? BrushChanged;

        private bool _transformDragging;
        private EditorTool _transformTool;
        private Point _transformStartMousePos;
        private Brush? _transformBrush;
        private List<FaceSnapshot>? _transformOriginalFaces;

        private DateTime _lastSceneUpdate = DateTime.MinValue;
        private static readonly TimeSpan SceneUpdateInterval =
            TimeSpan.FromMilliseconds(16);

        private sealed class FaceSnapshot
        {
            public Face Face { get; }
            public Vector3 P1 { get; }
            public Vector3 P2 { get; }
            public Vector3 P3 { get; }

            public FaceSnapshot(Face face)
            {
                Face = face;
                P1 = face.P1;
                P2 = face.P2;
                P3 = face.P3;
            }
        }

        public InputController(EditorState state)
        {
            _state = state;
        }

        public void SetViewport(IInputElement element)
        {
            _viewport = element;
        }

        #region Keyboard

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (_transformDragging)
            {
                if (e.Key == Key.Escape)
                {
                    CancelTransformDrag(switchToSelect: true);
                    e.Handled = true;
                    return;
                }

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

        #endregion

        #region Mouse

        public void HandleMouseDown(MouseButtonEventArgs e)
        {
            if (_viewport == null)
                return;

            if (_transformDragging)
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    CancelTransformDrag(switchToSelect: true);
                    e.Handled = true;
                    return;
                }
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (_state.CurrentTool == EditorTool.Move &&
                    _state.SelectedBrush != null)
                {
                    BeginTransformDrag(EditorTool.Move);
                    e.Handled = true;
                    return;
                }

                if (_state.CurrentTool == EditorTool.Rotate &&
                    _state.SelectedBrush != null)
                {
                    BeginTransformDrag(EditorTool.Rotate);
                    e.Handled = true;
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
            if (_transformDragging)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    CommitTransformDrag();
                    e.Handled = true;
                    return;
                }

                if (e.ChangedButton == MouseButton.Right)
                {
                    CancelTransformDrag(switchToSelect: true);
                    e.Handled = true;
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

            if (_transformDragging)
            {
                UpdateTransformDrag(e);
                e.Handled = true;
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

        #endregion

        public void Update()
        {
            if (_transformDragging)
                return;

            var camera = _state.Camera;

            if (_w) camera.Position += camera.Forward * MoveSpeed;
            if (_s) camera.Position -= camera.Forward * MoveSpeed;
            if (_a) camera.Position -= camera.Right * MoveSpeed;
            if (_d) camera.Position += camera.Right * MoveSpeed;
            if (_q) camera.Position += Vector3.UnitZ * MoveSpeed;
            if (_e) camera.Position -= Vector3.UnitZ * MoveSpeed;
        }

        #region Keyboard Move

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
                BrushMover.Move(_state.SelectedBrush, delta, step);
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

        #endregion

        #region Transform Drag

        private void BeginTransformDrag(EditorTool tool)
        {
            if (_viewport == null)
                return;

            if (_state.SelectedBrush == null)
                return;

            _transformDragging = true;
            _rightMouseDown = false;

            _transformTool = tool;
            _transformBrush = _state.SelectedBrush;
            _transformStartMousePos = Mouse.GetPosition(_viewport);

            _transformOriginalFaces = _transformBrush.Faces
                .Select(face => new FaceSnapshot(face))
                .ToList();

            Mouse.Capture(_viewport);
        }

        private void UpdateTransformDrag(MouseEventArgs e)
        {
            if (_viewport == null)
                return;

            if (_transformBrush == null || _transformOriginalFaces == null)
                return;

            var pos = e.GetPosition(_viewport);

            float deltaX = (float)(pos.X - _transformStartMousePos.X);
            float deltaY = (float)(pos.Y - _transformStartMousePos.Y);

            RestoreTransformOriginal();

            if (_transformTool == EditorTool.Move)
            {
                UpdateMoveDrag(deltaX, deltaY);
            }
            else if (_transformTool == EditorTool.Rotate)
            {
                UpdateRotateDrag(deltaX);
            }

            RequestSceneUpdate();
        }

        private void UpdateMoveDrag(float deltaX, float deltaY)
        {
            if (_transformBrush == null)
                return;

            // Első egyszerű verzió:
            // egér X = world X
            // egér Y = world Y
            var delta = new Vector3(
                deltaX * MouseMoveSensitivity,
                -deltaY * MouseMoveSensitivity,
                0);

            BrushMover.MoveRaw(_transformBrush, delta);
        }

        private void UpdateRotateDrag(float deltaX)
        {
            if (_transformBrush == null)
                return;

            float degrees = deltaX * RotateMouseSensitivity;

            BrushRotator.RotateAroundCenterZ(_transformBrush, degrees);
        }

        private void CommitTransformDrag()
        {
            if (!_transformDragging)
                return;

            _transformDragging = false;
            _rightMouseDown = false;

            _transformBrush = null;
            _transformOriginalFaces = null;

            Mouse.Capture(null);

            _state.IsDirty = true;

            SceneChanged?.Invoke();
        }

        private void CancelTransformDrag(bool switchToSelect)
        {
            if (!_transformDragging)
                return;

            RestoreTransformOriginal();

            _transformDragging = false;
            _rightMouseDown = false;

            _transformBrush = null;
            _transformOriginalFaces = null;

            Mouse.Capture(null);

            if (switchToSelect)
                _state.CurrentTool = EditorTool.Select;

            SceneChanged?.Invoke();
        }

        private void RestoreTransformOriginal()
        {
            if (_transformOriginalFaces == null)
                return;

            foreach (var snapshot in _transformOriginalFaces)
            {
                snapshot.Face.SetPoints(
                    snapshot.P1,
                    snapshot.P2,
                    snapshot.P3);
            }

            _transformBrush?.Invalidate();
        }

        private void RequestSceneUpdate()
        {
            var now = DateTime.UtcNow;

            if (now - _lastSceneUpdate < SceneUpdateInterval)
                return;

            _lastSceneUpdate = now;

            SceneChanged?.Invoke();
        }

        #endregion
    }
}