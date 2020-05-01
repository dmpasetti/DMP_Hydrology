using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;


namespace USP_Hydrology
{
    public class NodeInternal
    {
        private static int idCounter;
        public int ID_Watershed;
        public NodeInternal OBJ_Downstream;
        public int INT_Level;

        public NodeInternal(NodeInternal _dw)
        {
            ID_Watershed = idCounter++;
            OBJ_Downstream = _dw;
        }

        public NodeInternal()
        {
            ID_Watershed = idCounter++;
        }
    }

    public class NodeExternal
    {
        public string STR_Watershed;
        public NodeInternal OBJ_Node;
        public PLASH.UserInput OBJ_UInput;
        public (NodeInternal, double) TPL_CalibrationWS;
        public PLASH GetPLASH;
        
     
    }

    public class Tree
    {
        public static HashSet<int> ID_Level1(List<NodeInternal> lstWS)
        {
            HashSet<int> all = new HashSet<int>();
            HashSet<int> fathers = new HashSet<int>();

            foreach (NodeInternal _node in lstWS)
            {
                all.Add(_node.ID_Watershed);
                if (_node.OBJ_Downstream != null)
                {
                    fathers.Add(_node.OBJ_Downstream.ID_Watershed);
                }
            }
            all.ExceptWith(fathers);
            return all;
        }

        public static void AssignLevel(List<NodeInternal> lstWS)
        {
            Dictionary<int, NodeInternal> DictWS = new Dictionary<int, NodeInternal>();

            foreach (NodeInternal _obj in lstWS)
            {
                DictWS.Add(_obj.ID_Watershed, _obj);
            }

            HashSet<int> ID_1 = ID_Level1(lstWS);

            Queue<NodeInternal> workQueue = new Queue<NodeInternal>();

            foreach (int IDLeaf in ID_1)
            {
                NodeInternal Leaf = DictWS[IDLeaf];
                Leaf.INT_Level = 1;
                workQueue.Enqueue(Leaf);
            }

            while (workQueue.Count > 0)
            {
                NodeInternal node = workQueue.Peek();

                if (node.OBJ_Downstream != null)
                {
                    if (node.INT_Level >= node.OBJ_Downstream.INT_Level)
                    {
                        node.OBJ_Downstream.INT_Level = node.INT_Level + 1;
                        workQueue.Enqueue(node.OBJ_Downstream);
                    }
                }
                workQueue.Dequeue();
            }

        }


        public static List<NodeExternal> TreeFromExcel(FileInfo InputFile)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, double[]> DictArrayPrec = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayEvap = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQobs = new Dictionary<string, double[]>();
            DateTime[] arrDates;
            using (ExcelPackage package = new ExcelPackage(InputFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Bacias"];
                int ColCount = worksheet.Dimension.End.Column;
                int RowCount = worksheet.Dimension.End.Row;
                List<string> lstStringWS = new List<string>();
                
                Dictionary<string, NodeInternal> DictDownNode = new Dictionary<string, NodeInternal>();
                for (int row = 2; row <= RowCount; row++)
                {

                    string _ws = worksheet.Cells[row, 1].Value.ToString();

                    PLASH.UserInput _uinput = new PLASH.UserInput
                    {
                        FLT_Area = Convert.ToDouble(worksheet.Cells[row, 3].Value),
                        FLT_Imperv = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                        FLT_Perv = Convert.ToDouble(worksheet.Cells[row, 5].Value),
                        FLT_AvgCN = Convert.ToDouble(worksheet.Cells[row, 6].Value),
                        FLT_StreamLength = Convert.ToDouble(worksheet.Cells[row, 7].Value),
                        FLT_AvgSlope = Convert.ToDouble(worksheet.Cells[row, 8].Value)
                    };

                    NodeInternal node = new NodeInternal(null);

                    lstNode.Add(new NodeExternal
                    {
                        STR_Watershed = _ws,
                        OBJ_Node = node,
                        OBJ_UInput = _uinput
                    });
                    lstNodeInternal.Add(node);
                    DictDownNode.Add(_ws, node);
                }
                for (int row = 2; row <= RowCount; row++)
                {
                    if (worksheet.Cells[row, 2].Value != null)
                    {
                        if (worksheet.Cells[row, 2].Value.ToString() != "")
                        {
                            string _ws = worksheet.Cells[row, 1].Value.ToString();
                            string _dw = worksheet.Cells[row, 2].Value.ToString();

                            DictDownNode[_ws].OBJ_Downstream = DictDownNode[_dw];
                                                    
                        }
                    }
                    string WSCal = worksheet.Cells[row, 9].Value.ToString();
                    double FracCal = Convert.ToDouble(worksheet.Cells[row, 10].Value);
                    NodeInternal WSCalNode = DictDownNode[WSCal];
                    lstNode[row - 2].TPL_CalibrationWS = (WSCalNode, FracCal);
                }

                
                worksheet = package.Workbook.Worksheets["Precipitacao"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;
                List<DateTime> lstDates = new List<DateTime>();
                for(int row = 2; row <= RowCount; row++)
                {
                    lstDates.Add(Convert.ToDateTime(worksheet.Cells[row, 1].Value));
                }
                arrDates = lstDates.ToArray();
                for(int col = 2; col <= ColCount; col++)
                {
                    string WSName = worksheet.Cells[1, col].Value.ToString();
                    List<double> lstPrec = new List<double>();
                    for(int row = 2; row <= RowCount; row++)
                    {
                        lstPrec.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                    }
                    DictArrayPrec.Add(WSName, lstPrec.ToArray());
                }

                worksheet = package.Workbook.Worksheets["Evapotranspiracao"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;

                for (int col = 2; col <= ColCount; col++)
                {
                    string WSName = worksheet.Cells[1, col].Value.ToString();
                    List<double> lstEvap = new List<double>();
                    for (int row = 2; row <= RowCount; row++)
                    {
                        lstEvap.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                    }
                    DictArrayEvap.Add(WSName, lstEvap.ToArray());
                }

                worksheet = package.Workbook.Worksheets["Vazao"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;

                for (int col = 2; col <= ColCount; col++)
                {
                    string WSName = worksheet.Cells[1, col].Value.ToString();
                    List<double> lstQobs = new List<double>();
                    for (int row = 2; row <= RowCount; row++)
                    {
                        lstQobs.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                    }
                    DictArrayQobs.Add(WSName, lstQobs.ToArray());
                }
                


            }
            AssignLevel(lstNodeInternal);

            foreach (NodeExternal _node in lstNode)
            {
                string WSName = _node.STR_Watershed;
                NodeExternal WSCal = lstNode.Where(x => x.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
                double[] arrayPrec = DictArrayPrec[WSName];
                double[] arrayEvap = DictArrayEvap[WSName];
                double[] arrayQobs = DictArrayQobs[WSCal.STR_Watershed];

                _node.OBJ_UInput.TimeSeries = new PLASH.Input
                {
                    DTE_Arr_TimeSeries = arrDates,
                    FLT_Arr_PrecipSeries = arrayPrec,
                    FLT_Arr_EPSeries = arrayEvap,
                    FLT_Arr_QtObsSeries = arrayQobs,
                    FLT_Arr_QtUpstream = new double[arrayPrec.Count()]
                };                
            }

            return lstNode;

        }



    }
}
