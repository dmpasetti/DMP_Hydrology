using System;
using System.Linq;

namespace USP_Hydrology
{
    public static partial class Helper
    {
        public static Double Statistic_StdDev(this Double[] X)
        {
            if (X.Count() == 0) throw new ArgumentException();

            var Avg = X.Average();
            var Sum = X.Sum(v => Math.Pow(v - Avg, 2D));
            var StdDev = Math.Sqrt(Sum / (X.Count() - 1));

            return StdDev;
        }

        public static Double Statistic_R(this (Double X, Double Y)[] XY)
        {
            return Math.Sqrt(XY.Statistic_RSquared());
        }

        public static Double Statistic_RSquared(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var N = 0D;
            var D = 0D;
            var Xm = XY.Average(xy => xy.X);
            for (int i = 0; i < XY.Count(); i++)
            {
                N += Math.Pow(XY[i].X - XY[i].Y, 2D);
                D += Math.Pow(XY[i].X - Xm, 2D);
            }
            var RSquared = 1D - N / D;

            return RSquared;
        }
    }
}
