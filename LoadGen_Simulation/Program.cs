using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using USP_Hydrology;
using OfficeOpenXml;


namespace LoadGen_Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
            FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
            FileInfo outputPath = new FileInfo(CurrentDirectory + @"\output.xlsx");
            //List<NodeExternal> WSTree = Tree.BuWoTreeFromExcel(inputPath);
            List<NodeExternal> WSTree = Tree.BuWoTreeFromExcel_Simplified(inputPath);
            Buildup_Washoff.SimulateTree_NoTransport(WSTree);
            Tree.SaveBuWoTreeToExcel(WSTree, outputPath);
        }

        //static void Main(string[] args)
        //{
        //    var CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

        //    FileInfo inputPath = new FileInfo(CurrentDirectory + @"\input.xlsx");
        //    //FileInfo outputPath = new FileInfo(CurrentDirectory + @"\output.xlsx");
        //    FileInfo outputPath = new FileInfo(CurrentDirectory + @"\outputBuwoAnalysis.xlsx");

        //    List<NodeExternal> WSTree = Tree.BuWoTreeFromExcel_Simplified(inputPath);
        //    List<Buildup_Washoff> lstBuWo = WSTree[0].GetBuWo;

        //    double MinNw = 0;
        //    double MaxNw = 10;
        //    double MinKw = 0;
        //    double MaxKw = 10;
        //    double Step = 0.05;
        //    double RowNum = (MaxNw - MinNw) / Step;
        //    double ColNum = (MaxKw - MinKw) / Step;


        //    Dictionary<(double, double), double> DictTotalBuildup = new Dictionary<(double, double), double>();
        //    Dictionary<(double, double), double> DictTotalWashoff = new Dictionary<(double, double), double>();

        //    double[,] MatrixTotalBuildup = new double[(int)RowNum, (int)ColNum];
        //    double[,] MatrixTotalWashoff = new double[(int)RowNum, (int)ColNum];

        //    double Nw = MinNw;
        //    double Kw = MinKw;

        //    var FirstRowExcel = new List<string>();
        //    FirstRowExcel.Add(null);

        //    for (int i = 0; i < RowNum; i ++) {
        //        Nw += Step;
        //        Kw = MinKw;
        //        for(int j = 0; j < ColNum; j ++)
        //        {
        //            Kw += Step;
        //            if(i == 0)
        //            {
        //                FirstRowExcel.Add(Kw.ToString());
        //            }


        //            lstBuWo[0].GetParam.FLT_Nw = Nw;
        //            lstBuWo[0].GetParam.FLT_Kw = Kw;
        //            Buildup_Washoff.SimulateTree_NoTransport(WSTree);
        //            Buildup_Washoff AggregateUse = WSTree[0].GetBuWo.Where(x => x.GetParam.BOOL_Aggregate == true).FirstOrDefault();

        //            MatrixTotalBuildup[i, j] = Buildup_Washoff.TotalPeriodLoad(AggregateUse);
        //            MatrixTotalWashoff[i, j] = Buildup_Washoff.TotalPeriodWashoff(AggregateUse);
        //            lstBuWo.Remove(AggregateUse);
        //        }
        //    }

        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (ExcelPackage package = new ExcelPackage())
        //    {
        //        package.Workbook.Worksheets.Add("ResultBuildup");
        //        package.Workbook.Worksheets.Add("ResultWashoff");
        //        var HeaderRow = new List<string[]> {
        //            FirstRowExcel.ToArray()
        //        };                

        //        List<object[]> cellDataBuildup = new List<object[]>();
        //        List<object[]> cellDataWashoff = new List<object[]>();
        //        var NwOutput = MinNw;
        //        for (int i = 0; i < RowNum; i++)
        //        {
        //            List<object> RowBuildup = new List<object>();
        //            List<object> RowWashoff = new List<object>();
        //            NwOutput += Step;
        //            RowBuildup.Add(NwOutput);
        //            RowWashoff.Add(NwOutput);
        //            for(int j = 0; j < RowNum; j++)
        //            {
        //                RowBuildup.Add(MatrixTotalBuildup[i, j]);
        //                RowWashoff.Add(MatrixTotalWashoff[i, j]);
        //            }
        //            cellDataBuildup.Add(RowBuildup.ToArray());
        //            cellDataWashoff.Add(RowWashoff.ToArray());
        //        }

        //        var worksheet = package.Workbook.Worksheets["ResultBuildup"];

        //        worksheet.Cells[1, 1].LoadFromArrays(HeaderRow);
        //        worksheet.Cells[2, 1].LoadFromArrays(cellDataBuildup);

        //        worksheet = package.Workbook.Worksheets["ResultWashoff"];
        //        worksheet.Cells[1, 1].LoadFromArrays(HeaderRow);
        //        worksheet.Cells[2, 1].LoadFromArrays(cellDataWashoff);


        //        FileInfo excelFile = outputPath;
        //        package.SaveAs(excelFile);
        //    }

        //}
    }
}
