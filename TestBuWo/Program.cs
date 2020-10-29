using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;

namespace TestBuWo
{
    class Program
    {
        



        static void Main(string[] args)
        {
            double[] FLT_Arr_SurfaceFlow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7.28, 6.34, 5.51, 4.80, 4.18, 3.64, 3.17, 2.75, 2.40, 2.09, 1.82, 1.58, 1.37, 1.20, 1.04, 0.91, 0.79, 0.68, 0.60, 0.52 };
            Buildup_Washoff BuWoSim = new Buildup_Washoff
            {
                //FLT_Area = 100,
                //FLT_ThresholdFlow = 2,
                //FLT_Arr_SurfaceFlow = FLT_Arr_SurfaceFlow,
                //FLT_Timestep_h = 24,
                //FLT_BMax = 10,
                //FLT_Kb = 0.1,
                //FLT_Nb = 0.5,
                //FLT_Kw = 0.3,
                //FLT_Nw = 0.5,
                //FLT_InitialBuildup = 0,
                //INT_BuMethod = Buildup_Washoff.BuildupMethod.Exp,
                //INT_WoMethod = Buildup_Washoff.WashoffMethod.Exp
            };

            Buildup_Washoff.ProcessBuWo(BuWoSim);
            Console.ReadKey();
        }
    }
}
