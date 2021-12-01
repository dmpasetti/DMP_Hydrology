using System;
using System.Collections.Generic;
using System.Linq;
using UnitsNet;
using Muskingum = USP_Hydrology.Model_Muskingum.SimulationInstant;

namespace USP_Hydrology
{
    public partial class Model_SMAPd
    {
        public SpatialFeatures GetSpatialFeatures { get; private set; }
        public ModelParameters GetModelParameters { get; private set; }
        public InitialConditions GetInitialConditions { get; private set; }
        public Int32 GetCount { get; private set; }
        public Boolean GetCompose { get; }

        private List<SimulationInstant> _Simulation;
        public SimulationInstant[] GetSimulation
        {
            get
            {
                return _Simulation.ToArray();
            }
        }

        public Model_SMAPd(Boolean Compose)
        {
            GetCompose = Compose;
        }

        public void SimulationStart(SpatialFeatures SpatialFeatures, ModelParameters ModelParameters, InitialConditions InitialConditions)
        {
            if (GetCompose) throw new MethodAccessException();

            GetSpatialFeatures = SpatialFeatures;
            GetModelParameters = ModelParameters;
            GetInitialConditions = InitialConditions;

            _Simulation = new List<SimulationInstant>();
            GetCount = 0;
        }

        public Int32 SimulationNext(Length Precipitation, Length PotentialEvapotranspiration, Muskingum Stream, VolumeFlow Inflow, VolumeFlow Outflow)
        {
            if (GetCompose) throw new MethodAccessException();
            if (Stream is null) Stream = new Muskingum() { Inflow = VolumeFlow.Zero, Outflow = VolumeFlow.Zero };

            var SI = new SimulationInstant();

            Length SurfaceReservoirLevel;
            Length SoilReservoirLevel;
            Length GroundwaterReservoirLevel;

            if (GetCount == 0)
            {
                SurfaceReservoirLevel = Length.FromMillimeters(0D);
                SoilReservoirLevel = GetModelParameters.SaturationCapacity.Multiplication(GetInitialConditions.SoilMoisture);
                GroundwaterReservoirLevel = Flow2Runoff(GetInitialConditions.Baseflow).Division(1D.Subtraction(GetModelParameters.BaseflowRecession));
            }
            else
            {
                SurfaceReservoirLevel = _Simulation[GetCount - 1].SurfaceReservoirLevel;
                SoilReservoirLevel = _Simulation[GetCount - 1].SoilReservoirLevel;
                GroundwaterReservoirLevel = _Simulation[GetCount - 1].GroundwaterReservoirLevel;
            }

            SI.Time = Duration.FromDays(GetCount + 1);
            SI.SoilMoisture = SoilReservoirLevel.Division(GetModelParameters.SaturationCapacity);
            SI.Precipitation = Precipitation.Multiplication(GetModelParameters.PrecipitationWeighting);
            SI.PotentialEvapotranspiration = PotentialEvapotranspiration.Multiplication(GetModelParameters.EvapotranspirationWeighting);
            var SurfaceRunoffWithoutTranshipment = (SI.Precipitation > GetModelParameters.InitialAbstraction)
                                                   ? SI.Precipitation.Subtraction(GetModelParameters.InitialAbstraction).Power2().Division(SI.Precipitation.Subtraction(GetModelParameters.InitialAbstraction).Addition(GetModelParameters.SaturationCapacity).Subtraction(SoilReservoirLevel))
                                                   : Length.FromMillimeters(0D);
            SI.RealEvapotranspiration = (SI.Precipitation.Subtraction(SurfaceRunoffWithoutTranshipment) > SI.PotentialEvapotranspiration)
                                        ? SI.PotentialEvapotranspiration
                                        : SI.Precipitation.Subtraction(SurfaceRunoffWithoutTranshipment).Addition(SI.PotentialEvapotranspiration.Subtraction(SI.Precipitation).Addition(SurfaceRunoffWithoutTranshipment).Multiplication(SI.SoilMoisture));
            SI.GroundwaterRecharge = (SoilReservoirLevel > GetModelParameters.SaturationCapacity.Multiplication(GetModelParameters.FieldCapacity))
                                     ? SoilReservoirLevel.Subtraction(GetModelParameters.SaturationCapacity.Multiplication(GetModelParameters.FieldCapacity)).Multiplication(GetModelParameters.GroundwaterRecharge).Multiplication(SI.SoilMoisture)
                                     : Length.FromMillimeters(0D);
            var SoilReservoirLevelWithoutTranshipment = SoilReservoirLevel.Addition(SI.Precipitation).Subtraction(SurfaceRunoffWithoutTranshipment).Subtraction(SI.RealEvapotranspiration).Subtraction(SI.GroundwaterRecharge);
            SI.SoilReservoirLevel = (SoilReservoirLevelWithoutTranshipment < GetModelParameters.SaturationCapacity)
                                    ? SoilReservoirLevelWithoutTranshipment
                                    : GetModelParameters.SaturationCapacity;
            SI.SoilTranshipmentRunoff = SoilReservoirLevelWithoutTranshipment.Subtraction(SI.SoilReservoirLevel);
            SI.SurfaceRunoff = SurfaceRunoffWithoutTranshipment.Addition(SI.SoilTranshipmentRunoff);
            SI.Infiltration = SI.Precipitation.Subtraction(SI.SurfaceRunoff);
            SI.DirectRunoff = SurfaceReservoirLevel.Addition(SI.SurfaceRunoff).Multiplication(1D.Subtraction(GetModelParameters.DirectRunoffRecession));
            SI.SurfaceReservoirLevel = SurfaceReservoirLevel.Addition(SI.SurfaceRunoff).Subtraction(SI.DirectRunoff);
            SI.GroundwaterRunoff = GroundwaterReservoirLevel.Multiplication(1D.Subtraction(GetModelParameters.BaseflowRecession));
            SI.GroundwaterReservoirLevel = GroundwaterReservoirLevel.Addition(SI.GroundwaterRecharge).Subtraction(SI.GroundwaterRunoff);
            SI.Runoff = SI.DirectRunoff.Addition(SI.GroundwaterRunoff);
            SI.Storage = SI.Precipitation.Subtraction(SI.RealEvapotranspiration).Subtraction(SI.Runoff);
            SI.InflowRunoff = Flow2Runoff(Inflow);
            SI.OutflowRunoff = Flow2Runoff(Outflow);
            SI.UpstreamRunoff = Flow2Runoff(Stream.Inflow);
            var Routing = Stream.Outflow.Subtraction(Stream.Inflow);
            SI.RoutingRunoff = Flow2Runoff(Routing);
            SI.DownstreamRunoff = SI.UpstreamRunoff.Addition(SI.RoutingRunoff).Addition(SI.Runoff).Addition(SI.InflowRunoff).Subtraction(SI.OutflowRunoff).Maximum(Length.Zero);
            if (SI.DownstreamRunoff == Length.Zero)
                SI.OutflowRunoff = SI.UpstreamRunoff.Addition(SI.RoutingRunoff).Addition(SI.Runoff).Addition(SI.InflowRunoff).Subtraction(SI.DownstreamRunoff);
            SI.Produced = Runoff2Flow(SI.Runoff);
            SI.Inflow = Inflow;
            SI.Outflow = Runoff2Flow(SI.OutflowRunoff);
            SI.Upstream = Stream.Inflow;
            SI.Routing = Routing;
            SI.Downstream = Runoff2Flow(SI.DownstreamRunoff);
            SI.Baseflow = Runoff2Flow(SI.GroundwaterRunoff);

            _Simulation.Add(SI);
            GetCount += 1;

            return GetCount;
        }

