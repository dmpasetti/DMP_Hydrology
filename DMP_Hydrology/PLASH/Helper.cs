using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace USP_Hydrology
{
    public partial class PLASH
    {
        public static (double, double, double) PLASHParamFromCN(double CN)
        {
            double CH = PLASHKFromCN(CN);
            double S = PLASHSFromCN(CN);
            double FS = Math.Pow(S, 2) / (2 * CH);

            return (CH, S, FS);
        }
        
        public static double PLASHKFromCN(double CN)
        {
            if (CN <= 36D) return 47.07 - 0.82 * CN;
            if (CN <= 75D) return 31.39 - 0.39 * CN;
            return (100D - CN) / 12.42;
        } 

        public static double PLASHSFromCN(double CN)
        {
            if (CN <= 65D) return 30.25 - 0.146 * CN;
            return (100D - CN) / 1.66;
        }

        

        public static double PLASH_TCKirpich(double StreamLength, double Slope)
        {
            return 3.989 * (Math.Pow(StreamLength, 0.77) / Math.Pow(Slope, 0.385));
        }

        public static double PLASH_TCDooge(double Area, double Slope)
        {
            return 21.88 * (Math.Pow(Area, 0.41) / Math.Pow(Slope, 0.17));
        }

        public static void ParametersFromInput(List<NodeExternal> tree)
        {
            int _simlength;
            foreach (NodeExternal _node in tree)
            {
                _simlength = _node.OBJ_UInput.TimeSeries.FLT_Arr_PrecipSeries.Count();
                Parameters _param = new Parameters
                {
                    FLT_TimeStep = _node.OBJ_UInput.InputParameters.FLT_TimeStep,
                    FLT_AD = _node.OBJ_UInput.FLT_Area,
                    FLT_AI = _node.OBJ_UInput.FLT_Imperv,
                    FLT_AP = _node.OBJ_UInput.FLT_Perv,
                    FLT_IP = _node.OBJ_UInput.InputParameters.FLT_IP,
                    FLT_DI = _node.OBJ_UInput.InputParameters.FLT_DI,
                    FLT_DP = _node.OBJ_UInput.InputParameters.FLT_DP,
                    FLT_KSup = _node.OBJ_UInput.InputParameters.FLT_KSup,
                    FLT_CS = _node.OBJ_UInput.InputParameters.FLT_CS,
                    FLT_CC = _node.OBJ_UInput.InputParameters.FLT_CC,
                    FLT_CR = _node.OBJ_UInput.InputParameters.FLT_CR,
                    FLT_PP = _node.OBJ_UInput.InputParameters.FLT_PP,
                    FLT_KSub = _node.OBJ_UInput.InputParameters.FLT_KSub,
                    FLT_KCan = _node.OBJ_UInput.InputParameters.FLT_KCan,
                    FLT_CH = _node.OBJ_UInput.InputParameters.FLT_CH,
                    FLT_PS = _node.OBJ_UInput.InputParameters.FLT_PS,
                    FLT_UI = _node.OBJ_UInput.InputParameters.FLT_UI,
                    FLT_CalibrationFraction = _node.TPL_CalibrationWS.Item2
                };
                _node.GetPLASH = new PLASH(_simlength, _node.OBJ_UInput.TimeSeries, new InitialConditions(), _param);
            }
        }

        public static bool ValidateSimulation(PLASH Sim) {
            var Reservoir = Sim.GetReservoir;
            var Output = Sim.GetOutput;

            if (!ValidateArray(Output.FLT_Arr_QBas_Calc))       return false;
            if (!ValidateArray(Output.FLT_Arr_QSup_Calc))       return false;
            if (!ValidateArray(Output.FLT_Arr_Qt_Calc))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_RImp))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_RInt))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_RSup))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_RSol))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_RSub))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_RCan))         return false;
            if (!ValidateArray(Reservoir.FLT_Arr_Infiltration)) return false;
            //for (int i = 0; i < Sim.GetSimulationLength; i++)
            //{            
            //    if (Double.IsNaN(Sim.GetOutput.FLT_Arr_QBas_Calc[i])        || Double.IsInfinity(Sim.GetOutput.FLT_Arr_QBas_Calc[i]         )   )   return false;
            //    if (Double.IsNaN(Sim.GetOutput.FLT_Arr_QSup_Calc[i])        || Double.IsInfinity(Sim.GetOutput.FLT_Arr_QSup_Calc[i]         )   )   return false;
            //    if (Double.IsNaN(Sim.GetOutput.FLT_Arr_Qt_Calc[i])          || Double.IsInfinity(Sim.GetOutput.FLT_Arr_Qt_Calc[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_RImp[i])          || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_RImp[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_RInt[i])          || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_RInt[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_RSup[i])          || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_RSup[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_RSol[i])          || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_RSol[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_RSub[i])          || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_RSub[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_RCan[i])          || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_RCan[i]           )   )   return false;
            //    if (Double.IsNaN(Sim.GetReservoir.FLT_Arr_Infiltration[i])  || Double.IsInfinity(Sim.GetReservoir.FLT_Arr_Infiltration[i]   )   )   return false;
            //}
            return true;
        }

        public static bool ValidateArray(double[] DataArray)
        {
            for(int i = 0; i < DataArray.Count(); i++)
            {
                if(Double.IsNaN(DataArray[i]) || Double.IsInfinity(DataArray[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<double> GAParametersSetMin(Parameters Sim = null)
        {
            //var _param = Sim.GetParameters;
            List<double> output = new List<double>();            
            output.Add(0); //DI
            output.Add(0); //IP
            output.Add(0); //DP
            output.Add(1); //KSup
            output.Add(100); //CS
            output.Add(0.3); //CC
            output.Add(0.02); //CR
            output.Add(0.01); //PP
            output.Add(30); //KSub
            output.Add(Sim != null ? Sim.FLT_KCan * 0.5 : 1); //KCan
            output.Add(Sim != null ? Sim.FLT_CH * 0.5 : 0.001); //CH
            output.Add(1); //FS
            output.Add(0.3); //PS
            output.Add(0); //UI
            return output;
        }

        public static List<double> GAParametersSetMax(Parameters Sim = null)
        {
            //var _param = Sim.GetParameters;
            List<double> output = new List<double>();
            output.Add(20); //DI
            output.Add(20); //IP
            output.Add(20); //DP
            output.Add(240); //KSup
            output.Add(2000); //CS
            output.Add(0.5); //CC
            output.Add(0.5); //CR
            output.Add(10); //PP
            output.Add(360); //KSub
            output.Add(Sim != null ? Sim.FLT_KCan * 1.5 : 120); //KCan
            output.Add(Sim != null ? Sim.FLT_CH * 1.5 : 100); //CH
            output.Add(1000); //FS
            output.Add(1); //PS
            output.Add(1); //UI
            return output;
        }

        public static void GAGetParametersMultiple(List<NodeExternal> Tree, double[] ArrayParameters)
        {
            for(int i = 0; i < Tree.Count; i++)
            {
                Parameters _param = Tree[i].GetPLASH.GetParameters;
                _param.FLT_DI = ArrayParameters[i * 14];
                _param.FLT_IP = ArrayParameters[i * 14 + 1];
                _param.FLT_DP = ArrayParameters[i * 14 + 2];
                _param.FLT_KSup = ArrayParameters[i * 14 + 3];
                _param.FLT_CS = ArrayParameters[i * 14 + 4];
                _param.FLT_CC = ArrayParameters[i * 14 + 5];
                _param.FLT_CR = ArrayParameters[i * 14 + 6];
                _param.FLT_PP = ArrayParameters[i * 14 + 7];
                _param.FLT_KSub = ArrayParameters[i * 14 + 8];
                _param.FLT_KCan = ArrayParameters[i * 14 + 9];
                //Tree[i].GetMusk.FLT_K = ArrayParameters[i * 14 + 9];
                _param.FLT_CH = ArrayParameters[i * 14 + 10];
                _param.FLT_FS = ArrayParameters[i * 14 + 11];
                _param.FLT_PS = ArrayParameters[i * 14 + 12];
                _param.FLT_UI = ArrayParameters[i * 14 + 13];
            }
        }


        public static void GAGetParametersSingle(List<NodeExternal> Tree, double[] ArrayParameters)
        {
            for (int i = 0; i < Tree.Count; i++)
            {
                Parameters _param = Tree[i].GetPLASH.GetParameters;
                _param.FLT_DI = ArrayParameters[0];
                _param.FLT_IP = ArrayParameters[1];
                _param.FLT_DP = ArrayParameters[2];
                _param.FLT_KSup = ArrayParameters[3];
                _param.FLT_CS = ArrayParameters[4];
                _param.FLT_CC = ArrayParameters[5];
                _param.FLT_CR = ArrayParameters[6];
                _param.FLT_PP = ArrayParameters[7];
                _param.FLT_KSub = ArrayParameters[8];
                _param.FLT_KCan = ArrayParameters[9];
                _param.FLT_CH = ArrayParameters[10];
                _param.FLT_FS = ArrayParameters[11];
                _param.FLT_PS = ArrayParameters[12];
                _param.FLT_UI = ArrayParameters[13];
            }
        }

        public static double PLASHNashSutcliffe(PLASH Sim)
        {
            double[] Observed = Sim.GetInput.FLT_Arr_QtObsSeries;
            double[] Calculated = Sim.GetOutput.FLT_Arr_Qt_Calibration;

            double SquareSumupper = 0;
            double SquareSumLower = 0;
            double MeanObserved = Observed.Average();

            for(int i = 0; i < Observed.Length; i++)
            {
                SquareSumupper += Math.Pow(Calculated[i] - Observed[i], 2);
                SquareSumLower += Math.Pow(Observed[i] - MeanObserved, 2);
            }
            return 1 - (SquareSumupper / SquareSumLower);
        }

        public static double PLASH_PearsonCorrelation_Rainfall_SurfaceFlow(PLASH Sim) {
            return Correlation.Pearson(Sim.GetInput.FLT_Arr_PrecipSeries, Sim.GetReservoir.FLT_Arr_ESSup);
        }

        public static double PLASH_BaseFlow_SurfaceFlow_Quantile(PLASH Sim)
        {
            double[] OrderedSurfaceFlow = Sim.GetOutput.FLT_Arr_Qt_Calc.OrderBy(x => x).ToArray();
            var MeanBaseFlow = Sim.GetOutput.FLT_Arr_QBas_Calc.Average();
            return SortedArrayStatistics.QuantileRank(OrderedSurfaceFlow, MeanBaseFlow);
        }

        public static bool PLASH_ObservedFlowAnalysis(double[] ObservedData, double[] SimulatedData, double TolerableVariation)
        {
            double[] SortedArray = ObservedData.OrderBy(x=>x).ToArray();
            double Q10 = SortedArrayStatistics.Quantile(SortedArray, 0.9);
            double Q50 = SortedArrayStatistics.Quantile(SortedArray, 0.5);
            double Q90 = SortedArrayStatistics.Quantile(SortedArray, 0.1);
            
            double Q50_Q10_Ratio = Q50 / Q10;
            double Q90_Q10_Ratio = Q90 / Q10;
            double Q90_Q50_Ratio = Q90 / Q50;

            double[] Sim_SortedArray = SimulatedData.OrderBy(x => x).ToArray();
            double Sim_Q10 = SortedArrayStatistics.Quantile(Sim_SortedArray, 0.9);
            double Sim_Q50 = SortedArrayStatistics.Quantile(Sim_SortedArray, 0.5);
            double Sim_Q90 = SortedArrayStatistics.Quantile(Sim_SortedArray, 0.1);

            double Sim_Q50_Q10_Ratio = Sim_Q50 / Sim_Q10;
            double Sim_Q90_Q10_Ratio = Sim_Q90 / Sim_Q10;
            double Sim_Q90_Q50_Ratio = Sim_Q90 / Sim_Q50;

            bool Q50_Q10_Variation = Sim_Q50_Q10_Ratio >= Q50_Q10_Ratio * (1 - TolerableVariation) && Sim_Q50_Q10_Ratio <= Q50_Q10_Ratio * TolerableVariation;
            bool Q90_Q10_Variation = Sim_Q90_Q10_Ratio >= Q90_Q10_Ratio * (1 - TolerableVariation) && Sim_Q90_Q10_Ratio <= Q90_Q10_Ratio * TolerableVariation;
            bool Q90_Q50_Variation = Sim_Q90_Q50_Ratio >= Q90_Q50_Ratio * (1 - TolerableVariation) && Sim_Q90_Q50_Ratio <= Q90_Q50_Ratio * TolerableVariation;

            return Q50_Q10_Variation && Q90_Q10_Variation && Q90_Q50_Variation;
            
        }

        public static bool Tree_Validation_ObservedFlowAnalysis(List<NodeExternal> Tree, double TolerableVariation)
        {
            foreach(NodeExternal _node in Tree)
            {
                PLASH Simulation = _node.GetPLASH;
                bool ValidNode = PLASH_ObservedFlowAnalysis(Simulation.GetInput.FLT_Arr_QtObsSeries, Simulation.GetOutput.FLT_Arr_Qt_Calc, TolerableVariation);
                if (!ValidNode)
                {
                    return false;
                }
            }
            return true;
        }



        public static bool Tree_ValidadeBaseFlow_SurfaceFlow_Quantile(List<NodeExternal> Tree, double MinQuantile, double MaxQuantile)
        {            
            foreach(NodeExternal _node in Tree)
            {
                double Quantile = PLASH_BaseFlow_SurfaceFlow_Quantile(_node.GetPLASH);
                if(Quantile > MaxQuantile || Quantile  < MinQuantile)
                {
                    return false;
                }
            }
            return true;
        }

        

        public static double Tree_MinimumCorrelation_Rainfall_SurfaceFlow(List<NodeExternal> Tree)
        {
            List<double> lstCorrelation = new List<double>();
            foreach(NodeExternal _node in Tree)
            {
                double Correlation = PLASH_PearsonCorrelation_Rainfall_SurfaceFlow(_node.GetPLASH);
                lstCorrelation.Add(Correlation);                
            }
            if(!Double.IsNaN(lstCorrelation.Minimum())){
                return lstCorrelation.Minimum();
            }
            return Double.NegativeInfinity;
            
        }

    }
}
