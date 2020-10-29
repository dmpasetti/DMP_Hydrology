using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
namespace USP_Hydrology
{
    public partial class SMAPd_Network
    {
        public static void SimulateTree(List<NodeExternal> Tree)
        {
            List<NodeExternal> OrderedTree = Tree.OrderBy(x => x.OBJ_Node.INT_Level).ToList();

            for(int i = 0; i < Tree[0].GetSimulationLength; i++)
            {
                foreach(NodeExternal _node in OrderedTree)
                {
                    SMAPd_Network.SMAPd_Input input = _node.GetSMAP.GetInput;
                    if (_node.OBJ_Node.INT_Level > 1)
                    {
                        List<NodeExternal> lstNodeUpstream = new List<NodeExternal>();
                        for (int j = 0; j < OrderedTree.Count(); j++)
                        {
                            if(OrderedTree[j].OBJ_Node.INT_Level < _node.OBJ_Node.INT_Level)
                            {
                                if(OrderedTree[j].OBJ_Node.OBJ_Downstream.ID_Watershed == _node.OBJ_Node.ID_Watershed)
                                {
                                    lstNodeUpstream.Add(OrderedTree[j]);
                                }
                            }
                        }                        
                        VolumeFlow UpstreamFlow = input.UpstreamFlow[i];
                        foreach(NodeExternal _upnode in lstNodeUpstream)
                        {
                            UpstreamFlow += _upnode.GetSMAP.SMAPSimulation.GetSimulation[i].Downstream;
                        }
                        _node.GetMusk.SimulationNext(UpstreamFlow);
                        _node.GetSMAP.SMAPSimulation.SimulationNext(input.Precipitation[i], input.Evapotranspiration[i], _node.GetMusk.GetSimulation[i], VolumeFlow.Zero, VolumeFlow.Zero);
                    }
                    else
                    {
                        _node.GetSMAP.SMAPSimulation.SimulationNext(input.Precipitation[i], input.Evapotranspiration[i], null, VolumeFlow.Zero, VolumeFlow.Zero);
                    }
                    
                }
            }

        }


    }
}
