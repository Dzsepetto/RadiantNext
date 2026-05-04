using MapMaker.Core.Builders;
using MapMaker.Core.Geometry;
using System.Numerics;

namespace MapMaker.Core.Models
{
    public class Brush
    {
        public List<Face> Faces { get; } = new();
        public Transform3D Transform { get; } = new();

        private List<Vector3>? _cachedVertices;

        public List<Vector3> GetVertices()
        {
            if (_cachedVertices == null)
                _cachedVertices = BrushBuilder.GenerateVertices(this);

            return _cachedVertices;
        }

        public void Invalidate()
        {
            _cachedVertices = null;
        }
    }
}