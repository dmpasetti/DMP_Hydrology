using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace USP_Hydrology
{
    public partial class Buildup_Washoff
    {
        public static double TotalPeriodLoad(Buildup_Washoff Sim)
        {
            Console.WriteLine("Buildup: {0}", Sim.FLT_Arr_Buildup.Sum());
            Console.ReadKey();
            return Sim.FLT_Arr_Buildup.Sum();
        }

        public static double AveragePeriodLoad(Buildup_Washoff Sim)
        {
            return Sim.FLT_Arr_Buildup.Average();
        }


        public static double TotalPeriodWashoff(Buildup_Washoff Sim)
        {
            return Sim.FLT_Arr_EffectiveWashoff.Sum();
        }

        public static double GetPercentileFlow(double[] FlowArray, double Quantile)
        {
            return ArrayStatistics.QuantileInplace(FlowArray, Quantile);
        }

        public static void SetThresholdFlow(Buildup_Washoff[] lstBuWo, double Quantile)
        {
            foreach(Buildup_Washoff _Buwo in lstBuWo)
            {
                double[] surfaceFlowArray = (double[])_Buwo.FLT_Arr_SurfaceFlow.Clone();
                var ThresholdFlow = GetPercentileFlow(surfaceFlowArray, Quantile);
                _Buwo.GetParam.FLT_ThresholdFlow = ThresholdFlow;
            }
        }

        public static Buildup_Washoff CreateUnitBuildup(double Kb, double TimeStep = 24)
        {
            var Param = new Buildup_Washoff.Parameters
            {
                FLT_Kb = Kb,
                FLT_Area = 1,
                FLT_AreaFraction = 1,
                FLT_BMax = 100,
                FLT_Kw = 0,
                FLT_Nw = 0,
                FLT_ThresholdFlow = 10,
                FLT_Timestep_h = TimeStep,
                FLT_InitialBuildup = 0,
                FLT_Nb = 0,
                INT_BuMethod = BuildupMethod.Exp,
                INT_WoMethod = WashoffMethod.Exp,
            };
            var FlowArray = new double[100];

            var Simulation = new Buildup_Washoff
            {
                GetParam = Param,
                FLT_Arr_SurfaceFlow = FlowArray
            };
            ProcessBuWo(Simulation);
            return Simulation;
        }

        public static Buildup_Washoff[] CreateUnitBuildupArray(double Kb_Min, double Kb_Max, double Kb_Step, double Timestep = 24)
        {
            double Kb = Kb_Min;
            List<Buildup_Washoff> Output = new List<Buildup_Washoff>(); 
            while(Kb <= Kb_Max)
            {
                Output.Add(CreateUnitBuildup(Kb, Timestep));
                Kb += Kb_Step;
            }
            return Output.ToArray();
        }

        public static double GetAdjustedKb_OLD(double totalBuildupPercentage, double elapsedDays, Buildup_Washoff[] unitBuildupArray)
        {
            int Index = Convert.ToInt32(Math.Round(elapsedDays / unitBuildupArray[0].GetParam.FLT_Timestep_d, 0));
            double[] arrayKb = unitBuildupArray.Select(x => x.GetParam.FLT_Kb).ToArray();
            double[] arrayBuildup = unitBuildupArray.Select(x => x.FLT_Arr_Buildup[Index]).ToArray();

            var LinearInterpolation = MathNet.Numerics.Interpolate.Linear(arrayBuildup, arrayKb);
            return LinearInterpolation.Interpolate(totalBuildupPercentage);
        }

        public static double GetAdjustedKb(double totalBuildupPercentage, double elapsedDays)
        {
            return -1D * Math.Log(1D - totalBuildupPercentage) / elapsedDays;
        }

        public static List<Buildup_Washoff> BuWo_AdjustedBMax()
        {
            return null;
        }

        public static void SimulateNode_AdjustedBMaxByBuildup(NodeExternal node)
        {
            double WatershedArea = 0;
            foreach(Buildup_Washoff _use in node.GetBuWo)
            {
                double nonPointLoad = node.BaseLoad.EventNonPointBOD_kgd.Kilograms * _use.GetParam.FLT_AreaFraction;
                _use.GetParam.FLT_BMax = 1;
                ProcessBuWo(_use);
                _use.GetParam.FLT_BMax = nonPointLoad / CalculateArrayAverage(_use.FLT_Arr_Buildup);
                ProcessBuWo(_use);
                WatershedArea += _use.GetParam.FLT_Area * _use.GetParam.FLT_AreaFraction;
            }
            node.BuWoAggregate = AggregateUses(node.GetBuWo, WatershedArea);
        }

        public static void SimulateNode_AdjustedBMaxByWashoff(NodeExternal node)
        {
            double WatershedArea = 0;
            foreach (Buildup_Washoff _use in node.GetBuWo)
            {
                double nonPointLoad = node.BaseLoad.EventNonPointBOD_kgd.Kilograms * _use.GetParam.FLT_AreaFraction;
                _use.GetParam.FLT_BMax = 1;
                ProcessBuWo(_use);
                _use.GetParam.FLT_BMax = nonPointLoad / CalculateArrayAverage(_use.FLT_Arr_EffectiveWashoff);
                ProcessBuWo(_use);
                WatershedArea += _use.GetParam.FLT_Area * _use.GetParam.FLT_AreaFraction;
            }
            node.BuWoAggregate = AggregateUses(node.GetBuWo, WatershedArea);
        }



        public static void AdjustBMax_CompleteTree(List<NodeExternal> Tree)
        {
            foreach (NodeExternal _node in Tree)
            {
                Buildup_Washoff.SimulateNode_AdjustedBMaxByWashoff(_node);            
            }
        }

        public static void SimulateBuWoTree(List<NodeExternal> Tree)
        {
            foreach(NodeExternal _node in Tree)
            {
                double WatershedArea = 0;
                foreach(Buildup_Washoff _use in _node.GetBuWo)
                {
                    ProcessBuWo(_use);
                    WatershedArea += _use.GetParam.FLT_Area * _use.GetParam.FLT_AreaFraction;
                }
                _node.BuWoAggregate = AggregateUses(_node.GetBuWo, WatershedArea);
            }
        }

        public static void SetBMaxTree_WithBaseTree(List<NodeExternal> Tree, List<NodeExternal> BaseTree)
        {
            foreach (NodeExternal _node in Tree)
            {
                NodeExternal _baseNode = BaseTree.Where(x => x.STR_Watershed == _node.STR_Watershed).FirstOrDefault();
                if (_baseNode != null)
                {
                    foreach(Buildup_Washoff _use in _node.GetBuWo)
                    {
                        Buildup_Washoff _baseUse = _baseNode.GetBuWo.Where(x => x.GetParam.STR_UseName == _use.GetParam.STR_UseName).FirstOrDefault();
                        _use.GetParam.FLT_BMax = _baseUse.GetParam.FLT_BMax;
                    }
                }
            }
        }

    }
}
