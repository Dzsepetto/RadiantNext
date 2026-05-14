using System.Numerics;

namespace MapMaker.Core.Editing
{
    public static class GridSnapper
    {
        public static Vector3 Snap(Vector3 value, float gridSize)
        {
            return new Vector3(
                Snap(value.X, gridSize),
                Snap(value.Y, gridSize),
                Snap(value.Z, gridSize));
        }

        public static float Snap(float value, float gridSize)
        {
            if (gridSize <= 0)
                return value;

            return MathF.Round(value / gridSize) * gridSize;
        }
    }
}