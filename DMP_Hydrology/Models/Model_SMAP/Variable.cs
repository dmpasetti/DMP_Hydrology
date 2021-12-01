using System;
using UnitsNet;

namespace USP_Hydrology
{
    public abstract class Model_SMAP
    {
        public class SpatialFeatures
        {
            public Area DrainageArea;
            public Boolean Start;
            public Boolean End;
        }

        public class ModelParameters
        {
            public Ratio PrecipitationWeighting;
            public Ratio EvapotranspirationWeighting;
        }

        public class InitialConditions
        {
            public Ratio SoilMoisture;
            public VolumeFlow Baseflow;
        }

        public class SimulationInstant
        {
            public Duration Time;
            public VolumeFlow Produced;
            public VolumeFlow Inflow;
            public VolumeFlow Outflow;
            public VolumeFlow Upstream;
            public VolumeFlow Routing;
            public VolumeFlow Downstream;
            public VolumeFlow Baseflow;
        }
    }
}
