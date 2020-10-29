using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace USP_Hydrology
{
    public partial class Pollutogram
    {
        public MassConcentration[] ConstantLoadPollutogram;
        public MassConcentration[] WashoffPollutogram;
        public MassConcentration[] TotalPollutogram;
        public MassConcentration[] UpstreamPollutogram;
    }
}
