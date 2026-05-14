using System.Numerics;

namespace MapMaker.Editor.Models
{
    public class Camera3D
    {
        public Vector3 Position { get; set; } = new(0, -500, 200);

        public float Yaw { get; set; } = 0;
        public float Pitch { get; set; } = 0;

        public Vector3 Forward { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; } = Vector3.UnitX;
        public Vector3 Up { get; private set; } = Vector3.UnitZ;

        public float AspectRatio { get; set; } = 1f;

        public Camera3D()
        {
            UpdateVectors();
        }

        public void UpdateVectors()
        {
            Forward = Vector3.Normalize(new Vector3(
                MathF.Cos(Pitch) * MathF.Sin(Yaw),
                MathF.Cos(Pitch) * MathF.Cos(Yaw),
                MathF.Sin(Pitch)
            ));

            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitZ));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
        }
    }
}