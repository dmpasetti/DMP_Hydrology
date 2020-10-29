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

    }
}
