    using MapMaker.Core.Editing;
    using MapMaker.Core.Geometry;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

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
        public Vector3 Normal
        {
            get
            {
                var a = P2 - P1;
                var b = P3 - P1;

                var normal = Vector3.Cross(a, b);

                if (normal.LengthSquared() == 0)
                    return Vector3.Zero;

                return Vector3.Normalize(normal);
            }
        }
        public void SnapToGrid(float gridSize)
        {
            P1 = GridSnapper.Snap(P1, gridSize);
            P2 = GridSnapper.Snap(P2, gridSize);
            P3 = GridSnapper.Snap(P3, gridSize);

            Polygon = null;
        }
        public void Translate(Vector3 delta)
        {
            P1 += delta;
            P2 += delta;
            P3 += delta;

            Polygon = null;
        }
        public void TransformPoints(Matrix4x4 matrix)
        {
            P1 = Vector3.Transform(P1, matrix);
            P2 = Vector3.Transform(P2, matrix);
            P3 = Vector3.Transform(P3, matrix);

            Polygon = null;
        }
        public void SetPoints(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;

            Polygon = null;
        }
    }
    }
