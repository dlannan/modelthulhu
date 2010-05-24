using System;

namespace Modelthulhu
{
    // Struct representing a 4D vector
    public struct Vec4
    {
        // The 4 components of the vector
        public double w, x, y, z;

        // Get a 4-dimensional zero-vector
        // Careful! This creates a new vector every time it's called!
        public static Vec4 Zero { get { return new Vec4 { w = 0, x = 0, y = 0, z = 0 }; } }

        // Create a Vec4 from a Vec3 and the value for the Vec4's w-component
        public static Vec4 FromVec3(Vec3 xyz, double w) { return new Vec4 { x = xyz.x, y = xyz.y, z = xyz.z, w = w }; }

        // Get a Vec3 containing the x, y, and z of this vector
        public Vec3 Xyz { get { return new Vec3 { x = x, y = y, z = z }; } }

        // Scalar multiplication
        public static Vec4 operator *(Vec4 left, double right) { return new Vec4 { x = left.x * right, y = left.y * right, z = left.z * right, w = left.w * right }; }
        public static Vec4 operator *(double left, Vec4 right) { return right * left; }

        // Scalar division
        public static Vec4 operator /(Vec4 left, double right) { return left * (1.0 / right); }
    }
}
