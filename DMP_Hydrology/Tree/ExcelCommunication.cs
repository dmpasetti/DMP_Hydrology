using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using LabSid.Sabesp.Hydrology;
using UnitsNet;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;

namespace USP_Hydrology
{
    public partial class Tree
    {

        public static List<NodeExternal> SMAPTreeFromExcel(FileInfo InputFile)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, double[]> DictArrayPrec = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayEvap = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQobs = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQMont = new Dictionary<string, double[]>();
            Dictionary<string, Model_SMAPd.ModelParameters> DictParam = new Dictionary<string, Model_SMAPd.ModelParameters>();
            Dictionary<string, Model_SMAPd.InitialConditions> DictInitialConditions = new Dictionary<string, Model_SMAP.InitialConditions>();
            Dictionary<string, Model_Muskingum> DictMusk = new Dictionary<string, Model_Muskingum>();
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

                    NodeInternal node = new NodeInternal(null);

                    lstNode.Add(new NodeExternal
                    {
                        STR_Watershed = _ws,
                        OBJ_Node = node,
                        WatershedArea = Convert.ToDouble(worksheet.Cells[row, 3].Value)
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

                worksheet = package.Workbook.Worksheets["ParametrosSMAP"];
                RowCount = worksheet.Dimension.End.Row;

                for (int row = 2; row <= RowCount; row++)
                {
                    string WSName = worksheet.Cells[row, 1].Value.ToString();

                    Model_SMAPd.ModelParameters _param = new Model_SMAPd.ModelParameters
                    {
                        PrecipitationWeighting = Ratio.FromDecimalFractions(Convert.ToDouble(worksheet.Cells[row, 2].Value)),
                        EvapotranspirationWeighting = Ratio.FromDecimalFractions(Convert.ToDouble(worksheet.Cells[row, 3].Value)),
                        SaturationCapacity = Length.FromMillimeters(Convert.ToDouble(worksheet.Cells[row, 4].Value)),
                        InitialAbstraction = Length.FromMillimeters(Convert.ToDouble(worksheet.Cells[row, 5].Value)),
                        FieldCapacity = Ratio.FromPercent(Convert.ToDouble(worksheet.Cells[row, 6].Value)),
                        GroundwaterRecharge = Ratio.FromPercent(Convert.ToDouble(worksheet.Cells[row, 7].Value)),
                        DirectRunoffHalf = Duration.FromDays(Convert.ToDouble(worksheet.Cells[row, 8].Value)),
                        BaseflowHalf = Duration.FromDays(Convert.ToDouble(worksheet.Cells[row, 9].Value))
                    };
                    DictParam.Add(WSName, _param);

                    Model_SMAPd.InitialConditions _initial = new Model_SMAP.InitialConditions
                    {
                        SoilMoisture = Ratio.FromPercent(Convert.ToDouble(worksheet.Cells[row, 10].Value)),
                        Baseflow = VolumeFlow.FromCubicMetersPerSecond(Convert.ToDouble(worksheet.Cells[row, 11].Value))
                    };
                    DictInitialConditions.Add(WSName, _initial);

                    Model_Muskingum.ModelParameters _muskParam = new Model_Muskingum.ModelParameters
                    {
                        TimeStep = Duration.FromDays(1D),
                        TravelTime = Duration.FromDays(Convert.ToDouble(worksheet.Cells[row, 12].Value)),
                        WeightingFactor = Ratio.FromDecimalFractions(Convert.ToDouble(worksheet.Cells[row, 13].Value))
                    };

                    Model_Muskingum.InitialConditions _initMusk = new Model_Muskingum.InitialConditions
                    {
                        Channel = _initial.Baseflow
                    };

                    Model_Muskingum _muskingumConfiguration = new Model_Muskingum();
                    _muskingumConfiguration.SimulationStart(_muskParam, _initMusk);
                    DictMusk.Add(WSName, _muskingumConfiguration);
                }

                worksheet = package.Workbook.Worksheets["Precipitacao"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;
                List<DateTime> lstDates = new List<DateTime>();
                for (int row = 2; row <= RowCount; row++)
                {
                    lstDates.Add(Convert.ToDateTime((worksheet.Cells[row, 1].Value)));
                    //lstDates.Add(DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[row, 1].Value)));
                }
                arrDates = lstDates.ToArray();
                for (int col = 2; col <= ColCount; col++)
                {
                    string WSName = worksheet.Cells[1, col].Value.ToString();
                    List<double> lstPrec = new List<double>();
                    for (int row = 2; row <= RowCount; row++)
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

                worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "VazaoMontante");

                if (worksheet != null)
                {
                    ColCount = worksheet.Dimension.End.Column;
                    RowCount = worksheet.Dimension.End.Row;

                    for (int col = 2; col <= ColCount; col++)
                    {
                        string WSName = worksheet.Cells[1, col].Value.ToString();
                        List<double> lstQMont = new List<double>();
                        for (int row = 2; row <= RowCount; row++)
                        {
                            lstQMont.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                        }
                        DictArrayQMont.Add(WSName, lstQMont.ToArray());
                    }
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
                double[] arrayQMont = DictArrayQMont.ContainsKey(WSName) ? DictArrayQMont[WSName] : null;
                _node.GetSimulationLength = arrayPrec.Count();

                List<Length> UNPrecList = new List<Length>();
                List<Length> UNEvapList = new List<Length>();
                List<VolumeFlow> UNQObsList = new List<VolumeFlow>();
                List<VolumeFlow> UNQMontList = new List<VolumeFlow>();

                for (int i = 0; i < _node.GetSimulationLength; i++)
                {
                    UNPrecList.Add(Length.FromMillimeters(arrayPrec[i]));
                    UNEvapList.Add(Length.FromMillimeters(arrayEvap[i]));
                    UNQObsList.Add(VolumeFlow.FromCubicMetersPerSecond(arrayQobs[i]));
                    UNQMontList.Add(arrayQMont != null ? VolumeFlow.FromCubicMetersPerSecond(arrayQMont[i]) : VolumeFlow.Zero);
                }

                Model_SMAPd.SpatialFeatures SMAPsf = new Model_SMAP.SpatialFeatures
                {
                    DrainageArea = Area.FromSquareKilometers(_node.WatershedArea),
                    Start = false,
                    End = false
                };

                if (_node.OBJ_Node.INT_Level == 1)
                {
                    SMAPsf.Start = true;
                }

                if (_node.OBJ_Node.OBJ_Downstream == null)
                {
                    SMAPsf.End = true;
                }

                SMAPd_Network _SMAPsimulation = new SMAPd_Network
                {
                    GetInput = new SMAPd_Network.SMAPd_Input
                    {
                        Time = arrDates,
                        Precipitation = UNPrecList.ToArray(),
                        Evapotranspiration = UNEvapList.ToArray(),
                        ObservedFlow = UNQObsList.ToArray(),
                        UpstreamFlow = UNQMontList.ToArray()
                    },
                    SMAPSimulation = new Model_SMAPd(false)
                };
                _SMAPsimulation.SMAPSimulation.SimulationStart(SMAPsf, DictParam[WSName], DictInitialConditions[WSName]);
                _node.GetSMAP = _SMAPsimulation;
                _node.GetMusk = DictMusk[WSName];
            }
            return lstNode;
        }

        public static List<NodeExternal> PLASHTreeFromExcel(FileInfo InputFile)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, double[]> DictArrayPrec = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayEvap = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQobs = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQMont = new Dictionary<string, double[]>();
            Dictionary<string, PLASH.Parameters> DictParam = new Dictionary<string, PLASH.Parameters>();
            Dictionary<string, Muskingum_Daniel> DictMusk = new Dictionary<string, Muskingum_Daniel>();
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
                        FLT_AvgCN = worksheet.Cells[row, 6].Value.ToString() != "agua" ? Convert.ToDouble(worksheet.Cells[row, 6].Value) : -1,
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

                worksheet = package.Workbook.Worksheets["ParametrosPLASH"];
                RowCount = worksheet.Dimension.End.Row;

                for (int row = 2; row <= RowCount; row++)
                {
                    string WSName = worksheet.Cells[row, 1].Value.ToString();

                    PLASH.Parameters _param = new PLASH.Parameters
                    {
                        FLT_TimeStep = Convert.ToDouble(worksheet.Cells[row, 2].Value),
                        FLT_DI = Convert.ToDouble(worksheet.Cells[row, 3].Value),
                        FLT_IP = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                        FLT_DP = Convert.ToDouble(worksheet.Cells[row, 5].Value),
                        FLT_KSup = Convert.ToDouble(worksheet.Cells[row, 6].Value),
                        FLT_CS = Convert.ToDouble(worksheet.Cells[row, 7].Value),
                        FLT_CC = Convert.ToDouble(worksheet.Cells[row, 8].Value),
                        FLT_CR = Convert.ToDouble(worksheet.Cells[row, 9].Value),
                        FLT_PP = Convert.ToDouble(worksheet.Cells[row, 10].Value),
                        FLT_KSub = Convert.ToDouble(worksheet.Cells[row, 11].Value),
                        FLT_KCan = Convert.ToDouble(worksheet.Cells[row, 12].Value),
                        FLT_CH = Convert.ToDouble(worksheet.Cells[row, 13].Value),
                        FLT_FS = Convert.ToDouble(worksheet.Cells[row, 14].Value),
                        FLT_PS = Convert.ToDouble(worksheet.Cells[row, 15].Value),
                        FLT_UI = Convert.ToDouble(worksheet.Cells[row, 16].Value)
                    };
                    DictParam.Add(WSName, _param);

                    Muskingum_Daniel _musk = new Muskingum_Daniel
                    {
                        FLT_Timestep = _param.FLT_TimeStep,
                        FLT_K = Convert.ToDouble(worksheet.Cells[row, 17].Value),
                        FLT_X = Convert.ToDouble(worksheet.Cells[row, 18].Value)
                    };
                    DictMusk.Add(WSName, _musk);


                }



                worksheet = package.Workbook.Worksheets["Precipitacao"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;
                List<DateTime> lstDates = new List<DateTime>();
                for (int row = 2; row <= RowCount; row++)
                {
                    lstDates.Add(Convert.ToDateTime((worksheet.Cells[row, 1].Value)));
                    //lstDates.Add(DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[row, 1].Value)));
                }
                arrDates = lstDates.ToArray();
                for (int col = 2; col <= ColCount; col++)
                {
                    string WSName = worksheet.Cells[1, col].Value.ToString();
                    List<double> lstPrec = new List<double>();
                    for (int row = 2; row <= RowCount; row++)
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

                worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "VazaoMontante");

                if (worksheet != null)
                {
                    ColCount = worksheet.Dimension.End.Column;
                    RowCount = worksheet.Dimension.End.Row;

                    for (int col = 2; col <= ColCount; col++)
                    {
                        string WSName = worksheet.Cells[1, col].Value.ToString();
                        List<double> lstQMont = new List<double>();
                        for (int row = 2; row <= RowCount; row++)
                        {
                            lstQMont.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                        }
                        DictArrayQMont.Add(WSName, lstQMont.ToArray());
                    }

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
                double[] arrayQMont = DictArrayQMont.ContainsKey(WSName) ? DictArrayQMont[WSName] : null;

                _node.OBJ_UInput.InputParameters = DictParam[WSName];

                _node.OBJ_UInput.TimeSeries = new PLASH.Input
                {
                    DTE_Arr_TimeSeries = arrDates,
                    FLT_Arr_PrecipSeries = arrayPrec,
                    FLT_Arr_EPSeries = arrayEvap,
                    FLT_Arr_QtObsSeries = arrayQobs,
                    FLT_Arr_QtUpstream = arrayQMont
                };
                _node.GetMusk_OLD = DictMusk[WSName];

            }

            return lstNode;

        }

        public static void SaveSMAPTreeToExcel(List<NodeExternal> Tree, FileInfo OutputFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {

                #region Topologia
                package.Workbook.Worksheets.Add("Topologia");

                var HeaderRowTopology = new List<string[]>()
                {
                    new string[]
                    {
                        "Bacia", "Jusante", "Area (km2)",
                        //"Frac_Imperm (%)", "Frac_Perm (%)",
                        //"CN_Medio", "Comp_Talv (km)", "Decliv_Media (%)",
                        "Bacia_Cal", "Frac_Cal", "Nash_Sut"
                    }
                };

                var worksheet = package.Workbook.Worksheets["Topologia"];
                List<object[]> cellDataTopology = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {
                    //var _TopoParam = _node.OBJ_UInput;
                    NodeExternal NodeDown = new NodeExternal();
                    foreach (NodeExternal _obj in Tree)
                    {
                        if (_node.OBJ_Node.OBJ_Downstream != null)
                        {
                            if (_obj.OBJ_Node.ID_Watershed == _node.OBJ_Node.OBJ_Downstream.ID_Watershed)
                            {
                                NodeDown = _obj;
                            }
                        }
                    }
                    NodeExternal NodeCal = Tree.Where(x => x.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
                    bool IsNodeCal = _node.OBJ_Node.ID_Watershed == NodeCal.OBJ_Node.ID_Watershed;
                    string strDown = NodeDown?.STR_Watershed;
                    string strCal = NodeCal?.STR_Watershed;
                    cellDataTopology.Add(new object[] {
                        _node.STR_Watershed, strDown, _node.WatershedArea,
                        //_TopoParam.FLT_Imperv, _TopoParam.FLT_Perv,
                        //_TopoParam.FLT_AvgCN, _TopoParam.FLT_StreamLength, _TopoParam.FLT_AvgSlope,
                        strCal, _node.TPL_CalibrationWS.Item2, IsNodeCal ? Math.Round(SMAPd_Network.SMAPNashSutcliffe(_node.GetSMAP),3).ToString() : ""
                    });
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowTopology);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataTopology);

                #endregion Topologia

                #region Parametros

                package.Workbook.Worksheets.Add("ParametrosSMAP");

                var HeaderRowSMAPParameters = new List<string[]>()
                {
                    new string[]
                    {
                        "Bacia", "SimValida",
                        "PCoef", "ECoef",
                        "Sat (mm)", "Ai (mm)", "Capc (%)", "Crec (%)",
                        "k2t (d)", "kkt (d)",
                        "UI (%)", "EBin (m³/s)",
                        "K (d)", "X"
                    }
                };
                worksheet = package.Workbook.Worksheets["ParametrosSMAP"];
                List<object[]> cellDataSMAPParam = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {

                    var _param = _node.GetSMAP.SMAPSimulation.GetModelParameters;
                    var _name = _node.STR_Watershed;
                    var _init = _node.GetSMAP.SMAPSimulation.GetInitialConditions;
                    var _musk = _node.GetMusk;
                    cellDataSMAPParam.Add(new object[]{
                        _name, SMAPd_Network.ValidSimulation(),
                        _param.PrecipitationWeighting, _param.EvapotranspirationWeighting,
                        _param.SaturationCapacity.Millimeters, _param.InitialAbstraction.Millimeters, _param.FieldCapacity.Percent, _param.GroundwaterRecharge.Percent,
                        _param.DirectRunoffHalf.Days, _param.BaseflowHalf.Days,
                        _init.SoilMoisture.Percent, _init.Baseflow.CubicMetersPerSecond,
                        _musk.GetModelParameters.TravelTime.Days, _musk.GetModelParameters.WeightingFactor.DecimalFractions
                    });
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowSMAPParameters);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataSMAPParam);

                #endregion Parametros

                #region Resultados
                var HeaderRowSMAPResults = new List<string[]>()
                {
                    new string[]
                    {
                        "Data", "Prec (mm)", "EvapPot (mm)",
                        "RSup (mm)", "RSolo (mm)", "RSub (mm)", "Inf (mm)",
                        "EscSup (mm)", "EvapR (mm)", "Rec (mm)", "EscDir (mm)", "EscBas (mm)",
                        "QIncr (m³/s)", "QMont (m³/s)", "QAmort (m³/s)", "QBas (m³/s)", "QTotal (m³/s)", "QObs (m³/s)"
                    }
                };

                foreach (NodeExternal _node in Tree)
                {
                    package.Workbook.Worksheets.Add("Res" + _node.STR_Watershed);
                    worksheet = package.Workbook.Worksheets["Res" + _node.STR_Watershed];
                    List<object[]> cellDataSMAPResults = new List<object[]>();
                    var Simulation = _node.GetSMAP.SMAPSimulation.GetSimulation;
                    var Dates = _node.GetSMAP.GetInput.Time;
                    var PrecipitationInput = _node.GetSMAP.GetInput.Precipitation;
                    var EvapotranspirationInput = _node.GetSMAP.GetInput.Evapotranspiration;
                    var ObservedFlow = _node.GetSMAP.GetInput.ObservedFlow;
                    int _simlength = _node.GetSimulationLength;

                    for (int i = 0; i < _simlength; i++)
                    {
                        cellDataSMAPResults.Add(new object[]
                        {
                            Dates[i], PrecipitationInput[i].Millimeters, EvapotranspirationInput[i].Millimeters,
                            Simulation[i].SurfaceReservoirLevel.Millimeters, Simulation[i].SoilReservoirLevel.Millimeters, Simulation[i].GroundwaterReservoirLevel.Millimeters, Simulation[i].Infiltration.Millimeters,
                            Simulation[i].SurfaceRunoff.Millimeters, Simulation[i].RealEvapotranspiration.Millimeters, Simulation[i].GroundwaterRecharge.Millimeters, Simulation[i].DirectRunoff.Millimeters, Simulation[i].GroundwaterRunoff.Millimeters,
                            Simulation[i].Produced.CubicMetersPerSecond, Simulation[i].Upstream.CubicMetersPerSecond, Simulation[i].Routing.CubicMetersPerSecond, Simulation[i].Baseflow.CubicMetersPerSecond, Simulation[i].Downstream.CubicMetersPerSecond, ObservedFlow[i].CubicMetersPerSecond
                        });
                    }

                    worksheet.Cells[1, 1].LoadFromArrays(HeaderRowSMAPResults);
                    worksheet.Cells[2, 1].LoadFromArrays(cellDataSMAPResults);
                    worksheet.Cells[2, 1, _simlength + 2, 1].Style.Numberformat.Format = "dd/mm/yyyy";

                    #endregion Resultados

                    #region Charts

                    ExcelRangeBase rangeXSeries = worksheet.Cells[2, 1, _simlength + 1, 1];
                    ExcelRangeBase rangeSeriesIncremental = worksheet.Cells[2, 13, _simlength + 1, 13];
                    ExcelRangeBase rangeSeriesBasic = worksheet.Cells[2, 16, _simlength + 1, 16];
                    ExcelRangeBase rangeSeriesTotal = worksheet.Cells[2, 17, _simlength + 1, 17];
                    ExcelRangeBase rangeSeriesObs = worksheet.Cells[2, 18, _simlength + 1, 18];


                    var chartResults = worksheet.Drawings.AddScatterChart("Results", eScatterChartType.XYScatterSmoothNoMarkers);
                    var seriesQIncrement = chartResults.Series.Add(rangeSeriesIncremental, rangeXSeries);
                    var seriesQBasic = chartResults.Series.Add(rangeSeriesBasic, rangeXSeries);
                    var seriesQTotal = chartResults.Series.Add(rangeSeriesTotal, rangeXSeries);

                    if (_node.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed)
                    {
                        var seriesQObs = chartResults.Series.Add(rangeSeriesObs, rangeXSeries);
                        chartResults.Series[3].Header = "Vazão observada";
                    }

                    chartResults.Series[0].Header = "Vazão incremental";
                    chartResults.Series[1].Header = "Vazão básica";
                    chartResults.Series[2].Header = "Vazão total";

                    chartResults.Title.Text = "Resumo";
                    chartResults.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    chartResults.XAxis.MajorTickMark = eAxisTickMark.In;
                    chartResults.XAxis.MinorTickMark = eAxisTickMark.None;
                    chartResults.XAxis.MajorUnit = null;
                    chartResults.XAxis.Title.Font.Size = 12;
                    chartResults.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartResults.YAxis.MinValue = 0;
                    chartResults.YAxis.Format = "0.0";
                    chartResults.YAxis.Title.Text = "Vazão (m³/s)";
                    chartResults.YAxis.Title.Font.Size = 12;
                    chartResults.YAxis.Title.Rotation = 270;
                    chartResults.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    chartResults.RoundedCorners = false;
                    chartResults.Legend.Position = eLegendPosition.Right;

                    var Width = 1200;
                    var Height = 600;

                    var vOffset = 50;
                    var hOffset = 50;
                    chartResults.SetSize(Width, Height);
                    chartResults.SetPosition(vOffset, hOffset);

                    #endregion Charts                    
                }

                FileInfo excelFile = OutputFile;
                package.SaveAs(excelFile);
                
            }


        }

        public static void SavePLASHTreeToExcel(List<NodeExternal> Tree, FileInfo OutputFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Topologia");

                var HeaderRowTopology = new List<string[]>()
                {
                    new string[]
                    {
                        "Bacia", "Jusante", "Area (km2)",
                        "Frac_Imperm (%)", "Frac_Perm (%)",
                        "CN_Medio", "Comp_Talv (km)", "Decliv_Media (%)",
                        "Bacia_Cal", "Frac_Cal", "Nash_Sut"
                    }
                };

                var worksheet = package.Workbook.Worksheets["Topologia"];
                List<object[]> cellDataTopology = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {
                    var _TopoParam = _node.OBJ_UInput;
                    NodeExternal NodeDown = new NodeExternal();
                    foreach (NodeExternal _obj in Tree)
                    {
                        if (_node.OBJ_Node.OBJ_Downstream != null)
                        {
                            if (_obj.OBJ_Node.ID_Watershed == _node.OBJ_Node.OBJ_Downstream.ID_Watershed)
                            {
                                NodeDown = _obj;
                            }
                        }
                    }

                    NodeExternal NodeCal = Tree.Where(x => x.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
                    bool IsNodeCal = _node.OBJ_Node.ID_Watershed == NodeCal.OBJ_Node.ID_Watershed;
                    string strDown = NodeDown?.STR_Watershed;
                    string strCal = NodeCal?.STR_Watershed;
                    cellDataTopology.Add(new object[] {
                        _node.STR_Watershed, strDown, _TopoParam.FLT_Area,
                        _TopoParam.FLT_Imperv, _TopoParam.FLT_Perv,
                        _TopoParam.FLT_AvgCN, _TopoParam.FLT_StreamLength, _TopoParam.FLT_AvgSlope,
                        strCal, _node.TPL_CalibrationWS.Item2, IsNodeCal ? Math.Round(PLASH.PLASHNashSutcliffe(_node.GetPLASH),3).ToString() : ""
                    });
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowTopology);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataTopology);

                package.Workbook.Worksheets.Add("ParametrosPLASH");

                var HeaderRowPLASHParameters = new List<string[]>()
                {
                    new string[]
                    {
                        "Bacia", "SimValida",
                        "ImpermDetention", "PermIntercept", "PermDetention",
                        "KSup", "Saturation", "FieldCap", "Recharge",
                        "DeepPerc", "KSub", "KCan", "HydCond",
                        "SoilSuction", "SoilPor", "InitMoisture",
                        "Muskingum_K", "Muskingum_X"
                    }
                };
                worksheet = package.Workbook.Worksheets["ParametrosPLASH"];
                List<object[]> cellDataPLASHParam = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {
                    var _param = _node.GetPLASH.GetParameters;
                    var _name = _node.STR_Watershed;
                    var _musk = _node.GetMusk_OLD;
                    cellDataPLASHParam.Add(new object[]{
                        _name, _param.BOOL_ValidSimulation,
                        _param.FLT_DI, _param.FLT_IP, _param.FLT_DP,
                        _param.FLT_KSup, _param.FLT_CS, _param.FLT_CC, _param.FLT_CR,
                        _param.FLT_PP, _param.FLT_KSub, _param.FLT_KCan, _param.FLT_CH,
                        _param.FLT_FS, _param.FLT_PS, _param.FLT_UI,
                        _musk.FLT_K, _musk.FLT_X
                    });
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowPLASHParameters);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataPLASHParam);


                var HeaderRowPLASHResults = new List<string[]>()
                {
                    new string[]
                    {
                        "Data", "Prec", "EvapPot",
                        "EvapRDetImp", "EscOutDetImp", "RDetImp",
                        "EvapRIntPerm", "EscOutIntPerm", "RIntPerm",
                        "EvapPSup", "EvapRSup", "EscInSup", "InfSup", "InfCumSup", "UmiSup", "EscOutSup", "RSup",
                        "EvapPSol", "EscInSol", "EvapRSol", "EscOutSol", "RSol",
                        "EscInSub", "DeepPSub", "EscOutSub", "RSub",
                        "EvapPCan", "EvapRCan", "EscInCan", "EscOutCan", "RCan",
                        "QBasCal", "QSupCal", "QMont", "QtCalc", "QtObs"
                    }
                };

                foreach (NodeExternal _node in Tree)
                {
                    package.Workbook.Worksheets.Add("Res" + _node.STR_Watershed);
                    worksheet = package.Workbook.Worksheets["Res" + _node.STR_Watershed];
                    List<object[]> cellDataPLASHResults = new List<object[]>();
                    var _res = _node.GetPLASH.GetReservoir;
                    var _in = _node.GetPLASH.GetInput;
                    var _out = _node.GetPLASH.GetOutput;
                    int _simlength = _node.GetPLASH.GetInput.FLT_Arr_PrecipSeries.Count();

                    for (int i = 0; i < _simlength; i++)
                    {
                        cellDataPLASHResults.Add(new object[]
                        {
                            _in.DTE_Arr_TimeSeries[i], _in.FLT_Arr_PrecipSeries[i], _in.FLT_Arr_EPSeries[i],
                            _res.FLT_Arr_ERImp[i], _res.FLT_Arr_ESImp[i], _res.FLT_Arr_RImp[i],
                            _res.FLT_Arr_ERInt[i], _res.FLT_Arr_ESInt[i], _res.FLT_Arr_RInt[i],
                            _res.FLT_Arr_EPSup[i], _res.FLT_Arr_ERSup[i], _res.FLT_Arr_EESup[i], _res.FLT_Arr_Infiltration[i], _res.FLT_Arr_Infiltration_Cumulative[i], _res.FLT_Arr_SoilMoisture[i], _res.FLT_Arr_ESSup[i], _res.FLT_Arr_RSup[i],
                            _res.FLT_Arr_EPSol[i], _res.FLT_Arr_EESol[i], _res.FLT_Arr_ERSol[i], _res.FLT_Arr_ESSol[i], _res.FLT_Arr_RSol[i],
                            _res.FLT_Arr_EESub[i], _res.FLT_Arr_PPSub[i], _res.FLT_Arr_ESSub[i], _res.FLT_Arr_RSub[i],
                            _res.FLT_Arr_EPCan[i], _res.FLT_Arr_ERCan[i], _res.FLT_Arr_EECan[i], _res.FLT_Arr_ESCan[i], _res.FLT_Arr_RCan[i],
                            _out.FLT_Arr_QBas_Calc[i], _out.FLT_Arr_QSup_Calc[i], _in.FLT_Arr_QtUpstream[i], _out.FLT_Arr_Qt_Calc[i], _in.FLT_Arr_QtObsSeries[i]
                        });
                    }

                    worksheet.Cells[1, 1].LoadFromArrays(HeaderRowPLASHResults);
                    worksheet.Cells[2, 1].LoadFromArrays(cellDataPLASHResults);
                    worksheet.Cells[2, 1, _simlength + 2, 1].Style.Numberformat.Format = "dd/mm/yyyy";

                    ExcelRangeBase rangeXSeries = worksheet.Cells[2, 1, _simlength + 1, 1];
                    ExcelRangeBase rangeSeriesPrec = worksheet.Cells[2, 2, _simlength + 1, 2]; //Prec

                    ExcelRangeBase rangeSeriesEvapRDetImp = worksheet.Cells[2, 4, _simlength + 1, 4]; //EvapR Det Imp
                    ExcelRangeBase rangeSeriesEscOutDetImp = worksheet.Cells[2, 5, _simlength + 1, 5]; //EscOut Det Imp
                    ExcelRangeBase rangeSeriesRDetImp = worksheet.Cells[2, 6, _simlength + 1, 6]; // R Det Imp

                    ExcelRangeBase rangeSeriesEvapRIntPerm = worksheet.Cells[2, 7, _simlength + 1, 7]; //EvapR Int Perm
                    ExcelRangeBase rangeSeriesEscOutIntPerm = worksheet.Cells[2, 8, _simlength + 1, 8]; //EscOut Int Perm
                    ExcelRangeBase rangeSeriesRIntPerm = worksheet.Cells[2, 9, _simlength + 1, 9]; // R Int Perm

                    ExcelRangeBase rangeSeriesEvapPSup = worksheet.Cells[2, 10, _simlength + 1, 10]; // EvapP Sup
                    ExcelRangeBase rangeSeriesEvapRSup = worksheet.Cells[2, 11, _simlength + 1, 11]; // EvapR Sup
                    ExcelRangeBase rangeSeriesEscInSup = worksheet.Cells[2, 12, _simlength + 1, 12]; // Esc In Sup
                    ExcelRangeBase rangeSeriesInfSup = worksheet.Cells[2, 13, _simlength + 1, 13]; // Inf                    
                    ExcelRangeBase rangeSeriesUmiSup = worksheet.Cells[2, 15, _simlength + 1, 15]; // Moisture
                    ExcelRangeBase rangeSeriesEscOutSup = worksheet.Cells[2, 16, _simlength + 1, 16]; // Esc Out Sup
                    ExcelRangeBase rangeSeriesRSup = worksheet.Cells[2, 17, _simlength + 1, 17]; // R Sup

                    ExcelRangeBase rangeSeriesEvapPSol = worksheet.Cells[2, 18, _simlength + 1, 18]; // EvapP Sol
                    ExcelRangeBase rangeSeriesEscInSol = worksheet.Cells[2, 19, _simlength + 1, 19]; // Esc In Sol
                    ExcelRangeBase rangeSeriesEvapRSol = worksheet.Cells[2, 20, _simlength + 1, 20]; // EvapR Sol
                    ExcelRangeBase rangeSeriesEscOutSol = worksheet.Cells[2, 21, _simlength + 1, 21]; // Esc Out Sol
                    ExcelRangeBase rangeSeriesRSol = worksheet.Cells[2, 22, _simlength + 1, 22]; // R Sol

                    ExcelRangeBase rangeSeriesEscInSub = worksheet.Cells[2, 23, _simlength + 1, 23]; // Esc In Sub
                    ExcelRangeBase rangeSeriesDeepPSub = worksheet.Cells[2, 24, _simlength + 1, 24]; // Deep P Sub
                    ExcelRangeBase rangeSeriesEscOutSub = worksheet.Cells[2, 25, _simlength + 1, 25]; // Esc Out Sub
                    ExcelRangeBase rangeSeriesRSub = worksheet.Cells[2, 26, _simlength + 1, 26]; // R Sub

                    ExcelRangeBase rangeSeriesEvapPCan = worksheet.Cells[2, 27, _simlength + 1, 27]; // EvapP Can
                    ExcelRangeBase rangeSeriesEvapRCan = worksheet.Cells[2, 28, _simlength + 1, 28]; // EvapR Can
                    ExcelRangeBase rangeSeriesEscInCan = worksheet.Cells[2, 29, _simlength + 1, 29]; // EscIn Can
                    ExcelRangeBase rangeSeriesEscOutCan = worksheet.Cells[2, 30, _simlength + 1, 30]; // Esc Out Can
                    ExcelRangeBase rangeSeriesRCan = worksheet.Cells[2, 31, _simlength + 1, 31]; // R Can

                    ExcelRangeBase rangeSeriesQCan = worksheet.Cells[2, 32, _simlength + 1, 32]; //QBas
                    ExcelRangeBase rangeSeriesQt = worksheet.Cells[2, 35, _simlength + 1, 35]; //Qt
                    ExcelRangeBase rangeSeriesQObs = worksheet.Cells[2, 36, _simlength + 1, 36]; //QObs

                    var ChartRDetImp = worksheet.Drawings.AddScatterChart("RImp", eScatterChartType.XYScatterSmoothNoMarkers);
                    var ChartRIntPerm = worksheet.Drawings.AddScatterChart("RInt", eScatterChartType.XYScatterSmoothNoMarkers);
                    var ChartRSup = worksheet.Drawings.AddScatterChart("RSup", eScatterChartType.XYScatterSmoothNoMarkers);
                    var ChartRSol = worksheet.Drawings.AddScatterChart("RSol", eScatterChartType.XYScatterSmoothNoMarkers);
                    var ChartRSub = worksheet.Drawings.AddScatterChart("RSub", eScatterChartType.XYScatterSmoothNoMarkers);
                    var ChartRCan = worksheet.Drawings.AddScatterChart("RCan", eScatterChartType.XYScatterSmoothNoMarkers);

                    var ChartSummary = worksheet.Drawings.AddScatterChart("Summary", eScatterChartType.XYScatterSmoothNoMarkers);
                    var chartType2 = ChartSummary.PlotArea.ChartTypes.Add(eChartType.ColumnStacked);


                    var seriesEvapRDetImp = ChartRDetImp.Series.Add(rangeSeriesEvapRDetImp, rangeXSeries);
                    var seriesEscOutDetImp = ChartRDetImp.Series.Add(rangeSeriesEscOutDetImp, rangeXSeries);
                    var seriesRDetImp = ChartRDetImp.Series.Add(rangeSeriesRDetImp, rangeXSeries);

                    var seriesEvapRIntPerm = ChartRIntPerm.Series.Add(rangeSeriesEvapRIntPerm, rangeXSeries);
                    var seriesEscOutIntPerm = ChartRIntPerm.Series.Add(rangeSeriesEscOutIntPerm, rangeXSeries);
                    var seriesRIntPerm = ChartRIntPerm.Series.Add(rangeSeriesRIntPerm, rangeXSeries);

                    var seriesEvapPSup = ChartRSup.Series.Add(rangeSeriesEvapPSup, rangeXSeries);
                    var seriesEvapRSup = ChartRSup.Series.Add(rangeSeriesEvapRSup, rangeXSeries);
                    var seriesEscInSup = ChartRSup.Series.Add(rangeSeriesEscInSup, rangeXSeries);
                    var seriesInfSup = ChartRSup.Series.Add(rangeSeriesInfSup, rangeXSeries);
                    var seriesUmiSup = ChartRSup.Series.Add(rangeSeriesUmiSup, rangeXSeries);
                    var seriesEscOutSup = ChartRSup.Series.Add(rangeSeriesEscOutSup, rangeXSeries);
                    var seriesRSup = ChartRSup.Series.Add(rangeSeriesRSup, rangeXSeries);

                    var seriesEvapPSol = ChartRSol.Series.Add(rangeSeriesEvapPSol, rangeXSeries);
                    var seriesEscInSol = ChartRSol.Series.Add(rangeSeriesEscInSol, rangeXSeries);
                    var seriesEvapRSol = ChartRSol.Series.Add(rangeSeriesEvapRSol, rangeXSeries);
                    var seriesEscOutSol = ChartRSol.Series.Add(rangeSeriesEscOutSol, rangeXSeries);
                    var seriesRSol = ChartRSol.Series.Add(rangeSeriesRSol, rangeXSeries);

                    var seriesEscInSub = ChartRSub.Series.Add(rangeSeriesEscInSub, rangeXSeries);
                    var seriesDeepPSub = ChartRSub.Series.Add(rangeSeriesDeepPSub, rangeXSeries);
                    var seriesEscOutSub = ChartRSub.Series.Add(rangeSeriesEscOutSub, rangeXSeries);
                    var seriesRSub = ChartRSub.Series.Add(rangeSeriesRSub, rangeXSeries);

                    var seriesEvapPCan = ChartRCan.Series.Add(rangeSeriesEvapPCan, rangeXSeries);
                    var seriesEvapRCan = ChartRCan.Series.Add(rangeSeriesEvapRCan, rangeXSeries);
                    var seriesEscInCan = ChartRCan.Series.Add(rangeSeriesEscInCan, rangeXSeries);
                    var seriesEscOutCan = ChartRCan.Series.Add(rangeSeriesEscOutCan, rangeXSeries);
                    var seriesRCan = ChartRCan.Series.Add(rangeSeriesRCan, rangeXSeries);


                    var seriesPrec = chartType2.Series.Add(rangeSeriesPrec, rangeXSeries);
                    var seriesQBas = ChartSummary.Series.Add(rangeSeriesQCan, rangeXSeries);
                    var seriesQt = ChartSummary.Series.Add(rangeSeriesQt, rangeXSeries);

                    ChartRDetImp.Series[0].Header = "Evaspotranspiração";
                    ChartRDetImp.Series[1].Header = "Escoamento de saída";
                    ChartRDetImp.Series[2].Header = "Armazenamento";

                    ChartRIntPerm.Series[0].Header = "Evapotranspiração";
                    ChartRIntPerm.Series[1].Header = "Escoamento de saída";
                    ChartRIntPerm.Series[2].Header = "Armazenamento";

                    ChartRSup.Series[0].Header = "Evapotranspiração potencial";
                    ChartRSup.Series[1].Header = "Evapotranspiração real";
                    ChartRSup.Series[2].Header = "Escoamento de entrada";
                    ChartRSup.Series[3].Header = "Infiltração";
                    ChartRSup.Series[4].Header = "Umidade";
                    ChartRSup.Series[5].Header = "Escoamento de saída";
                    ChartRSup.Series[6].Header = "Armazenamento";

                    ChartRSol.Series[0].Header = "Evapotranspiração potencial";
                    ChartRSol.Series[1].Header = "Escoamento de entrada";
                    ChartRSol.Series[2].Header = "Evapotranspiração real";
                    ChartRSol.Series[3].Header = "Escoamento de saída";
                    ChartRSol.Series[4].Header = "Armazenamento";

                    ChartRSub.Series[0].Header = "Escoamento de entrada";
                    ChartRSub.Series[1].Header = "Percolação profunda";
                    ChartRSub.Series[2].Header = "Escoamento de saída";
                    ChartRSub.Series[3].Header = "Armazenamento";

                    ChartRCan.Series[0].Header = "Evapotranspiração potencial";
                    ChartRCan.Series[1].Header = "Evapotranspiração real";
                    ChartRCan.Series[2].Header = "Escoamento de entrada";
                    ChartRCan.Series[3].Header = "Escoamento de saída";
                    ChartRCan.Series[4].Header = "Armazenamento";


                    chartType2.Series[0].Header = "Precipitação";
                    ChartSummary.Series[0].Header = "Vazão Básica";
                    ChartSummary.Series[1].Header = "Vazão Calculada";
                    if (_node.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed)
                    {
                        var seriesQObs = ChartSummary.Series.Add(rangeSeriesQObs, rangeXSeries);
                        ChartSummary.Series[2].Header = "Vazao Observada";
                    }

                    ChartRDetImp.Title.Text = "Reservatório de detenção impermeável";
                    ChartRIntPerm.Title.Text = "Reservatório de interceptação";
                    ChartRSup.Title.Text = "Reservatório de detenção permeável";
                    ChartRSol.Title.Text = "Reservatório de solo não-saturado";
                    ChartRSub.Title.Text = "Reservatório de aquífero";
                    ChartRCan.Title.Text = "Reservatório de canal";
                    ChartSummary.Title.Text = "Vazão Resultante";

                    ChartRDetImp.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    ChartRIntPerm.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    ChartRSup.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    ChartRSol.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    ChartRSub.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    ChartRCan.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    ChartSummary.DisplayBlanksAs = eDisplayBlanksAs.Gap;

                    ChartRDetImp.XAxis.MajorTickMark = eAxisTickMark.In;
                    ChartRIntPerm.XAxis.MajorTickMark = eAxisTickMark.In;
                    ChartRSup.XAxis.MajorTickMark = eAxisTickMark.In;
                    ChartRSol.XAxis.MajorTickMark = eAxisTickMark.In;
                    ChartRSub.XAxis.MajorTickMark = eAxisTickMark.In;
                    ChartRCan.XAxis.MajorTickMark = eAxisTickMark.In;
                    ChartSummary.XAxis.MajorTickMark = eAxisTickMark.In;

                    ChartRDetImp.XAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRIntPerm.XAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRSup.XAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRSol.XAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRSub.XAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRCan.XAxis.MinorTickMark = eAxisTickMark.None;
                    ChartSummary.XAxis.MinorTickMark = eAxisTickMark.None;

                    ChartRDetImp.XAxis.MajorUnit = null;
                    ChartRIntPerm.XAxis.MajorUnit = null;
                    ChartRSup.XAxis.MajorUnit = null;
                    ChartRSol.XAxis.MajorUnit = null;
                    ChartRSub.XAxis.MajorUnit = null;
                    ChartRCan.XAxis.MajorUnit = null;
                    ChartSummary.XAxis.MajorUnit = null;

                    ChartRDetImp.XAxis.Title.Font.Size = 12;
                    ChartRIntPerm.XAxis.Title.Font.Size = 12;
                    ChartRSup.XAxis.Title.Font.Size = 12;
                    ChartRSol.XAxis.Title.Font.Size = 12;
                    ChartRSub.XAxis.Title.Font.Size = 12;
                    ChartRCan.XAxis.Title.Font.Size = 12;
                    ChartSummary.XAxis.Title.Font.Size = 12;

                    ChartRDetImp.YAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRIntPerm.YAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRSup.YAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRSol.YAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRSub.YAxis.MinorTickMark = eAxisTickMark.None;
                    ChartRCan.YAxis.MinorTickMark = eAxisTickMark.None;
                    ChartSummary.YAxis.MinorTickMark = eAxisTickMark.None;

                    ChartRDetImp.YAxis.MinValue = 0;
                    ChartRIntPerm.YAxis.MinValue = 0;
                    ChartRSup.YAxis.MinValue = 0;
                    ChartRSol.YAxis.MinValue = 0;
                    ChartRSub.YAxis.MinValue = 0;
                    ChartRCan.YAxis.MinValue = 0;
                    ChartSummary.YAxis.MinValue = 0;

                    ChartRDetImp.YAxis.Format = "0.0";
                    ChartRIntPerm.YAxis.Format = "0.0";
                    ChartRSup.YAxis.Format = "0.0";
                    ChartRSol.YAxis.Format = "0.0";
                    ChartRSub.YAxis.Format = "0.0";
                    ChartRCan.YAxis.Format = "0.0";
                    ChartSummary.YAxis.Format = "0.0";

                    ChartRDetImp.YAxis.Title.Text = "Lâmina d'água (mm)";
                    ChartRIntPerm.YAxis.Title.Text = "Lâmina d'água (mm)";
                    ChartRSup.YAxis.Title.Text = "Lâmina d'água (mm)";
                    ChartRSol.YAxis.Title.Text = "Lâmina d'água (mm)";
                    ChartRSub.YAxis.Title.Text = "Lâmina d'água (mm)";
                    ChartRCan.YAxis.Title.Text = "Lâmina d'água (mm)";
                    ChartSummary.YAxis.Title.Text = "Vazão (m³/s)";

                    ChartRDetImp.YAxis.Title.Font.Size = 11;
                    ChartRIntPerm.YAxis.Title.Font.Size = 11;
                    ChartRSup.YAxis.Title.Font.Size = 11;
                    ChartRSol.YAxis.Title.Font.Size = 11;
                    ChartRSub.YAxis.Title.Font.Size = 11;
                    ChartRCan.YAxis.Title.Font.Size = 11;
                    ChartSummary.YAxis.Title.Font.Size = 11;

                    ChartRDetImp.YAxis.Title.Rotation = 270;
                    ChartRIntPerm.YAxis.Title.Rotation = 270;
                    ChartRSup.YAxis.Title.Rotation = 270;
                    ChartRSol.YAxis.Title.Rotation = 270;
                    ChartRSub.YAxis.Title.Rotation = 270;
                    ChartRCan.YAxis.Title.Rotation = 270;
                    ChartSummary.YAxis.Title.Rotation = 270;

                    ChartRDetImp.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    ChartRIntPerm.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    ChartRSup.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    ChartRSol.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    ChartRSub.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    ChartRCan.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    ChartSummary.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);

                    ChartRDetImp.RoundedCorners = false;
                    ChartRIntPerm.RoundedCorners = false;
                    ChartRSup.RoundedCorners = false;
                    ChartRSol.RoundedCorners = false;
                    ChartRSub.RoundedCorners = false;
                    ChartRCan.RoundedCorners = false;
                    ChartSummary.RoundedCorners = false;

                    ChartRDetImp.Legend.Position = eLegendPosition.Right;
                    ChartRIntPerm.Legend.Position = eLegendPosition.Right;
                    ChartRSup.Legend.Position = eLegendPosition.Right;
                    ChartRSol.Legend.Position = eLegendPosition.Right;
                    ChartRSub.Legend.Position = eLegendPosition.Right;
                    ChartRCan.Legend.Position = eLegendPosition.Right;
                    ChartSummary.Legend.Position = eLegendPosition.Right;


                    chartType2.UseSecondaryAxis = true;
                    chartType2.YAxis.Orientation = eAxisOrientation.MaxMin;
                    chartType2.YAxis.Title.Text = "Precipitação (mm)";
                    chartType2.YAxis.Title.Rotation = 90;
                    var Width = 1000;
                    var Height = 400;

                    var vOffset = 50;
                    var hOffset = 50;
                    ChartRDetImp.SetSize(Width, Height);
                    ChartRIntPerm.SetSize(Width, Height);
                    ChartRSup.SetSize(Width, Height);
                    ChartRSol.SetSize(Width, Height);
                    ChartRSub.SetSize(Width, Height);
                    ChartRCan.SetSize(Width, Height);
                    ChartSummary.SetSize(Width, Height);

                    ChartRDetImp.SetPosition(vOffset, hOffset);
                    ChartRIntPerm.SetPosition(vOffset, hOffset + Width);
                    ChartRSup.SetPosition(vOffset, hOffset + 2 * Width);
                    ChartRSol.SetPosition(vOffset + Height, hOffset);
                    ChartRSub.SetPosition(vOffset + Height, hOffset + Width);
                    ChartRCan.SetPosition(vOffset + Height, hOffset + 2 * Width);
                    ChartSummary.SetPosition(vOffset + 2 * Height, hOffset);

                }

