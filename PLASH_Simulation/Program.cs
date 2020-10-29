using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using USP_Hydrology;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using System.Windows.Forms;





namespace PLASH_Simulation
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            Form1 form = new Form1();
            Application.Run(form);

            string pathInput = form.InputFileName;
            string pathOutput = form.OutputFileName;

            if (pathInput == null || pathOutput == null)
            {                
                Environment.Exit(0);
            }                        

            FileInfo InputFile = new FileInfo(form.InputFileName);
            FileInfo OutputFile = new FileInfo(form.OutputFileName);            

            List<NodeExternal> WSTree = Tree.PLASHTreeFromExcel(InputFile);

            PLASH.SimulateTree(WSTree);

            Tree.SavePLASHTreeToExcel(WSTree, OutputFile);
         
        }
    }
}
