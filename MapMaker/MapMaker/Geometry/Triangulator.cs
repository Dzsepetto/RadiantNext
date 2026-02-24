using System.Collections.Generic;

namespace MapMaker.Core.Geometry
{
    public static class Triangulator
    {
        public static List<Triangle3D> Triangulate(Polygon3D polygon)
        {
            var triangles = new List<Triangle3D>();

            var vertices = polygon.Vertices;

            if (vertices.Count < 3)
                return triangles;

            for (int i = 1; i < vertices.Count - 1; i++)
            {
                triangles.Add(new Triangle3D(
                    vertices[0],
                    vertices[i],
                    vertices[i + 1]));
            }

            return triangles;
        }
    }
}