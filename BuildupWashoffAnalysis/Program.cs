using System;
using System.Collections.Generic;
using USP_Hydrology;
using System.IO;
using System.Linq;


namespace BuildupWashoffAnalysis
{
    class Program
    {
        public static double FlowPercentage = 90;
        public static int minDaysBuildup = 5;
        public static int maxDaysBuildup = 60;
        public static int stepDaysBuildup = 5;
        public static int minPercentageBuildup = 75;
        public static int maxPercentageBuildup = 95;
        public static int stepPercentageBuildup = 5;
        public static double minKw = 0.05;
        public static double maxKw = 2;
        public static double stepKw = 0.05;
        public static double minNw = 0.05;
        public static double maxNw = 2;
        public static double stepNw = 0.05;

        public struct BuWoSimulationData
        {
            public int elapsedDaysBuildup { get; set; }
            public int percentageBuildup { get; set; }
            public double kb { get; set; }
            public double kw { get; set; }
            public double nw { get; set; }
            public double[] surfaceFlowArray { get; set; }
            public double[] buildupArray { get; set; }
            public double[] washoffArray { get; set; }
        }


        public static Dictionary<(double, double), BuWoSimulationData> CreateNwKwMatrix(List<BuWoSimulationData> simulationList)
        {
            Dictionary<(double, double), BuWoSimulationData> dictOutput = new Dictionary<(double, double), BuWoSimulationData>();
            while(simulationList.Count > 0)
            {
                double _nw = simulationList[0].nw;
                List<BuWoSimulationData> lstNw = simulationList.Where(x => x.nw == _nw).OrderBy(x=>x.kw).ToList();
                foreach(BuWoSimulationData _obj in lstNw)
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
            FileInfo BaseOutputPath = new FileInfo(CurrentDirectory + " " + FlowPercentage.ToString());
            
            
            
            
            SMAPd_Network.SimulateTree(WSTreePrototype);
            Tree.PrototypeIntegrateBuwoSMAP(WSTreePrototype);

            foreach(NodeExternal _node in WSTreePrototype)
            {
                Buildup_Washoff.SetThresholdFlow(_node.GetBuWo.ToArray(), FlowPercentage);
            }

            Dictionary<(int, int), double> DictKb = new Dictionary<(int, int), double>();
            Buildup_Washoff[] unitBuildupArray = Buildup_Washoff.CreateUnitBuildupArray(0.01, 2, 0.01);

            for(int t = minDaysBuildup; t <= maxDaysBuildup; t += stepDaysBuildup)
            {
                for(int p = minPercentageBuildup; p <= maxPercentageBuildup; p += stepPercentageBuildup)
                {
                    (int, int) key = (t, p);
                    double kb = Buildup_Washoff.GetAdjustedKb_OLD(p, t, unitBuildupArray);
                    DictKb.Add(key, kb);
                }
            }
            foreach(NodeExternal _node in WSTreePrototype)
            {
                FileInfo watershedOutputPath = new FileInfo(BaseOutputPath.FullName + @"\Bacia " + _node.STR_Watershed.ToString());
                watershedOutputPath.Directory.Create();
                List<BuWoSimulationData> simulations = new List<BuWoSimulationData>();
                Dictionary<(double, double), BuWoSimulationData> dictSimulations = new Dictionary<(double, double), BuWoSimulationData>();
                
                //Rodadas do modelo BuWo. Loop em Kb, Nw e Kw
                foreach (KeyValuePair<(int, int), double> _entry in DictKb)
                {
                    (int t, int p) = _entry.Key;
                    double kb = _entry.Value;
                    for(double nw = minNw; nw <= maxNw; nw += stepNw)
                    {
                        for(double kw = minKw; kw <= maxKw; kw += stepKw)
                        {
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
                                surfaceFlowArray = _node.BuWoAggregate.FLT_Arr_SurfaceFlow,
                                buildupArray = _node.BuWoAggregate.FLT_Arr_Buildup,
                                washoffArray = _node.BuWoAggregate.FLT_Arr_EffectiveWashoff
                            };
                            simulations.Add(data);
                            dictSimulations.Add((nw, kw), data);
                        }
                    }                    
                }
                
                //Cálculo de métricas para análise de sensibilidade de Buildup. Loop em 
                foreach((Buildup_Washoff.MetricFunction, double) metric in Buildup_Washoff.listFunctionsSensivitiyAnalysisBuildup)
                {
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
                        double[] arrayNw = listSimData.Select(x => x.nw).ToArray();
                        double[] arrayKw = listSimData.Select(x => x.kw).ToArray();
                        Dictionary<(double, double), double> dictMetric = new Dictionary<(double, double), double>();
                        Dictionary<(double, double), BuWoSimulationData> dictSim = CreateNwKwMatrix(listSimData);
                        foreach(KeyValuePair<(double, double), BuWoSimulationData> _simulation in dictSim)
                        {
                            dictMetric.Add(_simulation.Key, Convert.ToDouble(Buildup_Washoff.ExecuteMetricFunction(metric.Item1, _simulation.Value.buildupArray)));
                        }
                        arraySpreadsheet.Add(Tree.FillSpreadsheetWashoffMatrix(arrayNw, arrayKw, dictMetric));
                        elapsedDaysList.Add(t);
                        buildupPercentageList.Add(p);
                        kbList.Add(kb);
                    }
                    Tree.CreateSpreadsheetWashoffMatrix(FileInfoSpreadsheet, arraySpreadsheet, elapsedDaysList, buildupPercentageList, kbList);
                }

                foreach ((Buildup_Washoff.MetricFunction, double) metric in Buildup_Washoff.listFunctionsSensivitiyAnalysisWashoff)
                {
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
                        double[] arrayNw = listSimData.Select(x => x.nw).ToArray();
                        double[] arrayKw = listSimData.Select(x => x.kw).ToArray();
                        Dictionary<(double, double), double> dictMetric = new Dictionary<(double, double), double>();
                        Dictionary<(double, double), BuWoSimulationData> dictSim = CreateNwKwMatrix(listSimData);
                        foreach (KeyValuePair<(double, double), BuWoSimulationData> _simulation in dictSim)
                        {
                            dictMetric.Add(_simulation.Key, Convert.ToDouble(Buildup_Washoff.ExecuteMetricFunction(metric.Item1, _simulation.Value.washoffArray)));
                        }
                        arraySpreadsheet.Add(Tree.FillSpreadsheetWashoffMatrix(arrayNw, arrayKw, dictMetric));
                        elapsedDaysList.Add(t);
                        buildupPercentageList.Add(p);
                        kbList.Add(kb);
                    }
                    Tree.CreateSpreadsheetWashoffMatrix(FileInfoSpreadsheet, arraySpreadsheet, elapsedDaysList, buildupPercentageList, kbList);

                }
            }



            Buildup_Washoff.SimulateTree_NoTransport(WSTreePrototype);
            Pollutogram.SimulateBODTree(WSTreePrototype);
            //Tree.SaveSMAPTreeToExcel(WSTree, outputPath);
            //Tree.SavePrototypeTreeToExcel(WSTree, summaryPath);
            //Tree.SavePrototypeTreeToExcel_SMAP(WSTree, summaryPath);
            //Tree.SaveSMAPTreeToExcel(WSTreePrototype, outputPath);
            //Tree.SaveQualityTreeToExcel(WSTreePrototype, QualityOutputPath);
            //Tree.SavePrototypeTreeToExcel_SMAP_Qual(WSTreePrototype, summaryPath);

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
            Console.WriteLine("Hello World!");
        }
    }
}
