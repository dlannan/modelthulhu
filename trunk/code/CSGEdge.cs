using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu.NSided
{
    public class CSGEdge
    {
        public static long nextID = 0;
        public long id;                                                         // an identifying integer

        public CSGVertex[] endpoints = new CSGVertex[2];                        // the two endpoints of this edge
        public CSGShape[] separatedShapes = new CSGShape[2];                    // the two shapes separated by this edge

        public CSGSourceTriangle parentTriangle;                                // an edge generated within a single source triangle has this reference to its parent triangle

        public List<CSGIntersection> cuts = new List<CSGIntersection>();        // list of intersections of this edge

        public CSGEdge()
        {
            id = nextID++;
        }
        public CSGEdge(CSGVertex[] endpoints)
            : this()
        {
            for (int i = 0; i < 2; i++)
            {
                this.endpoints[i] = endpoints[i];
                endpoints[i].SetAsNeighbor(this);
            }
        }

        public void SetAsNeighbor(CSGShape shape)
        {
            if (separatedShapes[0] == null)
                separatedShapes[0] = shape;
            else if (separatedShapes[1] == null)
            {
                if (shape.id != separatedShapes[0].id)
                    separatedShapes[1] = shape;
                else
                { }
            }
        }
    }
}
