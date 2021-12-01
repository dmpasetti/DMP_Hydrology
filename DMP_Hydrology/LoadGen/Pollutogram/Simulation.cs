using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;


namespace USP_Hydrology
{
    public partial class Pollutogram
    {
        public void SimulateConcentration(VolumeFlow[] Flow) {
            List<MassConcentration> _arrPoint = new List<MassConcentration>();
            List<MassConcentration> _arrDryNonpoint = new List<MassConcentration>();
            List<MassConcentration> _arrWashoff = new List<MassConcentration>();
            List<MassConcentration> _arrProduced = new List<MassConcentration>();
            List<MassConcentration> _arrDownstream = new List<MassConcentration>();

            for(int i = 0; i < Flow.Length; i++)
            {
                Volume DailyVolume = Flow[i].Multiplication(Duration.FromDays(1));
                _arrPoint.Add(MassConcentration.FromKilogramsPerCubicMeter(this.PointLoadMass[i].Kilograms / DailyVolume.CubicMeters));
                _arrDryNonpoint.Add(MassConcentration.FromKilogramsPerCubicMeter(this.DryNonPointMass[i].Kilograms / DailyVolume.CubicMeters));
                if(WashoffMass != null)
                {
                    _arrWashoff.Add(MassConcentration.FromKilogramsPerCubicMeter(this.WashoffMass[i].Kilograms / DailyVolume.CubicMeters));
                }
                _arrProduced.Add(MassConcentration.FromKilogramsPerCubicMeter(this.TotalProducedMass[i].Kilograms / DailyVolume.CubicMeters));
                _arrDownstream.Add(MassConcentration.FromKilogramsPerCubicMeter(this.DownstreamMass[i].Kilograms / DailyVolume.CubicMeters));
                
            }
            this.PointLoadPollutogram = _arrPoint.ToArray();
            this.DryNonPointLoadPollutogram = _arrDryNonpoint.ToArray();
            this.WashoffPollutogram = _arrWashoff.ToArray() ?? null;
            this.TotalProducedPollutogram = _arrProduced.ToArray();
            this.DownstreamPollutogram = _arrDownstream.ToArray();
        }

        private void FillWashoffMass(double[] Arraywashoff)
        {
            this.WashoffMass = Arraywashoff.Select(x => Mass.FromKilograms(x)).ToArray();
        }
        private void FillPointLoadMass(Mass Load, int Length)
        {
            this.PointLoadMass = Enumerable.Repeat(Load, Length).ToArray();
        }

        private void FillDryNonPointLoadMass(Mass Load, int Length)
        {
            this.DryNonPointMass = Enumerable.Repeat(Load, Length).ToArray();
        }

        private void FillTotalProducedMass()
        {
            if(this.WashoffMass != null && this.PointLoadMass != null && this.DryNonPointMass != null)
            {
                this.TotalProducedMass = this.WashoffMass.Zip(this.PointLoadMass, (x, y) => x + y).Zip(this.DryNonPointMass, (z, w) => z + w).ToArray();
            }
        }
        private void FillDownstreamMass()
        {
            this.DownstreamMass = this.TotalProducedMass.Zip(this.UpstreamMass, (x, y) => x + y).ToArray();
        }

