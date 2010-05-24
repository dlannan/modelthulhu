using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;
using TheLibrary.Graphics;

namespace TheLibrary.CSG
{
    public static class ModelUtil
    {
        public static CSGModel ModelFromBMD(BasicModelData input) { return ModelFromBMD(input, Mat4.Identity); }

        public static CSGModel ModelFromBMD(BasicModelData input, Mat4 xform)
        {
            List<Vec3> verts = new List<Vec3>();
            List<VInfo> vinfos = new List<VInfo>();
            CopyAndTransformVert vx = VertexTransformationByMat4(xform);
            CopyAndTransformVInfo vi = BMDVInfoTransformationByMat4(xform);
            for (int i = 0; i < input.a_vert.Length; i++)
            {
                TransformAndAddVec3(verts, new Vec3 { x = input.x[input.a_vert[i]], y = input.y[input.a_vert[i]], z = input.z[input.a_vert[i]] }, vx);
                TransformAndAddVec3(verts, new Vec3 { x = input.x[input.b_vert[i]], y = input.y[input.b_vert[i]], z = input.z[input.b_vert[i]] }, vx);
                TransformAndAddVec3(verts, new Vec3 { x = input.x[input.c_vert[i]], y = input.y[input.c_vert[i]], z = input.z[input.c_vert[i]] }, vx);
                TransformAndAddVInfo(vinfos, new BMDVInfo { material = nextMaterial, uv = new Vec2 { x = input.u[input.a_uv[i]], y = input.v[input.a_uv[i]] }, normal = new Vec3 { x = input.nx[input.a_norm[i]], y = input.ny[input.a_norm[i]], z = input.nz[input.a_norm[i]] } }, vi);
                TransformAndAddVInfo(vinfos, new BMDVInfo { material = nextMaterial, uv = new Vec2 { x = input.u[input.b_uv[i]], y = input.v[input.b_uv[i]] }, normal = new Vec3 { x = input.nx[input.b_norm[i]], y = input.ny[input.b_norm[i]], z = input.nz[input.b_norm[i]] } }, vi);
                TransformAndAddVInfo(vinfos, new BMDVInfo { material = nextMaterial, uv = new Vec2 { x = input.u[input.c_uv[i]], y = input.v[input.c_uv[i]] }, normal = new Vec3 { x = input.nx[input.c_norm[i]], y = input.ny[input.c_norm[i]], z = input.nz[input.c_norm[i]] } }, vi);
            }
            nextMaterial++;
            return new CSGModel(verts, vinfos);
        }

        public delegate void CopyAndTransformVInfo(VInfo input, out VInfo result);
        public delegate void CopyAndTransformVert(Vec3 vert, out Vec3 result);

        public static CopyAndTransformVert VertexTransformationByMat4(Mat4 mat)
        {
            return new CopyAndTransformVert(
                (Vec3 basis, out Vec3 result) =>
                {
                    result = mat.TransformVec3(basis, 1.0);
                });
        }
        public static CopyAndTransformVInfo BMDVInfoTransformationByMat4(Mat4 mat)
        {
            return new CopyAndTransformVInfo(
                (VInfo basis, out VInfo result) =>
                {
                    BMDVInfo bmd = basis as BMDVInfo;
                    if (bmd == null)
                        throw new Exception("Can't treat some random VInfo as though it were a BMDVInfo when it isn't!");
                    result = new BMDVInfo { material = bmd.material, uv = bmd.uv, normal = Vec3.Normalize(mat.TransformVec3(bmd.normal, 0.0)) };
                });
        }

        private static void TransformAndAddVec3(List<Vec3> list, Vec3 input, CopyAndTransformVert xform)
        {
            Vec3 result;
            xform(input, out result);
            list.Add(result);
        }
        private static void TransformAndAddVInfo(List<VInfo> list, VInfo input, CopyAndTransformVInfo xform)
        {
            VInfo result;
            xform(input, out result);
            list.Add(result);
        }

        public static int nextMaterial = 0;
        public class BMDVInfo : VInfo
        {
            public int material;
            public Vec2 uv;
            public Vec3 normal;
        }
    }
}
