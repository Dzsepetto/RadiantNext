using System.Numerics;

namespace MapMaker.Core.Geometry
{
    public class Camera3D
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, -500);
        public Vector3 Target { get; set; } = Vector3.Zero;
        public Vector3 Up { get; set; } = Vector3.UnitZ;

        public float FieldOfView { get; set; } = MathF.PI / 4f;
        public float AspectRatio { get; set; } = 16f / 9f;
        public float NearPlane { get; set; } = 1f;
        public float FarPlane { get; set; } = 10000f;

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, Target, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfView,
                AspectRatio,
                NearPlane,
                FarPlane);
        }

        public Vector3 WorldToScreen(Vector3 worldPos, float screenWidth, float screenHeight)
        {
            var view = GetViewMatrix();
            var projection = GetProjectionMatrix();

            var worldVec = new Vector4(worldPos, 1f);

            var viewSpace = Vector4.Transform(worldVec, view);
            var clipSpace = Vector4.Transform(viewSpace, projection);

            if (clipSpace.W <= 0.0001f)
                return new Vector3(float.NaN, float.NaN, float.NaN);

            var ndc = new Vector3(
                clipSpace.X / clipSpace.W,
                clipSpace.Y / clipSpace.W,
                clipSpace.Z / clipSpace.W);

            var screenX = (ndc.X + 1f) * 0.5f * screenWidth;
            var screenY = (1f - ndc.Y) * 0.5f * screenHeight;

            return new Vector3(screenX, screenY, ndc.Z);
        }
    }
}