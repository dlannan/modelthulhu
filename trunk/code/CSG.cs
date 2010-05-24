using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu
{
    public enum RegionBehavior
    {
        Delete,
        Normal,
        Flip
    }
    public delegate RegionBehavior RegionKeepCondition(List<PreResultTriangle> regionTriangles);

    // Uh... well...
    // Create an instance of this to solve a single CSG operation, I guess
    public class CSG
    {
        // Assign these before doing stuff
        private BasicModelData first_in, second_in;
        public BasicModelData FirstInput { get { return first_in; } set { first_in = value; } }
        public BasicModelData SecondInput { get { return second_in; } set { second_in = value; } }
        // Set transforms to apply once it starts doing stuff, or just use the defaults
        private Mat4 first_xform = Mat4.Identity, second_xform = Mat4.Identity;
        public Mat4 FirstInputTransform { get { return first_xform; } set { first_xform = value; } }
        public Mat4 SecondInputTransform { get { return second_xform; } set { second_xform = value; } }
        // Read these after doing stuff
        private BasicModelData first_out, second_out;
        public BasicModelData FirstOutput { get { return first_out; } protected set { first_out = value; } }
        public BasicModelData SecondOutput { get { return second_out; } protected set { second_out = value; } }
        // Set this before doing stuff
        private OperationType op;
        public OperationType OperationType { get { return op; } set { op = value; } }
        private bool invert_first, invert_second;
        public bool InvertFirstObjectNormals { get { return invert_first; } set { invert_first = value; } }
        public bool InvertSecondObjectNormals { get { return invert_second; } set { invert_second = value; } }

        private RegionKeepCondition first_keep, second_keep;
        public RegionKeepCondition FirstKeepCondition { get { return first_keep; } set { first_keep = value; } }
        public RegionKeepCondition SecondKeepCondition { get { return second_keep; } set { second_keep = value; } }

        protected ModelInput first_blob, second_blob;

        public int pairs_to_test;

        public static string message = "";

        public List<Vec3> cutPoints;
        public List<Vec3[]> cutEdges;

        public CSG(OperationType op)
        {
            this.op = op;
            first_keep = MajorityCondition(
                ((p) => second_blob.IsPointInside(p)), 
                DoesOpAccept(op, 0, true) ? invert_first ? RegionBehavior.Flip : RegionBehavior.Normal : RegionBehavior.Delete, 
                DoesOpAccept(op, 0, false) ? invert_first ? RegionBehavior.Flip : RegionBehavior.Normal : RegionBehavior.Delete);
            second_keep = MajorityCondition(
                ((p) => first_blob.IsPointInside(p)),
                DoesOpAccept(op, 1, true) ? invert_first ? RegionBehavior.Flip : RegionBehavior.Normal : RegionBehavior.Delete,
                DoesOpAccept(op, 1, false) ? invert_first ? RegionBehavior.Flip : RegionBehavior.Normal : RegionBehavior.Delete);
        }

        // Do stuff
        // This is a virtual function, just in case someone thinks they can do it better
        public virtual void Compute()
        {
            first_blob = ModelInput.FromBasicModelData(first_in, first_xform);
            second_blob = ModelInput.FromBasicModelData(second_in, second_xform);

            /*
            CSGModel derp1 = ModelUtil.ModelFromBMD(first_in, first_xform);
            CSGModel derp2 = ModelUtil.ModelFromBMD(second_in, second_xform);
            int count = 0;
            ModelIntersectTree.CullIntersections(derp1, derp2,
                (i, j) =>
                {
                    count++;
                });
            */

            AABB aa_box = first_blob.AABB;
            AABB bb_box = second_blob.AABB;
            if (AABB.CheckIntersection(aa_box, bb_box))                     // check whether their bounding boxes even intersect... if they don't, most of the math can be skipped!
            {
                AABB intersection = AABB.Intersection(aa_box, bb_box);
                PartitioningGrid grid = new PartitioningGrid(intersection, 5);
                first_blob.PopulateGrid(grid, 0);
                second_blob.PopulateGrid(grid, 1);

                List<TrianglePair> pairs = new List<TrianglePair>();
                grid.ForLeafLevelPairs(
                    (first, second) =>
                    {
                        pairs.Add(new TrianglePair { a = first, b = second });
                    });
                Snip(first_blob, second_blob, pairs);
            }
            else
                Snip(first_blob, second_blob, new List<TrianglePair>());
        }

        // Once all of the triangle pairs that need intersection testing have been determined, we can do the actual intersection testing
        // Get out the scissors !!!
        protected virtual void Snip(ModelInput in_a, ModelInput in_b, List<TrianglePair> pairs)
        {
            pairs_to_test = pairs.Count;

            List<int> first_tris = new List<int>();
            List<int> second_tris = new List<int>();
            foreach (TrianglePair pair in pairs)
            {
                if (!first_tris.Contains(pair.a))
                    first_tris.Add(pair.a);
                if (!second_tris.Contains(pair.b))
                    second_tris.Add(pair.b);
            }

            WorkingModel a = in_a.ToWorkingModel(0, first_tris);
            WorkingModel b = in_b.ToWorkingModel(1, second_tris);

            WorkingModel.Intersect(a, b, pairs);

            List<BasicModelVert> first_notsafe_bmv = a.GetBMVList(first_in, first_xform);
            List<BasicModelVert> first_safe_bmv = new List<BasicModelVert>();
            for (int i = 0; i < first_in.a_vert.Length; i++)
                if (!first_tris.Contains(i))
                    for (int j = 0; j < 3; j++)
                    {
                        int xyz = (int)((j == 0 ? first_in.a_vert : j == 1 ? first_in.b_vert : first_in.c_vert)[i]);
                        int uv = (int)((j == 0 ? first_in.a_uv : j == 1 ? first_in.b_uv : first_in.c_uv)[i]);
                        int norm = (int)((j == 0 ? first_in.a_norm : j == 1 ? first_in.b_norm : first_in.c_norm)[i]);
                        first_safe_bmv.Add(new BasicModelVert { position = first_xform.TransformVec3(new Vec3 { x = first_in.x[xyz], y = first_in.y[xyz], z = first_in.z[xyz] }, 1.0), uv = new Vec2 { x = first_in.u[uv], y = first_in.v[uv] }, normal = Vec3.Normalize(first_xform.TransformVec3(new Vec3 { x = first_in.nx[norm], y = first_in.ny[norm], z = first_in.nz[norm] }, 0.0)) });
                    }
            List<BasicModelVert> second_notsafe_bmv = b.GetBMVList(second_in, second_xform);
            List<BasicModelVert> second_safe_bmv = new List<BasicModelVert>();
            for (int i = 0; i < second_in.a_vert.Length; i++)
                if (!second_tris.Contains(i))
                    for (int j = 0; j < 3; j++)
                    {
                        int xyz = (int)((j == 0 ? second_in.a_vert : j == 1 ? second_in.b_vert : second_in.c_vert)[i]);
                        int uv = (int)((j == 0 ? second_in.a_uv : j == 1 ? second_in.b_uv : second_in.c_uv)[i]);
                        int norm = (int)((j == 0 ? second_in.a_norm : j == 1 ? second_in.b_norm : second_in.c_norm)[i]);
                        second_safe_bmv.Add(new BasicModelVert { position = second_xform.TransformVec3(new Vec3 { x = second_in.x[xyz], y = second_in.y[xyz], z = second_in.z[xyz] }, 1.0), uv = new Vec2 { x = second_in.u[uv], y = second_in.v[uv] }, normal = Vec3.Normalize(second_xform.TransformVec3(new Vec3 { x = second_in.nx[norm], y = second_in.ny[norm], z = second_in.nz[norm] }, 0.0)) });
                    }

            cutEdges = new List<Vec3[]>();
            cutEdges.AddRange(a.GetCutEdgesList());
            cutEdges.AddRange(b.GetCutEdgesList());

            cutPoints = new List<Vec3>();
            foreach (Vec3[] edge in b.GetCutEdgesList())
                foreach (Vec3 vert in edge)
                    if (!cutPoints.Exists((v) => (v - vert).ComputeMagnitudeSquared() < 0.0000000000000000000001))
                        cutPoints.Add(vert);

            message = cutPoints.Count + " edge points";

            List<BasicModelVert> first_bmv_trimmed = ScrapTrimmedStuff(first_safe_bmv, first_notsafe_bmv, cutEdges, first_keep);
            List<BasicModelVert> second_bmv_trimmed = ScrapTrimmedStuff(second_safe_bmv, second_notsafe_bmv, cutEdges, second_keep);

            first_out = BMVListToModel(first_bmv_trimmed);
            second_out = BMVListToModel(second_bmv_trimmed);
        }

        protected virtual List<BasicModelVert> ScrapTrimmedStuff(List<BasicModelVert> safeZone, List<BasicModelVert> notsafeZone, List<Vec3[]> cutEdges, RegionKeepCondition regionCondition)
        {
            PreResultModel prm = new PreResultModel();
            prm.ProhibitEdges(cutEdges);
            prm.AddTriangles(notsafeZone);
            prm.AddTriangles(safeZone);
            return prm.Trim(regionCondition);
        }

        public static BasicModelVert WorkingVertexToBMV(Vec3 position, VInfoReference[] vinfos, BasicModelData input_model, Mat4 input_xform, WorkingModel working_model)
        {
            Vec3 normal = Vec3.Zero;
            Vec2 uv = Vec2.Zero;
            foreach (VInfoReference vinfo in vinfos)
            {
                int index = vinfo.index;
                if (index != -1)
                {
                    int tri = index / 3;
                    int vert = index % 3;
                    double weight = vinfo.weight;
                    int normal_index = (int)(vert == 0 ? input_model.a_norm[tri] : vert == 1 ? input_model.b_norm[tri] : input_model.c_norm[tri]);
                    normal += weight * input_xform.TransformVec3(new Vec3 { x = input_model.nx[normal_index], y = input_model.ny[normal_index], z = input_model.nz[normal_index] }, 0.0);
                    int uv_index = (int)(vert == 0 ? input_model.a_uv[tri] : vert == 1 ? input_model.b_uv[tri] : input_model.c_uv[tri]);
                    uv += weight * new Vec2 { x = input_model.u[uv_index], y = input_model.v[uv_index] };
                }
            }
            return new BasicModelVert { position = position, normal = Vec3.Normalize(normal), uv = uv };
        }

        public bool DoesOpAccept(int fromWhich, bool insideOther)
        {
            bool outside = !insideOther;
            OperationType needed = fromWhich == 0 ? outside ? OperationType.KeepFirstOutsideSecond : OperationType.KeepFirstInsideSecond : outside ? OperationType.KeepSecondOutsideFirst : OperationType.KeepSecondInsideFirst;
            return ((op & needed) != 0);
        }

        public static bool DoesOpAccept(OperationType op, int fromWhich, bool insideOther)
        {
            bool outside = !insideOther;
            OperationType needed = fromWhich == 0 ? outside ? OperationType.KeepFirstOutsideSecond : OperationType.KeepFirstInsideSecond : outside ? OperationType.KeepSecondOutsideFirst : OperationType.KeepSecondInsideFirst;
            return ((op & needed) != 0);
        }

        protected virtual BasicModelData BMVListToModel(List<BasicModelVert> bmVerts)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> z = new List<double>();
            List<double> nx = new List<double>();
            List<double> ny = new List<double>();
            List<double> nz = new List<double>();
            List<double> u = new List<double>();
            List<double> v = new List<double>();
            List<uint> a_vert = new List<uint>();
            List<uint> b_vert = new List<uint>();
            List<uint> c_vert = new List<uint>();
            List<uint> a_norm = new List<uint>();
            List<uint> b_norm = new List<uint>();
            List<uint> c_norm = new List<uint>();
            List<uint> a_uv = new List<uint>();
            List<uint> b_uv = new List<uint>();
            List<uint> c_uv = new List<uint>();
            int index = 0;
            foreach (BasicModelVert vert in bmVerts)
            {
                int i = index % 3;
                //int i = winding[j];
                List<uint> target_vert = (i == 0 ? a_vert : i == 1 ? b_vert : c_vert);
                List<uint> target_norm = (i == 0 ? a_norm : i == 1 ? b_norm : c_norm);
                List<uint> target_uv = (i == 0 ? a_uv : i == 1 ? b_uv : c_uv);
                target_vert.Add((uint)x.Count);
                target_norm.Add((uint)nx.Count);
                target_uv.Add((uint)u.Count);
                x.Add(vert.position.x);
                y.Add(vert.position.y);
                z.Add(vert.position.z);
                nx.Add(vert.normal.x);
                ny.Add(vert.normal.y);
                nz.Add(vert.normal.z);
                u.Add(vert.uv.x);
                v.Add(vert.uv.y);
                index++;
            }
            return new BasicModelData { x = x.ToArray(), y = y.ToArray(), z = z.ToArray(), nx = nx.ToArray(), ny = ny.ToArray(), nz = nz.ToArray(), u = u.ToArray(), v = v.ToArray(), a_vert = a_vert.ToArray(), b_vert = b_vert.ToArray(), c_vert = c_vert.ToArray(), a_norm = a_norm.ToArray(), b_norm = b_norm.ToArray(), c_norm = c_norm.ToArray(), a_uv = a_uv.ToArray(), b_uv = b_uv.ToArray(), c_uv = c_uv.ToArray() };
        }

        public static RegionKeepCondition MajorityCondition(Predicate<Vec3> insideTestFunction, RegionBehavior insideBehavior, RegionBehavior outsideBehavior)
        {
            return new RegionKeepCondition(
                (triangles) =>
                {
                    int inside = 0, outside = 0;
                    for (int i = 0; i < 23; i++)
                        if (insideTestFunction(triangles[Random3D.RandInt(triangles.Count)].GetCenterPoint()))
                            inside++;
                        else
                            outside++;
                    return (inside > outside) ? insideBehavior : outsideBehavior;
                });
        }
    }
}
