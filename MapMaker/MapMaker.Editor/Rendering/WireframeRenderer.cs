using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Editor.State;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using MapMaker.Editor.State;

namespace MapMaker.Editor.Rendering
{
    public class WireframeRenderer
    {
        public void Render(
    DrawingContext dc,
    Camera3D camera,
    Map map,
    EditorState state,
    double width,
    double height)
        {
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, width, height)));

            foreach (var entity in map.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    foreach (var face in brush.Faces)
                    {
                        var pen = face == state.SelectedFace
                            ? new Pen(Brushes.Yellow, 2)
                            : new Pen(Brushes.White, 1);

                        DrawFace(dc, pen, camera, face, width, height);
                    }
                }
            }

            dc.Pop();
        }

        private void DrawFace(
    DrawingContext dc,
    Pen pen,
    Camera3D camera,
    Face face,
    double width,
    double height)
        {
            if (face.Polygon == null || face.Polygon.Vertices.Count < 2)
                return;

            var vertices = face.Polygon.Vertices;

            for (int i = 0; i < vertices.Count; i++)
            {
                var v1 = camera.WorldToScreen(vertices[i], (float)width, (float)height);
                var v2 = camera.WorldToScreen(
                    vertices[(i + 1) % vertices.Count],
                    (float)width,
                    (float)height);

                dc.DrawLine(pen,
                    new Point(v1.X, v1.Y),
                    new Point(v2.X, v2.Y));
            }
        }
    }
}