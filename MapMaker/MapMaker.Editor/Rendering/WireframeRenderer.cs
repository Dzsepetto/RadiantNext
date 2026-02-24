using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace MapMaker.Editor.Rendering
{
    public class WireframeRenderer
    {
        public void Render(
            DrawingContext dc,
            Camera3D camera,
            Map map,
            double width,
            double height)
        {
            var pen = new Pen(Brushes.White, 1);

            foreach (var entity in map.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    foreach (var face in brush.Faces)
                    {
                        DrawFace(dc, pen, camera, face, width, height);
                    }
                }
            }
        }

        private void DrawFace(
            DrawingContext dc,
            Pen pen,
            Camera3D camera,
            Face face,
            double width,
            double height)
        {
            var p1 = camera.WorldToScreen(face.P1, (float)width, (float)height);
            var p2 = camera.WorldToScreen(face.P2, (float)width, (float)height);
            var p3 = camera.WorldToScreen(face.P3, (float)width, (float)height);

            var pt1 = new Point(p1.X, p1.Y);
            var pt2 = new Point(p2.X, p2.Y);
            var pt3 = new Point(p3.X, p3.Y);

            dc.DrawLine(pen, pt1, pt2);
            dc.DrawLine(pen, pt2, pt3);
            dc.DrawLine(pen, pt3, pt1);
        }
    }
}