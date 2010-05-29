using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu.NSided
{
    public class CSGIntersection
    {
        public static long nextID = 0;
        public long id;                                     // an identifying integer

        public CSGSourceTriangle shape;                     // the triangle whose surface was struck by an edge
        public CSGEdge edge;                                // the edge which struck a surface

        public CSGVertex vertex;                            // the vertex created by the intersection

        public CSGIntersection()
        {
            id = nextID++;
        }
    }
}
