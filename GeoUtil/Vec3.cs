using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtil
{
    public struct Vec3
    {
        public double X, Y, Z;

        public Vec3(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
            => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vec3 operator -(Vec3 a, Vec3 b)
            => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vec3 operator *(Vec3 v, double s)
            => new Vec3(v.X * s, v.Y * s, v.Z * s);
    }
}
