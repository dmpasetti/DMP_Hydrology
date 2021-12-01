using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USP_Hydrology;
using System.IO;
using System.Diagnostics;

namespace SMAPSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            double doubleMin = 0D;
            double doubleMax = 100000D;
            double doubleStep = 0.001D;

            decimal decimalMin = 0M;
            decimal decimalMax = 100000M;
            decimal decimalStep = 0.001M;
            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();


            Console.WriteLine("loop double");
            sw1.Start();
            for (double i = doubleMin; i <= doubleMax; i += doubleStep)
            {
                double num = Math.Round(i, 2);                
            }
            sw1.Stop();

            TimeSpan ts1 = sw1.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime1 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts1.Hours, ts1.Minutes, ts1.Seconds,
                ts1.Milliseconds / 10);
            Console.WriteLine("RunTime Math round " + elapsedTime1);


            Console.WriteLine("loop decimal");

            sw2.Start();
            for (decimal i = decimalMin; i <= decimalMax; i += decimalStep)
            {
                double num = (double)i;                
            }
            sw2.Stop();

            TimeSpan ts2 = sw2.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts2.Hours, ts2.Minutes, ts2.Seconds,
                ts2.Milliseconds / 10);
            Console.WriteLine("RunTime cast " + elapsedTime2);


            Console.ReadKey();

            //var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

            //FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
            //FileInfo outputPath = new FileInfo(CurrentDirectory + @"\output.xlsx");

            //List<NodeExternal> WSTree = Tree.SMAPTreeFromExcel(inputPath);

            //SMAPd_Network.SimulateTree(WSTree);
            //Tree.SaveSMAPTreeToExcel(WSTree, outputPath);
            //Console.WriteLine("Done!");

            //Console.ReadKey();

        }
    }
}
