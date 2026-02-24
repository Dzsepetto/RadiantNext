using System.Numerics;

namespace MapMaker.Core.Geometry
{
    public class Transform3D
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4x4 GetMatrix()
        {
            var scale = Matrix4x4.CreateScale(Scale);

            var rotation =
                Matrix4x4.CreateRotationX(Rotation.X) *
                Matrix4x4.CreateRotationY(Rotation.Y) *
                Matrix4x4.CreateRotationZ(Rotation.Z);

            var translation = Matrix4x4.CreateTranslation(Position);

            return scale * rotation * translation;
        }
    }
}