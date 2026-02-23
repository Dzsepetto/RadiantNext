using System.Numerics;

namespace MapMaker.Core.Geometry
{
    public struct Plane3D
    {
        public Vector3 Normal { get; }
        public float D { get; }

        public Plane3D(Vector3 normal, float d)
        {
            Normal = Vector3.Normalize(normal);
            D = d;
        }

        public static Plane3D FromPoints(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var v1 = p2 - p1;
            var v2 = p3 - p1;

            var normal = Vector3.Cross(v1, v2);

            if (normal.LengthSquared() < 0.00001f)
                throw new InvalidOperationException("Degenerate plane (collinear points).");

            normal = Vector3.Normalize(normal);

            float d = -Vector3.Dot(normal, p1);

            return new Plane3D(normal, d);
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Dot(Normal, point) + D;
        }

        public static Vector3? Intersect(
        Plane3D p1,
        Plane3D p2,
        Plane3D p3)
        {
            var n1 = p1.Normal;
            var n2 = p2.Normal;
            var n3 = p3.Normal;

            var cross23 = Vector3.Cross(n2, n3);
            var cross31 = Vector3.Cross(n3, n1);
            var cross12 = Vector3.Cross(n1, n2);

            float denom = Vector3.Dot(n1, cross23);

            const float epsilon = 0.00001f;

            if (MathF.Abs(denom) < epsilon)
                return null;

            var term1 = -p1.D * cross23;
            var term2 = -p2.D * cross31;
            var term3 = -p3.D * cross12;

            var intersection = (term1 + term2 + term3) / denom;

            return intersection;
        }
    }
}