using System;

namespace Modelthulhu.Math3D
{
    // Represents a 4x4 matrix
    // Functions in this class assume the element order is:
    // 0123
    // 4567
    // 89AB
    // CDEF
    // Where A through F are 10 through 15, in order
    public struct Mat4
    {
        // The actual matrix data
        public double[] values;

        // Returns a clone this matrix
        public Mat4 Clone()
        {
            return new Mat4
            {
                values = new double[]
                {
                    values[0], values[1], values[2], values[3],
                    values[4], values[5], values[6], values[7],
                    values[8], values[9], values[10], values[11],
                    values[12], values[13], values[14], values[15]
                }
            };
        }

        // Creates a new identity matrix
        // Be warned, it will actually create a new matrix !!!
        public static Mat4 Identity { get { return new Mat4 { values = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 } }; } }

        // Generates a 4x4 matrix with no translation/perspective components, and with rotation/scaling/skew component taken from the specified 3x3 matrix
        public static Mat4 FromMat3(Mat3 mat)
        {
            return new Mat4
            {
                values = new double[] 
                {
                    mat[0], mat[1], mat[2], 0,
                    mat[3], mat[4], mat[5], 0,
                    mat[6], mat[7], mat[8], 0,
                    0,      0,      0,      1
                }
            };
        }

        // A 4x4 matrix representing an object at a specific position, with a specific orientation
        public static Mat4 FromPositionAndOrientation(Vec3 pos, Mat3 mat)
        {
            return new Mat4
            {
                values = new double[] 
                {
                    mat[0], mat[3], mat[6], pos.x,
                    mat[1], mat[4], mat[7], pos.y,
                    mat[2], mat[5], mat[8], pos.z,
                    0,      0,      0,      1
                }
            };
        }

        public static Mat4 FromPosOriScale(Vec3 pos, Mat3 ori, double scale)
        {
            return new Mat4
            {
                values = new double[] 
                {
                    ori[0] * scale, ori[3] * scale, ori[6] * scale, pos.x,
                    ori[1] * scale, ori[4] * scale, ori[7] * scale, pos.y,
                    ori[2] * scale, ori[5] * scale, ori[8] * scale, pos.z,
                    0,      0,      0,      1
                }
            };
        }

        // 4x4 matrix multiplication
        public static Mat4 operator *(Mat4 left, Mat4 right)
        {
            return new Mat4 {
                values = new double[] {
                left.values[0] * right.values[0] + left.values[1] * right.values[4] + left.values[2] * right.values[8] + left.values[3] * right.values[12],
                left.values[0] * right.values[1] + left.values[1] * right.values[5] + left.values[2] * right.values[9] + left.values[3] * right.values[13],
                left.values[0] * right.values[2] + left.values[1] * right.values[6] + left.values[2] * right.values[10] + left.values[3] * right.values[14],
                left.values[0] * right.values[3] + left.values[1] * right.values[7] + left.values[2] * right.values[11] + left.values[3] * right.values[15],

                left.values[4] * right.values[0] + left.values[5] * right.values[4] + left.values[6] * right.values[8] + left.values[7] * right.values[12],
                left.values[4] * right.values[1] + left.values[5] * right.values[5] + left.values[6] * right.values[9] + left.values[7] * right.values[13],
                left.values[4] * right.values[2] + left.values[5] * right.values[6] + left.values[6] * right.values[10] + left.values[7] * right.values[14],
                left.values[4] * right.values[3] + left.values[5] * right.values[7] + left.values[6] * right.values[11] + left.values[7] * right.values[15],

                left.values[8] * right.values[0] + left.values[9] * right.values[4] + left.values[10] * right.values[8] + left.values[11] * right.values[12],
                left.values[8] * right.values[1] + left.values[9] * right.values[5] + left.values[10] * right.values[9] + left.values[11] * right.values[13],
                left.values[8] * right.values[2] + left.values[9] * right.values[6] + left.values[10] * right.values[10] + left.values[11] * right.values[14],
                left.values[8] * right.values[3] + left.values[9] * right.values[7] + left.values[10] * right.values[11] + left.values[11] * right.values[15],

                left.values[12] * right.values[0] + left.values[13] * right.values[4] + left.values[14] * right.values[8] + left.values[15] * right.values[12],
                left.values[12] * right.values[1] + left.values[13] * right.values[5] + left.values[14] * right.values[9] + left.values[15] * right.values[13],
                left.values[12] * right.values[2] + left.values[13] * right.values[6] + left.values[14] * right.values[10] + left.values[15] * right.values[14],
                left.values[12] * right.values[3] + left.values[13] * right.values[7] + left.values[14] * right.values[11] + left.values[15] * right.values[15]
            }
            };
        }

