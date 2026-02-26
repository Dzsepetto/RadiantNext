using System;
using System.Numerics;

public class Camera3D
{
    public Vector3 Position { get; set; }

    public float Yaw { get; set; }
    public float Pitch { get; set; }

    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; private set; }

    public float FieldOfView { get; set; } = MathF.PI / 4f;
    public float AspectRatio { get; set; } = 16f / 9f;
    public float NearPlane { get; set; } = 1f;
    public float FarPlane { get; set; } = 10000f;

    public Camera3D()
    {
        Position = new Vector3(0, -500, 200);

        // +Y irányba nézzen induláskor
        Yaw = MathF.PI / 2f;
        Pitch = 0f;

        UpdateVectors();
    }

    public void UpdateVectors()
    {
        Forward = new Vector3(
            MathF.Cos(Pitch) * MathF.Cos(Yaw),
            MathF.Cos(Pitch) * MathF.Sin(Yaw),
            MathF.Sin(Pitch));

        Forward = Vector3.Normalize(Forward);

        // Z az up tengely a világban
        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitZ));
        Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
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