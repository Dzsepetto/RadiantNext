using MapMaker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Core.Editing
{
    public class FaceMover
    {
        public static void Move(Face face, Vector3 delta)
        {
            face.Translate(delta);
        }
    }
}