        // Returns the L matrix right-multiplied by the R column vector
        public static Vec4 operator *(Mat4 left, Vec4 right)
        {
            return new Vec4
            {
                x = right.x * left.values[0] + right.y * left.values[1] + right.z * left.values[2] + right.w * left.values[3],
                y = right.x * left.values[4] + right.y * left.values[5] + right.z * left.values[6] + right.w * left.values[7],
                z = right.x * left.values[8] + right.y * left.values[9] + right.z * left.values[10] + right.w * left.values[11],
                w = right.x * left.values[12] + right.y * left.values[13] + right.z * left.values[14] + right.w * left.values[15]
            };
        }

        // Returns the L matrix right-multiplied by the R scalar
        public static Mat4 operator *(Mat4 left, double right)
        {
            return new Mat4
            {
                values = new double[] 
                {
                    left.values[0] * right,     left.values[1] * right,     left.values[2] * right,     left.values[3] * right,
                    left.values[4] * right,     left.values[5] * right,     left.values[6] * right,     left.values[7] * right,
                    left.values[8] * right,     left.values[9] * right,     left.values[10] * right,    left.values[11] * right,
                    left.values[12] * right,    left.values[13] * right,    left.values[14] * right,    left.values[15] * right
                }
            };
        }

        // Returns the R matrix right-multiplied by the L scalar
        // But really just calls R * L and returns that
        public static Mat4 operator *(double left, Mat4 right) { return right * left; }

        // Gets or sets one of the matrix's 16 values
        public double this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        // Convert to a 16-element array of floats ( NOT doubles !!! )
        public float[] ToFloat16()
        {
            return new float[] 
            {
                (float)values[0], (float)values[1], (float)values[2], (float)values[3],
                (float)values[4], (float)values[5], (float)values[6], (float)values[7],
                (float)values[8], (float)values[9], (float)values[10], (float)values[11],
                (float)values[12], (float)values[13], (float)values[14], (float)values[15]
            };
        }

        // Creates a rotation around a point
        // TODO: check whether this actually works or not !!!
        public static Mat4 RotationAroundPoint(Mat3 rot, Vec3 point)
        {
            Mat3 ident = Mat3.Identity;
            return Mat4.FromPositionAndOrientation((rot * point), ident) * Mat4.FromMat3(rot) * Mat4.FromPositionAndOrientation(-(point), ident);
        }

        // Creates a translation matrix
        public static Mat4 Translation(Vec3 translation) { return Translation(translation.x, translation.y, translation.z); }
        public static Mat4 Translation(double x, double y, double z)
        {
            return new Mat4
            {
                values = new double[]
                {
                    1, 0, 0, x,
                    0, 1, 0, y,
                    0, 0, 1, z,
                    0, 0, 0, 1
                }
            };
        }

        // Creates a uniform scaling transform
        public static Mat4 UniformScale(double scale)
        {
            return new Mat4
            {
                values = new double[] 
                {
                    scale,  0,      0,      0,
                    0,      scale,  0,      0,
                    0,      0,      scale,  0,
                    0,      0,      0,      1
                }
            };
        }

        public static Mat4 Scale(double x, double y, double z)
        {
            return new Mat4
            {
                values = new double[] 
                {
                    x,  0,  0,  0,
                    0,  y,  0,  0,
                    0,  0,  z,  0,
                    0,  0,  0,  1
                }
            };
        }


        // Gets the transpose of this 4x4 matrix
        // Equivalent to switching to/from row-major to column-major
        // Note that this is a necessary step in order to get the sort of matrix that OpenGL needs
        public Mat4 Transpose
        {
            get
            {
                return new Mat4 
                {
                    values = new double[] 
                    {
                        values[0], values[4], values[8], values[12],
                        values[1], values[5], values[9], values[13],
                        values[2], values[6], values[10], values[14],
                        values[3], values[7], values[11], values[15]
                    }
                };
            }
        }

        // Transforms a 4-component vector by this Mat4
        // Returns the xyz of the result divided by the w of the result
        public Vec3 TransformVec3(Vec3 xyz, double w)
        {
            Vec4 result = this * Vec4.FromVec3(xyz, w);
            return result.w == 0 ? result.Xyz : result.Xyz / result.w;
        }
    }
}