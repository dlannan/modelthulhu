using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modelthulhu
{
    public struct AABB
    {
        // first index is x/y/z, second index is min/max
        public double[][] array;

        // Gets a default (all zeros) array with the proper dimensions
        public static double[][] EmptyBoundsArray { get { return new double[][] { new double[2], new double[2], new double[2] }; } }

        // Creates an AABB with both the min and max bounds set to the specified point
        public static AABB PointBB(Vec3 point)
        {
            return new AABB { array = new double[][] { new double[] { point.x, point.x }, new double[] { point.y, point.y }, new double[] { point.z, point.z } } };
        }
        // Creates an AABB with the specified minimum and maximum bounds
        public static AABB FromMinMax(Vec3 min, Vec3 max)
        {
            return new AABB { array = new double[][] { new double[] { min.x, max.x }, new double[] { min.y, max.y }, new double[] { min.z, max.z } } };
        }

        // Returns an AABB expanded from the input AABB as necessary in order to include the specified point
        public static AABB ExpandedToFit(AABB input, Vec3 point)
        {
            AABB result = new AABB { array = EmptyBoundsArray };
            double[] point_array = new double[] { point.x, point.y, point.z };
            for (int dim = 0; dim < 3; dim++)
            {
                result.array[dim][0] = Math.Min(input.array[dim][0], point_array[dim]);
                result.array[dim][1] = Math.Max(input.array[dim][1], point_array[dim]);
            }
            return result;
        }

        // Computes and returns an AABB encompassing the volumes of both input boxes
        public static AABB Union(AABB first, AABB second)
        {
            AABB result = new AABB { array = EmptyBoundsArray };
            for (int dim = 0; dim < 3; dim++)
            {
                result.array[dim][0] = Math.Min(first.array[dim][0], second.array[dim][0]);
                result.array[dim][1] = Math.Max(first.array[dim][1], second.array[dim][1]);
            }
            return result;
        }

        // Computes and returns an AABB encompassing the intersection of the two input boxes' volumes
        // Don't use this value if CheckIntersection failed!
        public static AABB Intersection(AABB first, AABB second)
        {
            AABB result = new AABB { array = EmptyBoundsArray };
            for (int dim = 0; dim < 3; dim++)
            {
                result.array[dim][0] = Math.Max(first.array[dim][0], second.array[dim][0]);
                result.array[dim][1] = Math.Min(first.array[dim][1], second.array[dim][1]);
            }
            return result;
        }

        // Returns true if the two AABBs intersect, false otherwise
        public static bool CheckIntersection(AABB first, AABB second)
        {
            for (int dim = 0; dim < 3; dim++)
                if (first.array[dim][1] < second.array[dim][0] || first.array[dim][0] > second.array[dim][1])
                    return false;
            return true;
        }

        // Makes an AABB to fit the specified list of points
        public static AABB FitPointList(List<Vec3> points)
        {
            double[][] bounds = EmptyBoundsArray;
            Vec3 first = points[0];
            bounds[0][0] = bounds[0][1] = first.x;
            bounds[1][0] = bounds[1][1] = first.y;
            bounds[2][0] = bounds[2][1] = first.z;
            for (int i = 1; i < points.Count; i++)
            {
                Vec3 point = points[i];
                bounds[0][0] = Math.Min(bounds[0][0], point.x);
                bounds[0][1] = Math.Max(bounds[0][1], point.x);
                bounds[1][0] = Math.Min(bounds[1][0], point.y);
                bounds[1][1] = Math.Max(bounds[1][1], point.y);
                bounds[2][0] = Math.Min(bounds[2][0], point.z);
                bounds[2][1] = Math.Max(bounds[2][1], point.z);
            }
            return new AABB { array = bounds };
        }
    }
}
