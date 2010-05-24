using System;
using System.Collections.Generic;
using System.Text;



namespace Modelthulhu
{
    public class CSGVertex
    {
        public static long nextID = 0;
        public long id;                                             // an identifying integer
        public Vec3 position;                                       // position of this vertex

        public CSGSourceTriangle parentTriangle;                    // a vertex generated within a single source triangle has this reference to its parent triangle

        public List<CSGEdge> neighbors = new List<CSGEdge>();       // list of edges connecting at this vertex

        public CSGVertex()
        {
            id = nextID++;
        }
        public CSGVertex(Vec3 pos)
            : this()
        {
            position = pos;
        }

        public void SetAsNeighbor(CSGEdge edge)
        {
            if (!neighbors.Exists((e) => e.id == edge.id))
                neighbors.Add(edge);
        }
    }
}
