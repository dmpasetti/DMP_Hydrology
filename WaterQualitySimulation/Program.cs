﻿using System;
using System.Collections.Generic;
using System.Linq;
using USP_Hydrology;
using System.IO;
using UnitsNet;
using MathNet.Numerics.Statistics;


namespace WaterQualitySimulation
{
    class Program
    {
        public static double PercentageFlow = 50D;
        public static double elapsedDays = 7;
        public static double totalBuildupPercentage = 80D;

        public static double[] KwRange = { 0.1D, 0.25D, 0.5D, 0.75D, 1D };
        public static double[] NwRange = { 0.5D, 1D, 1.5D };

        static void Main(string[] args)
        {
            var rootDirectory = @"C:\EstudoMQualBuWo\AT";
            FileInfo inputPath = new FileInfo( rootDirectory +  @"\input.xlsx");
            FileInfo inputPathCal = new FileInfo(rootDirectory + @"\inputcal.xlsx");
            DateTime now = DateTime.Now;
            string outputSystem = "_AT";
            string outputPrefix = $@"\{now.Year}-{now.Month}-{now.Day}_{now.Hour}{now.Minute.ToString("00.##")}_Output";
            outputPrefix += outputSystem;
            Directory.CreateDirectory(rootDirectory + outputPrefix);
            FileInfo outputPath = new FileInfo(rootDirectory + outputPrefix + @"\hydrology.xlsx");
            string qualityOutputBase = @"\quality--t_p_kb_Nw_Kw=";
            List<NodeExternal> WSTreePrototype = Tree.PrototypeTreeFromExcel(inputPath);
            List<NodeExternal> WSTreeCalibration = Tree.PrototypeTreeFromExcel(inputPathCal);

            Console.WriteLine("Simulação do modelo hidrológico...");
            SMAPd_Network.SimulateTree(WSTreePrototype);
            SMAPd_Network.SimulateTree(WSTreeCalibration);
            Tree.PrototypeIntegrateBuwoSMAP(WSTreePrototype);
            Tree.PrototypeIntegrateBuwoSMAP(WSTreeCalibration);

            Tree.SaveSMAPTreeToExcel(WSTreePrototype, outputPath);

            //Ajuste Kb
            double kb = Buildup_Washoff.GetAdjustedKb(totalBuildupPercentage /100D, elapsedDays);
            foreach (NodeExternal _node in WSTreePrototype)
            {
                foreach(Buildup_Washoff _use in _node.GetBuWo)
                {
                    _use.GetParam.FLT_Kb = kb;
                }
            }

            foreach (NodeExternal _node in WSTreeCalibration)
            {
                foreach (Buildup_Washoff _use in _node.GetBuWo)
                {
                    _use.GetParam.FLT_Kb = kb;
                }
            }

            List<object> firstLine = new List<object> { "t-p-kb-kw-nw" };
            foreach(NodeExternal _node in WSTreePrototype)
            {
                firstLine.Add(_node.STR_Watershed);
            }

            List<object[]> outputStdDev = new List<object[]>();
            List<object[]> outputStdDevNorm = new List<object[]>();
            List<object[]> outputMax = new List<object[]>();
            List<object[]> outputMaxNorm = new List<object[]>();
            List<object[]> outputNumWash0 = new List<object[]>();
            List<object[]> outputNumWash0Rel = new List<object[]>();
            outputStdDev.Add(firstLine.ToArray());
            outputStdDevNorm.Add(firstLine.ToArray());
            outputMax.Add(firstLine.ToArray());
            outputMaxNorm.Add(firstLine.ToArray());
            outputNumWash0.Add(firstLine.ToArray());
            outputNumWash0Rel.Add(firstLine.ToArray());


            foreach (double _nw in NwRange)
            {
                foreach (double _kw in KwRange)
                {
                    foreach (NodeExternal _node in WSTreePrototype)
                    {
                        foreach (Buildup_Washoff _buwo in _node.GetBuWo)
                        {
                            _buwo.GetParam.FLT_Nw = _nw;
                            _buwo.GetParam.FLT_Kw = _kw;
                        }
                    }

                    foreach (NodeExternal _node in WSTreeCalibration)
                    {
                        foreach (Buildup_Washoff _buwo in _node.GetBuWo)
                        {
                            _buwo.GetParam.FLT_Nw = _nw;
                            _buwo.GetParam.FLT_Kw = _kw;
                        }
                    }

                    Buildup_Washoff.AdjustBMax_CompleteTree(WSTreeCalibration);
                    Buildup_Washoff.SetBMaxTree_WithBaseTree(WSTreePrototype, WSTreeCalibration);
                    Buildup_Washoff.SimulateBuWoTree(WSTreePrototype);
                    Pollutogram.SimulatePhosphorusTree(WSTreePrototype);

                    string qualityOutputValues = $"{elapsedDays}_{totalBuildupPercentage}_{Math.Round(kb, 3)}_{_nw}_{_kw}";
                    List<object> lineStdDev = new List<object>();
                    List<object> lineStdDevNorm = new List<object>();
                    List<object> lineMax = new List<object>();
                    List<object> lineMaxNorm = new List<object>();
                    List<object> lineNumWash0 = new List<object>();
                    List<object> lineNumWash0Rel = new List<object>();

                    lineStdDev.Add(qualityOutputValues);
                    lineStdDevNorm.Add(qualityOutputValues);
                    lineMax.Add(qualityOutputValues);
                    lineMaxNorm.Add(qualityOutputValues);
                    lineNumWash0.Add(qualityOutputValues);
                    lineNumWash0Rel.Add(qualityOutputValues);

                    foreach (NodeExternal _node in WSTreePrototype)
                    {
                        DateTime[] Dates = _node.GetSMAP.GetInput.Time;
                        Mass[] washoffArray = _node.POutput.WashoffMass;
                        List<Mass> washoffArrayMonthly = new List<Mass>();
                        var monthly = false;
                        if (monthly)
                        {
                            List<DateTime> monthlyDateList = new List<DateTime>();

                            for (DateTime i = Dates.Min(); i <= Dates.Max(); i = i.AddMonths(1))
                            {
                                monthlyDateList.Add(new DateTime(i.Year, i.Month, 1));
                            }

                            foreach (DateTime _month in monthlyDateList)
                            {
                                int initialIndex = Dates.ToList().FindIndex(x => x == _month);
                                int monthLength = DateTime.DaysInMonth(_month.Year, _month.Month);
                                washoffArrayMonthly.Add(Mass.FromKilograms(washoffArray.Skip(initialIndex).Take(monthLength).Select(x => x.Kilograms).Sum()));
                            }

                        }

                        double[] washoffArrayDouble = new double[washoffArray.Length];

                        for (int i = 0; i < washoffArray.Length; i++)
                        {
                            washoffArrayDouble[i] = washoffArray[i].Kilograms;
                        }

                        lineStdDev.Add(ArrayStatistics.StandardDeviation(washoffArrayDouble));
                        lineStdDevNorm.Add(ArrayStatistics.StandardDeviation(washoffArrayDouble) / ArrayStatistics.Mean(washoffArrayDouble));
                        lineMax.Add(ArrayStatistics.Maximum(washoffArrayDouble));
                        lineMaxNorm.Add(ArrayStatistics.Maximum(washoffArrayDouble) / ArrayStatistics.Mean(washoffArrayDouble));
                        lineNumWash0.Add(washoffArrayDouble.Where(x => x == 0).ToList().Count);
                        lineNumWash0Rel.Add(((double)washoffArrayDouble.Where(x => x == 0).ToList().Count) / washoffArrayDouble.Length);
                    }

                    outputStdDev.Add(lineStdDev.ToArray());
                    outputStdDevNorm.Add(lineStdDevNorm.ToArray());
                    outputMax.Add(lineMax.ToArray());
                    outputMaxNorm.Add(lineMaxNorm.ToArray());
                    outputNumWash0.Add(lineNumWash0.ToArray());
                    outputNumWash0Rel.Add(lineNumWash0Rel.ToArray());

                    Tree.SaveQualityTreeToExcel(WSTreePrototype, new FileInfo(rootDirectory + outputPrefix + qualityOutputBase + qualityOutputValues + ".xlsx"));
                }
            }

            Dictionary<string, List<object[]>> dictStatistics = new Dictionary<string, List<object[]>>()
            {
                { "StdDev", outputStdDev },
                {"StdDevNorm", outputStdDevNorm },
                {"Max", outputMax },
                {"MaxNorm", outputMaxNorm },
                {"NumWash0", outputNumWash0 },
                {"NumWash0Rel", outputNumWash0Rel }
            };

            Tree.CreateStatisticsSpreadsheetQuality(new FileInfo(rootDirectory + outputPrefix + "Analysis.xlsx"), dictStatistics);

            Console.WriteLine("Simulação pronta! aperte qualquer tecla para sair.");
            Console.ReadKey();
        }

    }

}
