using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;

namespace Modelthulhu.Geom
{
    // Struct representing a plane in 3D space
    public struct Plane
    {
        public Vec3 normal;             // Unit normal vector
        public double offset;           // Distance from the plane to the origin

        // Creates a plane with the specified normal, containing the specified position
        public static Plane FromPositionNormal(Vec3 pos, Vec3 normal)
        {
            Vec3 uNorm = Vec3.Normalize(normal);
            double dot = Vec3.Dot(pos, uNorm);
            return new Plane { normal = uNorm, offset = dot };
        }

        public static double CheckEquality(Plane a, Plane b)
        {
            double magprodsq = a.normal.ComputeMagnitudeSquared() * b.normal.ComputeMagnitudeSquared();     // although the magnitude of the normals SHOULD be 1... maybe somebody did something funky
            double dot = Vec3.Dot(a.normal, b.normal);
            double aparallelness = 1.0 - Math.Sqrt((dot * dot) / magprodsq);                                // closer to parallel yields smaller values of this
            aparallelness *= aparallelness;

            double a_from_b = b.PointDistance(a.normal * a.offset);
            double b_from_a = a.PointDistance(b.normal * b.offset);
            Vec3 a_on_b = a.normal * (a.offset + a_from_b);
            Vec3 b_on_a = b.normal * (b.offset + b_from_a);
            double distsq1 = (a_on_b - b.normal * b.offset).ComputeMagnitudeSquared();                      // coplanar --> same point
            double distsq2 = (b_on_a - a.normal * a.offset).ComputeMagnitudeSquared();                      // coplanar --> same point

            return aparallelness + distsq1 + distsq1;
        }

        // Returns distance of the point from the plane
        // It's signed, so one side has negative values
        public double PointDistance(Vec3 point)
        {
            return Vec3.Dot(normal, point) - offset;
        }

        // Creates a Plane object matching the plane of the specified triangle
        public static Plane FromTriangleVertices(Vec3 a, Vec3 b, Vec3 c)
        {
            Vec3 normal = Vec3.Cross(b - a, c - a);
            return FromPositionNormal(a, normal);
        }

        // Static function to find the intersection of two planes
        // If they are parallel, returns false and outputs an invalid line struct
        // Otherwise, returns true and outputs the line of intersection
        public static bool Intersect(Plane a, Plane b, out Line result)
        {
            Vec3 cross = Vec3.Cross(a.normal, b.normal);
            double magsq = cross.ComputeMagnitudeSquared();
            if (magsq == 0)
            {
                // failure! planes did not intersect, or planes were equal
                result = new Line { direction = Vec3.Zero, origin = Vec3.Zero };                // not a valid line!
                return false;
            }
            double invmag = 1.0 / Math.Sqrt(magsq);
            Vec3 line_direction = cross * invmag;
            // using plane a to find intersection (also could try b?)
            Vec3 in_a_toward_edge = Vec3.Normalize(Vec3.Cross(a.normal, line_direction));
            Vec3 point_in_a = a.normal * a.offset;
            double dist = b.PointDistance(point_in_a);
            // seems this number could be either the positive or negative of what we want...
            double unsigned_r = dist * invmag;
            Vec3 positive = point_in_a + in_a_toward_edge * unsigned_r;
            Vec3 negative = point_in_a - in_a_toward_edge * unsigned_r;
            // figure out which one is actually at the intersection (or closest to it)
            double positive_check = new Vec2 { x = a.PointDistance(positive), y = b.PointDistance(positive) }.ComputeMagnitudeSquared();
            double negative_check = new Vec2 { x = a.PointDistance(negative), y = b.PointDistance(negative) }.ComputeMagnitudeSquared();
            // and use that one as a point on the line (for the out value)
            Vec3 point_on_line;
            if (positive_check < negative_check)
                point_on_line = positive;
            else
                point_on_line = negative;
            // success! planes intersectedx
            result = new Line { origin = point_on_line, direction = line_direction };
            return true;
        }
    }
}
