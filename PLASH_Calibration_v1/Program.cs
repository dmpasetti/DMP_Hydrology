using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;
using System.IO;

namespace PLASH_Calibration_v1
{
    class Program
    {
        static void Main(string[] args)
        {
            var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

            FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");

            List<NodeExternal> Tree = USP_Hydrology.Tree.PLASHTreeFromExcel(inputPath);

            PLASH.SimulateTree(Tree);

            NodeExternal CalibrationNode = Tree.Where(x => x.OBJ_Node.ID_Watershed == x.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
            PLASH CalibrationSim = new PLASH(0, new PLASH.Input(), new PLASH.InitialConditions(), new PLASH.Parameters());
            if(CalibrationNode != null)
            {
                CalibrationSim = CalibrationNode.GetPLASH;
            }

            string outputPath = CurrentDirectory + @"\output.txt";

            using (StreamWriter file = new StreamWriter(outputPath))
            {
                file.WriteLine(CalibrationNode.GetPLASH.GetParameters.BOOL_ValidSimulation);
                for(int i = 0; i < CalibrationSim.GetSimulationLength; i++)
                {
                    file.WriteLine("{0} \t {1}",  Math.Round(CalibrationSim.GetOutput.FLT_Arr_Qt_Calibration[i], 5).ToString("F5"), Math.Round(CalibrationSim.GetInput.FLT_Arr_QtObsSeries[i], 5).ToString("F5"));
                }
            }
        }
    }
}
