using System;
using System.Collections.Generic;
using System.Linq;
using UnitsNet;

namespace USP_Hydrology
{
    public partial class Model_Muskingum
    {
        private ModelParameters _ModelParameters;
        public ModelParameters GetModelParameters
        {
            get
            {
                return _ModelParameters;
            }
        }

        private InitialConditions _InitialConditions;
        public InitialConditions GetInitialConditions
        {
            get
            {
                return _InitialConditions;
            }
        }

        private List<SimulationInstant> _Simulation;
        public SimulationInstant[] GetSimulation
        {
            get
            {
                return _Simulation.ToArray();
            }
        }

        private Int32 _Count;
        public Int32 GetCount
        {
            get
            {
                return _Count;
            }
        }

        private (Ratio C1, Ratio C2, Ratio C3)[] _Parameters;
        private VolumeFlow[] _Previous;

        public void SimulationStart(ModelParameters ModelParameters, InitialConditions InitialConditions)
        {
            _ModelParameters = ModelParameters;
            _InitialConditions = InitialConditions;
            _Simulation = new List<SimulationInstant>();
            _Count = -1;

            var dT = _ModelParameters.TimeStep.Hours; if (dT <= 0D) throw new ArgumentOutOfRangeException("TimeStep");
            var K  = _ModelParameters.TravelTime.Hours; if (K <= 0D) throw new ArgumentOutOfRangeException("TravelTime");
            var X  = _ModelParameters.WeightingFactor.DecimalFractions; if (X <= 0D || X >= 0.5D) throw new ArgumentOutOfRangeException("WeightingFactor");

            if (dT <= K)
            {
                var N = (Int32)Math.Floor(K / dT);
                var Kn = dT;
                var Ke = K - N * Kn;
                var Ne = (Ke > 0D ? 1 : 0);

                _Parameters = new (Ratio C1, Ratio C2, Ratio C3)[N + Ne];
                var C1 = (dT - 2D * Kn * X) / (2D * Kn * (1D - X) + dT);
                var C2 = (dT + 2D * Kn * X) / (2D * Kn * (1D - X) + dT);
                var C3 = (2D * Kn * (1D - X) - dT) / (2D * Kn * (1D - X) + dT);
                for (Int32 n = 0; n < N; n++)
                {
                    _Parameters[n] = (Ratio.FromDecimalFractions(C1), Ratio.FromDecimalFractions(C2), Ratio.FromDecimalFractions(C3));
                }
                if (Ke > 0D)
                {
                    C1 = (dT - 2D * Ke * X) / (2D * Ke * (1D - X) + dT);
                    C2 = (dT + 2D * Ke * X) / (2D * Ke * (1D - X) + dT);
                    C3 = (2D * Ke * (1D - X) - dT) / (2D * Ke * (1D - X) + dT);
                    _Parameters[N] = (Ratio.FromDecimalFractions(C1), Ratio.FromDecimalFractions(C2), Ratio.FromDecimalFractions(C3));
                }
                _Previous = Enumerable.Repeat<VolumeFlow>(_InitialConditions.Channel, N).ToArray();
            }
            else
            {
                _Parameters = null;
                _Previous = null;
            }
        }

        public Int32 SimulationNext(VolumeFlow Inflow)
        {
            _Count += 1;

            var SI = new SimulationInstant
            {
                Time = _ModelParameters.TimeStep.Multiplication(_Count + 1)
            };
            if (_Previous != null)
            {
                var S = _Parameters.Count();
                var Current = new VolumeFlow[S];
                Current[0] = Inflow;
                for (Int32 s = 1; s < S; s++)
                    Current[s] = Current[s - 1].Multiplication(_Parameters[s].C1).Addition(_Previous[s - 1].Multiplication(_Parameters[s].C2)).Addition(_Previous[s].Multiplication(_Parameters[s].C3));
                _Previous = Current;

                SI.Inflow = Inflow;
                SI.Outflow = Current.Last();
            }
            else
            {
                SI.Inflow = Inflow;
                SI.Outflow = Inflow;
            }
            _Simulation.Add(SI);

            return _Count;
        }
    }
}
