using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using LabSid.Sabesp.Hydrology;

namespace USP_Hydrology
{
    public partial class Pollutogram
    {
        public void SimulateConcentration(VolumeFlow[] Flow) {
            List<MassConcentration> _arrConst = new List<MassConcentration>();
            List<MassConcentration> _arrWashoff = new List<MassConcentration>();
            List<MassConcentration> _arrProduced = new List<MassConcentration>();
            List<MassConcentration> _arrDownstream = new List<MassConcentration>();

            for(int i = 0; i < Flow.Length; i++)
            {
                Volume DailyVolume = Flow[i].Multiplication(Duration.FromDays(1));
                _arrConst.Add(MassConcentration.FromKilogramsPerCubicMeter(this.ConstantLoadMass[i].Kilograms / DailyVolume.CubicMeters));
                if(WashoffMass != null)
                {
                    _arrWashoff.Add(MassConcentration.FromKilogramsPerCubicMeter(this.WashoffMass[i].Kilograms / DailyVolume.CubicMeters));
                }
                _arrProduced.Add(MassConcentration.FromKilogramsPerCubicMeter(this.TotalProducedMass[i].Kilograms / DailyVolume.CubicMeters));
                _arrDownstream.Add(MassConcentration.FromKilogramsPerCubicMeter(this.DownstreamMass[i].Kilograms / DailyVolume.CubicMeters));
                
            }
            this.ConstantLoadPollutogram = _arrConst.ToArray();
            this.WashoffPollutogram = _arrWashoff.ToArray() ?? null;
            this.TotalProducedPollutogram = _arrProduced.ToArray();
            this.DownstreamPollutogram = _arrDownstream.ToArray();
        }

        private void FillWashoffMass(double[] Arraywashoff)
        {
            this.WashoffMass = Arraywashoff.Select(x => Mass.FromKilograms(x)).ToArray();
        }
        private void FillConstLoadMass(Mass Load, int Length)
        {
            this.ConstantLoadMass = Enumerable.Repeat(Load, Length).ToArray();
        }
        private void FillTotalProducedMass()
        {
            if(this.WashoffMass != null && this.ConstantLoadMass != null)
            {
                this.TotalProducedMass = this.WashoffMass.Zip(this.ConstantLoadMass, (x, y) => x + y).ToArray();
            }
        }
        private void FillDownstreamMass()
        {
            this.DownstreamMass = this.TotalProducedMass.Zip(this.UpstreamMass, (x, y) => x + y).ToArray();
        }

        public static void SimulateTree(List<NodeExternal> Tree)
        {
            List<NodeExternal> OrderedTree = Tree.OrderBy(x => x.OBJ_Node.INT_Level).ToList();
            foreach (NodeExternal _node in OrderedTree)
            {
                _node.BODOutput = new Pollutogram();
                _node.BODOutput.FillConstLoadMass(_node.BaseLoad.BODLoad_kgd, _node.GetSimulationLength);
                Buildup_Washoff ArrayWashoff = _node.GetBuWo.Where(x => x.GetParam.STR_UseName == Buildup_Washoff.LandUse.Aggregate).FirstOrDefault();
                if(ArrayWashoff != null)
                {
                    _node.BODOutput.FillWashoffMass(ArrayWashoff.FLT_Arr_EffectiveWashoff);
                }
                _node.BODOutput.FillTotalProducedMass();

                Mass[] _upstreamLoad = new Mass[_node.GetSimulationLength];
                if(_node.OBJ_Node.INT_Level > 1)
                {                    
                    for(int i = 0; i < OrderedTree.Count(); i++)
                    {
                        if(OrderedTree[i].OBJ_Node.INT_Level < _node.OBJ_Node.INT_Level)
                        {
                            if(OrderedTree[i].OBJ_Node.OBJ_Downstream != null)
                            {
                                if(OrderedTree[i].OBJ_Node.OBJ_Downstream.ID_Watershed == _node.OBJ_Node.ID_Watershed)
                                {
                                    _upstreamLoad = _upstreamLoad.Zip(OrderedTree[i].BODOutput.DownstreamMass, (x, y) => x + y).ToArray();
                                }
                            }
                        }
                    }
                }
                _node.BODOutput.UpstreamMass = _upstreamLoad;
                _node.BODOutput.FillDownstreamMass();

                if(_node.GetSMAP != null)
                {
                    VolumeFlow[] ArrayFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => x.Downstream).ToArray();
                    _node.BODOutput.SimulateConcentration(ArrayFlow);
                }

            }
        }
    }
} 