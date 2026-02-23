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
    }
}
