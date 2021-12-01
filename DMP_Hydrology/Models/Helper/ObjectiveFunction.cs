using System;
using System.Linq;

namespace USP_Hydrology
{
    public static partial class Helper
    {
        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_CORR_Lag0CrossCorrelationCoefficient(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 0D;
            var Worst = Double.MaxValue;

            var N = 0D;
            var DL = 0D;
            var DR = 0D;
            var Xm = XY.Average(xy => xy.X);
            var Ym = XY.Average(xy => xy.Y);
            for (int i = 0; i < XY.Count(); i++)
            {
                N += (XY[i].X - Xm) * (XY[i].Y - Ym);
                DL += Math.Pow(XY[i].X - Xm, 2D);
                DR += Math.Pow(XY[i].Y - Xm, 2D);
            }
            var CORR = N / Math.Sqrt(DL * DR);

            return (Best, Worst, CORR);
        }

        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_NSE_NashSutcliffeEfficiency(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 1D;
            var Worst = Double.MinValue;

            var N = 0D;
            var D = 0D;
            var Xm = XY.Average(xy => xy.X);
            for (int i = 0; i < XY.Count(); i++)
            {
                N += Math.Pow(XY[i].X - XY[i].Y, 2D);
                D += Math.Pow(XY[i].X - Xm, 2D);
            }
            var NSE = 1D - N / D;

            return (Best, Worst, NSE);
        }

        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_PWRMSE_PeakWeightedRootMeanSquareError(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 0D;
            var Worst = Double.MaxValue;

            var S = 0D;
            var Xm = XY.Average(xy => xy.X);
            for (int i = 0; i < XY.Count(); i++)
                S += Math.Pow(XY[i].X - XY[i].Y, 2D) * ((XY[i].X - Xm) / (2D * Xm));
            var PWRMSE = Math.Sqrt(S / XY.Count());

            return (Best, Worst, PWRMSE);
        }

        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_RBIAS_RelativeBias(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 0D;
            var Worst = Double.MaxValue;

            var S = 0D;
            for (int i = 0; i < XY.Count(); i++)
                S += (XY[i].Y - XY[i].X) / XY[i].X;
            var RBIAS = 100D * S / XY.Count();

            return (Best, Worst, RBIAS);
        }

        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_RRMSE_RelativeRootMeanSquareError(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 0D;
            var Worst = Double.MaxValue;

            var S = 0D;
            for (int i = 0; i < XY.Count(); i++)
                S += Math.Pow((XY[i].Y - XY[i].X) / XY[i].X, 2D);
            var RRMSE = 100D * Math.Sqrt(S / XY.Count());

            return (Best, Worst, RRMSE);
        }

        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_SAR_SumAbsoluteResiduals(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 0D;
            var Worst = Double.MaxValue;

            var SAR = 0D;
            for (int i = 0; i < XY.Count(); i++)
                SAR += Math.Abs(XY[i].X - XY[i].Y);

            return (Best, Worst, SAR);
        }

        public static (Double Best, Double Worst, Double Value) ObjectiveFunction_SSR_SumSquaredResiduals(this (Double X, Double Y)[] XY)
        {
            if (XY.Count() == 0) throw new ArgumentException();

            var Best = 0D;
            var Worst = Double.MaxValue;

            var SSR = 0D;
            for (int i = 0; i < XY.Count(); i++)
                SSR += Math.Pow(XY[i].X - XY[i].Y, 2D);

            return (Best, Worst, SSR);
        }

        public static Double ReRange_Log(this (Double Best, Double Worst, Double Value) Info)
        {
            if (Info.Best < Info.Worst)
                return 1D / Math.Log10(10D + Info.Value);
            else
                return 1D / Math.Log10(10D + (Info.Best - Info.Value));
        }
    }
}
