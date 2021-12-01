using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using USP_Hydrology;
using System.Diagnostics;

namespace BuildupWashoffAnalysis_NetFramework
{
    class Program
    {
        public static double FlowPercentage = 50D;
        public static int minDaysBuildup = 5;
        public static int maxDaysBuildup = 60;
        public static int stepDaysBuildup = 5;
        public static int minPercentageBuildup = 75;
        public static int maxPercentageBuildup = 95;
        public static int stepPercentageBuildup = 5;
        public static double minKw = 0.05D;
        public static double maxKw = 2D;
        public static double stepKw = 0.05D;
        public static double minNw = 0.05D;
        public static double maxNw = 2D;
        public static double stepNw = 0.05D;

        public struct BuWoSimulationData
        {
            public int elapsedDaysBuildup { get; set; }
            public int percentageBuildup { get; set; }
            public double kb { get; set; }
            public double kw { get; set; }
            public double nw { get; set; }
            //public double[] surfaceFlowArray { get; set; }
            public double[] exceedingFlowArray { get; set; }
            public double[] buildupArray { get; set; }
            public double[] washoffArray { get; set; }
            public DateTime[] datesArray { get; set; }
        }


        public static Dictionary<(double, double), BuWoSimulationData> CreateNwKwMatrix(List<BuWoSimulationData> simulationList)
        {
            Dictionary<(double, double), BuWoSimulationData> dictOutput = new Dictionary<(double, double), BuWoSimulationData>();
            double[] lstNw = simulationList.Select(x => x.nw).Distinct().ToArray();
            foreach(double _nw in lstNw)
            {
                List<BuWoSimulationData> lstSimulation = simulationList.Where(x => x.nw == _nw).OrderBy(x => x.kw).ToList();
                foreach(BuWoSimulationData _obj in lstSimulation)
                {
                    dictOutput.Add((_nw, _obj.kw), _obj);
                }
            }
            return dictOutput;            
        }