        private Model_SMAPd[] _Items;
        public void ComposeStart(Model_SMAPd[] Items)
        {
            _Items = Items;

            GetSpatialFeatures = new SpatialFeatures();
            GetModelParameters = new ModelParameters();
            GetInitialConditions = new InitialConditions();

            _Simulation = new List<SimulationInstant>();
            GetCount = 0;

            foreach (var Item in _Items)
                GetSpatialFeatures.DrainageArea += Item.GetSpatialFeatures.DrainageArea;
            foreach (var Item in _Items)
            {
                var Ratio = Item.GetSpatialFeatures.DrainageArea / GetSpatialFeatures.DrainageArea;

                // Spatial Features
                if (Item.GetSpatialFeatures.Start) GetSpatialFeatures.Start = true;
                if (Item.GetSpatialFeatures.End) GetSpatialFeatures.End = true;

                // Model Parameters
                GetModelParameters.PrecipitationWeighting += Ratio * Item.GetModelParameters.PrecipitationWeighting;
                GetModelParameters.EvapotranspirationWeighting += Ratio * Item.GetModelParameters.EvapotranspirationWeighting;
                GetModelParameters.SaturationCapacity += Ratio * Item.GetModelParameters.SaturationCapacity;
                GetModelParameters.InitialAbstraction += Ratio * Item.GetModelParameters.InitialAbstraction;
                GetModelParameters.FieldCapacity += Ratio * Item.GetModelParameters.FieldCapacity;
                GetModelParameters.GroundwaterRecharge += Ratio * Item.GetModelParameters.GroundwaterRecharge;
                GetModelParameters.DirectRunoffHalf += Ratio * Item.GetModelParameters.DirectRunoffHalf;
                GetModelParameters.BaseflowHalf += Ratio * Item.GetModelParameters.BaseflowHalf;

                // Initial Conditions
                GetInitialConditions.SoilMoisture += Ratio * Item.GetInitialConditions.SoilMoisture;
                GetInitialConditions.Baseflow += Item.GetInitialConditions.Baseflow;
            }
        }

