using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Buildup_Washoff
    {
        public class Parameters
        {
            public LandUse STR_UseName;
            public double FLT_BMax; //Maximum Buildup, normalized (kg/km²)
            public double FLT_Nb; //Buildup Exponent  (-). Nb <= 1 
            public double FLT_Kb;/*Buildup rate constant (Varies.   Pow: kg/km²*d^-Nb; 
                                                                Exp: d^-1; 
                                                                Sat: d.)*/

            public double FLT_ThresholdFlow;
            public double FLT_Nw; //Washoff exponent (-)
            public double FLT_Kw; /*Washoff rate constante (Varies. Exp:  (mm/h)*h^-1
                                                                Rating: (Kg/h)*(L/h)^-Nw
                                                                EMC: mg/m³*/
            public BuildupMethod INT_BuMethod;
            public WashoffMethod INT_WoMethod;

            public double FLT_Timestep_h;
            public double FLT_Timestep_d { get => FLT_Timestep_h / 24; }
            public double FLT_InitialBuildup;
            public double FLT_Area;
            public double FLT_AreaFraction;

            public bool BOOL_Aggregate;
        }

        public Parameters GetParam;


        public DateTime[] DTE_Arr_TimeSeries;
        public double[] FLT_Arr_SurfaceFlow;

        public double[] FLT_Arr_Washoff; //Washoff time series in the entire timestep (kg)
        public double[] FLT_Arr_Buildup; //Buildup time series, non-normalized (kg)
        public double[] FLT_Arr_EffectiveWashoff; //Washoff time series limited by available Buildup (kg)


        

        public enum BuildupMethod
        {
            Pow = 1,
            Exp = 2,
            Sat = 3
        }

        public enum WashoffMethod
        {
            Exp = 1,
            Rating = 2,
            EMC = 3
        }

        #region Time From Buildup Equations
        public double TimeFromBuildup_Pow(double FLT_Buildup, Parameters _param)
        {
            double Time = Math.Pow(FLT_Buildup / _param.FLT_Kb, (1 / _param.FLT_Nb));
            if (double.IsNaN(Time))
            {
                return 0;
            }
            else
            {
                return Time;
            }
        }

        public double TimeFromBuildup_Exp(double FLT_Buildup, Parameters _param)
        {
            double Time = -Math.Log(1 - (FLT_Buildup / _param.FLT_BMax)) / _param.FLT_Kb;
            if (double.IsNaN(Time))
            {
                return 0;
            }
            else
            {
                return Time;
            }
        }

        public double TimeFromBuildup_Sat(double FLT_Buildup, Parameters _param)
        {
            double Time = (FLT_Buildup * _param.FLT_Kb) / (_param.FLT_BMax - FLT_Buildup);
            if (double.IsNaN(Time))
            {
                return 0;
            }
            else
            {
                return Time;
            }
        }
        #endregion Time From Buildup Equations

        #region Buildup Equations

        public double Buildup_Pow(double FLT_Time, Parameters _param)
        {
            return Math.Min(_param.FLT_BMax, _param.FLT_Kb * Math.Pow(FLT_Time, _param.FLT_Nb));
        }

        public double Buildup_Exp(double FLT_Time, Parameters _param)
        {
            return _param.FLT_BMax * (1 - Math.Exp(-_param.FLT_Kb * FLT_Time));
        }

        public double Buildup_Sat(double FLT_Time, Parameters _param)
        {
            return (_param.FLT_BMax * FLT_Time) / (_param.FLT_Kb + FLT_Time);
        }
        #endregion BuildupEquations

        #region Washoff Equations
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FLT_SurfaceFlow"></param>
        /// Surface Flow computed from hydrological model. Unit: mm
        /// <param name="FLT_BuildupMass"></param>
        /// Total buildup in timestep. Unit: kg
        /// <returns></returns>
        public double Washoff_Exp(double FLT_SurfaceFlow, double FLT_BuildupMass, Parameters _param)
        {
            return _param.FLT_Kw * Math.Pow(FLT_SurfaceFlow, _param.FLT_Nw) * FLT_BuildupMass;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FLT_SurfaceFlow"></param>
        /// Surface Flow computed from hydrological model. Unit: mm
        /// <param name="FLT_WatershedArea"></param>
        /// Area of the watershed. Should be input after considering land use fractions. Unit: km²
        /// <returns></returns>
        public double Washoff_Rating(double FLT_SurfaceFlow, double FLT_WatershedArea, Parameters _param)
        {
            return _param.FLT_Kw * Math.Pow(1000 * FLT_SurfaceFlow * FLT_WatershedArea, _param.FLT_Nw);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FLT_SurfaceFlow"></param>
        /// Surface Flow computed from hydrological model. Unit: mm
        /// <param name="FLT_WatershedArea"></param>
        /// Area of the watershed. Should be input after considering land use fractions. Unit: km²
        /// <returns></returns>
        public double Washoff_EMC(double FLT_SurfaceFlow, double FLT_WatershedArea, Parameters _param)
        {
            return _param.FLT_Kw * FLT_SurfaceFlow * FLT_WatershedArea;
        }
        #endregion Washoff Equations



        public static void ProcessBuWo(Buildup_Washoff Sim)
        {
            Sim.FLT_Arr_Washoff = new double[Sim.FLT_Arr_SurfaceFlow.Length];
            Sim.FLT_Arr_Buildup = new double[Sim.FLT_Arr_SurfaceFlow.Length];
            Sim.FLT_Arr_EffectiveWashoff = new double[Sim.FLT_Arr_SurfaceFlow.Length];
            Sim.GetParam.BOOL_Aggregate = false;

            bool BOOL_Buildup;
            double FLT_BuildupSpecific = 0;
            double FLT_BuildupMass = Sim.GetParam.FLT_InitialBuildup;
            double FLT_WashoffRate = 0;


            BuildupMethod BMethod = Sim.GetParam.INT_BuMethod;
            WashoffMethod WMethod = Sim.GetParam.INT_WoMethod;

            double FLT_BuildupTime = 0;
            switch (BMethod)
            {
                case BuildupMethod.Pow:
                    FLT_BuildupTime = Sim.TimeFromBuildup_Pow(Sim.GetParam.FLT_InitialBuildup / (Sim.GetParam.FLT_Area * Sim.GetParam.FLT_AreaFraction), Sim.GetParam);
                    break;
                case BuildupMethod.Exp:
                    FLT_BuildupTime = Sim.TimeFromBuildup_Exp(Sim.GetParam.FLT_InitialBuildup / (Sim.GetParam.FLT_Area * Sim.GetParam.FLT_AreaFraction), Sim.GetParam);
                    break;
                case BuildupMethod.Sat:
                    FLT_BuildupTime = Sim.TimeFromBuildup_Sat(Sim.GetParam .FLT_InitialBuildup / (Sim.GetParam.FLT_Area * Sim.GetParam.FLT_AreaFraction), Sim.GetParam);
                    break;
                default:
                    break;
            }


            for (int i = 0; i < Sim.FLT_Arr_Washoff.Length; i++)
            {                
                BOOL_Buildup = Sim.FLT_Arr_SurfaceFlow[i] < Sim.GetParam.FLT_ThresholdFlow;
                if (BOOL_Buildup)
                {
                    FLT_BuildupTime += Sim.GetParam.FLT_Timestep_d;
                    Sim.FLT_Arr_Washoff[i] = 0;
                    switch (BMethod)
                    {
                        case BuildupMethod.Pow:
                            FLT_BuildupSpecific = Sim.Buildup_Pow(FLT_BuildupTime, Sim.GetParam);
                            break;
                        case BuildupMethod.Exp:
                            FLT_BuildupSpecific = Sim.Buildup_Exp(FLT_BuildupTime, Sim.GetParam);
                            break;
                        case BuildupMethod.Sat:
                            FLT_BuildupSpecific = Sim.Buildup_Sat(FLT_BuildupTime, Sim.GetParam);
                            break;
                        default:
                            break;
                    }
                    FLT_BuildupMass = FLT_BuildupSpecific * Sim.GetParam.FLT_Area * Sim.GetParam.FLT_AreaFraction;
                }
                else
                {
                    switch (WMethod)
                    {
                        case WashoffMethod.Exp:
                            FLT_WashoffRate = Sim.Washoff_Exp(Sim.FLT_Arr_SurfaceFlow[i], FLT_BuildupMass, Sim.GetParam);
                            break;
                        case WashoffMethod.Rating:
                            FLT_WashoffRate = Sim.Washoff_Rating(Sim.FLT_Arr_SurfaceFlow[i], Sim.GetParam.FLT_Area * Sim.GetParam.FLT_AreaFraction, Sim.GetParam);
                            break;
                        case WashoffMethod.EMC:
                            FLT_WashoffRate = Sim.Washoff_EMC(Sim.FLT_Arr_SurfaceFlow[i], Sim.GetParam.FLT_Area * Sim.GetParam.FLT_AreaFraction, Sim.GetParam);
                            break;
                        default:
                            break;
                    }
                    FLT_BuildupSpecific = 0;
                    Sim.FLT_Arr_Washoff[i] = FLT_WashoffRate * Sim.GetParam.FLT_Timestep_d;
                    Sim.FLT_Arr_EffectiveWashoff[i] = Math.Min(Sim.FLT_Arr_Washoff[i], FLT_BuildupMass);
                    FLT_BuildupMass = FLT_BuildupMass - Sim.FLT_Arr_EffectiveWashoff[i];

                    switch (BMethod)
                    {
                        case BuildupMethod.Pow:
                            FLT_BuildupTime = Sim.TimeFromBuildup_Pow(FLT_BuildupMass / Sim.GetParam.FLT_Area, Sim.GetParam);
                            break;
                        case BuildupMethod.Exp:
                            FLT_BuildupTime = Sim.TimeFromBuildup_Exp(FLT_BuildupMass / Sim.GetParam.FLT_Area, Sim.GetParam);
                            break;
                        case BuildupMethod.Sat:
                            FLT_BuildupTime = Sim.TimeFromBuildup_Sat(FLT_BuildupMass / Sim.GetParam.FLT_Area, Sim.GetParam);
                            break;
                        default:
                            break;
                    }
                }
                Sim.FLT_Arr_Buildup[i] = FLT_BuildupMass;
                //Console.WriteLine("i: {5}, Bool_Buildup: {0}, BuildupRate: {1}, Buildup Total: {2}, Washoff: {3}, Effective Washoff: {4}, Time: {5}",
                //    BOOL_Buildup,
                //    Math.Round(FLT_BuildupSpecific, 3),
                //    Math.Round(FLT_BuildupMass, 3),
                //    Math.Round(Sim.FLT_Arr_Washoff[i], 3),
                //    Math.Round(Sim.FLT_Arr_EffectiveWashoff[i], 3),
                //    Math.Round(FLT_BuildupTime, 3),
                //    i);
            }
        }

        public static Buildup_Washoff AggregateUses(List<Buildup_Washoff> lstUses, double FLT_WatershedArea)
        {
            
            int Arraylength = lstUses[0].FLT_Arr_Buildup.Length;
            
            double[] TotalBuildup = new double[Arraylength];
            double[] TotalWashoff = new double[Arraylength];
            double[] TotalEffectiveWashoff = new double[Arraylength];

            foreach(Buildup_Washoff _use in lstUses)
            {
                TotalBuildup = TotalBuildup.Zip(_use.FLT_Arr_Buildup, (x, y) => x + y).ToArray();
                TotalWashoff = TotalWashoff.Zip(_use.FLT_Arr_Washoff, (x, y) => x + y).ToArray();
                TotalEffectiveWashoff = TotalEffectiveWashoff.Zip(_use.FLT_Arr_EffectiveWashoff, (x, y) => x + y).ToArray();
            }

            Buildup_Washoff AggregateObj = new Buildup_Washoff()
            {                
                FLT_Arr_Buildup = TotalBuildup,
                FLT_Arr_Washoff = TotalWashoff,
                FLT_Arr_EffectiveWashoff = TotalEffectiveWashoff
            };
            AggregateObj.GetParam = new Parameters
            {
                FLT_Area = FLT_WatershedArea,
                BOOL_Aggregate = true,
                STR_UseName = (LandUse)1000
            };
            return AggregateObj;
        }

        public static Buildup_Washoff Transport(Buildup_Washoff Upstream, Buildup_Washoff Downstream)
        {
            return new Buildup_Washoff()
            {                
                FLT_Arr_Buildup = Upstream.FLT_Arr_Buildup.Zip(Downstream.FLT_Arr_Buildup, (x, y) => x + y).ToArray(),
                FLT_Arr_EffectiveWashoff = Upstream.FLT_Arr_EffectiveWashoff.Zip(Downstream.FLT_Arr_EffectiveWashoff, (x, y) => x + y).ToArray()
            };
        }

        public static void SimulateTree_NoTransport(List<NodeExternal> Tree)
        {
            foreach(NodeExternal _node in Tree)
            {
                double WatershedArea = 0;
                foreach (Buildup_Washoff _use in _node.GetBuWo)
                {
                    ProcessBuWo(_use);
                    WatershedArea += _use.GetParam.FLT_Area*_use.GetParam.FLT_AreaFraction;
                }
                _node.GetBuWo.Add(AggregateUses(_node.GetBuWo, WatershedArea));                
            }
        }


        
        public enum LandUse
        {
            Urban = 1,
            OpenField,
            Agriculture,
            Forest,
            WaterBody,
            Pasture,
            Silviculture,
            Aggregate = 1000
        }

        public static Dictionary<LandUse, double> ExportCoef = new Dictionary<LandUse, double>()
        {
            { LandUse.Urban, 0.034 },
            { LandUse.OpenField, 0.028 },
            {LandUse.Agriculture, 0.346 },
            {LandUse.Forest, 0.039 },
            {LandUse.WaterBody, 0 },
            {LandUse.Pasture, 0.05 },
            {LandUse.Silviculture, 0.039 }
        };

    }
}


/*
 Calculo taxa de buildup e de washoff
 Buildup e washoff são mutualmente excludentes
 Se ocorrer buildup, acumular carga
 Se ocorrer washoff, lavar (subtrair) carga do valor existente
 Determinar o tempo equivalente: 
    
     
     */
