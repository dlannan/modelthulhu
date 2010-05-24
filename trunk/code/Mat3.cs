using System;

namespace Modelthulhu
{
    // Structure representing a 3x3 matrix
    // Functions in this class assume the element order is:
    // 012
    // 345
    // 678
    public struct Mat3
    {
        // The data of the matrix
        public double[] values;

        // Creates and returns a 3x3 matrix with the same values as this matrix's values
        public Mat3 Clone()
        {
            return new Mat3 { values = new double[] {
                values[0], values[1], values[2],
                values[3], values[4], values[5],
                values[6], values[7], values[8]} };
        }

        public static Mat3 Identity { get { return new Mat3 { values = new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 } }; } }

        // Creates and returns a rotation matrix, with a rotation of angle radians about the specified axis
        // The axis should be a unit vector
        public static Mat3 FromAxisAngle(double x, double y, double z, double angle)
        {
            double costheta = Math.Cos(angle);
            double sintheta = Math.Sin(angle);
            double oneminuscostheta = 1.0f - costheta;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;
            return new Mat3 { values = new double[] {
                (double)(costheta + oneminuscostheta * x * x),
                (double)(oneminuscostheta * xy - z * sintheta),
                (double)(oneminuscostheta * xz + sintheta * y),
                (double)(oneminuscostheta * xy + sintheta * z),
                (double)(costheta + oneminuscostheta * y * y),
                (double)(oneminuscostheta * yz - sintheta * x),
                (double)(oneminuscostheta * xz - sintheta * y),
                (double)(oneminuscostheta * yz + sintheta * x),
                (double)(costheta + oneminuscostheta * z * z)
            }};
        }

        // Creates and returns a rotation matrix about the specified axis,
        // Where the angle of rotation is equal to the magnitude of the provided axis vector
        // If the magnitude is zero, the matrix will be an identity matrix
        // Alternatively known as FromPYR (pitch, yaw, roll)
        public static Mat3 FromScaledAxis(double x, double y, double z)
        {
            double mag = Math.Sqrt(x * x + y * y + z * z);

            if (mag == 0)
                return Identity;

            double inv = 1.0 / mag;
            return FromAxisAngle(x * inv, y * inv, z * inv, mag);
        }
        public static Mat3 FromScaledAxis(Vec3 xyz) { return FromScaledAxis(xyz.x, xyz.y, xyz.z); }

        // Returns the L matrix right-multiplied by the R matrix
        public static Mat3 operator *(Mat3 left, Mat3 right)
        {
            return new Mat3 { values = new double[] {
                    // Top row values
                    left.values[0] * right.values[0] + left.values[1] * right.values[3] + left.values[2] * right.values[6],
                    left.values[0] * right.values[1] + left.values[1] * right.values[4] + left.values[2] * right.values[7],
                    left.values[0] * right.values[2] + left.values[1] * right.values[5] + left.values[2] * right.values[8],
                    // Middle row values
                    left.values[3] * right.values[0] + left.values[4] * right.values[3] + left.values[5] * right.values[6],
                    left.values[3] * right.values[1] + left.values[4] * right.values[4] + left.values[5] * right.values[7],
                    left.values[3] * right.values[2] + left.values[4] * right.values[5] + left.values[5] * right.values[8],
                    // Bottom row values
                    left.values[6] * right.values[0] + left.values[7] * right.values[3] + left.values[8] * right.values[6],
                    left.values[6] * right.values[1] + left.values[7] * right.values[4] + left.values[8] * right.values[7],
                    left.values[6] * right.values[2] + left.values[7] * right.values[5] + left.values[8] * right.values[8]
                }};
        }

        // Returns the L matrix right-multiplied by the R column vector
        public static Vec3 operator *(Mat3 left, Vec3 right)
        {
            return new Vec3 {
                x = right.x * left.values[0] + right.y * left.values[1] + right.z * left.values[2],
                y = right.x * left.values[3] + right.y * left.values[4] + right.z * left.values[5],
                z = right.x * left.values[6] + right.y * left.values[7] + right.z * left.values[8]
            };
        }

        // Gets the transpose of this matrix
        public Mat3 Transpose { get { return new Mat3 { values = new double[] { values[0], values[3], values[6], values[1], values[4], values[7], values[2], values[5], values[8] } }; } }

        // indices correspond to the indices of the values array, and get/set the corresponding values
        public double this[int index] { get { return values[index]; } set { values[index] = value; } }

        // Get an orthonormal matrix as close to the given matrix as possible
        public static Mat3 Normalize(Mat3 mat)
        {
            Vec3 a = new Vec3 { x = mat[0], y = mat[1], z = mat[2] };
            Vec3 b = new Vec3 { x = mat[3], y = mat[4], z = mat[5] };

            Vec3 c = Vec3.Cross(a, b);

            a = a * (1.0 / a.ComputeMagnitude());
            return new Mat3 { values = new double[] { a.x, a.y, a.z, b.x, b.y, b.z, c.x, c.y, c.z } };
        }

        // Get the determinant of this 3x3 matrix
        public double Determinant { get { return values[0] * values[4] * values[8] + values[1] * values[5] * values[6] + values[2] * values[3] * values[7] - values[0] * values[5] * values[7] - values[1] * values[3] * values[8] - values[2] * values[5] * values[7]; } }

        // Does a proper inverse (as opposed to a transpose, which conveniently happens to be the same as inverse IF we're using an orthonormal matrix)
        public static Mat3 Invert(Mat3 matrix)
        {
            /*
            double det = matrix.Determinant, inv = 1.0 / det, ninv = -inv;
            double[] v = matrix.values;         // for convenience (maybe also faster?)
            return new Mat3
            {
                values = new double[]                
                {
                    (v[4] * v[8] - v[5] * v[7]) * inv,
                    (v[3] * v[8] - v[5] * v[6]) * ninv,
                    (v[3] * v[7] - v[4] * v[6]) * inv,
                    (v[1] * v[8] - v[2] * v[7]) * ninv,
                    (v[0] * v[8] - v[2] * v[6]) * inv,
                    (v[0] * v[7] - v[1] * v[6]) * ninv,
                    (v[1] * v[5] - v[2] * v[4]) * inv,
                    (v[0] * v[5] - v[2] * v[3]) * ninv,
                    (v[0] * v[4] - v[1] * v[3]) * inv
                }
            };
             */

            double inv = 1.0 / matrix.Determinant;
            Vec3 x0 = new Vec3 { x = matrix[0], y = matrix[3], z = matrix[6] };
            Vec3 x1 = new Vec3 { x = matrix[1], y = matrix[4], z = matrix[7] };
            Vec3 x2 = new Vec3 { x = matrix[2], y = matrix[5], z = matrix[8] };
            Vec3 y0 = Vec3.Cross(x1, x2);
            Vec3 y1 = Vec3.Cross(x2, x0);
            Vec3 y2 = Vec3.Cross(x0, x1);
            return new Mat3
            {
                values = new double[]
                {
                    y0.x * inv, y0.y * inv, y0.z * inv,
                    y1.x * inv, y1.y * inv, y1.z * inv,
                    y2.x * inv, y2.y * inv, y2.z * inv
                }
            };
        }
    }
}
