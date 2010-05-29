using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;
using Modelthulhu.UserModel;

namespace Modelthulhu.Triangulated
{
    public class PreResultModel
    {
        protected List<PreResultTriangle> triangles = new List<PreResultTriangle>();

        protected List<Vec3> uniqueVerts = new List<Vec3>();
        protected List<int[]> prohibitedEdges = new List<int[]>();

        public PreResultModel()
        {
        }

        // Function to add a list of triangles, given the triangles' verts (grouped by 3's)
        public void AddTriangles(List<BasicModelVert> verts)
        {
            if (verts.Count == 0)
                return;

            for (int i = 0; i < verts.Count; i += 3)
            {
                PreResultTriangle tri = new PreResultTriangle();
                for (int j = 0; j < 3; j++)
                {
                    tri.verts[j] = verts[i + j];
                    tri.v_indices[j] = AddVertex(tri.verts[j].position);
                }

                tri.Prepare();
                tri.id = triangles.Count;
                List<int[]> edges = tri.GetEdges();

                for(int j = 0; j < 3; j++)
                    foreach (PreResultTriangle other in triangles)
                    {
                        int index = other.IndexOfEdge(edges[j]);
                        if(index != -1)
                        {
                            other.edge_neighbors[index] = tri.id;
                            tri.edge_neighbors[j] = other.id;
                        }
                    }

                triangles.Add(tri);
            }
        }

        // adds the specified vert if it is not yet present, and returns the new vert's index
        // otherwise returns the index of the existing vert it's a duplicate of
        public int AddVertex(Vec3 vert)
        {
            int index = uniqueVerts.FindIndex((v) => (v - vert).ComputeMagnitudeSquared() < 0.0000000001);
            if (index == -1)
            {
                uniqueVerts.Add(vert);
                return uniqueVerts.Count - 1;
            }
            else
                return index;
        }

        public void ProhibitEdges(List<Vec3[]> edgeList)
        {
            foreach (Vec3[] edge in edgeList)
            {
                int[] vertex_indices = new int[] { AddVertex(edge[0]), AddVertex(edge[1]) };
                prohibitedEdges.Add(vertex_indices);
            }
        }

        protected List<List<int>> DivideIntoRegions()
        {
            foreach (int[] edge in prohibitedEdges)
                foreach (PreResultTriangle tri in triangles)
                    tri.ProhibitEdge(edge);

            int numTriangles = triangles.Count;

            List<int> unassigned = new List<int>();
            for (int i = 0; i < numTriangles; i++)
            {
                unassigned.Add(i);
                triangles[i].Prepare();
            }

            List<List<int>> regions = new List<List<int>>();
            while (unassigned.Count > 0)
            {
                List<int> currentRegion = new List<int>();
                List<int> addition = new List<int>();

                int selection = unassigned[0];

                currentRegion.Add(selection);
                addition.Add(selection);
                unassigned.RemoveAt(0);

                do
                {
                    List<int> lastAddition = addition;
                    addition = new List<int>();

                    foreach (int testSpread in unassigned)
                        foreach (int spreader in lastAddition)
                            if (triangles[spreader].GetValidNeighbors().Contains(testSpread) && !addition.Contains(testSpread))
                                addition.Add(testSpread);

                    unassigned.RemoveAll((index) => addition.Contains(index));
                    currentRegion.AddRange(addition);
                } while (addition.Count != 0);

                regions.Add(currentRegion);
            }

            return regions;
        }

        public List<BasicModelVert> Trim(RegionKeepCondition keepCondition)      // hypothetically, only one of keepInside or keepOutside should be true
        {
            List<List<int>> regions = DivideIntoRegions();
            int numRegions = regions.Count;

            List<BasicModelVert> result = new List<BasicModelVert>();

            RegionBehavior[] behaviors = new RegionBehavior[numRegions];
            for (int i = 0; i < numRegions; i++)
            {
                List<PreResultTriangle> regionTriangles = new List<PreResultTriangle>();
                foreach(int j in regions[i])
                    regionTriangles.Add(triangles[j]);

                RegionBehavior behavior = behaviors[i] = keepCondition(regionTriangles);
                if (behavior != RegionBehavior.Delete)
                {
                    if (behavior == RegionBehavior.Normal)
                    {
                        foreach (int index in regions[i])
                            result.AddRange(triangles[index].verts);
                    }
                    else
                    {
                        int[] winding = new int[] {2, 1, 0 };
                        int numTriangles = regions[i].Count;
                        for (int j = 0; j < numTriangles; j++)
                        {
                            int index = regions[i][j];
                            foreach(int k in winding)
                            {
                                BasicModelVert vert = triangles[regions[i][j]].verts[k];
                                vert.normal = -vert.normal;
                                result.Add(vert);
                            }
                        }
                    }
                }
            }
           
            return result;
        }
    }
}