using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Core.Editing
{
    public static class BrushMover
    {
        public static void Move(Brush brush, Vector3 delta)
        {
            foreach (var face in brush.Faces)
            {
                face.Translate(delta);
            }
        }
    }
}