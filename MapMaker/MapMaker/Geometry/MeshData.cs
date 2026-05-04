using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Geometry
{
    public class MeshData
    {
        public List<Vector3> Vertices { get; } = new();
        public List<int> Indices { get; } = new();
        public List<Vector3> Normals { get; } = new();
    }
}