        public static void SimulateBODTree(List<NodeExternal> Tree)
        {
            List<NodeExternal> OrderedTree = Tree.OrderBy(x => x.OBJ_Node.INT_Level).ToList();
            foreach (NodeExternal _node in OrderedTree)
            {
                _node.BODOutput = new Pollutogram();
                _node.BODOutput.FillPointLoadMass(_node.BaseLoad.BODLoad_kgd, _node.GetSimulationLength);
                _node.BODOutput.FillDryNonPointLoadMass(_node.BaseLoad.DryNonPointBOD_kgd, _node.GetSimulationLength);
                Buildup_Washoff ArrayWashoff = _node.BuWoAggregate;                
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

        public static void SimulatePhosphorusTree(List<NodeExternal> Tree)
        {
            List<NodeExternal> OrderedTree = Tree.OrderBy(x => x.OBJ_Node.INT_Level).ToList();
            foreach (NodeExternal _node in OrderedTree)
            {
                _node.POutput = new Pollutogram();
                _node.POutput.FillPointLoadMass(_node.BaseLoad.PhosphorusLoad_kgd, _node.GetSimulationLength);
                _node.POutput.FillDryNonPointLoadMass(_node.BaseLoad.DryNonPointPhosphorus_kgd, _node.GetSimulationLength);
                Buildup_Washoff ArrayWashoff = _node.BuWoAggregate; 
                if (ArrayWashoff != null)
                {
                    _node.POutput.FillWashoffMass(ArrayWashoff.FLT_Arr_EffectiveWashoff);
                }
                _node.POutput.FillTotalProducedMass();

                Mass[] _upstreamLoad = new Mass[_node.GetSimulationLength];
                if (_node.OBJ_Node.INT_Level > 1)
                {
                    for (int i = 0; i < OrderedTree.Count(); i++)
                    {
                        if (OrderedTree[i].OBJ_Node.INT_Level < _node.OBJ_Node.INT_Level)
                        {
                            if (OrderedTree[i].OBJ_Node.OBJ_Downstream != null)
                            {
                                if (OrderedTree[i].OBJ_Node.OBJ_Downstream.ID_Watershed == _node.OBJ_Node.ID_Watershed)
                                {
                                    _upstreamLoad = _upstreamLoad.Zip(OrderedTree[i].POutput.DownstreamMass, (x, y) => x + y).ToArray();
                                }
                            }
                        }
                    }
                }
                _node.POutput.UpstreamMass = _upstreamLoad;
                _node.POutput.FillDownstreamMass();

                if (_node.GetSMAP != null)
                {
                    VolumeFlow[] ArrayFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => x.Downstream).ToArray();
                    _node.POutput.SimulateConcentration(ArrayFlow);
                }

            }
        }

        public static void SimulateNitrogenTree(List<NodeExternal> Tree)
        {
            List<NodeExternal> OrderedTree = Tree.OrderBy(x => x.OBJ_Node.INT_Level).ToList();
            foreach (NodeExternal _node in OrderedTree)
            {
                _node.NOutput = new Pollutogram();
                _node.NOutput.FillPointLoadMass(_node.BaseLoad.NitrogenLoad_kgd, _node.GetSimulationLength);
                _node.NOutput.FillDryNonPointLoadMass(_node.BaseLoad.DryNonPointNitrogen_kgd, _node.GetSimulationLength);
                Buildup_Washoff ArrayWashoff = _node.BuWoAggregate;
                if (ArrayWashoff != null)
                {
                    _node.NOutput.FillWashoffMass(ArrayWashoff.FLT_Arr_EffectiveWashoff);
                }
                _node.NOutput.FillTotalProducedMass();

                Mass[] _upstreamLoad = new Mass[_node.GetSimulationLength];
                if (_node.OBJ_Node.INT_Level > 1)
                {
                    for (int i = 0; i < OrderedTree.Count(); i++)
                    {
                        if (OrderedTree[i].OBJ_Node.INT_Level < _node.OBJ_Node.INT_Level)
                        {
                            if (OrderedTree[i].OBJ_Node.OBJ_Downstream != null)
                            {
                                if (OrderedTree[i].OBJ_Node.OBJ_Downstream.ID_Watershed == _node.OBJ_Node.ID_Watershed)
                                {
                                    _upstreamLoad = _upstreamLoad.Zip(OrderedTree[i].NOutput.DownstreamMass, (x, y) => x + y).ToArray();
                                }
                            }
                        }
                    }
                }
                _node.NOutput.UpstreamMass = _upstreamLoad;
                _node.NOutput.FillDownstreamMass();

                if (_node.GetSMAP != null)
                {
                    VolumeFlow[] ArrayFlow = _node.GetSMAP.SMAPSimulation.GetSimulation.Select(x => x.Downstream).ToArray();
                    _node.NOutput.SimulateConcentration(ArrayFlow);
                }

            }
        }

    }
} 