        static void Main(string[] args)
        {
            var CurrentDirectory = Directory.GetCurrentDirectory();

            FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
            List<NodeExternal> WSTreePrototype = Tree.PrototypeTreeFromExcel(inputPath);
            FileInfo BaseOutputPath = new FileInfo(CurrentDirectory + @"\Output\" + FlowPercentage.ToString());
            Directory.CreateDirectory(BaseOutputPath.FullName);



            SMAPd_Network.SimulateTree(WSTreePrototype);
            Tree.PrototypeIntegrateBuwoSMAP(WSTreePrototype);

            foreach (NodeExternal _node in WSTreePrototype)
            {
                Buildup_Washoff.SetThresholdFlow(_node.GetBuWo.ToArray(), FlowPercentage / 100D);
            }

            Dictionary<(int, int), double> DictKb = new Dictionary<(int, int), double>();
            Buildup_Washoff[] unitBuildupArray = Buildup_Washoff.CreateUnitBuildupArray(0.01, 2, 0.01);

            for (int t = minDaysBuildup; t <= maxDaysBuildup; t += stepDaysBuildup)
            {
                for (int p = minPercentageBuildup; p <= maxPercentageBuildup; p += stepPercentageBuildup)
                {
                    (int, int) key = (t, p);
                    double kb = Buildup_Washoff.GetAdjustedKb_OLD(p, t, unitBuildupArray);
                    DictKb.Add(key, kb);
                }
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (NodeExternal _node in WSTreePrototype)
            {
                FileInfo watershedOutputPath = new FileInfo(BaseOutputPath.FullName + @"\Bacia " + _node.STR_Watershed.ToString());
                Directory.CreateDirectory(watershedOutputPath.FullName);
                //watershedOutputPath.Directory.Create();
                List<BuWoSimulationData> simulations = new List<BuWoSimulationData>();
                Console.WriteLine();
                Console.WriteLine("Processando Bacia {0}", _node.STR_Watershed);
                Console.WriteLine("Cálculo das simulacoes de Buildup-Washoff");
                //Rodadas do modelo BuWo. Loop em Kb, Nw e Kw
                foreach (KeyValuePair<(int, int), double> _entry in DictKb)
                {
                    (int t, int p) = _entry.Key;
                    double kb = _entry.Value;
                    for (double nw = minNw; nw <= maxNw; nw += stepNw)
                    {
                        nw = Math.Round(nw, 2);
                        for (double kw = minKw; kw <= maxKw; kw += stepKw)
                        {
                            kw = Math.Round(kw, 2);
                            foreach (Buildup_Washoff _use in _node.GetBuWo)
                            {
                                _use.GetParam.FLT_Kb = kb;
                                _use.GetParam.FLT_Nw = nw;
                                _use.GetParam.FLT_Kw = kw;
                            }
                            Buildup_Washoff.SimulateNode_AdjustedBMaxByBuildup(_node);
                            BuWoSimulationData data = new BuWoSimulationData
                            {
                                elapsedDaysBuildup = t,
                                percentageBuildup = p,
                                kb = kb,
                                nw = nw,
                                kw = kw,
                                //surfaceFlowArray = _node.BuWoAggregate.FLT_Arr_SurfaceFlow,
                                exceedingFlowArray = _node.BuWoAggregate.FLT_Arr_SurfaceFlow.Select(x => x - _node.BuWoAggregate.GetParam.FLT_ThresholdFlow).ToArray(),
                                buildupArray = _node.BuWoAggregate.FLT_Arr_Buildup,
                                washoffArray = _node.BuWoAggregate.FLT_Arr_EffectiveWashoff,
                                datesArray = _node.BuWoAggregate.DTE_Arr_TimeSeries
                            };
                            simulations.Add(data);
                            
                        }
                    }
                }
                Console.WriteLine("Cálculo finalizado. Começando processamento das planilhas.");
                DictKb = DictKb.OrderBy(x => x.Value).ThenBy(x=>x.Key.Item1).ToDictionary(x => x.Key, x => x.Value);
                //Cálculo de métricas para análise de sensibilidade de Buildup. Loop em 
                foreach ((Buildup_Washoff.MetricFunction, double?) metric in Buildup_Washoff.listFunctionsSensivitiyAnalysisBuildup)
                {
                    Console.WriteLine("Cálculo de {0} para Buildup", Buildup_Washoff.GetMetricName(metric.Item1));
                    string FileName = watershedOutputPath.FullName + @"\Buildup - " + Buildup_Washoff.GetMetricName(metric.Item1) + ".xlsx";
                    FileInfo FileInfoSpreadsheet = new FileInfo(FileName);

                    List<List<object[]>> arraySpreadsheet = new List<List<object[]>>();
                    List<double> elapsedDaysList = new List<double>();
                    List<double> buildupPercentageList = new List<double>();
                    List<double> kbList = new List<double>();
                    foreach (KeyValuePair<(int, int), double> _entry in DictKb)
                    {
                        (int t, int p) = _entry.Key;
                        double kb = _entry.Value;
                        List<BuWoSimulationData> listSimData = (from _obj in simulations where _obj.elapsedDaysBuildup == t && _obj.percentageBuildup == p select _obj)
                            .OrderBy(x => x.nw)
                            .ThenBy(x => x.kw)
                            .ToList();
                        double[] arrayNw = listSimData.Select(x => x.nw).Distinct().ToArray();
                        double[] arrayKw = listSimData.Select(x => x.kw).Distinct().ToArray();
                        Dictionary<(double, double), double> dictMetric = new Dictionary<(double, double), double>();
                        Dictionary<(double, double), BuWoSimulationData> dictSim = CreateNwKwMatrix(listSimData);
                        foreach (KeyValuePair<(double, double), BuWoSimulationData> _simulation in dictSim)
                        {
                            var metricResult = Buildup_Washoff.ExecuteMetricFunction(metric.Item1, _simulation.Value.buildupArray, null, _simulation.Value.datesArray);
                            dictMetric.Add(_simulation.Key, Convert.ToDouble(metricResult));
                        }
                        arraySpreadsheet.Add(Tree.FillSpreadsheetWashoffMatrix(arrayNw, arrayKw, dictMetric));
                        elapsedDaysList.Add(t);
                        buildupPercentageList.Add(p);
                        kbList.Add(kb);
                    }
                    Tree.CreateSpreadsheetWashoffMatrix(FileInfoSpreadsheet, arraySpreadsheet, elapsedDaysList, buildupPercentageList, kbList);
                }

                foreach ((Buildup_Washoff.MetricFunction, double?) metric in Buildup_Washoff.listFunctionsSensivitiyAnalysisWashoff)
                {
                    Console.WriteLine("Cálculo de {0} para Washoff", Buildup_Washoff.GetMetricName(metric.Item1));
                    string FileName = watershedOutputPath.FullName + @"\Washoff - " + Buildup_Washoff.GetMetricName(metric.Item1) + ".xlsx";
                    FileInfo FileInfoSpreadsheet = new FileInfo(FileName);

                    List<List<object[]>> arraySpreadsheet = new List<List<object[]>>();
                    List<double> elapsedDaysList = new List<double>();
                    List<double> buildupPercentageList = new List<double>();
                    List<double> kbList = new List<double>();
                    foreach (KeyValuePair<(int, int), double> _entry in DictKb)
                    {
                        (int t, int p) = _entry.Key;
                        double kb = _entry.Value;
                        List<BuWoSimulationData> listSimData = (from _obj in simulations where _obj.elapsedDaysBuildup == t && _obj.percentageBuildup == p select _obj)
                            .OrderBy(x => x.nw)
                            .ThenBy(x => x.kw)
                            .ToList();
                        double[] arrayNw = listSimData.Select(x => x.nw).Distinct().ToArray();
                        double[] arrayKw = listSimData.Select(x => x.kw).Distinct().ToArray();
                        Dictionary<(double, double), double> dictMetric = new Dictionary<(double, double), double>();
                        Dictionary<(double, double), BuWoSimulationData> dictSim = CreateNwKwMatrix(listSimData);
                        foreach (KeyValuePair<(double, double), BuWoSimulationData> _simulation in dictSim)
                        {
                            var metricResult = Buildup_Washoff.ExecuteMetricFunction(metric.Item1, _simulation.Value.washoffArray, _simulation.Value.exceedingFlowArray, _simulation.Value.datesArray);
                            dictMetric.Add(_simulation.Key, Convert.ToDouble(metricResult));
                        }
                        arraySpreadsheet.Add(Tree.FillSpreadsheetWashoffMatrix(arrayNw, arrayKw, dictMetric));
                        elapsedDaysList.Add(t);
                        buildupPercentageList.Add(p);
                        kbList.Add(kb);
                    }
                    Tree.CreateSpreadsheetWashoffMatrix(FileInfoSpreadsheet, arraySpreadsheet, elapsedDaysList, buildupPercentageList, kbList);

                }
                simulations = null;
            }

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);


            /*Definir variaveis de simulacao:
            -Percentil do escoamento superficial
            -Endereco de planilha de entrada
            -Intervalo de percentis do acumulo para um intervalo de tempos decorridos (poucos, uns dois de cada, tres no maximo)             
             */
            //Ler a planilha de dados de entrada - OK
            //Simular SMAP - OK
            //Integrar dados de escoamento superficial nos objetos de Buildup/washoff - OK
            //Loop Percentil de acumulo e tempo decorrido (Loop Kb):
            //  Loop Kw e Nw
            //      Em cada conjunto, determino BMax
            //-Cada percentil do escoamento superficial gera uma pasta.
            //Cada pasta gera planilhas com apenas uma informacao (uma planilha de acumulo/lavagem total, uma com media mensal, uma com media movel etc)
            //Cada planilha tem uma aba para cada Kb
            //Cada aba tem uma matriz relacionando a metrica calculada com os valores de Kw e Nw correspondentes
            //Isso para cada bacia
            Console.ReadKey();
        }
    }
}
