using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;
using System.IO;
namespace SMAPSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

            FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
            FileInfo outputPath = new FileInfo(CurrentDirectory + @"\output.xlsx");

            List<NodeExternal> WSTree = Tree.SMAPTreeFromExcel(inputPath);

            SMAPd_Network.SimulateTree(WSTree);
            Tree.SaveSMAPTreeToExcel(WSTree, outputPath);
            Console.WriteLine("Done!");

            Console.ReadKey();

        }
    }
}
