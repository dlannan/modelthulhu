using System;

namespace Modelthulhu
{
    // Utility class for generating random stuff,
    // including 3d stuff, of the random variety
    public static class Random3D
    {
        // 'Random' requires an instance, we do not
        private static Random random = new Random();

        // Get a random double-precision number on the specified range
        public static double Rand(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }

        // Get a random double-precision number between 0 and the specified maximum
        public static double Rand(double max)
        {
            return random.NextDouble() * max;
        }

        // Get a random double-precision number between 0 and 1
        public static double Rand()
        {
            return random.NextDouble();
        }

        // Gets a random integer, via the built in Random.Next() function
        public static int RandInt()
        {
            return random.Next();
        }

        // Gets a random integer on the specified range (including both the minimum and maximum as possible values)
        public static int RandInt(int min, int max)
        {
            return random.Next() % (max - min + 1) + min;
        }

        // Gets a random integer between 0 and maxPlusOne, including zero but not maxPlusOne
        public static int RandInt(int maxPlusOne)
        {
            return random.Next() % maxPlusOne;
        }

        // Generates a vector with the specified magnitude, with random direction 
        public static Vec3 RandomNormalizedVector(double len)
        {
            while (true)
            {
                double x = Rand(-1, 1), y = Rand(-1, 1), z = Rand(-1, 1);
                double mag_sq = x * x + y * y + z * z;
                if (mag_sq == 0 || mag_sq > 1)
                    continue;
                double inv = len / Math.Sqrt(mag_sq);
                return new Vec3 { x = x * inv, y = y * inv, z = z * inv };
            }
        }

        // Generates a vector with the specified magnitude, with random direction 
        public static Vec2 RandomNormalizedVec2(double len)
        {
            while (true)
            {
                double x = Rand(-1, 1), y = Rand(-1, 1);
                double mag_sq = x * x + y * y;
                if (mag_sq == 0 || mag_sq > 1)
                    continue;
                double inv = len / Math.Sqrt(mag_sq);
                return new Vec2 { x = x * inv, y = y * inv };
            }
        }

        // Generates a rotation matrix randomly
        public static Mat3 RandomRotationMatrix()
        {
            Mat3 temp = Mat3.Identity;
            for (int i = 0; i < 3; i++)
            {
                Vec3 axis = RandomNormalizedVector(1.0);
                temp *= Mat3.FromAxisAngle(axis.x, axis.y, axis.z, Rand(16.0 * Math.PI));
            }
            return temp;
        }

        public static Quaternion RandomQuaternionRotation()
        {
            Quaternion temp = Quaternion.Identity;
            for (int i = 0; i < 3; i++)
            {
                Vec3 axis = RandomNormalizedVector(1.0);
                temp *= Quaternion.FromAxisAngle(axis.x, axis.y, axis.z, Rand(16.0 * Math.PI));
            }
            return temp;
        }
    }
}
