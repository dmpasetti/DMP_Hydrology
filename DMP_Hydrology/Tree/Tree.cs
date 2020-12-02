using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabSid.Sabesp.Hydrology;

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
        public Muskingum_Daniel GetMusk_OLD;
        public List<Buildup_Washoff> GetBuWo;
        public Model_Muskingum GetMusk;
        public SMAPd_Network GetSMAP;
        public int GetSimulationLength;
        public double WatershedArea;
        public int GetGeneralResults;
        public ConstantLoad BaseLoad;
        public Pollutogram BODOutput;
        public Pollutogram POutput;
        public Pollutogram NOutput;
    }

    public partial class Tree
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

        public static List<NodeExternal> DuplicateTree(List<NodeExternal> Original)
        {
            List<NodeExternal> lstNode = new List<NodeExternal>();
            List<NodeInternal> lstNodeInternal = new List<NodeInternal>();            

            for (int i = 0; i < Original.Count; i++)
            {
                string _ws = Original[i].STR_Watershed;
                var _UInputOriginal = Original[i].OBJ_UInput;
                PLASH.UserInput _uinput = new PLASH.UserInput
                {
                    FLT_Area = _UInputOriginal.FLT_Area,
                    FLT_Imperv = _UInputOriginal.FLT_Imperv,
                    FLT_Perv = _UInputOriginal.FLT_Perv,
                    FLT_AvgCN = _UInputOriginal.FLT_AvgCN,
                    FLT_StreamLength = _UInputOriginal.FLT_StreamLength,
                    FLT_AvgSlope = _UInputOriginal.FLT_AvgSlope
                };
                NodeInternal node = new NodeInternal(null);
                node.ID_Watershed = Original[i].OBJ_Node.ID_Watershed;
                lstNode.Add(new NodeExternal
                {
                    STR_Watershed = _ws,
                    OBJ_Node = node,
                    OBJ_UInput = _uinput
                });
                lstNodeInternal.Add(node);             
            }
            for(int i = 0; i < lstNode.Count; i++)
            {
                if(Original[i].OBJ_Node.OBJ_Downstream != null)
                {
                    NodeExternal Downstream = Original.Where(x => x.OBJ_Node.ID_Watershed == Original[i].OBJ_Node.OBJ_Downstream.ID_Watershed).FirstOrDefault();
                    NodeExternal Downstream_NewTree = lstNode.Where(x => x.OBJ_Node.ID_Watershed == Downstream.OBJ_Node.ID_Watershed).FirstOrDefault();
                    lstNode[i].OBJ_Node.OBJ_Downstream = Downstream_NewTree.OBJ_Node;
                }
                NodeInternal CalibrationWS = Original[i].TPL_CalibrationWS.Item1;
                NodeExternal CalibrationWS_NewTree = lstNode.Where(x => x.OBJ_Node.ID_Watershed == CalibrationWS.ID_Watershed).FirstOrDefault();
                lstNode[i].TPL_CalibrationWS.Item1 = CalibrationWS_NewTree.OBJ_Node;
                lstNode[i].TPL_CalibrationWS.Item2 = Original[i].TPL_CalibrationWS.Item2;


            }
            AssignLevel(lstNodeInternal);

            for(int i = 0; i < lstNode.Count; i++)
            {
                NodeExternal NodeOriginal = Original.Where(x => x.STR_Watershed == lstNode[i].STR_Watershed).FirstOrDefault();                
                PLASH.Parameters ParamOriginal = NodeOriginal.OBJ_UInput.InputParameters;
                PLASH.Parameters _param = new PLASH.Parameters
                {
                    FLT_TimeStep = ParamOriginal.FLT_TimeStep,
                    FLT_DI = ParamOriginal.FLT_DI,
                    FLT_IP = ParamOriginal.FLT_IP,
                    FLT_DP = ParamOriginal.FLT_DP,
                    FLT_KSup = ParamOriginal.FLT_KSup,
                    FLT_CS = ParamOriginal.FLT_CS,
                    FLT_CC = ParamOriginal.FLT_CC,
                    FLT_CR = ParamOriginal.FLT_CR,
                    FLT_PP = ParamOriginal.FLT_PP,
                    FLT_KSub = ParamOriginal.FLT_KSub,
                    FLT_KCan = ParamOriginal.FLT_KCan,
                    FLT_CH = ParamOriginal.FLT_CH,
                    FLT_FS = ParamOriginal.FLT_FS,
                    FLT_PS = ParamOriginal.FLT_PS,
                    FLT_UI = ParamOriginal.FLT_UI
                };

                PLASH.Input TimeSeriesOriginal = NodeOriginal.OBJ_UInput.TimeSeries;

                PLASH.Input _timeseries = new PLASH.Input
                {
                    DTE_Arr_TimeSeries = (DateTime[])TimeSeriesOriginal.DTE_Arr_TimeSeries.Clone(),
                    FLT_Arr_PrecipSeries = (double[])TimeSeriesOriginal.FLT_Arr_PrecipSeries.Clone(),
                    FLT_Arr_EPSeries = (double[])TimeSeriesOriginal.FLT_Arr_EPSeries.Clone(),
                    FLT_Arr_QtObsSeries = (double[])TimeSeriesOriginal.FLT_Arr_QtObsSeries.Clone(),                    
                };
                if(TimeSeriesOriginal.FLT_Arr_QtUpstream != null)
                {
                    _timeseries.FLT_Arr_QtUpstream = (double[])TimeSeriesOriginal.FLT_Arr_QtUpstream.Clone();
                }


                Muskingum_Daniel MuskOriginal = NodeOriginal.GetMusk_OLD;
                Muskingum_Daniel _musk = new Muskingum_Daniel
                {
                    FLT_Timestep = MuskOriginal.FLT_Timestep,
                    FLT_K = MuskOriginal.FLT_K,
                    FLT_X = MuskOriginal.FLT_X
                };
                                
                lstNode[i].OBJ_UInput.InputParameters = _param;
                lstNode[i].OBJ_UInput.TimeSeries = _timeseries;
                lstNode[i].GetMusk_OLD = _musk;

            }
            return lstNode;
        }

        public static bool ValidTree(List<NodeExternal> Tree)
        {
            bool Valid = true;
            foreach(NodeExternal _obj in Tree)
            {
                if (!_obj.GetPLASH.GetParameters.BOOL_ValidSimulation)
                {
                    Valid = false;
                }
            }
            return Valid;
        }

        public static void PrototypeIntegrateBuwoSMAP(List<NodeExternal> Tree)
        {
            foreach(NodeExternal _node in Tree)
            {
                double[] ArraySurfaceFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => x.DirectRunoff.Millimeters).ToArray();
                foreach(Buildup_Washoff _BuWoLandUse in _node.GetBuWo)
                {
                    _BuWoLandUse.FLT_Arr_SurfaceFlow = ArraySurfaceFlow;
                }
            }
        }


    }
}