        public Int32 ComposeNext()
        {
            if (!GetCompose) throw new MethodAccessException();

            var SI = new SimulationInstant();

            foreach (var Item in _Items)
            {
                var Ratio = Item.GetSpatialFeatures.DrainageArea / GetSpatialFeatures.DrainageArea;

                // Simulation
                SI.Time = Item.GetSimulation[GetCount].Time;
                SI.SoilMoisture += Ratio * Item.GetSimulation[GetCount].SoilMoisture;
                SI.SurfaceReservoirLevel += Ratio * Item.GetSimulation[GetCount].SurfaceReservoirLevel;
                SI.SoilReservoirLevel += Ratio * Item.GetSimulation[GetCount].SoilReservoirLevel;
                SI.GroundwaterReservoirLevel += Ratio * Item.GetSimulation[GetCount].GroundwaterReservoirLevel;
                SI.Precipitation += Ratio * Item.GetSimulation[GetCount].Precipitation;
                SI.PotentialEvapotranspiration += Ratio * Item.GetSimulation[GetCount].PotentialEvapotranspiration;
                SI.RealEvapotranspiration += Ratio * Item.GetSimulation[GetCount].RealEvapotranspiration;
                SI.Infiltration += Ratio * Item.GetSimulation[GetCount].Infiltration;
                SI.GroundwaterRecharge += Ratio * Item.GetSimulation[GetCount].GroundwaterRecharge;
                SI.SurfaceRunoff += Ratio * Item.GetSimulation[GetCount].SurfaceRunoff;
                SI.SoilTranshipmentRunoff += Ratio * Item.GetSimulation[GetCount].SoilTranshipmentRunoff;
                SI.DirectRunoff += Ratio * Item.GetSimulation[GetCount].DirectRunoff;
                SI.GroundwaterRunoff += Ratio * Item.GetSimulation[GetCount].GroundwaterRunoff;
                SI.Runoff += Ratio * Item.GetSimulation[GetCount].Runoff;
                SI.Storage += Ratio * Item.GetSimulation[GetCount].Storage;
                SI.InflowRunoff += Ratio * Item.GetSimulation[GetCount].InflowRunoff;
                SI.OutflowRunoff += Ratio * Item.GetSimulation[GetCount].OutflowRunoff;
                if (Item.GetSpatialFeatures.Start) SI.UpstreamRunoff += Ratio * Item.GetSimulation[GetCount].UpstreamRunoff;
                SI.RoutingRunoff += Ratio * Item.GetSimulation[GetCount].RoutingRunoff;
                if (Item.GetSpatialFeatures.End) SI.DownstreamRunoff += Ratio * Item.GetSimulation[GetCount].DownstreamRunoff;
                SI.Produced += Item.GetSimulation[GetCount].Produced;
                SI.Inflow += Item.GetSimulation[GetCount].Inflow;
                SI.Outflow += Item.GetSimulation[GetCount].Outflow;
                if (Item.GetSpatialFeatures.Start) SI.Upstream += Item.GetSimulation[GetCount].Upstream;
                SI.Routing += Item.GetSimulation[GetCount].Routing;
                if (Item.GetSpatialFeatures.End) SI.Downstream += Item.GetSimulation[GetCount].Downstream;
                SI.Baseflow += Item.GetSimulation[GetCount].Baseflow;
            }

            _Simulation.Add(SI);
            GetCount += 1;

            return GetCount;
        }

