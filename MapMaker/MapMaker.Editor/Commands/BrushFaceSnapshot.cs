using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Editor.Commands;

public sealed class BrushFaceSnapshot
{
    public Face Face { get; }
    public Vector3 P1 { get; }
    public Vector3 P2 { get; }
    public Vector3 P3 { get; }

    public BrushFaceSnapshot(Face face)
    {
        Face = face;
        P1 = face.P1;
        P2 = face.P2;
        P3 = face.P3;
    }

    public void Restore()
    {
        Face.SetPoints(P1, P2, P3);
    }
}