using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Buildup_Washoff
    {
        public static double TotalPeriodLoad(Buildup_Washoff Sim)
        {
            Console.WriteLine("Buildup: {0}", Sim.FLT_Arr_Buildup.Sum());
            Console.ReadKey();
            return Sim.FLT_Arr_Buildup.Sum();
        }

        public static double TotalPeriodWashoff(Buildup_Washoff Sim)
        {
            return Sim.FLT_Arr_EffectiveWashoff.Sum();
        }

    }
}
