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
            DateTime now = DateTime.Now;
            string outputPrefix = @"\" + now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString() + "_" + now.Hour.ToString() + now.Minute.ToString() + "_Output";            
            FileInfo outputPath = new FileInfo(CurrentDirectory + outputPrefix + @"\hydrology.xlsx");
            FileInfo summaryPath = new FileInfo(CurrentDirectory + outputPrefix + @"\summary.xlsx");
            //FileInfo resultsPath = new FileInfo(CurrentDirectory + @"\resumo.xlsx");
            FileInfo QualityOutputPath = new FileInfo(CurrentDirectory + outputPrefix + @"\quality.xlsx");
            //List<NodeExternal> WSTree = Tree.SMAPTreeFromExcel(inputPath);
            Console.WriteLine("Lendo dados de entrada...");
            List<NodeExternal> WSTreePrototype = Tree.PrototypeTreeFromExcel(inputPath);

            //SMAPd_Network.SimulateTree(WSTree);
            Console.WriteLine("Simulação do modelo hidrológico...");
            SMAPd_Network.SimulateTree(WSTreePrototype);
            Tree.PrototypeIntegrateBuwoSMAP(WSTreePrototype);

            Console.WriteLine("Simulação do modelo de qualidade...");
            Buildup_Washoff.SimulateTree_NoTransport(WSTreePrototype);
            Pollutogram.SimulateBODTree(WSTreePrototype);

            Console.WriteLine("Gerando planilhas de resultados...");
            Tree.SaveSMAPTreeToExcel(WSTreePrototype, outputPath);            
            Tree.SaveQualityTreeToExcel(WSTreePrototype, QualityOutputPath);
            Tree.SavePrototypeTreeToExcel_SMAP_Qual(WSTreePrototype, summaryPath);
            //Tree.SavePrototypeOverviewDataToExcel(WSTreePrototype, resultsPath);
            Console.WriteLine("Simulação pronta! aperte qualquer tecla para sair.");

            Console.ReadKey();
        }
    }
}
