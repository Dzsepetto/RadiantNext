using MapMaker.Core;
using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Editor.Input;
using MapMaker.Editor.Rendering;
using MapMaker.Editor.Selection;
using MapMaker.Editor.State;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MapMaker.Editor.Views
{
    public class ViewportControl : FrameworkElement
    {
        private readonly WireframeRenderer _renderer = new();
        private readonly EditorState _state = new();
        private InputController? _input;
        private SelectionService? _selection;

        public ViewportControl()
        {
            Focusable = true;

            Loaded += OnLoaded;
            CompositionTarget.Rendering += OnRenderFrame;
        }
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        public void LoadMap(Map map)
        {
            _state.CurrentMap = map;
            foreach (var entity in map.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    brush.GenerateFacePolygons();
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _input = new InputController(_state);
            _input.SetViewport(this);
            _selection = new SelectionService(_state);

            Focus();
        }

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            _input?.Update();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(
                null,
                new Pen(Brushes.Red, 2),
                new Rect(0, 0, ActualWidth, ActualHeight));

            if (ActualHeight == 0) return;

            _state.Camera.AspectRatio = (float)(ActualWidth / ActualHeight);

            _renderer.Render(
                dc,
                _state.Camera,
                _state.CurrentMap,
                _state,
                ActualWidth,
                ActualHeight);
        }

        #region Keyboard

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _input?.HandleKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _input?.HandleKeyUp(e);
        }

        #endregion

        #region Mouse

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            var pos = e.GetPosition(this);

            TrySelectFace(pos);

            _input?.HandleMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            _input?.HandleMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _input?.HandleMouseMove(e);
        }

        #endregion

        private void CreateTestScene()
        {
            var world = new Entity();
            world.Properties["classname"] = "worldspawn";

            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(128, 128, 128),
                "caulk");

            brush.GenerateFacePolygons();

            world.Brushes.Add(brush);
            _state.CurrentMap.Entities.Add(world);
        }
        private void TrySelectFace(Point mousePos)
        {
            if (_state.CurrentMap == null)
                return;

            float bestDistance = float.MaxValue;
            Face? bestFace = null;

            foreach (var entity in _state.CurrentMap.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    foreach (var face in brush.Faces)
                    {
                        if (face.Polygon == null)
                            continue;

                        foreach (var vertex in face.Polygon.Vertices)
                        {
                            var screen = _state.Camera.WorldToScreen(
                                vertex,
                                (float)ActualWidth,
                                (float)ActualHeight);

                            var dx = (float)(screen.X - mousePos.X);
                            var dy = (float)(screen.Y - mousePos.Y);
                            var dist = dx * dx + dy * dy;

                            if (dist < bestDistance && dist < 100) // 10px threshold
                            {
                                bestDistance = dist;
                                bestFace = face;
                            }
                        }
                    }
                }
            }

            if (bestFace != null)
                _selection?.SelectFace(bestFace);
            else
                _selection?.ClearSelection();
        }
    }
}