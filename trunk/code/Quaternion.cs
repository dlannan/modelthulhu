using System;
using System.Collections.Generic;

namespace Modelthulhu
{
    // Represents a Quaternion
    // Thanks to Wikipedia for lots of formulas and barely-adequate explanation of what's going on in them!
    public struct Quaternion
    {
        public double w;                // real part
        public double x, y, z;          // imaginary parts

        // CAUTION: Quaternion multiplication is NOT commutative
        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            return new Quaternion
            {
                w = left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z,
                x = left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y,
                y = left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z,
                z = left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x
            };
        }

        // Okay, quaternion addition is nicer
        public static Quaternion operator +(Quaternion left, Quaternion right)
        {
            return new Quaternion
            {
                w = left.w + right.w,
                x = left.x + right.x,
                y = left.y + right.y,
                z = left.z + right.z
            };
        }

        // Scalar multiplication
        public static Quaternion operator *(Quaternion left, double right)
        {
            return new Quaternion
            {
                w = left.w * right,
                x = left.x * right,
                y = left.y * right,
                z = left.z * right
            };
        }
        // Scalar multiplication is (conveniently) commutative
        public static Quaternion operator *(double left, Quaternion right) { return right * left; }

        // Scalar division
        public static Quaternion operator /(Quaternion left, double right) { return left * (1.0 / right); }

        // Negation (additive inverse)
        public static Quaternion operator -(Quaternion q)
        {
            return new Quaternion
            {
                w = -q.w,
                x = -q.x,
                y = -q.y,
                z = -q.z
            };
        }

        // Subtraction
        public static Quaternion operator -(Quaternion left, Quaternion right)
        {
            return new Quaternion { w = left.w - right.w, x = left.x - right.x, y = left.y - right.y, z = left.z - right.z };
        }

        // Creates a 3x3 rotation matrix representing the same rotation as represented by this quaternion
        // First the quaternion must be copied and normalized
        public Mat3 ToMat3()
        {
            Quaternion n = this * (1.0 / Norm());               // normalized copy

            double W = n.w, X = n.x, Y = n.y, Z = n.z;

            return new Mat3
            {
                values = new double[]
                {
                    W * W + X * X - Y * Y - Z * Z,      2.0 * (X * Y - W * Z),              2.0 * (X * Z + W * Y),
                    2.0 * (X * Y + W * Z),              W * W - X * X + Y * Y - Z * Z,      2.0 * (Y * Z - W * X),
                    2.0 * (X * Z - W * Y),              2.0 * (Y * Z + W * X),              W * W - X * X - Y * Y + Z * Z
                }
            };
        }

        // Gets a Quaternion which most closely (we hope) matches the given Mat3
        public static Quaternion FromRotationMatrix(Mat3 mat)
        {
            Vec3 four_w_xyz = new Vec3 { x = mat[7] - mat[5], y = mat[2] - mat[6], z = mat[3] - mat[1] };
            double plus_or_minus = Math.Sqrt(1 - 0.25 * four_w_xyz.ComputeMagnitudeSquared());
            double[] w_squared = new double[] { (1 + plus_or_minus) * 0.5, (1 - plus_or_minus) * 0.5 };
            double[] positive_w = new double[] { Math.Sqrt(w_squared[0]), Math.Sqrt(w_squared[1]) };
            double[] all_possible_w = new double[] { positive_w[0], -positive_w[0], positive_w[1], -positive_w[1] };

            List<Quaternion> results = new List<Quaternion>();
            foreach (double w in all_possible_w)
            {
                // Making sure it's nonzero AND it's a valid number (not NaN or somesuch)
                if (w > 0 || w < 0)
                {
                    Vec3 xyz = four_w_xyz * 0.25 / w;
                    results.Add(new Quaternion { w = w, x = xyz.x, y = xyz.y, z = xyz.z });
                }
            }
            Quaternion? choice = null;
            double least_fail = -1.0;
            foreach (Quaternion q in results)
            {
                Mat3 regenerated = q.ToMat3();
                double fail_total = 0.0;
                for (int i = 0; i < 9; i++)
                {
                    double fail = mat[i] - regenerated[i];
                    fail_total += fail * fail;
                }
                if (least_fail == -1.0 || fail_total < least_fail)
                {
                    choice = q;
                    least_fail = fail_total;
                }
            }
            if (choice == null) throw new Exception("No suitable quaternion found in Quaternion.FromRotationMatrix !!!");

            return choice.Value;
        }

        // Computes the norm (like magnitude) of this quaternion
        public double Norm()
        {
            return Math.Sqrt(w * w + x * x + y * y + z * z);
        }

        // Identity quaternion (represents an identity rotation/orientation)
        // Note that w = 1, not 0
        public static Quaternion Identity { get { return new Quaternion { w = 1.0, x = 0.0, y = 0.0, z = 0.0 }; } }

        // Generate a quaternion representing a rotation about the specified unit axis, with the specified angle (in radians)
        public static Quaternion FromAxisAngle(double x, double y, double z, double angle)
        {
            double half = angle * 0.5, sine = Math.Sin(half);
            return new Quaternion 
            { 
                x = x * sine, 
                y = y * sine, 
                z = z * sine, 
                w = Math.Cos(half) 
            };
        }
        // aka from scaled axis
        public static Quaternion FromPYR(double p, double y, double r)
        {
            double mag = Vec3.Magnitude(p, y, r), inv = (mag == 0 ? 1.0 : 1.0 / mag);
            return FromAxisAngle(p * inv, y * inv, r * inv, mag);
        }
        public static Quaternion FromPYR(Vec3 pyrVector) { return FromPYR(pyrVector.x, pyrVector.y, pyrVector.z); }

        // i.e. to scaled axis
        public Vec3 ToPYR()
        {
            Vec3 axis = new Vec3 { x = x, y = y, z = z };
            double sine = axis.ComputeMagnitude();                      // doesn't cover the possibility of a negative sine
            double cosine = w;
            double half = Math.Asin(sine);
            double angle = half * 2.0;
            return Vec3.Normalize(axis, angle);
        }

        public static Vec3 operator *(Quaternion left, Vec3 right)
        {
            return left.ToMat3() * right;
        }
    }
}