using System;
using UnitsNet;

namespace USP_Hydrology
{
    public partial class Model_Muskingum
    {
        public class ModelParameters
        {
            public Duration TimeStep;
            public Duration TravelTime;
            public Ratio WeightingFactor;
        }

        public class InitialConditions
        {
            public VolumeFlow Channel;
        }

        public class SimulationInstant
        {
            public Duration Time;
            public VolumeFlow Inflow;
            public VolumeFlow Outflow;
        }
    }
}
