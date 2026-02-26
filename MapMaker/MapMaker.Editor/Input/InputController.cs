using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
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

        private IInputElement? _viewport;

  
        public InputController(EditorState state)
        {
            _state = state;
        }

        #region Keyboard

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.W) _w = true;
            if (e.Key == Key.S) _s = true;
            if (e.Key == Key.A) _a = true;
            if (e.Key == Key.D) _d = true;
            if (e.Key == Key.Q) _q = true;
            if (e.Key == Key.E) _e = true;
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

       

        public void SetViewport(IInputElement element)
        {
            _viewport = element;
        }

        public void HandleMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && _viewport != null)
            {
                _rightMouseDown = true;
                _lastMousePos = e.GetPosition(_viewport);
                Mouse.Capture((IInputElement)_viewport);
            }
        }

        public void HandleMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _rightMouseDown = false;
                Mouse.Capture(null);
            }
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            if (!_rightMouseDown || _viewport == null)
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
        }

        #endregion

        public void Update()
        {
            var camera = _state.Camera;

            if (_w) camera.Position += camera.Forward * MoveSpeed;
            if (_s) camera.Position -= camera.Forward * MoveSpeed;
            if (_a) camera.Position -= camera.Right * MoveSpeed;
            if (_d) camera.Position += camera.Right * MoveSpeed;
            if (_q) camera.Position += Vector3.UnitZ * MoveSpeed;
            if (_e) camera.Position -= Vector3.UnitZ * MoveSpeed;
        }
    }
}