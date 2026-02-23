using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Models
{
    public class Entity
    {
        public Dictionary<string, string> Properties { get; } = new();
        public List<Brush> Brushes { get; } = new();
    }

}
