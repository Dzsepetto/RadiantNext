    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Numerics;
    using MapMaker.Core.Geometry;

    namespace MapMaker.Core.Models
    {
        public class Face
        {
            public Vector3 P1 { get; private set; }
            public Vector3 P2 { get; private set; }
            public Vector3 P3 { get; private set; }

            public string Texture { get; }

            public float ShiftX { get; }
            public float ShiftY { get; }
            public float Rotation { get; }
            public float ScaleX { get; }
            public float ScaleY { get; }

            public Plane3D Plane => Plane3D.FromPoints(P1, P2, P3);
            public Polygon3D? Polygon { get; set; }
            public Face(
                Vector3 p1,
                Vector3 p2,
                Vector3 p3,
                string texture,
                float shiftX = 0,
                float shiftY = 0,
                float rotation = 0,
                float scaleX = 0.25f,
                float scaleY = 0.25f)
            {
                P1 = p1;
                P2 = p2;
                P3 = p3;
                Texture = texture;
                ShiftX = shiftX;
                ShiftY = shiftY;
                Rotation = rotation;
                ScaleX = scaleX;
                ScaleY = scaleY;
            }
            public Face Flip()
            {
                return new Face(
                    P1,
                    P3,
                    P2,
                    Texture,
                    ShiftX,
                    ShiftY,
                    Rotation,
                    ScaleX,
                    ScaleY
                );
            }
        public void Translate(Vector3 delta)
        {
            P1 += delta;
            P2 += delta;
            P3 += delta;

            Polygon = null;
        }
    }
    }
