using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Reservoir_0D
    {

        public static void SimulateQuality(Reservoir_0D Sim)
        {
            Sim.SimCount = Sim.GetInput.Volume.Count();

            for(int i = 0; i < Sim.SimCount; i++)
            {
                var _In = Sim.GetInput;
                var _Out = Sim.GetOutput;
                var _Param = Sim.GetParam;
                if(i == 0)
                {
                    _Out.Concentration_Out[i] = _In.Concentration_In[i];
                }
                else
                {
                    _Out.Concentration_Out[i] = (_Out.Concentration_Out[i - 1] + (Sim.Timestep / _In.Volume[i]) * (_In.Inflow[i] * _In.Concentration_In[i] + _In.Load[i])) 
                        / (1 + (Sim.Timestep / _In.Volume[i]) * 
                        (_In.Outflow[i] + _Param.Reaction_Coef * _In.Volume[i] + _Param.SettlingVelocity * _In.ContactArea[i] + ((_In.Volume[i] - _In.Volume[i - 1]) / Sim.Timestep)));
                }
                
            }


        }

    }
}
