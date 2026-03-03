using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtil
{
    public class InterpolateVec3
    {
        static double SecondsBetween(DateTime t0, DateTime t)
        {
            return (t - t0).TotalSeconds;
        }

        public static Vec3 LinearInterpolate(
            DateTime t1, Vec3 p1,
            DateTime t2, Vec3 p2,
            DateTime tNew)
        {
            double dtTotal = (t2 - t1).TotalSeconds;
            double dtNew = (tNew - t1).TotalSeconds;

            double alpha = dtNew / dtTotal;
            return p1 + (p2 - p1) * alpha;
        }

        public static Vec3 QuadraticInterpolate(
                    DateTime t1, Vec3 p1,
                    DateTime t2, Vec3 p2,
                    DateTime t3, Vec3 p3,
                    DateTime tNew)
        {
            double T1 = 0.0;
            double T2 = (t2 - t1).TotalSeconds;
            double T3 = (t3 - t1).TotalSeconds;
            double TN = (tNew - t1).TotalSeconds;

            double L1 = (TN - T2) * (TN - T3) / ((T1 - T2) * (T1 - T3));
            double L2 = (TN - T1) * (TN - T3) / ((T2 - T1) * (T2 - T3));
            double L3 = (TN - T1) * (TN - T2) / ((T3 - T1) * (T3 - T2));

            return p1 * L1 + p2 * L2 + p3 * L3;
        }

    }
}
