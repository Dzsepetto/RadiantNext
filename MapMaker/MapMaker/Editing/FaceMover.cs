using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Core.Editing
{
    public static class FaceMover
    {
        public static void Move(Face face, Vector3 delta, float gridSize)
        {
            face.Translate(delta);
            face.SnapToGrid(gridSize);
        }

        public static void MoveAlongNormal(Face face, float distance, float gridSize)
        {
            var normal = face.Normal;

            face.Translate(normal * distance);
            face.SnapToGrid(gridSize);
        }
    }
}