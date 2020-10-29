using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;
using System.IO;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;


namespace PLASH_Calibration
{
    

    class Program
    {
        static void Main(string[] args)
        {
            var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

            FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
            FileInfo outputPath = new FileInfo(CurrentDirectory + @"\output.xlsx");

            

            List<NodeExternal> WSTree = Tree.PLASHTreeFromExcel(inputPath);

            List<double> GAParamMin = new List<double>();
            List<double> GAParamMax = new List<double>();

            for (int i = 0; i < WSTree.Count; i++)
            {
                PLASH.Parameters _param = new PLASH.Parameters
                {
                    FLT_CH = PLASH.PLASHKFromCN(WSTree[i].OBJ_UInput.FLT_AvgCN),
                    FLT_KCan = PLASH.PLASH_TCDooge(WSTree[i].OBJ_UInput.FLT_Area, 10 * WSTree[i].OBJ_UInput.FLT_AvgSlope)
                };
                GAParamMin.AddRange(PLASH.GAParametersSetMin(/*_param*/));
                GAParamMax.AddRange(PLASH.GAParametersSetMax(/*_param*/));
            }

            //GAParamMin.AddRange(PLASH.GAParametersSetMin());
            //GAParamMax.AddRange(PLASH.GAParametersSetMax());
            Console.WriteLine("Correlacao Minima:");
            double MinCorr = Convert.ToDouble(Console.ReadLine());
            var Chromosome = new FloatingPointChromosome(GAParamMin.ToArray(), GAParamMax.ToArray(), Enumerable.Repeat(64, GAParamMax.Count).ToArray(), Enumerable.Repeat(3, GAParamMax.Count).ToArray());

            var Population = new Population(50, 200, Chromosome);

            var Fitness = new FuncFitness((c) =>
            {
                var fc = c as FloatingPointChromosome;
                var values = fc.ToFloatingPoints();

                //List<NodeExternal> newTree = Tree.PLASHTreeFromExcel(inputPath);
                List<NodeExternal> newTree = Tree.DuplicateTree(WSTree);
                PLASH.SimulateTree(newTree, true, values);
                
                PLASH NodeCalibration = newTree.Where(x => x.OBJ_Node.ID_Watershed == x.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault().GetPLASH;

                double MinimumCorrelation = MinCorr;
                double TreeCorrelation = PLASH.Tree_MinimumCorrelation_Rainfall_SurfaceFlow(newTree);

                //double BFSFQt_Min = 0.05;
                //double BFSFQt_Max = 0.5;
                //double ObsFlowStatVariation = 0.9;

                bool NoCorruptedResults = Tree.ValidTree(newTree);

                //bool RainSFlowCorrelationValid = TreeCorrelation >= MinimumCorrelation;
                //bool BFSFValid = PLASH.Tree_ValidadeBaseFlow_SurfaceFlow_Quantile(newTree, BFSFQt_Min, BFSFQt_Max);
                //bool ObsFlowValid = PLASH.Tree_Validation_ObservedFlowAnalysis(newTree, ObsFlowStatVariation);

                //bool AllValid = NoCorruptedResults && BFSFValid && ObsFlowValid && RainSFlowCorrelationValid;

                //double Penalty = 0;

                //if (!BFSFValid)
                //{
                //    Penalty += 500;
                //}
                //if (!ObsFlowValid)
                //{
                //    Penalty += 200;
                //}
                //if (!RainSFlowCorrelationValid)
                //{
                //    Penalty += 100;
                //}

                if (NoCorruptedResults/* && RainSFlowCorrelationValid*/)
                {
                    double NS = PLASH.PLASHNashSutcliffe(NodeCalibration);
                    return NS;// - Penalty;
                }
                else
                {
                    return Double.NegativeInfinity;
                }

            });

            var Selection = new EliteSelection();
            var Crossover = new UniformCrossover(0.9F);
            var Mutation = new FlipBitMutation();
            
            Console.WriteLine("Selecionar método de terminação:");
            Console.WriteLine("1 - Numero fixo de gerações");
            Console.WriteLine("2 - Valor mínimo de NSE");
            Console.WriteLine("3 - Estagnação");
            int Method = Convert.ToInt32(Console.ReadLine());
            double Limit = 0;
            TerminationBase Termination = new GenerationNumberTermination(100);
            switch (Method)
            {
                case 1:
                    Console.WriteLine("Número de gerações:");
                    Limit = Convert.ToDouble(Console.ReadLine());
                    Termination = new GenerationNumberTermination((int)Limit);
                    break;
                case 2:
                    Console.WriteLine("Valor mínimo de NSE:");
                    Limit = Convert.ToDouble(Console.ReadLine());
                    Termination = new FitnessThresholdTermination(Limit);
                    break;
                case 3:
                    Console.WriteLine("Número de gerações estagnadas:");
                    Limit = Convert.ToDouble(Console.ReadLine());
                    Termination = new FitnessStagnationTermination((int)Limit);
                    break;
            }



            //var Termination = new FitnessThresholdTermination(0.92);


            //var Termination = new FitnessStagnationTermination(50);

            var GA = new GeneticAlgorithm(
                Population,
                Fitness,
                Selection,
                Crossover,
                Mutation);

            GA.CrossoverProbability = 0.9F;            
            GA.MutationProbability = 0.9F;
            GA.Termination = Termination;

            Console.WriteLine("Genetic algorithm tests");
            var latestFitness = 0.0;

            GA.GenerationRan += (sender, e) =>
            {
                var bestChromosome = GA.BestChromosome as FloatingPointChromosome;
                var bestFitness = bestChromosome.Fitness.Value;

                if (bestFitness != latestFitness)
                {
                    latestFitness = bestFitness;
                    var phenotype = bestChromosome.ToFloatingPoints();

                    Console.WriteLine("Generation {0}: {1}",

                        GA.GenerationsNumber,
                        bestFitness);

                }
            };

            GA.Start();

            Console.WriteLine("GA Over!");

            var BestChrom = GA.BestChromosome as FloatingPointChromosome;
            var BestVal = BestChrom.ToFloatingPoints();
            
            PLASH.SimulateTree(WSTree, true, BestVal);

            Tree.SavePLASHTreeToExcel(WSTree, outputPath);

            Console.WriteLine("OK!");
            Console.ReadKey();
        }
    }
}
