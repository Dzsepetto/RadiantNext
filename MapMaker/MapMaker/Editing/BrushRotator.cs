using MapMaker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Editing
{
    public static class BrushRotator
    {
        public static void RotateAroundCenterZ(Brush brush, float degrees)
        {
            var center = GetCenter(brush);
            RotateAroundPointZ(brush, center, degrees);
        }

        public static void RotateAroundPointZ(Brush brush, Vector3 pivot, float degrees)
        {
            float radians = degrees * MathF.PI / 180f;

            var matrix =
                Matrix4x4.CreateTranslation(-pivot) *
                Matrix4x4.CreateRotationZ(radians) *
                Matrix4x4.CreateTranslation(pivot);

            foreach (var face in brush.Faces)
            {
                face.TransformPoints(matrix);
            }

            brush.Invalidate();
        }

        private static Vector3 GetCenter(Brush brush)
        {
            var points = brush.Faces
                .SelectMany(face => new[]
                {
                    face.P1,
                    face.P2,
                    face.P3
                })
                .ToList();

            if (points.Count == 0)
                return Vector3.Zero;

            var min = new Vector3(
                points.Min(p => p.X),
                points.Min(p => p.Y),
                points.Min(p => p.Z));

            var max = new Vector3(
                points.Max(p => p.X),
                points.Max(p => p.Y),
                points.Max(p => p.Z));

            return (min + max) * 0.5f;
        }
    }
}
