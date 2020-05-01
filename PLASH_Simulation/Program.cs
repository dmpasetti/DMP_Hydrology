using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using USP_Hydrology;
using OfficeOpenXml;

namespace PLASH_Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo InputFile = new FileInfo(@"D:\VisualStudio_Mestrado\DMP_Hydrology\DMP_Hydrology\Files\Planilha_Input_PLASH.xlsx");

            List<NodeExternal> WSTree = Tree.TreeFromExcel(InputFile);
            double FLT_Timestep = 24;

            foreach(NodeExternal _node in WSTree)
            {
                int _simlength = _node.OBJ_UInput.TimeSeries.FLT_Arr_PrecipSeries.Count();
                PLASH.Parameters _param = new PLASH.Parameters
                {
                    FLT_TimeStep = FLT_Timestep,
                    FLT_AD = _node.OBJ_UInput.FLT_Area,
                    FLT_AI = _node.OBJ_UInput.FLT_Imperv,
                    FLT_AP = _node.OBJ_UInput.FLT_Perv,
                    FLT_IP = 0,
                    FLT_DI = 0,
                    FLT_DP = 10,
                    FLT_KSup = 24,
                    FLT_CS = 200,
                    FLT_CC = 0.4,
                    FLT_CR = 0.01,
                    FLT_PP = 0.2,
                    FLT_KSub = 120,
                    FLT_KCan = 10,
                    FLT_CH = 2,
                    FLT_PS = 1,
                    FLT_UI = 0.5
                };

                _node.GetPLASH = new PLASH(_simlength, _node.OBJ_UInput.TimeSeries, new PLASH.InitialConditions(), _param);                
            }

            List<NodeExternal> OrderedTree = WSTree.OrderBy(x => x.OBJ_Node.INT_Level).ToList();

            foreach(NodeExternal _node in OrderedTree)
            {
                if(_node.OBJ_Node.INT_Level > 1)
                {
                    double[] QUp = new double[_node.GetPLASH.GetSimulationLength];
                    List<PLASH> LstUpstreamWS = (from _obj in OrderedTree where _obj.OBJ_Node.OBJ_Downstream.ID_Watershed == _node.OBJ_Node.ID_Watershed select _obj.GetPLASH).ToList();  
                    foreach(PLASH _sim in LstUpstreamWS)
                    {
                        QUp.Zip(_sim.GetOutput.FLT_Arr_Qt_Calc, (x, y) => x + y);
                    }
                    _node.GetPLASH.GetInput.FLT_Arr_QtUpstream = QUp;
                }
                PLASH.Run(_node.GetPLASH);
            }

            //foreach(NodeExternal _node in WSTree)
            //{
            //    NodeExternal WSCal = WSTree.Where(x => x.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
            //    string STRCal = WSCal.STR_Watershed ?? "";
            //    Console.WriteLine($"Bacia {_node.STR_Watershed}: " +
            //        $"\nArea: {_node.OBJ_UInput.FLT_Area};" +
            //        $"\nFracao Impermeavel: {_node.OBJ_UInput.FLT_Imperv};" +
            //        $"\nFracao Permeavel: {_node.OBJ_UInput.FLT_Perv};" +
            //        $"\nComprimento do talvegue: {_node.OBJ_UInput.FLT_StreamLength};" +
            //        $"\nDeclividade Media: {_node.OBJ_UInput.FLT_AvgSlope};" +
            //        $"\nCN Medio: {_node.OBJ_UInput.FLT_AvgCN}" +
            //        $"\nBacia de calibracao {STRCal}");
            //    Console.ReadKey();
            //}

            
            //int SimLength = 30;
            //PLASH.Input _Input = new PLASH.Input
            //{
            //    FLT_Arr_PrecipSeries = new double[SimLength],
            //    FLT_Arr_EPSeries = new double[SimLength],
            //    FLT_Arr_QtObsSeries = new double[SimLength],
            //    FLT_Arr_QtUpstream = new double[SimLength]
            //};

            //PLASH.InitialConditions _Init = new PLASH.InitialConditions();

            //PLASH.Parameters _Param = new PLASH.Parameters
            //{
            //    FLT_TimeStep = 24,
            //    FLT_AD = 100,
            //    FLT_AI = 0.02,
            //    FLT_DI = 3,
            //    FLT_AP = 0.9,
            //    FLT_IP = 5,
            //    FLT_DP = 3,
            //    FLT_KSup = 24,
            //    FLT_CS = 200,
            //    FLT_CC = 0.3,
            //    FLT_CR = 0.01,
            //    FLT_PP = 0.5,
            //    FLT_KSub = 120,
            //    FLT_KCan = 10,
            //    FLT_CH = 3,
            //    FLT_FS = 30,
            //    FLT_PS = 1,
            //    FLT_UI = 0.5
            //};

            //PLASH test = new PLASH(SimLength, _Input, _Init, _Param);

            //PLASH.Run(test);
        }
    }
}
