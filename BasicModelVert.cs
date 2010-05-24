using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;

namespace TheLibrary.CSG
{
    public struct BasicModelVert
    {
        public Vec3 position;
        public Vec3 normal;
        public Vec2 uv;

        public static BasicModelVert Interpolate(BasicModelVert[] verts, double[] weights)
        {
            Vec3 position = Vec3.Zero;
            Vec3 normal = Vec3.Zero;
            Vec2 uv = Vec2.Zero;
            for (int i = 0; i < weights.Length; i++)
            {
                position += verts[i].position * weights[i];
                normal += verts[i].normal * weights[i];
                uv += verts[i].uv * weights[i];
            }
            normal /= normal.ComputeMagnitude();
            return new BasicModelVert { position = position, normal = normal, uv = uv };
        }
    }
}
