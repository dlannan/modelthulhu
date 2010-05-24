using System;
using System.Collections.Generic;
using System.Text;

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

        // Type representing an intersection (for ray intersect test)
        public struct Intersection
        {
            public double time;
            public Vec3 position;

            public int face;
            public Vec3 normal;
            public Ray ray;

            public double i, j;             // triangle-space coordinates, (0,0) = vertex A, (1,0) = vertex B, (0, 1) = vertex C
        }

        // For ray intersect test
        public struct Ray
        {
            public Vec3 origin, direction;
        }

        // Batch test a bunch of rays and triangles for intersection
        public static Intersection[][] RayTriangleListIntersect(Vec3[] verts, uint[] a_vert, uint[] b_vert, uint[] c_vert, Ray[] rays)
        {
            int num_rays = rays.Length;
            int num_verts = verts.Length;
            int num_faces = a_vert.Length;

            Vec3[] A = new Vec3[num_faces], B = new Vec3[num_faces], C = new Vec3[num_faces];
            Vec3[] AB = new Vec3[num_faces], AC = new Vec3[num_faces], Normal = new Vec3[num_faces];
            double[] Offset = new double[num_faces];
            Vec3[] P = new Vec3[num_faces], Q = new Vec3[num_faces];
            double[] UOffset = new double[num_faces], VOffset = new double[num_faces];
            for (int face_index = 0; face_index < num_faces; face_index++)
            {
                A[face_index] = verts[a_vert[face_index]];
                B[face_index] = verts[b_vert[face_index]];
                C[face_index] = verts[c_vert[face_index]];
                AB[face_index] = B[face_index] - A[face_index];
                AC[face_index] = C[face_index] - A[face_index];
                Normal[face_index] = Vec3.Cross(AB[face_index], AC[face_index]);
                Normal[face_index] = Normal[face_index] / Normal[face_index].ComputeMagnitude();
                Offset[face_index] = Vec3.Dot(ref Normal[face_index], ref A[face_index]);
                P[face_index] = Vec3.Cross(AC[face_index], Normal[face_index]);
                P[face_index] = P[face_index] / Vec3.Dot(ref P[face_index], ref AB[face_index]);
                Q[face_index] = Vec3.Cross(AB[face_index], Normal[face_index]);
                Q[face_index] = Q[face_index] / Vec3.Dot(ref Q[face_index], ref AC[face_index]);
                UOffset[face_index] = Vec3.Dot(ref P[face_index], ref A[face_index]);
                VOffset[face_index] = Vec3.Dot(ref Q[face_index], ref A[face_index]);
            }

            Intersection[][] results = new Intersection[num_rays][];
            for (int ray_index = 0; ray_index < num_rays; ray_index++)
            {
                Ray ray = rays[ray_index];
                List<Intersection> test = new List<Intersection>();
                for (int face_index = 0; face_index < num_faces; face_index++)
                {
                    double hit = Util.RayPlaneIntersect(ray.origin, ray.direction, Normal[face_index], Offset[face_index]);
                    // TODO: maybe put (optional) conditions here to cull 'backward' items, etc.
                    if (hit > 0 || hit <= 0)            // for now just check that it's a real number
                    {
                        Vec3 pos = ray.origin + ray.direction * hit;
                        double dist = Vec3.Dot(ref pos, ref Normal[face_index]) - Offset[face_index];            // should be 0, otherewise it's not on the plane at all!
                        double u = Vec3.Dot(ref P[face_index], ref pos) - UOffset[face_index];
                        double v = Vec3.Dot(ref Q[face_index], ref pos) - VOffset[face_index];
                        if (u >= 0 && v >= 0 && u + v <= 1)
                            test.Add(
                                new Intersection
                                {
                                    face = face_index,
                                    i = u,
                                    j = v,
                                    position = pos,
                                    time = hit,
                                    normal = Normal[face_index],
                                    ray = ray
                                });
                    }
                }
                results[ray_index] = test.ToArray();
            }

            return results;
        }

        // If it's too low, returns min
        // If it's too high, returns max
        // Otherwise, returns the value
        public static double Clamp(double val, double min, double max)
        {
            return val > max ? max : val < min ? min : val;
        }

        // Finds the minimum distance between the specified point and any of the points in the provided array of points
        public static double PointCloudMinimumDistance(Vec3 point, Vec3[] verts)
        {
            double best = Double.PositiveInfinity;
            for (int i = 0; i < verts.Length; i++)
            {
                double d = (verts[i] - point).ComputeMagnitude();
                if (d < best)
                    best = d;
            }
            return best;
        }

        // Finds the minimum distance from the triangle formed by ABC to the point at X
        public static double TriangleMinimumDistance(Vec3 a, Vec3 b, Vec3 c, Vec3 x)
        {
            Vec3 ab = b - a, bc = c - b, ca = a - c;                    // edge vectors
            Vec3 ax = x - a, bx = x - b, cx = x - c;                    // vectors from verts to X
            Vec3 normal = Vec3.Normalize(Vec3.Cross(ab, bc));           // unit normal vector

            // distances from verts; guaranteed to be valid
            double dist_a_x = ax.ComputeMagnitude();
            double dist_b_x = bx.ComputeMagnitude();
            double dist_c_x = cx.ComputeMagnitude();

            // minimum value so far encountered
            double min = Math.Min(dist_a_x, Math.Min(dist_b_x, dist_c_x));

            // lengths of the edges
            double len_ab = ab.ComputeMagnitude();
            double len_bc = bc.ComputeMagnitude();
            double len_ca = ca.ComputeMagnitude();

            // unit vectors perpendicular to each edge, lying in the plane of the triangle
            Vec3 n_ab = Vec3.Cross(ab, normal) / len_ab;
            Vec3 n_bc = Vec3.Cross(bc, normal) / len_bc;
            Vec3 n_ca = Vec3.Cross(ca, normal) / len_ca;

            // finding the positions of X along each of the edges (necessary to determine if the edge distances are valid)
            double part_x_ab = Vec3.Dot(ax, ab) / (len_ab * len_ab);
            double part_x_bc = Vec3.Dot(bx, bc) / (len_bc * len_bc);
            double part_x_ca = Vec3.Dot(cx, ca) / (len_ca * len_ca);

            // determining whether or not the edge distances are valid
            if (part_x_ab >= 0 && part_x_ab <= 1)
                min = Math.Min(min, Vec3.Cross(ab, ax).ComputeMagnitude() / len_ab);
            if (part_x_bc >= 0 && part_x_bc <= 1)
                min = Math.Min(min, Vec3.Cross(bc, bx).ComputeMagnitude() / len_bc);
            if (part_x_ca >= 0 && part_x_ca <= 1)
                min = Math.Min(min, Vec3.Cross(ca, cx).ComputeMagnitude() / len_ca);

            // finding the distance from the plane; valid under the least frequently satisfied conditions
            double dot_n_ab_a = Vec3.Dot(n_ab, a);                                                  // storing it because it's used twice in the expression... it'd be dumb to calculate twice
            if ((Vec3.Dot(n_ab, x) - dot_n_ab_a) * (Vec3.Dot(n_ab, c) - dot_n_ab_a) > 0)            // if they're on the same side, this product is positive
            {
                double dot_n_bc_b = Vec3.Dot(n_bc, b);
                if ((Vec3.Dot(n_bc, x) - dot_n_bc_b) * (Vec3.Dot(n_bc, a) - dot_n_bc_b) > 0)
                {
                    double dot_n_ca_c = Vec3.Dot(n_ca, c);
                    if ((Vec3.Dot(n_ca, x) - dot_n_ca_c) * (Vec3.Dot(n_ca, b) - dot_n_ca_c) > 0)
                    {
                        // too bad it's so much harder to find out if it's valid than it is to calculate the value itself
                        min = Math.Min(min, Math.Abs(Vec3.Dot(normal, ax)));
                    }
                }
            }

            return min;
        }

        // Finds the minimum distance from the triangle formed by ABC to the point at X
        public static List<double> PointListMinimumDistanceFromTriangles(Vec3[,] triangles, List<Vec3> points)
        {
            int num_triangles = triangles.GetLength(0);

            // precalculating a bunch of values which will be reused for each triangle
            Vec3[] a = new Vec3[num_triangles], b = new Vec3[num_triangles], c = new Vec3[num_triangles];
            Vec3[] ab = new Vec3[num_triangles], bc = new Vec3[num_triangles], ca = new Vec3[num_triangles];
            Vec3[] normal = new Vec3[num_triangles];
            double[] inv_len_ab = new double[num_triangles], inv_len_bc = new double[num_triangles], inv_len_ca = new double[num_triangles];
            Vec3[] n_ab = new Vec3[num_triangles], n_bc = new Vec3[num_triangles], n_ca = new Vec3[num_triangles];
            double[] dot_n_ab_a = new double[num_triangles], dot_n_bc_b = new double[num_triangles], dot_n_ca_c = new double[num_triangles];
            double[] funky_a = new double[num_triangles], funky_b = new double[num_triangles], funky_c = new double[num_triangles];
            for (int i = 0; i < num_triangles; i++)
            {
                // vertices
                a[i] = triangles[i, 0];
                b[i] = triangles[i, 1];
                c[i] = triangles[i, 2];
                // edge vectors
                ab[i] = b[i] - a[i];
                bc[i] = c[i] - b[i];
                ca[i] = a[i] - c[i];
                // unit normal vector
                normal[i] = Vec3.Normalize(Vec3.Cross(ab[i], bc[i]));
                // lengths of the edges
                inv_len_ab[i] = 1.0 / ab[i].ComputeMagnitude();
                inv_len_bc[i] = 1.0 / bc[i].ComputeMagnitude();
                inv_len_ca[i] = 1.0 / ca[i].ComputeMagnitude();
                // unit vectors perpendicular to each edge, lying in the plane of the triangle
                n_ab[i] = Vec3.Cross(ab[i], normal[i]) * inv_len_ab[i];
                n_bc[i] = Vec3.Cross(bc[i], normal[i]) * inv_len_bc[i];
                n_ca[i] = Vec3.Cross(ca[i], normal[i]) * inv_len_ca[i];
                // some dot products... storing them because they're used twice in the expression... it'd be dumb to calculate twice
                dot_n_ab_a[i] = Vec3.Dot(n_ab[i], a[i]);
                dot_n_bc_b[i] = Vec3.Dot(n_bc[i], b[i]);
                dot_n_ca_c[i] = Vec3.Dot(n_ca[i], c[i]);
                // some coefficients
                funky_a[i] = Vec3.Dot(n_ab[i], c[i]) - dot_n_ab_a[i];
                funky_b[i] = Vec3.Dot(n_bc[i], a[i]) - dot_n_bc_b[i];
                funky_c[i] = Vec3.Dot(n_ca[i], b[i]) - dot_n_ca_c[i];
            }

            List<double> distances = new List<double>();
            foreach (Vec3 x in points)
            {
                // minimum value so far encountered
                double min = -1;
                for (int i = 0; i < num_triangles; i++)
                {
                    Vec3 ax = x - a[i], bx = x - b[i], cx = x - c[i];                    // vectors from verts to X

                    // distances from verts; guaranteed to be valid
                    double dist_a_x = ax.ComputeMagnitude();
                    double dist_b_x = bx.ComputeMagnitude();
                    double dist_c_x = cx.ComputeMagnitude();

                    double min_vertex_distance = Math.Min(dist_a_x, Math.Min(dist_b_x, dist_c_x));
                    if (min == -1 || min_vertex_distance < min)
                        min = min_vertex_distance;

                    // finding the positions of X along each of the edges (necessary to determine if the edge distances are valid)
                    double part_x_ab = Vec3.Dot(ax, ab[i]) / (inv_len_ab[i] * inv_len_ab[i]);
                    double part_x_bc = Vec3.Dot(bx, bc[i]) / (inv_len_bc[i] * inv_len_bc[i]);
                    double part_x_ca = Vec3.Dot(cx, ca[i]) / (inv_len_ca[i] * inv_len_ca[i]);

                    // determining whether or not the edge distances are valid
                    if (part_x_ab >= 0 && part_x_ab <= 1)
                        min = Math.Min(min, Vec3.Cross(ab[i], ax).ComputeMagnitude() * inv_len_ab[i]);
                    if (part_x_bc >= 0 && part_x_bc <= 1)
                        min = Math.Min(min, Vec3.Cross(bc[i], bx).ComputeMagnitude() * inv_len_bc[i]);
                    if (part_x_ca >= 0 && part_x_ca <= 1)
                        min = Math.Min(min, Vec3.Cross(ca[i], cx).ComputeMagnitude() * inv_len_ca[i]);

                    // the distance from the plane is valid under the least frequently satisfied conditions
                    // it's easy to calculate, though, and if it's bigger than the minimum, we don't need to bother calculating its validity
                    double face_distance = Math.Abs(Vec3.Dot(normal[i], ax));
                    if (face_distance < min)            
                    {
                        if ((Vec3.Dot(n_ab[i], x) - dot_n_ab_a[i]) * funky_a[i] > 0)            // if they're on the same side, this product is positive
                            if ((Vec3.Dot(n_bc[i], x) - dot_n_bc_b[i]) * funky_b[i] > 0)
                                if ((Vec3.Dot(n_ca[i], x) - dot_n_ca_c[i]) * funky_c[i] > 0)
                                    min = face_distance;
                    }
                }
                distances.Add(min);
            }
            return distances;
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

        public static bool IsPointWithinIJTriangle(Vec2 test)
        {
            return test.x >= 0 && test.y >= 0 && test.x + test.y <= 1.0;
        }

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

        public static Vec2 VectorToTriangleCoords(Vec3[] tri_verts, Vec3 point_of_interest)
        {
            return VectorToTriangleCoords(tri_verts, Vec3.Normalize(Vec3.Cross(tri_verts[1] - tri_verts[0], tri_verts[2] - tri_verts[0])), point_of_interest);
        }

        public static Vec2 VectorToTriangleCoords(Vec3[] tri_verts, Vec3 tri_normal, Vec3 point_of_interest)
        {
            Vec3 relative = point_of_interest - tri_verts[0];
            Vec3 b_minus_a = tri_verts[1] - tri_verts[0];
            Vec3 c_minus_a = tri_verts[2] - tri_verts[0];
            Vec3 P = Vec3.Cross(c_minus_a, tri_normal), Q = Vec3.Cross(b_minus_a, tri_normal);
            double div_x = 1.0 / Vec3.Dot(P, b_minus_a);
            double div_y = 1.0 / Vec3.Dot(Q, c_minus_a);

            Vec2 result = new Vec2 { x = Vec3.Dot(P, relative) * div_x, y = Vec3.Dot(Q, relative) * div_y };
            //Vec3 reassembled = tri_verts[0] * (1 - result.x - result.y) + tri_verts[1] * result.x + tri_verts[2] * result.y;
            return result;
        }

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

        public static bool OrientedBoundingBoxIntersectionTest(AABB box1, AABB box2, Mat3 rm1, Mat3 rm2, Vec3 offset1, Vec3 offset2)
        {
            Vec3[, ,] verts1 = new Vec3[2, 2, 2];
            Vec3[, ,] verts2 = new Vec3[2, 2, 2];
            List<Vec3> vertList1 = new List<Vec3>();
            List<Vec3> vertList2 = new List<Vec3>();
            for(int i = 0; i < 2;i++)
                for(int j = 0; j < 2;j++)
                    for (int k = 0; k < 2; k++)
                    {
                        Vec3 vec1 = new Vec3{x = box1.array[0][i], y = box1.array[1][j], z = box1.array[2][k]};
                        verts1[i, j, k] = rm1 * vec1 + offset1;
                        vertList1.Add(verts1[i, j, k]);
                        Vec3 vec2 = new Vec3 { x = box2.array[0][i], y = box2.array[1][j], z = box2.array[2][k] };
                        verts2[i, j, k] = rm2 * vec2 + offset2;
                        vertList2.Add(verts2[i, j, k]);
                    }
            
            // nowhere near each other?
            AABB aabb1 = AABB.FitPointList(vertList1), aabb2 = AABB.FitPointList(vertList2);
            if (!AABB.CheckIntersection(aabb1, aabb2))
                return false;
        
            // verts inside one another?
            Mat3 invMat2 = rm2.Transpose;
            foreach (Vec3 vec in vertList1)
            {
                Vec3 dexform = invMat2 * (vec - offset2);
                if (dexform.x >= aabb2.array[0][0] && dexform.x <= aabb2.array[0][1] && dexform.y >= aabb2.array[1][0] && dexform.y <= aabb2.array[1][1] && dexform.z >= aabb2.array[2][0] && dexform.z <= aabb2.array[2][1])
                    return true;
            }

            // 2 inside 1?
            Mat3 invMat1 = rm1.Transpose;
            foreach (Vec3 vec in vertList2)
            {
                Vec3 dexform = invMat1 * (vec - offset1);
                if (dexform.x >= aabb1.array[0][0] && dexform.x <= aabb1.array[0][1] && dexform.y >= aabb1.array[1][0] && dexform.y <= aabb1.array[1][1] && dexform.z >= aabb1.array[2][0] && dexform.z <= aabb1.array[2][1])
                    return true;
            }

            // dang, gotta do edge checks
            List<Vec3[]> edges1 = new List<Vec3[]>();
            edges1.Add(new Vec3[] { verts1[0, 0, 0], verts1[0, 0, 1] });
            edges1.Add(new Vec3[] { verts1[0, 1, 0], verts1[0, 1, 1] });
            edges1.Add(new Vec3[] { verts1[1, 0, 0], verts1[1, 0, 1] });
            edges1.Add(new Vec3[] { verts1[1, 1, 0], verts1[1, 1, 1] });
            edges1.Add(new Vec3[] { verts1[0, 0, 0], verts1[0, 1, 0] });
            edges1.Add(new Vec3[] { verts1[0, 0, 1], verts1[0, 1, 1] });
            edges1.Add(new Vec3[] { verts1[1, 0, 0], verts1[1, 1, 0] });
            edges1.Add(new Vec3[] { verts1[1, 0, 1], verts1[1, 1, 1] });
            edges1.Add(new Vec3[] { verts1[0, 0, 0], verts1[1, 0, 0] });
            edges1.Add(new Vec3[] { verts1[0, 0, 1], verts1[1, 0, 1] });
            edges1.Add(new Vec3[] { verts1[0, 1, 0], verts1[1, 1, 0] });
            edges1.Add(new Vec3[] { verts1[0, 1, 1], verts1[1, 1, 1] });
            List<Vec3[]> edges2 = new List<Vec3[]>();
            edges2.Add(new Vec3[] { verts2[0, 0, 0], verts2[0, 0, 1] });
            edges2.Add(new Vec3[] { verts2[0, 1, 0], verts2[0, 1, 1] });
            edges2.Add(new Vec3[] { verts2[1, 0, 0], verts2[1, 0, 1] });
            edges2.Add(new Vec3[] { verts2[1, 1, 0], verts2[1, 1, 1] });
            edges2.Add(new Vec3[] { verts2[0, 0, 0], verts2[0, 1, 0] });
            edges2.Add(new Vec3[] { verts2[0, 0, 1], verts2[0, 1, 1] });
            edges2.Add(new Vec3[] { verts2[1, 0, 0], verts2[1, 1, 0] });
            edges2.Add(new Vec3[] { verts2[1, 0, 1], verts2[1, 1, 1] });
            edges2.Add(new Vec3[] { verts2[0, 0, 0], verts2[1, 0, 0] });
            edges2.Add(new Vec3[] { verts2[0, 0, 1], verts2[1, 0, 1] });
            edges2.Add(new Vec3[] { verts2[0, 1, 0], verts2[1, 1, 0] });
            edges2.Add(new Vec3[] { verts2[0, 1, 1], verts2[1, 1, 1] });
            Vec3 min1 = new Vec3 { x = aabb1.array[0][0], y = aabb1.array[1][0], z = aabb1.array[2][0] };
            Vec3 max1 = new Vec3 { x = aabb1.array[0][1], y = aabb1.array[1][1], z = aabb1.array[2][1] };
            Vec3 min2 = new Vec3 { x = aabb2.array[0][0], y = aabb2.array[1][0], z = aabb2.array[2][0] };
            Vec3 max2 = new Vec3 { x = aabb2.array[0][1], y = aabb2.array[1][1], z = aabb2.array[2][1] };

            // edges of first versus faces of second
            foreach (Vec3[] edge in edges1)
            {
                Vec3 dexform_0 = invMat2 * edge[0] - offset2;
                Vec3 dexform_1 = invMat2 * edge[1] - offset2;
                double hit;
                Vec3 impact;
                
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 1, y = 0, z = 0 }, min2.x);
                impact = dexform_0 * (1-hit) + dexform_1*hit;
                if(hit >= 0 && hit <= 1)
                    if (impact.y >= min2.y && impact.y <= max2.y && impact.z >= min2.z && impact.z <= max2.z)
                        return true;
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 1, y = 0, z = 0 }, max2.x);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min2.y && impact.y <= max2.y && impact.z >= min2.z && impact.z <= max2.z)
                        return true;

                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 1, z = 0 }, min2.y);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.x >= min2.x && impact.x <= max2.x && impact.z >= min2.z && impact.z <= max2.z)
                        return true;
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 1, z = 0 }, max2.y);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.x >= min2.x && impact.x <= max2.x && impact.z >= min2.z && impact.z <= max2.z)
                        return true;

                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 0, z = 1 }, min2.z);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min2.y && impact.y <= max2.y && impact.x >= min2.x && impact.x <= max2.x)
                        return true;
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 0, z = 1 }, max2.z);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min2.y && impact.y <= max2.y && impact.x >= min2.x && impact.x <= max2.x)
                        return true;
            }

            // edges of second versus faces of first
            foreach (Vec3[] edge in edges2)
            {
                Vec3 dexform_0 = invMat1 * edge[0] - offset1;
                Vec3 dexform_1 = invMat1 * edge[1] - offset1;
                double hit;
                Vec3 impact;

                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 1, y = 0, z = 0 }, min1.x);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min1.y && impact.y <= max1.y && impact.z >= min1.z && impact.z <= max1.z)
                        return true;
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 1, y = 0, z = 0 }, max1.x);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min1.y && impact.y <= max1.y && impact.z >= min1.z && impact.z <= max1.z)
                        return true;

                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 1, z = 0 }, min1.y);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.x >= min1.x && impact.x <= max1.x && impact.z >= min1.z && impact.z <= max1.z)
                        return true;
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 1, z = 0 }, max1.y);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.x >= min1.x && impact.x <= max1.x && impact.z >= min1.z && impact.z <= max1.z)
                        return true;

                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 0, z = 1 }, min1.z);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min1.y && impact.y <= max1.y && impact.x >= min1.x && impact.x <= max1.x)
                        return true;
                hit = RayPlaneIntersect(dexform_0, dexform_1, new Vec3 { x = 0, y = 0, z = 1 }, max1.z);
                impact = dexform_0 * (1 - hit) + dexform_1 * hit;
                if (hit >= 0 && hit <= 1)
                    if (impact.y >= min1.y && impact.y <= max1.y && impact.x >= min1.x && impact.x <= max1.x)
                        return true;
            }

            // no intersection!
            return false;
        }

        // Find how many times you must repeat raydirection to hit the sphere
        // Returns null, one intersection time, or two intersection times
        // rayStart is relative to center of sphere
        public static double[] RaySphereTest(Vec3 rayStart, Vec3 rayDirection, double sphereRadius)
        {
            double dirMagSq = rayDirection.ComputeMagnitudeSquared();
            double startMagSq = rayStart.ComputeMagnitudeSquared();
            double A = dirMagSq;
            double B = 2.0 * Vec3.Dot(rayStart, rayDirection);
            double C = startMagSq - sphereRadius * sphereRadius;
            double underRoot = B * B - 4.0 * A * C;
            if (underRoot < 0 || A == 0)
                return null;
            double root = Math.Sqrt(underRoot);
            if (root == 0)
                return new double[] { -0.5 * B / A };
            else
                return new double[] { 0.5 * (-B - root) / A, 0.5 * (-B + root) / A };

        }

        // Number of seconds to lead a target by
        public static double LeadTime(Vec3 dX, Vec3 dV, double muzzleSpeed)
        {
            double xmag_sq = dX.ComputeMagnitudeSquared();
            double vmag_sq = dV.ComputeMagnitudeSquared();
            double qa = muzzleSpeed * muzzleSpeed - vmag_sq;
            double qb = 2.0f * Vec3.Dot(dX, dV);
            double qc = -xmag_sq;
            double urad = qb * qb - 4.0f * qa * qc;
            if (urad < 0.0f)
                return -1.0f;
            double root = Math.Sqrt(urad);
            double min = -qb - root, max = -qb + root;
            double use = min > 0.0f ? min : max;
            use /= 2.0f * qa;
            return use;
        }
    }
}
