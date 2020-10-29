using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Muskingum_Daniel
    {

        

        private bool ValidParameters()
        {
            if(FLT_Timestep > FLT_K)
            {
                return false;
            }
            if (FLT_X > 0.5)
            {
                return false;
            }

            if (FLT_X > (FLT_Timestep / (2 * FLT_K)))
            {
                return false;
            }

            if ((1 - FLT_X) < (FLT_Timestep / (2 * FLT_K)))
            {
                return false;
            }

            return true;
        }

        public static Muskingum_Daniel[] DivideReach(Muskingum_Daniel Sim)
        {
            List<Muskingum_Daniel> lstMusk = new List<Muskingum_Daniel>();
            var N = (int)Math.Floor(Sim.FLT_K / Sim.FLT_Timestep);
            var Kn = Sim.FLT_Timestep;
            var Ke = Sim.FLT_K - N * Kn;
            if (N == 0) N = 1;
            for(int n = 0; n < N; n++)
            {
                lstMusk.Add(new Muskingum_Daniel
                {
                    FLT_Timestep = Sim.FLT_Timestep,
                    FLT_K = Kn,
                    FLT_X = Sim.FLT_X,
                    FLT_Arr_InputFlow = n == 0 ? Sim.FLT_Arr_InputFlow : null
                });
            }
            if(Ke < 0D)
            {
                lstMusk.Add(new Muskingum_Daniel
                {
                    FLT_Timestep = Sim.FLT_Timestep,
                    FLT_K = Ke,
                    FLT_X = Sim.FLT_X
                });
            }
            return lstMusk.ToArray();
        }



        public static double[] DampingSimulation(Muskingum_Daniel[] Sim)
        {
            for(int i = 0; i < Sim.Count(); i++)
            {
                if(i < Sim.Count() - 1)
                {
                    Sim[i + 1].FLT_Arr_InputFlow = ProcessDamping(Sim[i]);
                }
                else
                {
                    return ProcessDamping(Sim[i]);
                }
            }
            return null;
        }


        public static double[] ProcessDamping(Muskingum_Daniel Sim)
        {
            double[] Outflow = new double[Sim.FLT_Arr_InputFlow.Length];
            if (true /*Sim.ValidParameters()*/)
            {
                double C1 = Sim.C1();
                double C2 = Sim.C2();
                double C3 = Sim.C3();

                for (int i = 0; i < Sim.FLT_Arr_InputFlow.Length; i++)
                {
                    if (i == 0)
                    {
                        Outflow[i] = Sim.FLT_Arr_InputFlow[i];
                    }
                    else
                    {
                        Outflow[i] = C1 * Sim.FLT_Arr_InputFlow[i] + C2 * Sim.FLT_Arr_InputFlow[i - 1] + C3 * Outflow[i - 1];
                    }
                }
                return Outflow;
            }
            /*else
            {
                for (int i = 0; i < Sim.FLT_Arr_InputFlow.Length; i++)
                {
                    Outflow[i] = double.PositiveInfinity;
                }
                return Outflow;
            }*/
        }



    }
}
