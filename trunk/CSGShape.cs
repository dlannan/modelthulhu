using System;
using System.Collections.Generic;
using System.Text;

namespace TheLibrary.CSG
{
    // would just be a polygon, but since it can potentially have 'holes' it's a bit more than just that
    public class CSGShape
    {
        public static long nextID = 0;
        public long id;                                             // an identifying integer
            
        public CSGSourceTriangle parentTriangle;                    // the source triangle from which this shape was cut

        public List<CSGVertex> boundary = new List<CSGVertex>();    // outer boundary of this shape
        public List<List<CSGVertex>> holes = new List<List<CSGVertex>>();       // any inner boundaries this shape has

        public List<CSGEdge> edges = new List<CSGEdge>();           // edges of this shape

        public CSGShape()
        {
            id = nextID++;
        }
        public CSGShape(CSGModel model, CSGSourceTriangle triangleBasis)
            : this()
        {
            parentTriangle = triangleBasis;
            foreach (CSGVertex vert in triangleBasis.sourceVerts)
                boundary.Add(vert);

            for (int i = 0; i < 3; i++)
            {
                CSGEdge edge = model.GetEdge(boundary[i], boundary[(i + 1) % 3], true);
                edge.SetAsNeighbor(this);
                edges.Add(edge);
            }
        }
    }
}
