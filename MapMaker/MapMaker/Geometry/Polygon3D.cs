using System.Numerics;

namespace MapMaker.Core.Geometry
{
    public class Polygon3D
    {
        public List<Vector3> Vertices { get; } = new();

        public Polygon3D(IEnumerable<Vector3> vertices)
        {
            Vertices.AddRange(vertices);
        }
    }
}