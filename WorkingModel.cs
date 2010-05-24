using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;
using TheLibrary.Graphics;

namespace TheLibrary.CSG
{
    public class WorkingModel
    {
        public int objID;               // this should be matched by the objID's of all of the verts/triangles/etc. within this model

        public List<WorkingTriangle> triangles = new List<WorkingTriangle>();
        public List<VertexPosition> vertexPositions = new List<VertexPosition>();

        public List<int> originalTriangleIndices = new List<int>();

        public List<CutTriangle> cutTriangles;

        public static void Intersect(WorkingModel first, WorkingModel second, List<TrianglePair> pairs)
        {
            first.Intersect(second, pairs, false);
            second.Intersect(first, pairs, true);
            first.FindCuts();
            second.FindCuts();
        }

        // my edges versus the flat of other model's triangles
        public void Intersect(WorkingModel other, List<TrianglePair> pairs, bool reversed)
        {
            List<WorkingEdge> edges = new List<WorkingEdge>();
            List<int> triangleToEdgeIndex = new List<int>();
            foreach (WorkingTriangle tri in triangles)
                for (int i = 0; i < 3; i++)
                {
                    int index = edges.FindIndex((edge) => edge.Equals(tri.edges[i]));
                    if (index == -1)
                    {
                        triangleToEdgeIndex.Add(edges.Count);
                        edges.Add(tri.edges[i]);
                    }
                    else
                        triangleToEdgeIndex.Add(index);
                }

            List<int[]> pairsAlreadyHandled = new List<int[]>();

            foreach (TrianglePair pair in pairs)
            {
                int a = originalTriangleIndices.IndexOf(reversed ? pair.b : pair.a);
                int b = other.originalTriangleIndices.IndexOf(reversed ? pair.a : pair.b);
                WorkingTriangle aTri = triangles[a];
                WorkingTriangle bTri = other.triangles[b];
                for (int i = 0; i < 3; i++)
                {
                    int edgeIndex = a * 3 + i;
                    int[] matchup = new int[] { edgeIndex, b };
                    if (!pairsAlreadyHandled.Exists((item) => item[0] == matchup[0] && item[1] == matchup[1]))
                    {
                        aTri.edges[i].Intersect(bTri);
                        pairsAlreadyHandled.Add(matchup);
                    }
                }
            }
        }

        public void FindCuts()
        {
            cutTriangles = new List<CutTriangle>();
            foreach (WorkingTriangle tri in triangles)
                cutTriangles.Add(new CutTriangle(tri));
        }

        public List<BasicModelVert> GetBMVList(BasicModelData model, Mat4 xform)
        {
            List<BasicModelVert> list = new List<BasicModelVert>();
            foreach (CutTriangle tri in cutTriangles)
            {
                List<WorkingVertex> triVerts = tri.GetCutTriangleVerts();
                foreach(WorkingVertex vert in triVerts)
                    list.Add(CSG.WorkingVertexToBMV(vert.position.xyz, vert.vinfo, model, xform, this));
            }
            return list;
        }

        public List<Vec3[]> GetCutEdgesList()
        {
            List<Vec3[]> results = new List<Vec3[]>();
            foreach (CutTriangle tri in cutTriangles)
                foreach (int[] edge in tri.slicedEdges)
                    results.Add(new Vec3[] { tri.allVerts[edge[0]].position.xyz, tri.allVerts[edge[1]].position.xyz });
            return results;
        }
    }
}
