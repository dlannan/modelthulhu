using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;
using Modelthulhu.Geom;

namespace Modelthulhu.NSided
{
    public class CSGSourceTriangle
    {
        public static long nextID = 0;
        public long id;                                                                         // an identifying integer

        public CSGModel model;                                                                  // the model this source triangle is from
        public CSGVertex[] sourceVerts = new CSGVertex[3];                                      // the positions of the original 3 verts
        public Dictionary<long, VInfo> vertexVInfos = new Dictionary<long, VInfo>();            // mapping a vertex info to each vertex id

        public List<CSGIntersection> surfaceIntersections = new List<CSGIntersection>();        // places this triangle has been struck by edges
        public List<CSGShape> divisions = new List<CSGShape>();                                 // whatever shapes this triangle may have been divided into

        public Plane plane;

        public CSGSourceTriangle()
        {
            id = nextID++;
        }
        public CSGSourceTriangle(CSGModel model, CSGVertex[] verts, VInfo[] vinfos)
            : this()
        {
            this.model = model;

            for(int i = 0; i< 3;i++)
            {
                sourceVerts[i] = verts[i];
                vertexVInfos[verts[i].id] = vinfos[i];
            }

            ComputePlane();

            divisions.Add(new CSGShape(model, this));
        }

        public void ComputePlane()
        {
            plane = Plane.FromTriangleVertices(sourceVerts[0].position, sourceVerts[1].position, sourceVerts[2].position);
        }

        public Octree.Item ToOctreeItem()
        {
            return new Octree.Item { aabb = AABB.FitPointList(new List<Vec3>(new Vec3[] { sourceVerts[0].position, sourceVerts[1].position, sourceVerts[2].position })), obj = this };
        }
    }
}
