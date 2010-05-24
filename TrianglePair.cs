using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;
using TheLibrary.Graphics;

namespace TheLibrary.CSG
{
    // A pair of triangles -- referred to using indices into two objects' triangle arrays
    // You'll need to know which those are, or these indices will turn out to be rather useless!
    public struct TrianglePair
    {
        public int a, b;
    }
}
