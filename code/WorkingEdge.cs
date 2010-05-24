using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu
{
    public class WorkingEdge
    {
        public WorkingVertex[] verts = new WorkingVertex[2];                            // the two endpoints of this edge
        public WorkingTriangle[] triangles = new WorkingTriangle[2];                    // the triangles on either side of this edge (may be null)

        public List<EdgeIntersection> intersections = new List<EdgeIntersection>();

        public WorkingTriangle GetOtherTriangle(WorkingTriangle notThisOne)
        {
            if (notThisOne == triangles[0])
                return triangles[1];
            else
                return triangles[0];
        }

        public void Intersect(WorkingTriangle triangle)
        {
            Vec3 original = verts[0].position.xyz;
            Vec3 terminal = verts[1].position.xyz;

            Vec3 triVertexA = triangle.verts[0].position.xyz;
            Vec3 triVertexB = triangle.verts[1].position.xyz;
            Vec3 triVertexC = triangle.verts[2].position.xyz;

            double timeToImpact;
            Vec3 impactPosition;
            double u, v;

            if (Util.RayTriangleIntersect(original, terminal - original, triVertexA, triVertexB, triVertexC, out timeToImpact, out impactPosition, out u, out v))
            {
                // make sure it's not beyond the length of the edge!
                if (timeToImpact >= 0.0 && timeToImpact <= 1.0)
                {
                    EdgeIntersection intersection = new EdgeIntersection();
                    intersection.edge = this;
                    intersection.position = new VertexPosition { xyz = new Vec3 { x = impactPosition.x, y = impactPosition.y, z = impactPosition.z } };
                    intersection.triangle = triangle;
                    intersections.Add(intersection);
                    triangle.otherObjectEdgeIntersections.Add(intersection);
                }
            }
        }
    }
}
