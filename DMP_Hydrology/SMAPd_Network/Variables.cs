using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace USP_Hydrology
{
    public partial class SMAPd_Network
    {
        public class SMAPd_Input
        {
            public DateTime[] Time;
            public Length[] Precipitation;
            public Length[] Evapotranspiration;
            public VolumeFlow[] ObservedFlow;
            public VolumeFlow[] UpstreamFlow;
        }

        public SMAPd_Input GetInput;
        public Model_SMAPd SMAPSimulation;        
    }
}
