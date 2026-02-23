using MapMaker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core
{
    public static class BrushFactory
    {
        public static Brush CreateBox(Vector3 min, Vector3 max, string texture)
        {
            var b = new Brush();

            float x1 = min.X, y1 = min.Y, z1 = min.Z;
            float x2 = max.X, y2 = max.Y, z2 = max.Z;

            b.Faces.Add(new Face(
                new(x1, y1, z1),
                new(x2, y1, z1),
                new(x2, y2, z1),
                texture));

            b.Faces.Add(new Face(
                new(x1, y2, z2),
                new(x2, y2, z2),
                new(x2, y1, z2),
                texture));

            b.Faces.Add(new Face(
                new(x1, y1, z1),
                new(x1, y1, z2),
                new(x2, y1, z2),
                texture));

            b.Faces.Add(new Face(
                new(x2, y2, z1),
                new(x2, y2, z2),
                new(x1, y2, z2),
                texture));

            b.Faces.Add(new Face(
                new(x1, y2, z1),
                new(x1, y2, z2),
                new(x1, y1, z2),
                texture));

            b.Faces.Add(new Face(
                new(x2, y1, z1),
                new(x2, y1, z2),
                new(x2, y2, z2),
                texture));

            return b;
        }

    }

}
