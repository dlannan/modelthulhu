using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;
using TheLibrary.Graphics;

namespace TheLibrary.CSG
{
    // Input type for CSG operations
    public class ModelInput
    {
        private Vec3[] verts;           // the vertices

        private int[,] e_v;             // the 2 vertices of an edge
        private int[,] e_t;             // the 2 triangles separated by an edge

        private int[,] t_v;             // the 3 vertices of a triangle
        private int[,] t_e;             // the 3 edges of a triangle

        // Makes a ModelInput object
        // Verts is a list of all the vertices of the model, triangle_vertex_indices contains, for each triangle (first []) the indices of the 3 vertices (second [])
        public ModelInput(Vec3[] verts, int[][] triangle_vertex_indices)
        {
            int num_verts = verts.Length;
            // copy verts
            this.verts = new Vec3[num_verts];
            for (int i = 0; i < num_verts; i++)
                this.verts[i] = verts[i];

            int num_tris = triangle_vertex_indices.Length;
            // go from [][] to [,] array format
            t_v = new int[num_tris, 3];
            t_e = new int[num_tris, 3];
            for (int i = 0; i < num_tris; i++)
                for (int j = 0; j < 3; j++)
                    t_v[i, j] = triangle_vertex_indices[i][j];

            // find what pairs of verts have edges between 'em
            bool[,] edge_existence = new bool[num_verts, num_verts];
            int[,] edge_index = new int[num_verts, num_verts];
            for (int i = 0; i < num_tris; i++)
                for (int j = 0; j < 3; j++)
                {
                    int x = t_v[i, j], y = t_v[i, (j + 1) % 3];
                    edge_existence[x, y] = edge_existence[y, x] = true;
                }
            // assign each edge an index
            int next_index = 0;
            for (int i = 0; i < num_verts; i++)
                for (int j = i + 1; j < num_verts; j++)
                    if (edge_existence[i, j])
                        edge_index[i, j] = edge_index[j, i] = next_index++;

            int num_edges = next_index;
            // find out the indices of the two endpoints of each edge
            e_t = new int[num_edges, 2];
            e_v = new int[num_edges, 2];
            for (int i = 0; i < num_verts; i++)
                for (int j = i + 1; j < num_verts; j++)
                    if (edge_existence[i, j])
                    {
                        int index = edge_index[i, j];
                        e_v[index, 0] = i;
                        e_v[index, 1] = j;
                        e_t[index, 0] = e_t[index, 1] = -1;
                    }
            // find out which edges separate which triangles
            for (int i = 0; i < num_tris; i++)
                for (int j = 0; j < 3; j++)
                {
                    int index = t_e[i, j] = edge_index[t_v[i, j], t_v[i, (j + 1) % 3]];
                    if (e_t[index, 0] == -1)
                        e_t[index, 0] = i;
                    else
                        e_t[index, 1] = i;
                }
        }

        // Iterate through the vertex- and edge- neighbors of a triangle
        // Does not include any repeats, and does not include the input triangle itself
        protected IEnumerable<int> TriangleNeighbors(int tri)
        {
            List<int> found = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                int vert = t_v[tri, i];
                for (int j = 0; j < e_v.GetLength(0); j++)
                    if (e_v[j, 0] == vert || e_v[j, 1] == vert)
                    {
                        found.Add(e_t[j, 0]);
                        found.Add(e_t[j, 1]);
                    }
            }

            List<int> repeats = new List<int>(new int[] { tri });
            foreach (int index in found)
            {
                if (repeats.Contains(index))
                    continue;
                repeats.Add(index);
                yield return index;
            }
        }

        // Makes a ModelInput object from a BasicModelData object (with verts and normals transformed by the 4x4 xform matrix)
        public static ModelInput FromBasicModelData(BasicModelData data, Mat4 xform)
        {
            List<Vec3> verts = new List<Vec3>();

            List<int> vertMapping = new List<int>();
            for (int i = 0; i < data.x.Length; i++)
            {
                Vec3 vert = xform.TransformVec3(new Vec3 { x = data.x[i], y = data.y[i], z = data.z[i] }, 1.0);
                int index = verts.FindIndex(vec => vec.x == vert.x && vec.y == vert.y && vec.z == vert.z);
                if (index == -1)
                {
                    vertMapping.Add(verts.Count);
                    verts.Add(vert);
                }
                else
                    vertMapping.Add(index);
            }

            int[][] t_v_indices = new int[data.a_vert.Length][];
            for (int i = 0; i < data.a_vert.Length; i++)
            {
                int a = vertMapping[(int)data.a_vert[i]];
                int b = vertMapping[(int)data.b_vert[i]];
                int c = vertMapping[(int)data.c_vert[i]];
                t_v_indices[i] = new int[] { a, b, c };
            }

            return new ModelInput(verts.ToArray(), t_v_indices);
        }

        // Minimum and maximimum x/y/z of the model's vertices (ergo, the axis-aligned bounding box) 
        private AABB? aabb;
        // Force (re)computation of the AABB's dimensions
        public void ComputeAABB()
        {
            aabb = AABB.FitPointList(new List<Vec3>(verts));
        }
        // Get the minimum/maximum corner's position vector
        public AABB AABB { get { if (!aabb.HasValue) ComputeAABB(); return aabb.Value; } }

