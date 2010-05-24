using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu
{
    public class ModelIntersectTree
    {
        public AABB bounds;
        public Vec3 split;
        public List<Octree.Item>[] items;
        public ModelIntersectTree[, ,] children = null;
        public int max_subdivisions;

        public List<long>[] allItems = new List<long>[] { new List<long>(), new List<long>() };

        // Boring constructor
        // Just calls BoundsInit with the same arguments you gave the constructor
        public ModelIntersectTree(AABB bounds, int max_subdivisions) { BoundsInit(bounds, max_subdivisions); }

        // Interesting constructor
        // Does the populating of the tree and whatnot automatically, given the objects to populate it with
        public ModelIntersectTree(CSGModel a, CSGModel b)
        {
            AABB a_box = a.ComputeAABB();
            AABB b_box = b.ComputeAABB();

            if (!AABB.CheckIntersection(a_box, b_box))
                return;

            AABB intersection = AABB.Intersection(a_box, b_box);
            BoundsInit(intersection, 5);

            foreach (Octree.Item item in a.GetSourceTriangleOctreeData())
                if (item.obj is CSGSourceTriangle)
                {
                    allItems[0].Add((item.obj as CSGSourceTriangle).id);
                    InsertItem(0, item);
                }
            foreach (Octree.Item item in b.GetSourceTriangleOctreeData())
                if (item.obj is CSGSourceTriangle)
                {
                    allItems[1].Add((item.obj as CSGSourceTriangle).id);
                    InsertItem(1, item);
                }
        }

        protected virtual void BoundsInit(AABB bounds, int max_subdivisions)
        {
            this.bounds = bounds;
            this.max_subdivisions = max_subdivisions;
            items = new List<Octree.Item>[2];
            for (int i = 0; i < 2; i++)
                items[i] = new List<Octree.Item>();
        }

        // Inserts an item into the category-th list (possibly of children, based on the item's bounding box)
        public void InsertItem(int category, Octree.Item item)
        {
            if (AABB.CheckIntersection(item.aabb, bounds))
            {
                items[category].Add(item);

                if (children != null)
                    foreach (ModelIntersectTree grid in children)
                        grid.InsertItem(category, item);
                if (children == null && max_subdivisions > 0)
                    Subdivide();
            }
        }

        // Subdivides this ModelIntersectTree, splitting it exactly in half
        public void Subdivide()
        {
            Subdivide(new Vec3 { x = bounds.array[0][0] + bounds.array[0][1], y = bounds.array[1][0] + bounds.array[1][1], z = bounds.array[2][0] + bounds.array[2][1] } * 0.5);
        }
        // Subdivides this ModelIntersectTree, splitting it at the specified point (which ought to be insidwe this grid's AABB)
        public void Subdivide(Vec3 split)
        {
            this.split = split;
            int max_child_subd = max_subdivisions - 1;
            children = new ModelIntersectTree[2, 2, 2];
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
                        children[i, j, k] = new ModelIntersectTree(new AABB
                        {
                            array = new double[][] {
                                new double[] { array[0][i], array[0][i + 1] },
                                new double[] { array[1][j], array[1][j + 1] }, 
                                new double[] { array[2][k], array[2][k + 1] } }
                        }, max_child_subd);
                    }
            foreach (ModelIntersectTree child in children)
            {
                foreach (Octree.Item item in items[0])
                    child.InsertItem(0, item);
                foreach (Octree.Item item in items[1])
                    child.InsertItem(1, item);
            }
        }

        // Gets all unique pairs of items within this grid, or if this grid has children, from its children
        // Child grids will have better precision, so using the child grids instead of the parent might eliminate some bogus combinations
        // The results go into the result bool[,] array, which must have the proper dimensions initially
        protected virtual void GetLeafLevelPairs(SparseBoolMatrix result)
        {
            if (children == null)
            {
                foreach (Octree.Item i in items[0])
                    if (i.obj is CSGSourceTriangle)
                        foreach (Octree.Item j in items[1])
                            if (j.obj is CSGSourceTriangle)
                                result[(i.obj as CSGSourceTriangle).id, (j.obj as CSGSourceTriangle).id] = true;
            }
            else
            {
                foreach (ModelIntersectTree grid in children)
                    grid.GetLeafLevelPairs(result);
            }
        }
        // Top-level version of the above function, determines the array dimensions automatically and returns the array
        protected virtual SparseBoolMatrix GetLeafLevelPairs()
        {
            SparseBoolMatrix result = new SparseBoolMatrix();
            GetLeafLevelPairs(result);
            return result;
        }

        // Iterates through all the pairs and runs your callback function for them
        public void ForLeafLevelPairs(SparseBoolMatrix.PairCallback callback)
        {
            SparseBoolMatrix pairs = GetLeafLevelPairs();
            pairs.ForAllPairs(callback);
        }

        // Lets you define how to handle triangles which aren't in the intersection zone
        // whichModel is either 0 or 1 and tells you which of the two operands the safe triangle is from
        // triangleID is the id member of a CSGSourceTriangle in that model
        public delegate void SafeTriangleCallback(int whichModel, long triangleID);

        // Function to find out all of the possibly intersecting pairs of triangles, and also to find out which triangles are not possibly intersecting anything
        public static void CullIntersections(CSGModel a, CSGModel b, SparseBoolMatrix.PairCallback pairCallback, SafeTriangleCallback safeCallback)
        {
            ModelIntersectTree tree = new ModelIntersectTree(a, b);             // create the tree (operations are done automatically by the constructor

            // make lists of all the triangle indices in each of the input objects
            HashSet<long> aSafe = new HashSet<long>();
            HashSet<long> bSafe = new HashSet<long>();
            foreach (long i in tree.allItems[0])
                aSafe.Add(i);
            foreach (long j in tree.allItems[1])
                bSafe.Add(j);

            // iterate through all of the pairs
            tree.ForLeafLevelPairs(
                (i, j) =>
                {
                    aSafe.Remove(i);                                            // if an item is in a pair with another item, it isn't safe, so we remove it from the safe list
                    bSafe.Remove(j);

                    pairCallback(i, j);                                         // call the user-defined callback
                });

            foreach (long i in aSafe)                                           // let the user know which ones were determined to be 'safe'
                safeCallback(0, i);
            foreach (long j in bSafe)
                safeCallback(1, j);
        }

        // Similar, but without the part for finding out all the stuff stuff that's safe
        public static void CullIntersections(CSGModel a, CSGModel b, SparseBoolMatrix.PairCallback pairCallback)
        {
            ModelIntersectTree tree = new ModelIntersectTree(a, b);             // create the tree (operations are done automatically by the constructor
            tree.ForLeafLevelPairs(pairCallback);                               // iterate through all of the pairs and call the user-defined callback
        }
    }
}
