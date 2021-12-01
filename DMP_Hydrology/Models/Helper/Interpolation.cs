using System;
using System.Linq;

namespace USP_Hydrology
{
    public static partial class Helper
    {
        public static Double InterpolationExtrapolation_Linear_Right(this (Double X, Double Y)[] XY, Double X)
        {
            var N = XY.Count();
            if (X < XY[0].X)
                return XY[1].Y - (XY[1].X - X) * (XY[1].Y - XY[0].Y) / (XY[1].X - XY[0].X);
            if (XY[N - 1].X < X)
                return XY[N - 2].Y + (X - XY[N - 2].X) * (XY[N - 1].Y - XY[N - 2].Y) / (XY[N - 1].X - XY[N - 2].X);
            for (Int32 i = 0; i < XY.Count(); i++)
            {
                if (XY[i].X == X)
                    return XY[i].Y;
                if (i > 0 && XY[i - 1].X < X && X < XY[i].X)
                    return XY[i - 1].Y + (X - XY[i - 1].X) * (XY[i].Y - XY[i - 1].Y) / (XY[i].X - XY[i - 1].X);
            }
            return Double.NaN;
        }
        public static Double InterpolationExtrapolation_Linear_Left(this (Double X, Double Y)[] XY, Double Y)
        {
            var N = XY.Count();
            if (Y < XY[0].Y)
                return XY[1].X - (XY[1].Y - Y) * (XY[1].X - XY[0].X) / (XY[1].Y - XY[0].Y);
            if (XY[N - 1].Y < Y)
                return XY[N - 2].X + (Y - XY[N - 2].Y) * (XY[N - 1].X - XY[N - 2].X) / (XY[N - 1].Y - XY[N - 2].Y);
            for (Int32 i = 0; i < N; i++)
            {
                if (XY[i].Y == Y)
                    return XY[i].X;
                if (i > 0 && XY[i - 1].Y < Y && Y < XY[i].Y)
                    return XY[i - 1].X + (Y - XY[i - 1].Y) * (XY[i].X - XY[i - 1].X) / (XY[i].Y - XY[i - 1].Y);
            }
            return Double.NaN;
        }

        public static Double Interpolation_Linear_Right(this (Double X, Double Y)[] XY, Double X)
        {
            var N = XY.Count();
            for (Int32 i = 0; i < XY.Count(); i++)
            {
                if (XY[i].X == X)
                    return XY[i].Y;
                if (i > 0 && XY[i - 1].X < X && X < XY[i].X)
                    return XY[i - 1].Y + (X - XY[i - 1].X) * (XY[i].Y - XY[i - 1].Y) / (XY[i].X - XY[i - 1].X);
            }
            return Double.NaN;
        }
        public static Double Interpolation_Linear_Left(this (Double X, Double Y)[] XY, Double Y)
        {
            var N = XY.Count();
            for (Int32 i = 0; i < N; i++)
            {
                if (XY[i].Y == Y)
                    return XY[i].X;
                if (i > 0 && XY[i - 1].Y < Y && Y < XY[i].Y)
                    return XY[i - 1].X + (Y - XY[i - 1].Y) * (XY[i].X - XY[i - 1].X) / (XY[i].Y - XY[i - 1].Y);
            }
            return Double.NaN;
        }

        public static Double Interpolation_Lagrange_Right(this (Double X, Double Y)[] XY, Double X)
        {
            Double Sum = 0;
            for (Int32 i = 0, n = XY.Length; i < n; i++)
            {
                if (XY[i].X == X) return XY[i].Y;
                Double Product = XY[i].Y;
                for (Int32 j = 0; j < n; j++)
                {
                    if (i == j || XY[i].X == XY[j].X) continue;
                    Product *= (X - XY[i].X) / (XY[i].X - XY[j].X);
                }
                Sum += Product;
            }
            return Sum;
        }
        public static Double Interpolation_Lagrange_Left(this (Double X, Double Y)[] XY, Double Y)
        {
            Double Sum = 0;
            for (Int32 i = 0, n = XY.Length; i < n; i++)
            {
                if (XY[i].Y == Y) return XY[i].X;
                Double Product = XY[i].X;
                for (Int32 j = 0; j < n; j++)
                {
                    if (i == j || XY[i].Y == XY[j].Y) continue;
                    Product *= (Y - XY[i].Y) / (XY[i].Y - XY[j].Y);
                }
                Sum += Product;
            }
            return Sum;
        }
    }
}
