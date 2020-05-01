using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;

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
    public class PLASHFitness : IFitness
    {
        public PLASH Simulation { get; private set; }

        public PLASHFitness(PLASH _Simulation)
        {
            Simulation = _Simulation;
        }


        public double Evaluate(IChromosome chromosome)
        {
            var fc = chromosome as FloatingPointChromosome;

            var values = fc.ToFloatingPoints();




            return 0;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            List<double> GAParamMin = new List<double>
            {
                1,      //Maximum Impervious Detention (mm)
                1,      //Maximum Interception (mm)
                3,     //Maximum Pervious Detention (mm)
                10,     //Surface Reservoir Decay (h)
                100,    //Soil Saturation Capacity (mm)
                0.3,    //Field Capacity (%)
                0.02,   //Recharge Capacity (%)
                0.01,   //Deep Percolation (mm/h)
                120,    //Aquifer Reservoir Decay (d)
                6,     //Channel Reservoir Decay (h)
                0.2,      //Hydraulic Conductivity (mm/h)
                10,     //Soil Capilarity Factor (mm)
                0.6,      //Soil Porosity (cm3/cm3)
                0     //Initial Moisture (cm3/cm3)
            };

            List<double> GAParamMax = new List<double>
            {
                5,      //Maximum Impervious Detention (mm)
                3,      //Maximum Interception (mm)
                10,     //Maximum Pervious Detention (mm)
                24,     //Surface Reservoir Decay (h)
                200,    //Soil Saturation Capacity (mm)
                0.4,    //Field Capacity (%)
                0.08,   //Recharge Capacity (%)
                0.03,   //Deep Percolation (mm/h)
                240,    //Aquifer Reservoir Decay (d)
                12,     //Channel Reservoir Decay (h)
                3,      //Hydraulic Conductivity (mm/h)
                50,     //Soil Capilarity Factor (mm)
                1,      //Soil Porosity (cm3/cm3)
                0.3     //Initial Moisture (cm3/cm3)
            };

            var Chromosome = new FloatingPointChromosome(GAParamMin.ToArray(), GAParamMax.ToArray(), Enumerable.Repeat(64, GAParamMax.Count).ToArray(), Enumerable.Repeat(3, GAParamMax.Count).ToArray());

            var Population = new Population(50, 100, Chromosome);

            

            int SimLength = 30;
            PLASH.Input _Input = new PLASH.Input
            {
                FLT_Arr_PrecipSeries = new double[SimLength],
                FLT_Arr_EPSeries = new double[SimLength],
                FLT_Arr_QtObsSeries = new double[SimLength],
                FLT_Arr_QtUpstream = new double[SimLength]
            };

            PLASH.InitialConditions _Init = new PLASH.InitialConditions();

            PLASH.Parameters _Param = new PLASH.Parameters
            {
                FLT_TimeStep = 24,
                FLT_AD = 100,
                FLT_AI = 0.02,
                FLT_DI = 3,
                FLT_AP = 0.9,
                FLT_IP = 5,
                FLT_DP = 3,
                FLT_KSup = 24,
                FLT_CS = 200,
                FLT_CC = 0.3,
                FLT_CR = 0.01,
                FLT_PP = 0.5,
                FLT_KSub = 120,
                FLT_KCan = 10,
                FLT_CH = 3,
                FLT_FS = 30,
                FLT_PS = 1,
                FLT_UI = 0.5
            };

            PLASH test = new PLASH(SimLength, _Input, _Init, _Param);

            PLASH.Run(test);

            Console.WriteLine("OK!");
        }
    }
}
