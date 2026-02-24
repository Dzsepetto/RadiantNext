using System.Numerics;

namespace MapMaker.Core.Geometry
{
    public struct Triangle3D
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public Triangle3D(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}