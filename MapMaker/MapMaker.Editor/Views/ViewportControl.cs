using MapMaker.Core;
using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Editor.Input;
using MapMaker.Editor.Rendering;
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

        public ViewportControl()
        {
            Focusable = true;

            Loaded += OnLoaded;
            CompositionTarget.Rendering += OnRenderFrame;
        }
        public void LoadMap(Map map)
        {
            _state.CurrentMap = map;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _input = new InputController(_state);

            CreateTestScene();

            Focus();
        }

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(
                null,
                new Pen(Brushes.Red, 2),
                new Rect(0, 0, ActualWidth, ActualHeight));
            _state.Camera.AspectRatio = (float)(ActualWidth / ActualHeight);
            _state.Camera.Position = new Vector3(0, -500, 200);
            _state.Camera.Target = Vector3.Zero;
            _renderer.Render(
                dc,
                _state.Camera,
                _state.CurrentMap,
                ActualWidth,
                ActualHeight);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _input?.HandleKeyDown(e);
        }

        private void CreateTestScene()
        {
            var world = new Entity();
            world.Properties["classname"] = "worldspawn";

            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(128, 128, 128),
                "caulk");

            world.Brushes.Add(brush);
            _state.CurrentMap.Entities.Add(world);
        }
    }
}