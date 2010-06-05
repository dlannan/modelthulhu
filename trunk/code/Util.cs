using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;
using Modelthulhu.Geom;

namespace Modelthulhu
{
    // Collection of math utility functions
    public static class Util
    {
        // Note: result is number of times direction must be repeated to hit plane
        public static double RayPlaneIntersect(Vec3 rayOrigin, Vec3 rayDirection, Vec3 planeNormal, double planeOffset)
        {
            double dir_dot = Vec3.Dot(ref rayDirection, ref planeNormal);
            double origin_dot = Vec3.Dot(ref rayOrigin, ref planeNormal);
            return (planeOffset - origin_dot) / dir_dot;
        }

        // Return value is true if intersection occurs
        public static bool RayTriangleIntersect(Vec3 rayOrigin, Vec3 rayDirection, Vec3 a, Vec3 b, Vec3 c, out double timeToImpact, out Vec3 impactPosition, out double u, out double v)
        {
            Vec3 b_minus_a = b - a, c_minus_a = c - a;
            Vec3 normal = Vec3.Cross(b_minus_a, c_minus_a);
            normal = normal / normal.ComputeMagnitude();
            double offset = Vec3.Dot(ref normal, ref a);
            double hit = RayPlaneIntersect(rayOrigin, rayDirection, normal, offset);
            // check if it's real
            if (hit > 0 || hit <= 0)
            {
                timeToImpact = hit;
                impactPosition = rayOrigin + rayDirection * timeToImpact;
                double dist = Vec3.Dot(ref impactPosition, ref normal) - offset;            // should be 0, otherewise it's not on the plane at all!
                Vec3 relative = impactPosition - a;
                
                // given X = uA + vB, there should be some vectors P and Q
                // such that (P dot X) = u, and such that (Q dot X) = v
                // P and Q must be orthogonal to B and A, respectively, and lie in the plane of B and A
                Vec3 P = Vec3.Cross(ref c_minus_a, ref normal), Q = Vec3.Cross(ref b_minus_a, ref normal);
                u = Vec3.Dot(ref P, ref relative) / Vec3.Dot(ref P, ref b_minus_a);
                v = Vec3.Dot(ref Q, ref relative) / Vec3.Dot(ref Q, ref c_minus_a);
                if (u >= 0 && v >= 0 && u + v <= 1)
                    return true;
            }

            u = v = 0;
            timeToImpact = 0;
            impactPosition = Vec3.Zero;
            return false;
        }

        // Finds the intersection of two triangles
        // If the triangles are coplanar or on parallel planes, returns null
        // If there is an intersection, returns the A and B triangles' IJ coordinates of the endpoints of the intersecting line segment
        // The first indexer is which triangle, and the second is which of the two endpoints
        public static Vec2[,] TriangleTriangleIntersection(Vec3[] a_verts, Vec3[] b_verts)
        {
            Plane a_plane = Plane.FromTriangleVertices(a_verts[0], a_verts[1], a_verts[2]);
            Plane b_plane = Plane.FromTriangleVertices(b_verts[0], b_verts[1], b_verts[2]);

            Line line;
            if (!Plane.Intersect(a_plane, b_plane, out line))
                return null;

            Vec2 a_line_origin = VectorToTriangleCoords(a_verts, a_plane.normal, line.origin);
            Vec2 a_line_direction = VectorToTriangleCoords(a_verts, a_plane.normal, line.origin + line.direction) - a_line_origin;
            Vec2 b_line_origin = VectorToTriangleCoords(b_verts, b_plane.normal, line.origin);
            Vec2 b_line_direction = VectorToTriangleCoords(b_verts, b_plane.normal, line.origin + line.direction) - b_line_origin;

            double a_dot = Vec3.Dot(line.direction, a_plane.normal), b_dot = Vec3.Dot(line.direction, b_plane.normal);

            double a_min, a_max;
            if (!LineIntersectIJTriangle(a_line_origin, a_line_direction, out a_min, out a_max)) 
                return null;

            double b_min, b_max;
            if (!LineIntersectIJTriangle(b_line_origin, b_line_direction, out b_min, out b_max))
                return null;

            if (a_max < b_min || b_max < a_min)
                return null;
            double min = Math.Max(a_min, b_min), max = Math.Min(a_max, b_max);

            Vec2[,] result = new Vec2[2, 2];
            result[0, 0] = a_line_origin + a_line_direction * min;
            result[0, 1] = a_line_origin + a_line_direction * max;
            result[1, 0] = b_line_origin + b_line_direction * min;
            result[1, 1] = b_line_origin + b_line_direction * max;
            return result;
        }

