using MapMaker.Core.Geometry;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.Rendering
{
    public static class MeshConverter
    {
        public static MeshGeometry3D ToWpfMesh(MeshData data)
        {
            var mesh = new MeshGeometry3D();

            foreach (var v in data.Vertices)
            {
                mesh.Positions.Add(new Point3D(v.X, v.Y, v.Z));
            }

            foreach (var i in data.Indices)
            {
                mesh.TriangleIndices.Add(i);
            }

            foreach (var n in data.Normals)
            {
                mesh.Normals.Add(new Vector3D(n.X, n.Y, n.Z));
            }

            return mesh;
        }
    }
}
