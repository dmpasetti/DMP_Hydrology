using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Reservoir_0D
    {
        public double Timestep;
        public int SimCount;
        public Input GetInput;
        public Parameters GetParam;
        public Output GetOutput;
        public class Input
        {
            public double[] Volume;
            public double[] Inflow;
            public double[] Outflow;
            public double[] Concentration_In;
            public double[] Load;
            public double[] ContactArea;
        }
        public class Parameters
        {
            public double Reaction_Coef;            
            public double SettlingVelocity;
        }       
        public class Output
        {
            public double[] Concentration_Out;
        }
    }
}