        // Function to determine whether a 2D line intersects the triangle with vertices at (0,0), (1,0), and (0,1); returns true if they intersect, false otherwise
        // The output variables "enter" and "exit" are the number of times the direction vector must be repeated (starting at the line's origin) to reach the line's intersections with the triangle
        // If there is no interesection, both will be zero
        public static bool LineIntersectIJTriangle(Vec2 line_origin, Vec2 line_direction, out double enter, out double exit)
        {
            double[] edge_hits = new double[] { -line_origin.x / line_direction.x, -line_origin.y / line_direction.y, (1.0 - line_origin.x - line_origin.y) / (line_direction.x + line_direction.y) };
            List<double> hits = new List<double>();
            for (int i = 0; i < 3; i++)
            {
                double t = edge_hits[i];
                if (t >= 0 || t < 0)        // check that it's a real number
                {
                    Vec2 pos = line_origin + line_direction * t;
                    switch (i)
                    {
                        case 0:             // x is zero, check y
                            if (pos.y >= 0 && pos.y <= 1)
                                hits.Add(t);
                            break;
                        case 1:             // y is zero, check x
                            if (pos.x >= 0 && pos.x <= 1)
                                hits.Add(t);
                            break;
                        case 2:             // x + y is one, check x and y
                        default:
                            if (pos.x >= 0 && pos.y >= 0)
                                hits.Add(t);
                            break;
                    }
                }
            }

            if (hits.Count > 0)
            {
                double min = hits[0], max = min;
                for (int i = 1; i < hits.Count; i++)
                {
                    min = Math.Min(min, hits[i]);
                    max = Math.Max(max, hits[i]);
                }
                enter = min;
                exit = max;
                return true;
            }
            else
            {
                enter = exit = 0.0;
                return false;
            }
        }

        // Basically the same function as below, but it will auto-compute the normal vector
        // Use the other one if you're going to be using this on the same triangle repeatedly, so you can cache that value instead of recomputing it every time
        public static Vec2 VectorToTriangleCoords(Vec3[] tri_verts, Vec3 point_of_interest)
        {
            return VectorToTriangleCoords(tri_verts, Vec3.Normalize(Vec3.Cross(tri_verts[1] - tri_verts[0], tri_verts[2] - tri_verts[0])), point_of_interest);
        }

        // Returns a Vec2 representing the PoI in the triangle's coordinate system
        // A value of (0,0) corresponds to the 1st vertex, (1,0) corresponds to the 2nd vertex, (0,1) corresponds to the 3rd vertex
        public static Vec2 VectorToTriangleCoords(Vec3[] tri_verts, Vec3 tri_normal, Vec3 point_of_interest)
        {
            Vec3 relative = point_of_interest - tri_verts[0];
            Vec3 b_minus_a = tri_verts[1] - tri_verts[0];
            Vec3 c_minus_a = tri_verts[2] - tri_verts[0];
            Vec3 P = Vec3.Cross(c_minus_a, tri_normal), Q = Vec3.Cross(b_minus_a, tri_normal);
            double div_x = 1.0 / Vec3.Dot(P, b_minus_a);
            double div_y = 1.0 / Vec3.Dot(Q, c_minus_a);

            Vec2 result = new Vec2 { x = Vec3.Dot(P, relative) * div_x, y = Vec3.Dot(Q, relative) * div_y };
            return result;
        }

        // 2D yes-or-no intersection test for a pair of line segments
        public static bool LineSegmentIntersection2D(Vec2 a_begin, Vec2 a_end, Vec2 b_begin, Vec2 b_end)
        {
            Vec2 a_dir = a_end - a_begin;
            Vec2 b_dir = b_end - b_begin;
            Vec2 a_normal = new Vec2 { x = a_dir.y, y = -a_dir.x };
            a_normal /= a_normal.ComputeMagnitude();
            Vec2 b_normal = new Vec2 { x = b_dir.y, y = -b_dir.x };
            b_normal /= b_normal.ComputeMagnitude();

            double a_offset = Vec2.Dot(a_normal, a_begin);
            double b_offset = Vec2.Dot(b_normal, b_begin);

            double a_dir_dot = Vec2.Dot(a_dir, b_normal);
            double a_origin_dot = Vec2.Dot(a_begin, b_normal);
            double a_hit = (b_offset - a_origin_dot) / a_dir_dot;

            double b_dir_dot = Vec2.Dot(b_dir, a_normal);
            double b_origin_dot = Vec2.Dot(b_begin, a_normal);
            double b_hit = (a_offset - b_origin_dot) / b_dir_dot;

            Vec2 a_intersect = a_begin + a_dir * a_hit;
            Vec2 b_intersect = b_begin + b_dir * b_hit;

            return (a_hit >= 0 && a_hit <= 1 && b_hit >= 0 && b_hit <= 1);
        }
    }
}
