using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu
{
    public class WorkingTriangle
    {
        public int objID;
        public int triID;

        public WorkingVertex[] verts = new WorkingVertex[3];
        public WorkingEdge[] edges = new WorkingEdge[3];

        public List<EdgeIntersection> otherObjectEdgeIntersections = new List<EdgeIntersection>();          // places this triangle has been intersected by the edges of the other object

        public WorkingTriangle[] GetEdgeNeighbors()
        {
            return new WorkingTriangle[] { edges[0].GetOtherTriangle(this), edges[1].GetOtherTriangle(this), edges[2].GetOtherTriangle(this) };
        }

        public VInfoReference[] InterpolateVInfos(Vec3 targetPosition)
        {
            VInfoReference[] vinfo = new VInfoReference[3];
            Vec2 ij = Util.VectorToTriangleCoords(new Vec3[] { verts[0].position.xyz, verts[1].position.xyz, verts[2].position.xyz }, targetPosition);
            double[] weights = new double[] { 1.0 - (ij.x + ij.y), ij.x, ij.y };
            vinfo[0] = new VInfoReference { objID = objID, index = verts[0].vinfo[0].index, weight = weights[0]};
            vinfo[1] = new VInfoReference { objID = objID, index = verts[1].vinfo[0].index, weight = weights[1] };
            vinfo[2] = new VInfoReference { objID = objID, index = verts[2].vinfo[0].index, weight = weights[2] };
            return vinfo;
        }

        public override bool Equals(object obj)
        {
            return obj == this ? true : !(obj is WorkingTriangle) ? false : ((obj as WorkingTriangle).objID == objID && (obj as WorkingTriangle).triID == triID);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
