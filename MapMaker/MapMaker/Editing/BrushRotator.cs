using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Core.Editing
{
    public static class BrushRotator
    {
        private const float DuplicateEpsilon = 0.01f;

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
            var uniquePoints = new List<Vector3>();

            foreach (var face in brush.Faces)
            {
                AddUnique(uniquePoints, face.P1);
                AddUnique(uniquePoints, face.P2);
                AddUnique(uniquePoints, face.P3);
            }

            if (uniquePoints.Count == 0)
                return Vector3.Zero;

            var min = new Vector3(
                uniquePoints.Min(p => p.X),
                uniquePoints.Min(p => p.Y),
                uniquePoints.Min(p => p.Z));

            var max = new Vector3(
                uniquePoints.Max(p => p.X),
                uniquePoints.Max(p => p.Y),
                uniquePoints.Max(p => p.Z));

            return (min + max) * 0.5f;
        }

        private static void AddUnique(List<Vector3> points, Vector3 point)
        {
            bool exists = points.Any(existing =>
                Vector3.DistanceSquared(existing, point) <
                DuplicateEpsilon * DuplicateEpsilon);

            if (!exists)
                points.Add(point);
        }
    }
}