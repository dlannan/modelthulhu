using System;

namespace Modelthulhu
{
    // Struct representing a 2D Vector
    public struct Vec2
    {
        // Note, there's no protection against stupid stuff by the user
        public double x, y;

        // The 2D zero-vector
        public static Vec2 Zero { get { return new Vec2 { x = 0, y = 0 }; } }

        // The dot-product of a pair of 2D vectors
        public static double Dot(Vec2 a, Vec2 b) { return a.x * b.x + a.y * b.y; }
        public static double Dot(ref Vec2 a, ref Vec2 b) { return a.x * b.x + a.y * b.y; }

        // The sum of a pair of 2D vectors
        public static Vec2 operator +(Vec2 left, Vec2 right) { return new Vec2 { x = left.x + right.x, y = left.y + right.y, }; }

        // The additive opposite of a 2D Vector
        public static Vec2 operator -(Vec2 right) { return new Vec2 { x = -right.x, y = -right.y }; }

        // The difference of a pair of 2D vectors
        public static Vec2 operator -(Vec2 left, Vec2 right) { return new Vec2 { x = left.x - right.x, y = left.y - right.y }; }

        // Scalar multiplication
        public static Vec2 operator *(Vec2 left, double right) { return new Vec2 { x = left.x * right, y = left.y * right }; }
        public static Vec2 operator *(double left, Vec2 right) { return new Vec2 { x = right.x * left, y = right.y * left }; }

        // Scalar division
        public static Vec2 operator /(Vec2 left, double right) { return left * (1.0 / right); }

        // Finds the square of the magnitude of the 2D vector (the square root operation is done after this, so this may be all that's needed)
        public double ComputeMagnitudeSquared() { return x * x + y * y; }

        // Finds the magnitude of the 2D vector
        public double ComputeMagnitude() { return Math.Sqrt(x * x + y * y); }

        // Finds the square of the distance between two points
        public static double DistanceSquared(Vec2 a, Vec2 b)
        {
            double dx = a.x - b.x, dy = a.y - b.y;
            return dx * dx + dy * dy;
        }

        // Finds the distance between two points
        public static double Distance(Vec2 a, Vec2 b)
        {
            double dx = a.x - b.x, dy = a.y - b.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
