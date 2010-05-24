using System;

namespace Modelthulhu
{
    // Struct representing a 3D Vector
    public struct Vec3
    {
        // Note, there's no protection against stupid stuff by the user
        public double x, y, z;

        // The 3-component zero vector
        // Careful! This creates a new vector every time it's called!
        public static Vec3 Zero { get { return new Vec3 { x = 0, y = 0, z = 0 }; } }

        // The dot product of a pair of 3D vectors
        public static double Dot(Vec3 a, Vec3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
        public static double Dot(ref Vec3 a, ref Vec3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }

        // The cross product of a pair of 3D vectors
        public static Vec3 Cross(Vec3 a, Vec3 b) { return new Vec3 { x = a.y * b.z - a.z * b.y, y = a.z * b.x - a.x * b.z, z = a.x * b.y - a.y * b.x }; }
        public static Vec3 Cross(ref Vec3 a, ref Vec3 b) { return new Vec3 { x = a.y * b.z - a.z * b.y, y = a.z * b.x - a.x * b.z, z = a.x * b.y - a.y * b.x }; }

        // The sum of a pair of 3D vectors
        public static Vec3 operator +(Vec3 left, Vec3 right) { return new Vec3 { x = left.x + right.x, y = left.y + right.y, z = left.z + right.z }; }

        // The additive inverse of a vector
        public static Vec3 operator -(Vec3 right) { return new Vec3 { x = -right.x, y = -right.y, z = -right.z }; }

        // The difference between 3D vectors
        public static Vec3 operator -(Vec3 left, Vec3 right) { return new Vec3 { x = left.x - right.x, y = left.y - right.y, z = left.z - right.z }; }

        // Scalar product with a 3D vector
        public static Vec3 operator *(Vec3 left, double right) { return new Vec3 { x = left.x * right, y = left.y * right, z = left.z * right }; }
        public static Vec3 operator *(double left, Vec3 right) { return new Vec3 { x = right.x * left, y = right.y * left, z = right.z * left }; }

        // Scalar division
        public static Vec3 operator /(Vec3 left, double right) { return left * (1.0 / right); }


        // Finds the square of the magnitude of the 3D vector (the square root operation is done after this, so this may be all that's needed)
        public double ComputeMagnitudeSquared() { return x * x + y * y + z * z; }

        // Finds the magnitude of the 3D vector
        public double ComputeMagnitude() { return Math.Sqrt(x * x + y * y + z * z); }



        // Static versions of the Vec3 methods, but they are void functions and they act on references to components rather than on vector structs

        // Assigns to O the value of A + B
        public static void Add(
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            out double xO, out double yO, out double zO)
        {
            xO = x1 + x2;
            yO = y1 + y2;
            zO = z1 + z2;
        }

        // Assigns to O the value of A - B
        public static void Subtract(
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            out double xO, out double yO, out double zO)
        {
            xO = x1 - x2;
            yO = y1 - y2;
            zO = z1 - z2;
        }

        // Assigns to O the cross product of A and B
        public static void Cross(
            double xA, double yA, double zA,
            double xB, double yB, double zB,
            out double xO, out double yO, out double zO)
        {
            xO = yA * zB - zA * yB;
            yO = zA * xB - xA * zB;
            zO = xA * yB - yA * xB;
        }

        // Assigns to O the result of I * scaleFactor
        public static void Scale(
            double xI, double yI, double zI,
            double scaleFactor,
            out double xO, out double yO, out double zO)
        {
            xO = xI * scaleFactor;
            yO = yI * scaleFactor;
            zO = zI * scaleFactor;
        }

        // Assigns to O the additive opposite of I
        public static void Negate(
            double xI, double yI, double zI,
            out double xO, out double yO, out double zO)
        {
            xO = -xI;
            yO = -yI;
            zO = -zI;
        }

        // Linearly interpolates between A and B, i.e. f(A,B,fraction) = B * fraction + A * (1 - fraction), and assigns the result to O
        public static void Lerp(
            double xA, double yA, double zA,
            double xB, double yB, double zB,
            double fraction,
            out double xO, out double yO, out double zO)
        {
            double anti = 1.0 - fraction;
            xO = xA * anti + xB * fraction;
            yO = yA * anti + yB * fraction;
            zO = zA * anti + zB * fraction;
        }


        // Computes and returns the magnitude of the given vector
        public static double Magnitude(
            double x, double y, double z)
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        // Like the magnitude, but without taking the square root at the end
        public static double MagnitudeSquared(
            double x, double y, double z)
        {
            return x * x + y * y + z * z;
        }

        // Computes and returns the dot product of vectors A and B
        public static double Dot(
            double xA, double yA, double zA,
            double xB, double yB, double zB)
        {
            return xA * xB + yA * yB + zA * zB;
        }

        // Returns a vector with the same direction as vec, but scaled to have the specified length
        public static Vec3 Normalize(Vec3 vec, double length)
        {
            if (length == 0)
                return Vec3.Zero;

            double cur = vec.ComputeMagnitude();
            if (cur == 0)
                throw new Exception("Attempting to normalize a zero vector");

            double coeff = length / cur;
            return new Vec3 { x = vec.x * coeff, y = vec.y * coeff, z = vec.z * coeff };
        }
        // Returns a unit vector in the same direction as vec
        public static Vec3 Normalize(Vec3 vec)
        {
            double cur = vec.ComputeMagnitude();
            if (cur == 0)
                throw new Exception("Attempting to normalize a zero vector");

            double coeff = 1.0 / cur;
            return new Vec3 { x = vec.x * coeff, y = vec.y * coeff, z = vec.z * coeff };
        }
    }
}
