using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Editor.Editor
{
    public sealed class EditorGrid
    {
        public float Size { get; private set; } = 16f;

        public static readonly float[] AllowedSizes =
        {
        1, 2, 4, 8, 16, 32, 64
    };

        public void SetSize(float size)
        {
            if (!AllowedSizes.Contains(size))
                return;

            Size = size;
        }

        public float Snap(float value)
        {
            return MathF.Round(value / Size) * Size;
        }

        public Vector3 Snap(Vector3 position)
        {
            return new Vector3(
                Snap(position.X),
                Snap(position.Y),
                Snap(position.Z)
            );
        }
    }
}
