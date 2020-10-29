using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Muskingum_Daniel
    {
        public double[] FLT_Arr_InputFlow;
        //public double[] FLT_Arr_OutputFlow;

        public double FLT_K; // > Timestep/2
        public double FLT_X; // <= 0.5
        public double FLT_Timestep;

        private double C1()
        {
            return (FLT_Timestep - 2 * FLT_K * FLT_X) / (2 * FLT_K * (1 - FLT_X) + FLT_Timestep);
        }

        private double C2()
        {
            return (FLT_Timestep + 2 * FLT_K * FLT_X) / (2 * FLT_K * (1 - FLT_X) + FLT_Timestep);
        }

        private double C3()
        {
            return (2 * FLT_K * (1 - FLT_X) - FLT_Timestep) / (2 * FLT_K * (1 - FLT_X) + FLT_Timestep);
        }
    }
}