        // Populates the provided PartioningGrid with items (the triangle indices of this triangle) in the specified category
        // The category should be either 0 or 1, as it is used as an array index and will indicate which of the two objects this triangle goes with
        public void PopulateGrid(PartitioningGrid grid, int category)
        {
            int num_triangles = t_v.GetLength(0);
            for (int i = 0; i < num_triangles; i++)
                grid.InsertItem(category, i, AABB.FitPointList(new List<Vec3>(new Vec3[] { verts[t_v[i, 0]], verts[t_v[i, 1]], verts[t_v[i, 2]] })));
        }

        public static void IntersectTest(int a_tri, ModelInput a_obj, int b_tri, ModelInput b_obj, out Vec2[,] intersections, out Vec3[] a_verts, out Vec3[] b_verts)
        {
            a_verts = new Vec3[] { a_obj.verts[a_obj.t_v[a_tri, 0]], a_obj.verts[a_obj.t_v[a_tri, 1]], a_obj.verts[a_obj.t_v[a_tri, 2]] };
            b_verts = new Vec3[] { b_obj.verts[b_obj.t_v[b_tri, 0]], b_obj.verts[b_obj.t_v[b_tri, 1]], b_obj.verts[b_obj.t_v[b_tri, 2]] };

            intersections = Util.TriangleTriangleIntersection(a_verts, b_verts);
        }

        public bool IsPointInside(Vec3 point)
        {
            int crossings = 0;
            
            int num_triangles = t_v.GetLength(0);
            for (int i = 0; i < num_triangles; i++)
            {
                Vec3[] triangle_verts = new Vec3[] { verts[t_v[i, 0]], verts[t_v[i, 1]], verts[t_v[i, 2]] };
                double a, b, c;
                Vec3 d;
                if (Util.RayTriangleIntersect(point, new Vec3 { z = 1 }, triangle_verts[0], triangle_verts[1], triangle_verts[2], out a, out d, out b, out c))
                    if (a > 0)
                        crossings++;
            }

            return crossings % 2 == 1;
        }

        public WorkingModel ToWorkingModel(int id, List<int> keptTriangles)
        {
            WorkingModel model = new WorkingModel();
            model.objID = id;
            List<int> vertexIndices = new List<int>();
            List<int> useVerts = new List<int>();
            // get all the triangles and their vertices
            foreach (int triangleIndex in keptTriangles)
            {
                WorkingTriangle triangle = new WorkingTriangle();
                triangle.objID = id;
                triangle.triID = triangleIndex;
                for (int i = 0; i < 3; i++)
                {
                    int vi = t_v[triangleIndex, i];
                    int useIndex = vertexIndices.IndexOf(vi);
                    VertexPosition vpos;
                    if (useIndex == -1)
                    {
                        useVerts.Add(vertexIndices.Count);
                        vertexIndices.Add(vi);
                        Vec3 pos = verts[vi];
                        vpos = new VertexPosition { xyz = new Vec3 { x = pos.x, y = pos.y, z = pos.z } };
                        model.vertexPositions.Add(vpos);
                    }
                    else
                    {
                        useVerts.Add(useIndex);
                        Vec3 pos = verts[vi];
                        vpos = model.vertexPositions[useIndex];
                    }
                    WorkingVertex wvert = new WorkingVertex();
                    wvert.position = vpos;
                    wvert.vinfo[0] = new VInfoReference { objID = id, index = triangleIndex * 3 + i, weight = 1.0 };
                    wvert.vinfo[1] = new VInfoReference { objID = id, index = -1, weight = 0.0 };
                    wvert.vinfo[2] = new VInfoReference { objID = id, index = -1, weight = 0.0 };
                    triangle.verts[i] = wvert;
                }
                model.triangles.Add(triangle);
                model.originalTriangleIndices.Add(triangleIndex);
            }
            // now get all the edges between them
            foreach (WorkingTriangle tri in model.triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    // if a triangle hasn't got an edge, give it one, and find the matching triangle
                    if (tri.edges[i] == null)
                    {
                        WorkingEdge edge = tri.edges[i] = new WorkingEdge();
                        edge.verts[0] = tri.verts[i];
                        edge.verts[1] = tri.verts[(i + 1) % 3];
                        edge.triangles[0] = tri;
                        foreach(WorkingTriangle other in model.triangles)
                        {
                            if(other != tri)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    // if another triangle has an edge, it's got a matching triangle, and our triangle can't be their match
                                    if (other.edges[j] != null)
                                        continue;
                                    if ((other.verts[j] == edge.verts[0] && other.verts[(j + 1) % 3] == edge.verts[1]) || (other.verts[j] == edge.verts[1] && other.verts[(j + 1) % 3] == edge.verts[0]))
                                    {
                                        edge.triangles[1] = other;
                                        other.edges[j] = edge;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return model;
        }
    }
}