                FileInfo excelFile = OutputFile;
                package.SaveAs(excelFile);


            }


        }

        public static void SaveBuWoTreeToExcel(List<NodeExternal> Tree, FileInfo OutputFile)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Topologia");

                var HeaderRowTopology = new List<string[]>()
                {
                    new string[]
                    {
                        "Bacia", "Jusante", "Area (km2)",
                        "Frac_Imperm (%)", "Frac_Perm (%)",
                        "CN_Medio", "Comp_Talv (km)", "Decliv_Media (%)",
                        "Bacia_Cal", "Frac_Cal", "BuildupTotal", "WashoffTotal"
                    }
                };

                var worksheet = package.Workbook.Worksheets["Topologia"];
                List<object[]> cellDataTopology = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {
                    var _TopoParam = _node.OBJ_UInput;
                    NodeExternal NodeDown = new NodeExternal();
                    foreach (NodeExternal _obj in Tree)
                    {
                        if (_node.OBJ_Node.OBJ_Downstream != null)
                        {
                            if (_obj.OBJ_Node.ID_Watershed == _node.OBJ_Node.OBJ_Downstream.ID_Watershed)
                            {
                                NodeDown = _obj;
                            }
                        }
                    }

                    NodeExternal NodeCal = Tree.Where(x => x.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
                    Buildup_Washoff AggregateBuWo = _node.GetBuWo.Where(x => x.GetParam.STR_UseName == Buildup_Washoff.LandUse.Aggregate).FirstOrDefault();
                    string strDown = NodeDown?.STR_Watershed;
                    string strCal = NodeCal?.STR_Watershed;
                    cellDataTopology.Add(new object[] {
                        _node.STR_Watershed, strDown, _TopoParam.FLT_Area,
                        _TopoParam.FLT_Imperv, _TopoParam.FLT_Perv,
                        _TopoParam.FLT_AvgCN, _TopoParam.FLT_StreamLength, _TopoParam.FLT_AvgSlope,
                        strCal, _node.TPL_CalibrationWS.Item2, Buildup_Washoff.TotalPeriodLoad(AggregateBuWo), Buildup_Washoff.TotalPeriodWashoff(AggregateBuWo)
                    });
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowTopology);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataTopology);

                package.Workbook.Worksheets.Add("ParametrosProdCarga");

                var HeaderRowBuWoParameters = new List<string[]>()
                {
                    new string[]
                    {
                    "Bacia", "Passo_Tempo", "Uso_Solo",
                        "Metodo_Buildup", "Metodo_Washoff",
                        "BMax", "Kb", "Nb",
                        "Esc_Limite", "Nw", "Kw"
                    }
                };

                worksheet = package.Workbook.Worksheets["ParametrosProdCarga"];
                List<object[]> cellDataBuWoParam = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {
                    foreach (Buildup_Washoff _use in _node.GetBuWo)
                    {
                        var _param = _use.GetParam;
                        cellDataBuWoParam.Add(new object[]
                        {
                            _node.STR_Watershed, _param.FLT_Timestep_h, _param.STR_UseName,
                            _param.INT_BuMethod, _param.INT_WoMethod,
                            _param.FLT_BMax, _param.FLT_Kb, _param.FLT_Nb,
                            _param.FLT_ThresholdFlow, _param.FLT_Nw, _param.FLT_Kw
                        });
                    }
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowBuWoParameters);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataBuWoParam);

                foreach (NodeExternal _node in Tree)
                {
                    package.Workbook.Worksheets.Add("Res" + _node.STR_Watershed);
                    worksheet = package.Workbook.Worksheets["Res" + _node.STR_Watershed];
                    var HeaderRowBuWoResults = new List<string[]>();
                    var HeaderFirstRow = new List<string>();
                    HeaderFirstRow.Add("Data");
                    HeaderFirstRow.Add("EscSup");
                    var HeaderSecondRow = new List<string>();
                    HeaderSecondRow.Add(null);
                    HeaderSecondRow.Add(null);

                    int _UseCount = _node.GetBuWo.Count();


                    for (int i = 0; i < _UseCount; i++)
                    {
                        worksheet.Cells[1, 3 + 2 * i, 1, 4 + 2 * i].Merge = true;
                        HeaderFirstRow.Add(_node.GetBuWo[i].GetParam.STR_UseName.ToString());
                        HeaderFirstRow.Add(null);
                        HeaderSecondRow.Add("Buildup");
                        HeaderSecondRow.Add("Washoff");
                    }

                    HeaderRowBuWoResults.Add(HeaderFirstRow.ToArray());
                    HeaderRowBuWoResults.Add(HeaderSecondRow.ToArray());

                    List<object[]> cellDataBuWoResults = new List<object[]>();

                    int _simlength = _node.GetBuWo[0].FLT_Arr_SurfaceFlow.Count();
                    List<Buildup_Washoff> BuWoModel = _node.GetBuWo;
                    for (int i = 0; i < _simlength; i++)
                    {
                        List<object> _row = new List<object>()
                        {
                            BuWoModel[0].DTE_Arr_TimeSeries[i],
                            BuWoModel[0].FLT_Arr_SurfaceFlow[i]
                        };
                        for (int j = 0; j < _UseCount; j++)
                        {
                            _row.Add(BuWoModel[j].FLT_Arr_Buildup[i]);
                            _row.Add(BuWoModel[j].FLT_Arr_EffectiveWashoff[i]);
                        }
                        cellDataBuWoResults.Add(_row.ToArray());
                    }

                    worksheet.Cells[1, 1].LoadFromArrays(HeaderRowBuWoResults);
                    worksheet.Cells[3, 1].LoadFromArrays(cellDataBuWoResults);
                    worksheet.Cells[2, 1, _simlength + 2, 1].Style.Numberformat.Format = "dd/mm/yyyy";

                    ExcelRangeBase rangeXSeries = worksheet.Cells[2, 1, _simlength + 1, 1];
                    ExcelRangeBase rangeSeriesEscSup = worksheet.Cells[2, 2, _simlength + 1, 2];
                    for (int i = 0; i < _UseCount; i++)
                    {
                        ExcelRangeBase rangeSeriesBuildup = worksheet.Cells[2, 3 + 2 * i, _simlength + 1, 3 + 2 * i];
                        ExcelRangeBase rangeSeriesWashoff = worksheet.Cells[2, 4 + 2 * i, _simlength + 1, 4 + 2 * i];

                        var ChartBuwo = worksheet.Drawings.AddScatterChart(BuWoModel[i].GetParam.STR_UseName.ToString(), eScatterChartType.XYScatterSmoothNoMarkers);
                        var ChartEscSup = ChartBuwo.PlotArea.ChartTypes.Add(eChartType.XYScatterSmoothNoMarkers);

                        var seriesEscSup = (ExcelScatterChartSerie)ChartEscSup.Series.Add(rangeSeriesEscSup, rangeXSeries);
                        var seriesBuildup = ChartBuwo.Series.Add(rangeSeriesBuildup, rangeXSeries);
                        var seriesWashoff = ChartBuwo.Series.Add(rangeSeriesWashoff, rangeXSeries);

                        ChartEscSup.Series[0].Header = "Escoamento Superficial";
                        ChartBuwo.Series[0].Header = "Acúmulo";
                        ChartBuwo.Series[1].Header = "Lavagem";
                        seriesEscSup.Border.Fill.Color = System.Drawing.Color.White;

                        ChartBuwo.Title.Text = "Produção de carga - " + BuWoModel[i].GetParam.STR_UseName.ToString();

                        ChartBuwo.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                        ChartBuwo.XAxis.MajorTickMark = eAxisTickMark.In;
                        ChartBuwo.XAxis.MinorTickMark = eAxisTickMark.None;

                        ChartBuwo.XAxis.MajorUnit = null;
                        ChartBuwo.XAxis.Title.Font.Size = 12;
                        ChartBuwo.YAxis.MinorTickMark = eAxisTickMark.None;
                        ChartBuwo.YAxis.MinValue = 0;
                        ChartBuwo.YAxis.Format = "0.0";
                        ChartBuwo.YAxis.Title.Text = "Carga (kg)";
                        ChartBuwo.YAxis.Title.Font.Size = 11;
                        ChartBuwo.YAxis.Title.Rotation = 270;
                        ChartBuwo.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                        ChartBuwo.RoundedCorners = false;
                        ChartBuwo.Legend.Position = eLegendPosition.Bottom;



                        ChartEscSup.UseSecondaryAxis = true;
                        ChartEscSup.YAxis.Title.Text = "Lâmina d'água (mm)";
                        ChartEscSup.YAxis.Title.Rotation = 90;
                        ChartEscSup.YAxis.MinValue = 0;



                        var Width = 1000;
                        var Height = 400;
                        var vOffset = 50;
                        var hOffset = 50;
                        ChartBuwo.SetSize(Width, Height);
                        ChartBuwo.SetPosition(vOffset + Height * (i / 2), hOffset + Width * (i % 2));

                    }

                }


                FileInfo excelFile = OutputFile;
                package.SaveAs(excelFile);


            }


        }
        
        public static List<NodeExternal> BuWoTreeFromExcel(FileInfo InputFile)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<(string, Buildup_Washoff.LandUse), Buildup_Washoff.Parameters> DictBuWoParam = new Dictionary<(string, Buildup_Washoff.LandUse), Buildup_Washoff.Parameters>();

            using (ExcelPackage package = new ExcelPackage(InputFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Topologia"];
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
                        GetBuWo = new List<Buildup_Washoff>(),
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

                worksheet = package.Workbook.Worksheets["ParametrosBuWo"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;

                for (int row = 2; row <= RowCount; row++)
                {
                    string WSName = worksheet.Cells[row, 1].Value.ToString();

                    Buildup_Washoff.Parameters _param = new Buildup_Washoff.Parameters
                    {
                        FLT_Timestep_h = Convert.ToDouble(worksheet.Cells[row, 2].Value),
                        STR_UseName = (Buildup_Washoff.LandUse)Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        FLT_AreaFraction = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                        INT_BuMethod = (Buildup_Washoff.BuildupMethod)Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        INT_WoMethod = (Buildup_Washoff.WashoffMethod)Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        FLT_Area = Convert.ToDouble(worksheet.Cells[row, 7].Value),
                        FLT_InitialBuildup = Convert.ToDouble(worksheet.Cells[row, 8].Value),
                        FLT_ThresholdFlow = Convert.ToDouble(worksheet.Cells[row, 9].Value),
                        FLT_BMax = Convert.ToDouble(worksheet.Cells[row, 10].Value),
                        FLT_Nb = Convert.ToDouble(worksheet.Cells[row, 11].Value),
                        FLT_Kb = Convert.ToDouble(worksheet.Cells[row, 12].Value),
                        FLT_Nw = Convert.ToDouble(worksheet.Cells[row, 13].Value),
                        FLT_Kw = Convert.ToDouble(worksheet.Cells[row, 14].Value)
                    };
                    DictBuWoParam.Add((WSName, _param.STR_UseName), _param);
                }

                foreach (NodeExternal _node in lstNode)
                {
                    worksheet = package.Workbook.Worksheets["Res" + _node.STR_Watershed];
                    ColCount = worksheet.Dimension.End.Column;
                    RowCount = worksheet.Dimension.End.Row;
                    List<double> FLT_Arr_SurfaceFlow = new List<double>();
                    List<DateTime> DTE_Arr_Timeseries = new List<DateTime>();
                    for (int row = 2; row <= RowCount; row++)
                    {
                        DTE_Arr_Timeseries.Add(Convert.ToDateTime(worksheet.Cells[row, 1].Value));
                        FLT_Arr_SurfaceFlow.Add(Convert.ToDouble(worksheet.Cells[row, 16].Value));
                    }

                    List<Buildup_Washoff.Parameters> lstBuwoParam = (from _obj in DictBuWoParam where _obj.Key.Item1 == _node.STR_Watershed select _obj.Value).ToList();
                    foreach (Buildup_Washoff.Parameters _param in lstBuwoParam)
                    {
                        _node.GetBuWo.Add(new Buildup_Washoff
                        {
                            DTE_Arr_TimeSeries = DTE_Arr_Timeseries.ToArray(),
                            FLT_Arr_SurfaceFlow = FLT_Arr_SurfaceFlow.ToArray(),
                            GetParam = _param
                        });
                    }
                }
            }
            AssignLevel(lstNodeInternal);
            return lstNode;
        }

        public static List<NodeExternal> BuWoTreeFromExcel_Simplified(FileInfo InputFile)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<(string, Buildup_Washoff.LandUse), Buildup_Washoff.Parameters> DictBuWoParam = new Dictionary<(string, Buildup_Washoff.LandUse), Buildup_Washoff.Parameters>();

            using (ExcelPackage package = new ExcelPackage(InputFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Topologia"];
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
                        GetBuWo = new List<Buildup_Washoff>(),
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

                worksheet = package.Workbook.Worksheets["ParametrosBuWo"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;

                for (int row = 2; row <= RowCount; row++)
                {
                    string WSName = worksheet.Cells[row, 1].Value.ToString();

                    Buildup_Washoff.Parameters _param = new Buildup_Washoff.Parameters
                    {
                        FLT_Timestep_h = Convert.ToDouble(worksheet.Cells[row, 2].Value),
                        STR_UseName = (Buildup_Washoff.LandUse)Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        FLT_AreaFraction = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                        INT_BuMethod = (Buildup_Washoff.BuildupMethod)Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        INT_WoMethod = (Buildup_Washoff.WashoffMethod)Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        FLT_Area = Convert.ToDouble(worksheet.Cells[row, 7].Value),
                        FLT_InitialBuildup = Convert.ToDouble(worksheet.Cells[row, 8].Value),
                        FLT_ThresholdFlow = Convert.ToDouble(worksheet.Cells[row, 9].Value),
                        FLT_BMax = Convert.ToDouble(worksheet.Cells[row, 10].Value),
                        FLT_Nb = Convert.ToDouble(worksheet.Cells[row, 11].Value),
                        FLT_Kb = Convert.ToDouble(worksheet.Cells[row, 12].Value),
                        FLT_Nw = Convert.ToDouble(worksheet.Cells[row, 13].Value),
                        FLT_Kw = Convert.ToDouble(worksheet.Cells[row, 14].Value)
                    };

                    DictBuWoParam.Add((WSName, _param.STR_UseName), _param);

                }

                foreach (NodeExternal _node in lstNode)
                {
                    worksheet = package.Workbook.Worksheets["Res" + _node.STR_Watershed];
                    ColCount = worksheet.Dimension.End.Column;
                    RowCount = worksheet.Dimension.End.Row;
                    List<double> FLT_Arr_SurfaceFlow = new List<double>();
                    List<DateTime> DTE_Arr_Timeseries = new List<DateTime>();
                    for (int row = 2; row <= RowCount; row++)
                    {
                        DTE_Arr_Timeseries.Add(Convert.ToDateTime(worksheet.Cells[row, 1].Value));
                        FLT_Arr_SurfaceFlow.Add(Convert.ToDouble(worksheet.Cells[row, 2].Value));
                    }

                    List<Buildup_Washoff.Parameters> lstBuwoParam = (from _obj in DictBuWoParam where _obj.Key.Item1 == _node.STR_Watershed select _obj.Value).ToList();
                    foreach (Buildup_Washoff.Parameters _param in lstBuwoParam)
                    {
                        _node.GetBuWo.Add(new Buildup_Washoff
                        {
                            DTE_Arr_TimeSeries = DTE_Arr_Timeseries.ToArray(),
                            FLT_Arr_SurfaceFlow = FLT_Arr_SurfaceFlow.ToArray(),
                            GetParam = _param
                        });
                    }
                }
            }
            AssignLevel(lstNodeInternal);
            return lstNode;
        }

        public static Dictionary<string, int> DictPointHydrPrototype = new Dictionary<string, int>()
        {
            {"864997", 2 },
            {"864994", 3 },
            {"864992", 4 },
            {"864981", 5 },
            {"864978", 6 },
            {"864976", 7 },
            {"864974", 8 },
            {"864972", 9 },
            //{"864961", 10 },
            //{"864958", 11 },
            //{"864956", 12 },
            //{"864954", 13 },
            //{"864952", 14 },
        };

        public static Dictionary<string, int> DictDistrHydrPrototype = new Dictionary<string, int>()
        {
            {"864996", 2 },
            {"864995", 3 },
            {"864993", 4 },
            {"864991", 5 },
            {"864979", 6 },
            {"864977", 7 },
            {"864975", 8 },
            {"864973", 9 },
            {"864971", 10 },
            //{"864959", 11 },
            //{"864957", 12},
            //{"864955", 13 },
            //{"864953", 14 },
            //{"864951", 15 },
        };
        
        public static List<NodeExternal> PrototypeTreeFromExcel(FileInfo InputFile)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, double[]> DictArrayPrec = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayEvap = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQobs = new Dictionary<string, double[]>();
            Dictionary<string, double[]> DictArrayQMont = new Dictionary<string, double[]>();
            Dictionary<string, Model_SMAPd.ModelParameters> DictParam = new Dictionary<string, Model_SMAPd.ModelParameters>();
            Dictionary<string, Model_SMAPd.InitialConditions> DictInitialConditions = new Dictionary<string, Model_SMAP.InitialConditions>();
            Dictionary<string, Model_Muskingum> DictMusk = new Dictionary<string, Model_Muskingum>();
            Dictionary<(string, Buildup_Washoff.LandUse), Buildup_Washoff.Parameters> DictBuWoParam = new Dictionary<(string, Buildup_Washoff.LandUse), Buildup_Washoff.Parameters>();
            Dictionary<string, ConstantLoad> DictConstLoad = new Dictionary<string, ConstantLoad>();
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

                    NodeInternal node = new NodeInternal(null);

                    lstNode.Add(new NodeExternal
                    {
                        STR_Watershed = _ws,
                        OBJ_Node = node,
                        WatershedArea = Convert.ToDouble(worksheet.Cells[row, 3].Value)
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

                worksheet = package.Workbook.Worksheets["ParametrosSMAP"];
                RowCount = worksheet.Dimension.End.Row;

                for (int row = 2; row <= RowCount; row++)
                {
                    string WSName = worksheet.Cells[row, 1].Value.ToString();

                    Model_SMAPd.ModelParameters _param = new Model_SMAPd.ModelParameters
                    {
                        PrecipitationWeighting = Ratio.FromDecimalFractions(Convert.ToDouble(worksheet.Cells[row, 2].Value)),
                        EvapotranspirationWeighting = Ratio.FromDecimalFractions(Convert.ToDouble(worksheet.Cells[row, 3].Value)),
                        SaturationCapacity = Length.FromMillimeters(Convert.ToDouble(worksheet.Cells[row, 4].Value)),
                        InitialAbstraction = Length.FromMillimeters(Convert.ToDouble(worksheet.Cells[row, 5].Value)),
                        FieldCapacity = Ratio.FromPercent(Convert.ToDouble(worksheet.Cells[row, 6].Value)),
                        GroundwaterRecharge = Ratio.FromPercent(Convert.ToDouble(worksheet.Cells[row, 7].Value)),
                        DirectRunoffHalf = Duration.FromDays(Convert.ToDouble(worksheet.Cells[row, 8].Value)),
                        BaseflowHalf = Duration.FromDays(Convert.ToDouble(worksheet.Cells[row, 9].Value))
                    };
                    DictParam.Add(WSName, _param);

                    Model_SMAPd.InitialConditions _initial = new Model_SMAP.InitialConditions
                    {
                        SoilMoisture = Ratio.FromPercent(Convert.ToDouble(worksheet.Cells[row, 10].Value)),
                        Baseflow = VolumeFlow.FromCubicMetersPerSecond(Convert.ToDouble(worksheet.Cells[row, 11].Value))
                    };
                    DictInitialConditions.Add(WSName, _initial);

                    Model_Muskingum.ModelParameters _muskParam = new Model_Muskingum.ModelParameters
                    {
                        TimeStep = Duration.FromDays(1D),
                        TravelTime = Duration.FromDays(Convert.ToDouble(worksheet.Cells[row, 12].Value)),
                        WeightingFactor = Ratio.FromDecimalFractions(Convert.ToDouble(worksheet.Cells[row, 13].Value))
                    };

                    Model_Muskingum.InitialConditions _initMusk = new Model_Muskingum.InitialConditions
                    {
                        Channel = _initial.Baseflow
                    };

                    Model_Muskingum _muskingumConfiguration = new Model_Muskingum();
                    _muskingumConfiguration.SimulationStart(_muskParam, _initMusk);
                    DictMusk.Add(WSName, _muskingumConfiguration);
                }

                worksheet = package.Workbook.Worksheets["ParametrosQualidade"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;

                for (int row = 2; row <= RowCount; row++)
                {
                    string WSName = worksheet.Cells[row, 1].Value.ToString();

                    Buildup_Washoff.Parameters _param = new Buildup_Washoff.Parameters
                    {
                        FLT_Timestep_h = Convert.ToDouble(worksheet.Cells[row, 2].Value),
                        STR_UseName = (Buildup_Washoff.LandUse)Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        FLT_AreaFraction = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                        INT_BuMethod = (Buildup_Washoff.BuildupMethod)Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        INT_WoMethod = (Buildup_Washoff.WashoffMethod)Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        FLT_Area = Convert.ToDouble(worksheet.Cells[row, 7].Value),
                        FLT_InitialBuildup = Convert.ToDouble(worksheet.Cells[row, 8].Value),
                        FLT_ThresholdFlow = Convert.ToDouble(worksheet.Cells[row, 9].Value),
                        FLT_BMax = Convert.ToDouble(worksheet.Cells[row, 10].Value),
                        FLT_Nb = Convert.ToDouble(worksheet.Cells[row, 11].Value),
                        FLT_Kb = Convert.ToDouble(worksheet.Cells[row, 12].Value),
                        FLT_Nw = Convert.ToDouble(worksheet.Cells[row, 13].Value),
                        FLT_Kw = Convert.ToDouble(worksheet.Cells[row, 14].Value)
                    };
                    DictBuWoParam.Add((WSName, _param.STR_UseName), _param);

                    ConstantLoad _ctl = new ConstantLoad
                    {
                        BODLoad_kgd = Mass.FromKilograms(Convert.ToDouble(worksheet.Cells[row, 15].Value)),
                        //PhosporusLoad_kgd = Mass.FromKilograms(Convert.ToDouble(worksheet.Cells[row, 16].Value)),
                        //NitrogenLoad_kgd = Mass.FromKilograms(Convert.ToDouble(worksheet.Cells[row, 17].Value))
                    };
                    DictConstLoad.Add(WSName, _ctl);
                }

                worksheet = package.Workbook.Worksheets["Precipitacao"];
                ColCount = worksheet.Dimension.End.Column;
                RowCount = worksheet.Dimension.End.Row;
                List<DateTime> lstDates = new List<DateTime>();
                for (int row = 2; row <= RowCount; row++)
                {
                    lstDates.Add(Convert.ToDateTime((worksheet.Cells[row, 1].Value)));
                    //lstDates.Add(DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[row, 1].Value)));
                }
                arrDates = lstDates.ToArray();
                for (int col = 2; col <= ColCount; col++)
                {
                    string WSName = worksheet.Cells[1, col].Value.ToString();
                    List<double> lstPrec = new List<double>();
                    for (int row = 2; row <= RowCount; row++)
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

                worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "VazaoMontante");

                if (worksheet != null)
                {
                    ColCount = worksheet.Dimension.End.Column;
                    RowCount = worksheet.Dimension.End.Row;

                    for (int col = 2; col <= ColCount; col++)
                    {
                        string WSName = worksheet.Cells[1, col].Value.ToString();
                        List<double> lstQMont = new List<double>();
                        for (int row = 2; row <= RowCount; row++)
                        {
                            lstQMont.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                        }
                        DictArrayQMont.Add(WSName, lstQMont.ToArray());
                    }
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
                double[] arrayQMont = DictArrayQMont.ContainsKey(WSName) ? DictArrayQMont[WSName] : null;
                _node.GetSimulationLength = arrayPrec.Count();

                List<Length> UNPrecList = new List<Length>();
                List<Length> UNEvapList = new List<Length>();
                List<VolumeFlow> UNQObsList = new List<VolumeFlow>();
                List<VolumeFlow> UNQMontList = new List<VolumeFlow>();

                for (int i = 0; i < _node.GetSimulationLength; i++)
                {
                    UNPrecList.Add(Length.FromMillimeters(arrayPrec[i]));
                    UNEvapList.Add(Length.FromMillimeters(arrayEvap[i]));
                    UNQObsList.Add(VolumeFlow.FromCubicMetersPerSecond(arrayQobs[i]));
                    UNQMontList.Add(arrayQMont != null ? VolumeFlow.FromCubicMetersPerSecond(arrayQMont[i]) : VolumeFlow.Zero);
                }

                Model_SMAPd.SpatialFeatures SMAPsf = new Model_SMAP.SpatialFeatures
                {
                    DrainageArea = Area.FromSquareKilometers(_node.WatershedArea),
                    Start = false,
                    End = false
                };

                if (_node.OBJ_Node.INT_Level == 1)
                {
                    SMAPsf.Start = true;
                }

                if (_node.OBJ_Node.OBJ_Downstream == null)
                {
                    SMAPsf.End = true;
                }

                SMAPd_Network _SMAPsimulation = new SMAPd_Network
                {
                    GetInput = new SMAPd_Network.SMAPd_Input
                    {
                        Time = arrDates,
                        Precipitation = UNPrecList.ToArray(),
                        Evapotranspiration = UNEvapList.ToArray(),
                        ObservedFlow = UNQObsList.ToArray(),
                        UpstreamFlow = UNQMontList.ToArray()
                    },
                    SMAPSimulation = new Model_SMAPd(false)
                };
                _SMAPsimulation.SMAPSimulation.SimulationStart(SMAPsf, DictParam[WSName], DictInitialConditions[WSName]);
                _node.GetSMAP = _SMAPsimulation;
                _node.GetMusk = DictMusk[WSName];


                List<Buildup_Washoff.Parameters> lstBuwoParam = (from _obj in DictBuWoParam where _obj.Key.Item1 == _node.STR_Watershed select _obj.Value).ToList();
                _node.GetBuWo = new List<Buildup_Washoff>();
                foreach (Buildup_Washoff.Parameters _param in lstBuwoParam)
                {
                    _node.GetBuWo.Add(new Buildup_Washoff
                    {
                        DTE_Arr_TimeSeries = arrDates,                        
                        GetParam = _param
                    });
                }
                _node.BaseLoad = DictConstLoad[WSName];

            }
            return lstNode;

        }
        
        public static void SavePrototypeTreeToExcel_SMAP(List<NodeExternal> Tree, FileInfo OutputFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(OutputFile))
            {

                var worksheet = package.Workbook.Worksheets["HidPonto"];
                

                foreach (KeyValuePair<string, int> entry in DictPointHydrPrototype)
                {
                    NodeExternal _node = Tree.Where(x => x.STR_Watershed == entry.Key).FirstOrDefault();
                    if (_node != null)
                    {
                        List<object[]> lstFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => new object[] { Math.Round(x.Downstream.CubicMetersPerSecond, 3) }).ToList();
                        worksheet.Cells[3, entry.Value].LoadFromArrays(lstFlow);
                    }
                }

                worksheet = package.Workbook.Worksheets["HidDistr"];

                foreach (KeyValuePair<string, int> entry in DictDistrHydrPrototype)
                {
                    NodeExternal _node = Tree.Where(x => x.STR_Watershed == entry.Key).FirstOrDefault();
                    if (_node != null)
                    {
                        List<object[]> lstFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => new object[] { Math.Round(x.Produced.CubicMetersPerSecond, 3) }).ToList();
                        worksheet.Cells[4, entry.Value].LoadFromArrays(lstFlow);
                    }
                }
                package.SaveAsync();
                //package.SaveAs(OutputFile);
            }
            
        }
        
        public static void SavePrototypeTreeToExcel_SMAP_Qual(List<NodeExternal> Tree, FileInfo OutputFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(OutputFile))
            {
                #region Hydrograms
                var worksheet = package.Workbook.Worksheets["HidPonto"];                
                foreach (KeyValuePair<string, int> entry in DictPointHydrPrototype)
                {
                    NodeExternal _node = Tree.Where(x => x.STR_Watershed == entry.Key).FirstOrDefault();
                    if (_node != null)
                    {
                        List<object[]> lstFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => new object[] { Math.Round(x.Downstream.CubicMetersPerSecond, 3) }).ToList();
                        worksheet.Cells[3, entry.Value].LoadFromArrays(lstFlow);
                    }
                }
                worksheet = package.Workbook.Worksheets["HidDistr"];
                foreach (KeyValuePair<string, int> entry in DictDistrHydrPrototype)
                {
                    NodeExternal _node = Tree.Where(x => x.STR_Watershed == entry.Key).FirstOrDefault();
                    if (_node != null)
                    {
                        List<object[]> lstFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => new object[] { Math.Round(x.Produced.CubicMetersPerSecond, 3) }).ToList();
                        worksheet.Cells[4, entry.Value].LoadFromArrays(lstFlow);
                    }
                }

                #endregion Hydrograms

                #region Loads

                worksheet = package.Workbook.Worksheets["PolPonto"];
                foreach(KeyValuePair<string, int> entry in DictPointHydrPrototype)
                {
                    NodeExternal _node = Tree.Where(x => x.STR_Watershed == entry.Key).FirstOrDefault();
                    if(_node != null)
                    {
                        List<object[]> lstLoad = (from _obj in _node.BODOutput.DownstreamMass select new object[] { Math.Round(_obj.Kilograms, 3) }).ToList();
                        worksheet.Cells[3, entry.Value].LoadFromArrays(lstLoad);
                    }
                }
                worksheet = package.Workbook.Worksheets["PolDistr"];
                foreach (KeyValuePair<string, int> entry in DictDistrHydrPrototype)
                {
                    NodeExternal _node = Tree.Where(x => x.STR_Watershed == entry.Key).FirstOrDefault();
                    if (_node != null)
                    {
                        List<object[]> lstLoad = (from _obj in _node.BODOutput.TotalProducedMass select new object[] { Math.Round(_obj.Kilograms, 3) }).ToList();
                        worksheet.Cells[4, entry.Value].LoadFromArrays(lstLoad);
                    }
                }

                #endregion Loads

                List<NodeExternal> OrderedTree = Tree.OrderBy(x => x.STR_Watershed).ToList();

                worksheet = package.Workbook.Worksheets["CalibrationBuWo"];

                List<object[]> lstBuWo = new List<object[]>();

                foreach(NodeExternal _node in OrderedTree)
                {
                    Buildup_Washoff _buwo = _node.GetBuWo.Where(x => x.GetParam.STR_UseName == Buildup_Washoff.LandUse.Aggregate).FirstOrDefault();
                    lstBuWo.Add(new object[]
                    {
                        _buwo.FLT_Arr_Buildup.Average()
                    });
                }
                worksheet.Cells[2, 2].LoadFromArrays(lstBuWo);

                package.Save();
            }
        }




        public static void SaveQualityTreeToExcel(List<NodeExternal> Tree, FileInfo OutputFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {

                #region Topologia
                package.Workbook.Worksheets.Add("Topologia");

                var HeaderRowTopology = new List<string[]>()
                {
                    new string[]
                    {
                        "Bacia", "Jusante", "Area (km2)",
                        //"Frac_Imperm (%)", "Frac_Perm (%)",
                        //"CN_Medio", "Comp_Talv (km)", "Decliv_Media (%)",
                        "Bacia_Cal", "Frac_Cal"
                    }
                };

                var worksheet = package.Workbook.Worksheets["Topologia"];
                List<object[]> cellDataTopology = new List<object[]>();
                foreach (NodeExternal _node in Tree)
                {
                    //var _TopoParam = _node.OBJ_UInput;
                    NodeExternal NodeDown = new NodeExternal();
                    foreach (NodeExternal _obj in Tree)
                    {
                        if (_node.OBJ_Node.OBJ_Downstream != null)
                        {
                            if (_obj.OBJ_Node.ID_Watershed == _node.OBJ_Node.OBJ_Downstream.ID_Watershed)
                            {
                                NodeDown = _obj;
                            }
                        }
                    }
                    NodeExternal NodeCal = Tree.Where(x => x.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed).FirstOrDefault();
                    bool IsNodeCal = _node.OBJ_Node.ID_Watershed == NodeCal.OBJ_Node.ID_Watershed;
                    string strDown = NodeDown?.STR_Watershed;
                    string strCal = NodeCal?.STR_Watershed;
                    cellDataTopology.Add(new object[] {
                        _node.STR_Watershed, strDown, _node.WatershedArea,
                        //_TopoParam.FLT_Imperv, _TopoParam.FLT_Perv,
                        //_TopoParam.FLT_AvgCN, _TopoParam.FLT_StreamLength, _TopoParam.FLT_AvgSlope,
                        strCal, _node.TPL_CalibrationWS.Item2
                    });
                }
                worksheet.Cells[1, 1].LoadFromArrays(HeaderRowTopology);
                worksheet.Cells[2, 1].LoadFromArrays(cellDataTopology);

                #endregion Topologia

                

                #region Resultados
                var HeaderRowResults = new List<string[]>()
                {
                    new string[]
                    {
                        "Data",
                        "Carga base (kg/d)", "Carga lavagem (kg/d)", "Carga incremental (kg/d)", "Carga no exutório (kg/d)",
                        "Polutograma base (mg/l)", "Polutograma lavagem (mg/l)", "Polutograma incremental (mg/l)", "Polutograma no exutório (mg/l)"
                    }
                };

                foreach (NodeExternal _node in Tree)
                {
                    package.Workbook.Worksheets.Add("Res" + _node.STR_Watershed);
                    worksheet = package.Workbook.Worksheets["Res" + _node.STR_Watershed];
                    List<object[]> cellDataResults = new List<object[]>();
                    var Simulation = _node.GetSMAP.SMAPSimulation.GetSimulation;
                    var Dates = _node.GetSMAP.GetInput.Time;                    
                    int _simlength = _node.GetSimulationLength;
                    var NodeData = _node.BODOutput;
                    for (int i = 0; i < _simlength; i++)
                    {
                        cellDataResults.Add(new object[]
                        {
                            Dates[i],
                            NodeData.ConstantLoadMass[i].Kilograms, NodeData.WashoffMass[i].Kilograms, NodeData.TotalProducedMass[i].Kilograms, NodeData.DownstreamMass[i].Kilograms,
                            NodeData.ConstantLoadPollutogram[i].MilligramsPerLiter, NodeData.WashoffPollutogram[i].MilligramsPerLiter, NodeData.TotalProducedPollutogram[i].MilligramsPerLiter, NodeData.DownstreamPollutogram[i].MilligramsPerLiter
                        });
                    }

                    
                    worksheet.Cells[1, 1].LoadFromArrays(HeaderRowResults);
                    worksheet.Cells[2, 1].LoadFromArrays(cellDataResults);
                    worksheet.Cells[2, 1, _simlength + 2, 1].Style.Numberformat.Format = "dd/mm/yyyy";

                    #endregion Resultados

                    #region Charts

                    //ExcelRangeBase rangeXSeries = worksheet.Cells[2, 1, _simlength + 1, 1];
                    //ExcelRangeBase rangeSeriesIncremental = worksheet.Cells[2, 13, _simlength + 1, 13];
                    //ExcelRangeBase rangeSeriesBasic = worksheet.Cells[2, 16, _simlength + 1, 16];
                    //ExcelRangeBase rangeSeriesTotal = worksheet.Cells[2, 17, _simlength + 1, 17];
                    //ExcelRangeBase rangeSeriesObs = worksheet.Cells[2, 18, _simlength + 1, 18];


                    //var chartResults = worksheet.Drawings.AddScatterChart("Results", eScatterChartType.XYScatterSmoothNoMarkers);
                    //var seriesQIncrement = chartResults.Series.Add(rangeSeriesIncremental, rangeXSeries);
                    //var seriesQBasic = chartResults.Series.Add(rangeSeriesBasic, rangeXSeries);
                    //var seriesQTotal = chartResults.Series.Add(rangeSeriesTotal, rangeXSeries);

                    //if (_node.OBJ_Node.ID_Watershed == _node.TPL_CalibrationWS.Item1.ID_Watershed)
                    //{
                    //    var seriesQObs = chartResults.Series.Add(rangeSeriesObs, rangeXSeries);
                    //    chartResults.Series[3].Header = "Vazão observada";
                    //}

                    //chartResults.Series[0].Header = "Vazão incremental";
                    //chartResults.Series[1].Header = "Vazão básica";
                    //chartResults.Series[2].Header = "Vazão total";

                    //chartResults.Title.Text = "Resumo";
                    //chartResults.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    //chartResults.XAxis.MajorTickMark = eAxisTickMark.In;
                    //chartResults.XAxis.MinorTickMark = eAxisTickMark.None;
                    //chartResults.XAxis.MajorUnit = null;
                    //chartResults.XAxis.Title.Font.Size = 12;
                    //chartResults.YAxis.MinorTickMark = eAxisTickMark.None;
                    //chartResults.YAxis.MinValue = 0;
                    //chartResults.YAxis.Format = "0.0";
                    //chartResults.YAxis.Title.Text = "Vazão (m³/s)";
                    //chartResults.YAxis.Title.Font.Size = 12;
                    //chartResults.YAxis.Title.Rotation = 270;
                    //chartResults.StyleManager.SetChartStyle(ePresetChartStyle.ScatterChartStyle1, ePresetChartColors.ColorfulPalette1);
                    //chartResults.RoundedCorners = false;
                    //chartResults.Legend.Position = eLegendPosition.Right;

                    //var Width = 1200;
                    //var Height = 600;

                    //var vOffset = 50;
                    //var hOffset = 50;
                    //chartResults.SetSize(Width, Height);
                    //chartResults.SetPosition(vOffset, hOffset);

                    #endregion Charts                    
                }

                FileInfo excelFile = OutputFile;
                package.SaveAs(excelFile);

            }


        }




    }
}
