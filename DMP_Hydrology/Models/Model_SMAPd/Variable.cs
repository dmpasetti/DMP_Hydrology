using System;
using UnitsNet;

namespace USP_Hydrology
{
    public partial class Model_SMAPd : Model_SMAP
    {
        public new class ModelParameters : Model_SMAP.ModelParameters
        {
            public Length SaturationCapacity;
            public Length InitialAbstraction;
            public Ratio FieldCapacity;
            public Ratio GroundwaterRecharge;
            public Duration DirectRunoffHalf;
            public Duration BaseflowHalf;
            public Ratio DirectRunoffRecession { get => Ratio.FromDecimalFractions(Math.Pow(0.5D, 1D / DirectRunoffHalf.Days)); }
            public Ratio BaseflowRecession { get => Ratio.FromDecimalFractions(Math.Pow(0.5D, 1D / BaseflowHalf.Days)); }
        }

        public new class SimulationInstant : Model_SMAP.SimulationInstant
        {
            public Ratio SoilMoisture;
            public Length SurfaceReservoirLevel;
            public Length SoilReservoirLevel;
            public Length GroundwaterReservoirLevel;
            public Length Precipitation;
            public Length PotentialEvapotranspiration;
            public Length RealEvapotranspiration;
            public Length Infiltration;
            public Length GroundwaterRecharge;
            public Length SurfaceRunoff;
            public Length SoilTranshipmentRunoff;
            public Length DirectRunoff;
            public Length GroundwaterRunoff;
            public Length Runoff;
            public Length Storage;
            public Length InflowRunoff;
            public Length OutflowRunoff;
            public Length UpstreamRunoff;
            public Length RoutingRunoff;
            public Length DownstreamRunoff;
        }

        public class SimulationReport
        {
            public Ratio MeanSoilMoisture;
            public Length MeanSurfaceReservoirLevel;
            public Length MeanSoilReservoirLevel;
            public Length MeanGroundwaterReservoirLevel;
            public Length TotalPrecipitation;
            public Length TotalPotentialEvapotranspiration;
            public Length TotalRealEvapotranspiration;
            public Length TotalInfiltration;
            public Length TotalGroundwaterRecharge;
            public Length TotalSurfaceRunoff;
            public Length TotalSoilTranshipmentRunoff;
            public Length TotalDirectRunoff;
            public Length TotalGroundwaterRunoff;
            public Length TotalRunoff;
            public Length TotalStorage;
            public Length TotalInflowRunoff;
            public Length TotalOutflowRunoff;
            public Length TotalUpstreamRunoff;
            public Length TotalRoutingRunoff;
            public Length TotalDownstreamRunoff;
        }
    }
}
