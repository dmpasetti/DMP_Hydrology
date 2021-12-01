using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class SMAPd_Network
    {
        public static double SMAPNashSutcliffe(SMAPd_Network Smap)
        {
            double[] Observed = Smap.GetInput.ObservedFlow.Select(x => x.CubicMetersPerSecond).ToArray();
            double[] Calculated = Smap.SMAPSimulation.GetSimulation.Select(x => x.Downstream.CubicMetersPerSecond).ToArray();

            double SquareSumupper = 0;
            double SquareSumLower = 0;
            double MeanObserved = Observed.Average();

            for (int i = 0; i < Observed.Length; i++)
            {
                SquareSumupper += Math.Pow(Calculated[i] - Observed[i], 2);
                SquareSumLower += Math.Pow(Observed[i] - MeanObserved, 2);
            }
            return 1 - (SquareSumupper / SquareSumLower);
        }

        public static bool ValidSimulation()
        {
            return true;
        }

        public static double[] AggregateSeriesMonthlyAverage(DateTime[] dates, double[] vector)
        {
            DateTime firstMonth = new DateTime(dates[0].Year, dates[0].Month, 1);
            DateTime lastMonth = new DateTime(dates[dates.Length - 1].Year, dates[dates.Length - 1].Month, 1);

            List<double> lstAggregate = new List<double>();
            for(DateTime i = firstMonth; i <= lastMonth; i = i.AddMonths(1))
            {
                int firstIndex = Array.IndexOf(dates, i);
                int lastIndex = Array.IndexOf(dates, i.AddMonths(1).AddDays(-1));

                double[] vectorData = vector.Skip(firstIndex).Take(lastIndex).ToArray();
                lstAggregate.Add(vectorData.Average());
            }
            return lstAggregate.ToArray();
        }

        public static double[] AggregateSeriesMonthlySum(DateTime[] dates, double[] vector)
        {
            DateTime firstMonth = new DateTime(dates[0].Year, dates[0].Month, 1);
            DateTime lastMonth = new DateTime(dates[dates.Length - 1].Year, dates[dates.Length - 1].Month, 1);

            List<double> lstAggregate = new List<double>();
            for (DateTime i = firstMonth; i <= lastMonth; i = i.AddMonths(1))
            {
                int firstIndex = Array.IndexOf(dates, i);
                int lastIndex = Array.IndexOf(dates, i.AddMonths(1).AddDays(-1));

                double[] vectorData = vector.Skip(firstIndex).Take(lastIndex).ToArray();
                lstAggregate.Add(vectorData.Sum());
            }
            return lstAggregate.ToArray();
        }

    }
}
