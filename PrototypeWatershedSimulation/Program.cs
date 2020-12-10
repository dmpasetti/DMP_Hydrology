using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;
using System.IO;

namespace PrototypeWatershedSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

            FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
            FileInfo outputPath = new FileInfo(CurrentDirectory + @"\output.xlsx");
            FileInfo summaryPath = new FileInfo(CurrentDirectory + @"\summary.xlsx");
            FileInfo QualityOutputPath = new FileInfo(CurrentDirectory + @"\quality.xlsx");
            //List<NodeExternal> WSTree = Tree.SMAPTreeFromExcel(inputPath);
            List<NodeExternal> WSTreePrototype = Tree.PrototypeTreeFromExcel(inputPath);

            //SMAPd_Network.SimulateTree(WSTree);
            SMAPd_Network.SimulateTree(WSTreePrototype);
            Tree.PrototypeIntegrateBuwoSMAP(WSTreePrototype);




            Buildup_Washoff.SimulateTree_NoTransport(WSTreePrototype);
            Pollutogram.SimulateTree(WSTreePrototype);
            //Tree.SaveSMAPTreeToExcel(WSTree, outputPath);
            //Tree.SavePrototypeTreeToExcel(WSTree, summaryPath);
            //Tree.SavePrototypeTreeToExcel_SMAP(WSTree, summaryPath);
            //Tree.SaveSMAPTreeToExcel(WSTreePrototype, outputPath);
            Tree.SaveQualityTreeToExcel(WSTreePrototype, QualityOutputPath);
            Tree.SavePrototypeTreeToExcel_SMAP_Qual(WSTreePrototype, summaryPath);
            Console.WriteLine("Done!");

            Console.ReadKey();
        }
    }
}
