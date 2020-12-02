﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace USP_Hydrology
{
    public partial class Pollutogram
    {
        public MassConcentration[] ConstantLoadPollutogram;
        public MassConcentration[] WashoffPollutogram;
        public MassConcentration[] TotalProducedPollutogram;
        public MassConcentration[] UpstreamPollutogram;
        public MassConcentration[] DownstreamPollutogram;

        public Mass[] ConstantLoadMass;
        public Mass[] WashoffMass;
        public Mass[] TotalProducedMass;
        public Mass[] UpstreamMass;
        public Mass[] DownstreamMass;
    }
}
