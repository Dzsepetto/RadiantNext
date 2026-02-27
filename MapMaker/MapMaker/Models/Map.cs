using MapMaker.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Models
{
    public class Map
    {
        public List<Entity> Entities { get; } = new();

        public void BuildGeometry(ILogger? logger = null)
        {
            foreach (var entity in Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    Geometry.GeometryBuilder.BuildBrushPolygons(brush, logger);
                }
            }
        }
    }
}
