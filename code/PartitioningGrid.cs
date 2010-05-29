using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;
using Modelthulhu.Geom;

namespace Modelthulhu.Triangulated
{
    public class PartitioningGrid
    {
        public AABB bounds;
        public Vec3 split;
        public List<int>[] items;
        public PartitioningGrid[, ,] children = null;
        public int max_subdivisions;

        public int[] max_encountered_item = new int[2];

        public PartitioningGrid(AABB bounds, int max_subdivisions)
        {
            this.bounds = bounds;
            this.max_subdivisions = max_subdivisions;
            items = new List<int>[2];
            for(int i = 0; i < 2; i++)
                items[i] = new List<int>();
        }

        // Inserts an item into the category-th list (possibly of children, based on the item's bounding box)
        public void InsertItem(int category, int item, AABB box)
        {
            if(AABB.CheckIntersection(box, bounds))
            {
                items[category].Add(item);

                if (children == null && max_subdivisions > 0)
                    Subdivide();
                if (children != null)
                    foreach (PartitioningGrid grid in children)
                        grid.InsertItem(category, item, box);
            }
            max_encountered_item[category] = Math.Max(max_encountered_item[category], item);
        }

        // Subdivides this PartitioningGrid, splitting it exactly in half
        public void Subdivide()
        {
            Subdivide( new Vec3 { x = bounds.array[0][0] + bounds.array[0][1], y = bounds.array[1][0] + bounds.array[1][1], z = bounds.array[2][0] + bounds.array[2][1] } * 0.5);
        }
        // Subdivides this PartitioningGrid, splitting it at the specified point (which ought to be insidwe this grid's AABB)
        public void Subdivide(Vec3 split)
        {
            this.split = split;
            int max_child_subd = max_subdivisions - 1;
            children = new PartitioningGrid[2, 2, 2];
            double[][] array = new double[][] 
            { 
                new double[] { bounds.array[0][0], split.x, bounds.array[0][1] },
                new double[] { bounds.array[1][0], split.y, bounds.array[1][1] }, 
                new double[] { bounds.array[2][0], split.z, bounds.array[2][1] } 
            };
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        children[i, j, k] = new PartitioningGrid(new AABB
                        {
                            array = new double[][] {
                                new double[] { array[0][i], array[0][i + 1] },
                                new double[] { array[1][j], array[1][j + 1] }, 
                                new double[] { array[2][k], array[2][k + 1] } }
                        }, max_child_subd);
                    }
        }

        // Gets all unique pairs of items within this grid, or if this grid has children, from its children
        // Child grids will have better precision, so using the child grids instead of the parent might eliminate some bogus combinations
        // The results go into the result bool[,] array, which must have the proper dimensions initially
        protected virtual void GetLeafLevelPairs(bool[,] result)
        {
            if (children == null)
                foreach (int i in items[0])
                    foreach (int j in items[1])
                        result[i, j] = true;
            else
            {
                // if there are children, there could be duplicate pairs; that's bad!
                List<int[]> repeats = new List<int[]>();
                foreach (PartitioningGrid grid in children)
                    grid.GetLeafLevelPairs(result);
            }
        }
        // Top-level version of the above function, determines the array dimensions automatically and returns the array
        protected virtual bool[,] GetLeafLevelPairs()
        {
            bool[,] result = new bool[max_encountered_item[0] + 1, max_encountered_item[1] + 1];
            GetLeafLevelPairs(result);
            return result;
        }

        // Delegate function... let's you do something with a pair of items
        public delegate void PairCallback(int first, int second);

        // Iterates through all the pairs and runs your callback function for them
        public void ForLeafLevelPairs(PairCallback callback)
        {
            bool[,] pairs = GetLeafLevelPairs();
            int w = pairs.GetLength(0), h = pairs.GetLength(1);

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (pairs[x, y])
                        callback(x, y);
        }
    }
}
