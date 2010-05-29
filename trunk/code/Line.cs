using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;

namespace Modelthulhu.Geom
{
    // Struct representing a line in 3D space
    public struct Line
    {
        public Vec3 origin;             // any point on the line
        public Vec3 direction;          // any nonzero vector along the line

        // Checks whether two lines are equal... returns a value, closer to zero = closer to being equal
        public static double CheckEquality(Line a, Line b)
        {
            double magprodsq = a.direction.ComputeMagnitudeSquared() * b.direction.ComputeMagnitudeSquared();
            double dot = Vec3.Dot(a.direction, b.direction);
            double aparallelness = 1.0 - Math.Sqrt((dot * dot) / magprodsq);                    // closer to parallel yields smaller values of this
            aparallelness *= aparallelness;

            Plane plane_a = Plane.FromPositionNormal(a.origin, a.direction);
            Plane plane_b = Plane.FromPositionNormal(b.origin, b.direction);
            double a_from_b = plane_b.PointDistance(a.origin);
            double b_from_a = plane_a.PointDistance(b.origin);
            Vec3 a_on_b = a.origin + plane_a.normal * a_from_b;
            Vec3 b_on_a = b.origin + plane_b.normal * b_from_a;
            double distsq1 = (a_on_b - b.origin).ComputeMagnitudeSquared();                     // colinear --> same point
            double distsq2 = (b_on_a - a.origin).ComputeMagnitudeSquared();                     // colinear --> same point
            return aparallelness + distsq1 + distsq2;                                           // sum of 3 squared quantities... anything big --> big result
        }

        // Finds the intersection of the line and plane, and returns true if there is one
        // If there is no intersection, or the line is entirely within the plane, it returns false and the output position is the origin of the line
        public static bool IntersectPlane(Line line, Plane plane, out Vec3 pos)
        {
            Vec3 dir = Vec3.Normalize(line.direction);
            double dir_dot = Vec3.Dot(ref dir, ref plane.normal);
            if (dir_dot == 0.0)
            {
                pos = line.origin;
                return false;
            }
            else
            {
                double origin_dot = Vec3.Dot(ref line.origin, ref plane.normal);
                double tti = (plane.offset - origin_dot) / dir_dot;

                pos = line.origin + dir * tti;
                return true;
            }
        }
    }
}