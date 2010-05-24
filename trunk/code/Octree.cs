using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu
{
    public class Octree
    {
        public AABB bounds;
        public Vec3 split;
        public List<Item> items;
        public Octree[, ,] children = null;

        public Octree()
        {
            items = new List<Item>();
        }

        public Octree(AABB bounds)
        {
            this.bounds = bounds;
            items = new List<Item>();
        }

        // Inserts an item into the category-th list (possibly of children, based on the item's bounding box)
        public void InsertItem(Item item)
        {
            if (AABB.CheckIntersection(item.aabb, bounds))
            {
                items.Add(item);

                if (children != null)
                    foreach (Octree grid in children)
                        grid.InsertItem(item);
                else if (ConditionallySubdivide())
                    Subdivide();
                
            }
        }

        // Subdivides this PartitioningGrid, splitting it exactly in half
        public void Subdivide()
        {
            Subdivide(SplitPoint());
        }

        protected virtual Vec3 SplitPoint()
        {
            return CenterPoint();
        }
        public Vec3 CenterPoint()
        {
            return new Vec3 { x = bounds.array[0][0] + bounds.array[0][1], y = bounds.array[1][0] + bounds.array[1][1], z = bounds.array[2][0] + bounds.array[2][1] } * 0.5;
        }

        // Subdivides this PartitioningGrid, splitting it at the specified point (which ought to be insidwe this grid's AABB)
        public void Subdivide(Vec3 split)
        {
            this.split = split;
            children = new Octree[2, 2, 2];
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
                        children[i, j, k] = Subdivision(new AABB
                        {
                            array = new double[][] {
                                new double[] { array[0][i], array[0][i + 1] },
                                new double[] { array[1][j], array[1][j + 1] }, 
                                new double[] { array[2][k], array[2][k + 1] } }
                        });
                    }
            foreach (Octree child in children)
                foreach (Item item in items)
                    child.InsertItem(item);
        }

        // Utility function to figure out how well a subdivision point splits stuff
        public int[,,] CheckSubdivision(Vec3 split)
        {
            int[, ,] results = new int[2, 2, 2];
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
                        AABB subregion = new AABB { array = new double[][] { new double[] { array[0][i], array[0][i + 1] }, new double[] { array[1][j], array[1][j + 1] }, new double[] { array[2][k], array[2][k + 1] } } };
                        foreach (Item item in items)
                            if (AABB.CheckIntersection(item.aabb, subregion))
                                results[i, j, k]++;
                    }
            return results;
        }

        public static int RateSubdivision(int[, ,] checkValues)
        {
            int total = 0;
            foreach (int i in checkValues)
                total += i * i;
            return total;
        }

        // Override in order to control whether an octree should be subdivided or not
        protected virtual bool ConditionallySubdivide() { return false; }

        // Function defining how to subdivide an Octree
        // Return values should generally be the same type as the subtype of Octree
        protected virtual Octree Subdivision(AABB aabb) { return new Octree(aabb); }

        // Anything that can be stuck in an Octree must inmplement this
        public struct Item
        {
            public AABB aabb;
            public object obj;
        }
    }
}
