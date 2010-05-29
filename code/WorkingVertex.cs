using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;
using Modelthulhu.UserModel;

namespace Modelthulhu.Triangulated
{
    public class WorkingVertex
    {
        public VertexPosition position;

        // a vertex could be produced in the middle of a triangle, in which case its vinfo must be interpolated from that triangle's 3 vinfos
        public VInfoReference[] vinfo = new VInfoReference[3];

        public override bool Equals(object obj)
        {
            WorkingVertex v = obj as WorkingVertex;
            if (v == null) 
                return false;
            Vec3 dif = v.position.xyz - position.xyz;
            return dif.ComputeMagnitudeSquared() < 0.000000001;
        }
    }
}
