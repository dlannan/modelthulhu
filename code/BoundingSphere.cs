using System;
using System.Collections.Generic;
using System.Text;

using Modelthulhu.Math3D;

namespace Modelthulhu.Geom
{
    // Struct representing a bounding sphere
    public struct BoundingSphere
    {
        public Vec3 center;
        public double radius;

        // Get the minimum bounding sphere big enough to contain both input bounding spheres
        public static BoundingSphere Combine(BoundingSphere a, BoundingSphere b)
        {
            Vec3 dif = b.center - a.center;
            double distance = dif.ComputeMagnitude();

            // degenerate cases... one sphere completely inside the other
            if (a.radius > distance + b.radius)
                return new BoundingSphere { center = a.center, radius = a.radius };
            if (b.radius > distance + a.radius)
                return new BoundingSphere { center = b.center, radius = b.radius };

            // otherwise some actual work must be done... not too bad though
            double radius = (distance + a.radius + b.radius) * 0.5;
            Vec3 center = a.center + Vec3.Normalize(dif, radius - a.radius);

            return new BoundingSphere { center = center, radius = radius };
        }

        // Return a BoudingSphere which fully contains the specified bounding sphere and vertex
        // Like Combine, but simpler because the added point hasn't got a radius
        public static BoundingSphere Expand(BoundingSphere a, Vec3 b)
        {
            Vec3 dif = b - a.center;
            double distance = dif.ComputeMagnitude();

            // degenerate cases... one sphere completely inside the other
            if (a.radius > distance)
                return new BoundingSphere { center = a.center, radius = a.radius };

            // otherwise some actual work must be done... not too bad though
            double radius = (distance + a.radius) * 0.5;
            Vec3 center = a.center + Vec3.Normalize(dif, radius - a.radius);

            return new BoundingSphere { center = center, radius = radius };
        }
        // Same, but as a member function
        public void Expand(Vec3 b)
        {
            Vec3 dif = b - center;
            double distance = dif.ComputeMagnitude();

            // degenerate cases... one sphere completely inside the other
            if (radius > distance)
                return;

            // otherwise some actual work must be done... not too bad though
            double oldradius = radius;
            radius = (distance + radius) * 0.5;
            center += Vec3.Normalize(dif, radius - oldradius);
        }

        // Check if two bounding spheres intersect
        public static bool IntersectTest(BoundingSphere a, BoundingSphere b)
        {
            Vec3 dist = b.center - a.center;
            double radius = a.radius + b.radius;

            return (dist.ComputeMagnitudeSquared() < radius * radius);
        }
    }
}