        public SimulationReport Report()
        {
            return new SimulationReport()
            {
                MeanSoilMoisture = _Simulation.Select(s => s.SoilMoisture).Average(),
                MeanSurfaceReservoirLevel = _Simulation.Select(s => s.SurfaceReservoirLevel).Average(),
                MeanSoilReservoirLevel = _Simulation.Select(s => s.SoilReservoirLevel).Average(),
                MeanGroundwaterReservoirLevel = _Simulation.Select(s => s.GroundwaterReservoirLevel).Average(),
                TotalPrecipitation = _Simulation.Select(s => s.Precipitation).Sum(),
                TotalPotentialEvapotranspiration = _Simulation.Select(s => s.PotentialEvapotranspiration).Sum(),
                TotalRealEvapotranspiration = _Simulation.Select(s => s.RealEvapotranspiration).Sum(),
                TotalInfiltration = _Simulation.Select(s => s.Infiltration).Sum(),
                TotalGroundwaterRecharge = _Simulation.Select(s => s.GroundwaterRecharge).Sum(),
                TotalSurfaceRunoff = _Simulation.Select(s => s.SurfaceRunoff).Sum(),
                TotalSoilTranshipmentRunoff = _Simulation.Select(s => s.SoilTranshipmentRunoff).Sum(),
                TotalDirectRunoff = _Simulation.Select(s => s.DirectRunoff).Sum(),
                TotalGroundwaterRunoff = _Simulation.Select(s => s.GroundwaterRunoff).Sum(),
                TotalRunoff = _Simulation.Select(s => s.Runoff).Sum(),
                TotalStorage = _Simulation.Select(s => s.Storage).Sum(),
                TotalInflowRunoff = _Simulation.Select(s => s.InflowRunoff).Sum(),
                TotalOutflowRunoff = _Simulation.Select(s => s.OutflowRunoff).Sum(),
                TotalUpstreamRunoff = _Simulation.Select(s => s.UpstreamRunoff).Sum(),
                TotalRoutingRunoff = _Simulation.Select(s => s.RoutingRunoff).Sum(),
                TotalDownstreamRunoff = _Simulation.Select(s => s.DownstreamRunoff).Sum()
            };
        }

        private VolumeFlow Runoff2Flow(Length Length)
        {
            return Length.Multiplication(GetSpatialFeatures.DrainageArea).Division(Duration.FromDays(1D));
        }
        private Length Flow2Runoff(VolumeFlow VolumeFlow)
        {
            return VolumeFlow.Division(GetSpatialFeatures.DrainageArea).Multiplication(Duration.FromDays(1D));
        }
    }
}
