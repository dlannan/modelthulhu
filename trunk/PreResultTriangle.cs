using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;

namespace TheLibrary.CSG
{
    public class PreResultTriangle
    {
        public BasicModelVert[] verts = new BasicModelVert[3];
        public Vec3[] vertexPositions = new Vec3[3];
        public int[] v_indices = new int[3];
        public int[] edge_neighbors = new int[3];
        public int id;

        public void Prepare()
        {
            vertexPositions[0] = verts[0].position;
            vertexPositions[1] = verts[1].position;
            vertexPositions[2] = verts[2].position;
        }

        public List<int> GetValidNeighbors()
        {
            List<int> result = new List<int>();
            foreach (int n in edge_neighbors)
                if (n != -1)
                    result.Add(n);
            return result;
        }

        public List<int[]> GetEdges()
        {
            return new List<int[]>(new int[][] { new int[] { v_indices[0], v_indices[1] }, new int[] { v_indices[1], v_indices[2] }, new int[] { v_indices[2], v_indices[0] } });
        }

        public int IndexOfEdge(int[] edgeVerts)
        {
            for (int i = 0; i < 3; i++)
            {
                int[][] edgeOrders = new int[][] { new int[] { v_indices[i], v_indices[(i + 1) % 3] }, new int[] { v_indices[(i + 1) % 3], v_indices[i] } };
                foreach (int[] order in edgeOrders)
                    if (order[0] == edgeVerts[0] && order[1] == edgeVerts[1])
                        return i;
            }
            return -1;
        }

        public bool ProhibitEdge(int[] edgeVerts)
        {
            int index = IndexOfEdge(edgeVerts);
            if (index == -1)
                return false;
            else
            {
                edge_neighbors[index] = -1;
                return true;
            }
        }

        public Vec3 GetCenterPoint()
        {
            return (vertexPositions[0] + vertexPositions[1] + vertexPositions[2]) / 3.0;
        }
    }
}
