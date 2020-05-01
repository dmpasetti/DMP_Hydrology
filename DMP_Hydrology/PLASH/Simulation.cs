using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class PLASH
    {
        public int GetSimulationLength;
        public Input GetInput { get; private set; }
        public InitialConditions GetInitial { get; private set; }
        public Parameters GetParameters { get; private set; }
        public Reservoir GetReservoir { get; private set; }
        public Output GetOutput { get; private set; }
        public PLASH(int SimulationLength, Input _Input, InitialConditions _Initial, Parameters _Param)
        {
            GetSimulationLength = SimulationLength;
            GetInput = _Input;
            GetInitial = _Initial;
            GetParameters = _Param;
            InitReservoir();
            InitOutput();
        }



        public static void Run(PLASH WS)
        {
            
            double RImp0 = WS.GetInitial.RImp0;
            double RInt0 = WS.GetInitial.RInt0;
            double RSup0 = WS.GetInitial.RSup0;
            double RSol0 = WS.GetParameters.FLT_UI * WS.GetParameters.FLT_CS;
            double RSub0 = (WS.GetInput.FLT_Arr_QtObsSeries[0] / (1 - WS.GetParameters.FLT_kSub)) * (3.6 / WS.GetParameters.FLT_AD);
            double RCan0 = WS.GetInitial.RCan0;

            WS.GetReservoir.FLT_Arr_RImp[0] = RImp0;
            WS.GetReservoir.FLT_Arr_RInt[0] = RInt0;
            WS.GetReservoir.FLT_Arr_RSup[0] = RSup0;
            WS.GetReservoir.FLT_Arr_RSol[0] = RSol0;
            WS.GetReservoir.FLT_Arr_RSub[0] = RSub0;
            WS.GetReservoir.FLT_Arr_RCan[0] = RCan0;
            WS.GetReservoir.FLT_Arr_SoilMoisture[0] = WS.GetParameters.FLT_UI;


            for (int i = 0; i < WS.GetSimulationLength; i++)
            {

                #region Impervious Reservoir
                //Impervious Reservoir                
                WS.GetReservoir.FLT_Arr_ERImp[i] = Math.Min((i > 0 ? WS.GetReservoir.FLT_Arr_RImp[i - 1] : RImp0) + WS.GetInput.FLT_Arr_PrecipSeries[i], WS.GetInput.FLT_Arr_EPSeries[i]);
                WS.GetReservoir.FLT_Arr_ESImp[i] = Math.Max((i > 0 ? WS.GetReservoir.FLT_Arr_RImp[i - 1] : RImp0) + WS.GetInput.FLT_Arr_PrecipSeries[i] - WS.GetReservoir.FLT_Arr_ERImp[i] - WS.GetParameters.FLT_DI, 0);
                if (i > 0)
                {
                    WS.GetReservoir.FLT_Arr_RImp[i] = WS.GetReservoir.FLT_Arr_RImp[i - 1] + WS.GetInput.FLT_Arr_PrecipSeries[i] - WS.GetReservoir.FLT_Arr_ERImp[i] - WS.GetReservoir.FLT_Arr_ESImp[i];
                }
                #endregion Impervious Reservoir

                #region Interception Reservoir
                //Interception Reservoir
                WS.GetReservoir.FLT_Arr_ERInt[i] = Math.Min((i > 0 ? WS.GetReservoir.FLT_Arr_RInt[i - 1] : RInt0) + WS.GetInput.FLT_Arr_PrecipSeries[i], WS.GetInput.FLT_Arr_EPSeries[i]);
                WS.GetReservoir.FLT_Arr_ESInt[i] = Math.Max((i > 0 ? WS.GetReservoir.FLT_Arr_RInt[i - 1] : RInt0) + WS.GetInput.FLT_Arr_PrecipSeries[i] - WS.GetReservoir.FLT_Arr_ERInt[i] - WS.GetParameters.FLT_IP, 0);
                if (i > 0)
                {
                    WS.GetReservoir.FLT_Arr_RInt[i] = WS.GetReservoir.FLT_Arr_RInt[i - 1] + WS.GetInput.FLT_Arr_PrecipSeries[i] - WS.GetReservoir.FLT_Arr_ERInt[i] - WS.GetReservoir.FLT_Arr_ESInt[i];
                }
                #endregion Interception Reservoir

                #region Surface Reservoir
                //Surface Reservoir
                WS.GetReservoir.FLT_Arr_EESup[i] = WS.GetReservoir.FLT_Arr_ESImp[i] * (WS.GetParameters.FLT_AI / WS.GetParameters.FLT_AP) + WS.GetReservoir.FLT_Arr_ESInt[i];
                WS.GetReservoir.FLT_Arr_EPSup[i] = WS.GetInput.FLT_Arr_EPSeries[i] - WS.GetReservoir.FLT_Arr_ERInt[i];
                WS.GetReservoir.FLT_Arr_ERSup[i] = Math.Min((i > 0 ? WS.GetReservoir.FLT_Arr_RSup[i - 1] : RSup0) + WS.GetReservoir.FLT_Arr_EESup[i], WS.GetReservoir.FLT_Arr_EPSup[i]);

                #region Infiltration
                
                double FLT_S2 = 2 * WS.GetParameters.FLT_CH * (WS.GetParameters.FLT_PS - (i > 0 ? WS.GetReservoir.FLT_Arr_SoilMoisture[i - 1] : WS.GetParameters.FLT_UI)) * (WS.GetParameters.FLT_FS + (i > 0 ? WS.GetReservoir.FLT_Arr_RSup[i - 1] : RSup0) + WS.GetReservoir.FLT_Arr_EESup[i] - WS.GetReservoir.FLT_Arr_ERSup[i]);
                //double FLT_S2 = 2 * WS.GetParameters.FLT_CH * (WS.GetParameters.FLT_PS - WS.GetParameters.FLT_UI) * (WS.GetParameters.FLT_FS + (i > 0 ? WS.GetReservoir.FLT_Arr_RSup[i - 1] : RSup0) + WS.GetReservoir.FLT_Arr_EESup[i] - WS.GetReservoir.FLT_Arr_ERSup[i]);
                double FLT_it = (FLT_S2 / (2 * (i > 0 ? WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i - 1] : 0))) + WS.GetParameters.FLT_CH;
                double FLT_it1 = (FLT_S2 / (2 * ((i > 0 ? WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i - 1] : 0) + WS.GetInput.FLT_Arr_PrecipSeries[i]))) + WS.GetParameters.FLT_CH;
                double FLT_Puddling = Math.Max((WS.GetReservoir.FLT_Arr_EESup[i] - WS.GetReservoir.FLT_Arr_ERSup[i]) / WS.GetParameters.FLT_TimeStep, 0);
                
                //Infiltration 
                if (FLT_it1 > FLT_Puddling)
                {
                    WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i] = Math.Max((i > 0 ? WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i - 1] : 0) + WS.GetReservoir.FLT_Arr_EESup[i] - WS.GetReservoir.FLT_Arr_ERSup[i], 0);
                }
                else
                {
                    if (FLT_it <= FLT_Puddling)
                    {
                        WS.GetReservoir.FLT_Arr_IAE[i] = i > 0 ? WS.GetReservoir.FLT_Arr_IAE[i - 1] : 0;
                        WS.GetReservoir.FLT_Arr_TP[i] = 0;
                    }
                    else if (FLT_it1 <= FLT_Puddling)
                    {
                        WS.GetReservoir.FLT_Arr_IAE[i] = FLT_S2 / (2 * (FLT_Puddling - WS.GetParameters.FLT_CH));
                        WS.GetReservoir.FLT_Arr_TP[i] = (WS.GetReservoir.FLT_Arr_IAE[i] - (i > 0 ? WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i - 1] : 0)) / FLT_Puddling;
                    }

                    WS.GetReservoir.FLT_Arr_IAEAdim[i] = 2 * WS.GetParameters.FLT_CH * WS.GetReservoir.FLT_Arr_IAE[i] / FLT_S2;
                    WS.GetReservoir.FLT_Arr_TPAdim[i] = 2 * Math.Pow(WS.GetParameters.FLT_CH, 2) * (WS.GetParameters.FLT_TimeStep - WS.GetReservoir.FLT_Arr_TP[i]) / FLT_S2;


                    double FLT_Sigma = Math.Sqrt(2 * (WS.GetReservoir.FLT_Arr_TPAdim[i] + WS.GetReservoir.FLT_Arr_IAEAdim[i] - Math.Log(1 + WS.GetReservoir.FLT_Arr_IAEAdim[i])));
                    double FLT_Sigma_1 = (Math.Pow(FLT_Sigma, 2) / 2);
                    double FLT_Sigma_2 = Math.Pow((1 + FLT_Sigma / 6), -1);
                    double FLT_W_1 = (FLT_Sigma_1 + Math.Log(1 + FLT_Sigma_1 + FLT_Sigma * FLT_Sigma_2)) / (Math.Pow((1 + FLT_Sigma_1 + FLT_Sigma * FLT_Sigma_2), -1) - 1);
                    WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i] = Math.Max((FLT_S2 * (-1 - FLT_W_1)) / (2 * WS.GetParameters.FLT_CH), 0);
                }
                WS.GetReservoir.FLT_Arr_Infiltration[i] = Math.Max(WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i] - (i > 0 ? WS.GetReservoir.FLT_Arr_Infiltration_Cumulative[i - 1] : 0), 0);

                //END Infiltration
                #endregion Infiltration

                WS.GetReservoir.FLT_Arr_ESSup[i] = Math.Max((i > 0 ? WS.GetReservoir.FLT_Arr_RSup[i - 1] : RSup0) + WS.GetReservoir.FLT_Arr_EESup[i] - WS.GetReservoir.FLT_Arr_ERSup[i] - WS.GetReservoir.FLT_Arr_Infiltration[i] - WS.GetParameters.FLT_DP, 0) * (1 - WS.GetParameters.FLT_kSup);
                if (i > 0)
                {
                    WS.GetReservoir.FLT_Arr_RSup[i] = WS.GetReservoir.FLT_Arr_RSup[i - 1] + WS.GetReservoir.FLT_Arr_EESup[i] - WS.GetReservoir.FLT_Arr_ERSup[i] - WS.GetReservoir.FLT_Arr_Infiltration[i] - WS.GetReservoir.FLT_Arr_ESSup[i];
                }


                #endregion Surface Reservoir


                #region Soil Reservoir
                //Soil Reservoir
                WS.GetReservoir.FLT_Arr_EESol[i] = WS.GetReservoir.FLT_Arr_Infiltration[i];
                WS.GetReservoir.FLT_Arr_EPSol[i] = WS.GetReservoir.FLT_Arr_EPSup[i] - WS.GetReservoir.FLT_Arr_ERSup[i];
                WS.GetReservoir.FLT_Arr_ERSol[i] = WS.GetReservoir.FLT_Arr_EPSol[i] * ((i > 0 ? WS.GetReservoir.FLT_Arr_RSol[i - 1] : RSol0) / WS.GetParameters.FLT_CS);
                WS.GetReservoir.FLT_Arr_ESSol[i] = Math.Max((i > 0 ? WS.GetReservoir.FLT_Arr_RSol[i - 1] : RSol0) - WS.GetParameters.FLT_CC * WS.GetParameters.FLT_CS, 0) * (WS.GetParameters.FLT_CR * ((i > 0 ? WS.GetReservoir.FLT_Arr_RSol[i - 1] : RSol0) / WS.GetParameters.FLT_CS));
                if (i > 0)
                {
                    WS.GetReservoir.FLT_Arr_RSol[i] = WS.GetReservoir.FLT_Arr_RSol[i - 1] + WS.GetReservoir.FLT_Arr_EESol[i] - WS.GetReservoir.FLT_Arr_ERSol[i] - WS.GetReservoir.FLT_Arr_ESSol[i];
                }
                double FLT_SoilOverflow = 0;
                if(WS.GetReservoir.FLT_Arr_RSol[i] > WS.GetParameters.FLT_CS)
                {
                    FLT_SoilOverflow = WS.GetReservoir.FLT_Arr_RSol[i] - WS.GetParameters.FLT_CS;
                    WS.GetReservoir.FLT_Arr_RSol[i] = WS.GetParameters.FLT_CS;
                }

                WS.GetReservoir.FLT_Arr_SoilMoisture[i] =  WS.GetParameters.FLT_PS * (WS.GetReservoir.FLT_Arr_RSol[i] / WS.GetParameters.FLT_CS);
                //WS.GetReservoir.FLT_Arr_SoilMoisture[i] = WS.GetReservoir.FLT_Arr_RSol[i] / WS.GetParameters.FLT_CS;

                #endregion Soil Reservoir

                #region Aquifer Reservoir
                //Aquifer Reservoir
                WS.GetReservoir.FLT_Arr_EESub[i] = WS.GetReservoir.FLT_Arr_ESSol[i] * WS.GetParameters.FLT_AP;
                WS.GetReservoir.FLT_Arr_PPSub[i] = Math.Min((i > 0 ? WS.GetReservoir.FLT_Arr_RSub[i - 1] : RSub0) + WS.GetReservoir.FLT_Arr_EESub[i], WS.GetParameters.FLT_pp);
                WS.GetReservoir.FLT_Arr_ESSub[i] = ((i > 0 ? WS.GetReservoir.FLT_Arr_RSub[i - 1] : RSub0) + WS.GetReservoir.FLT_Arr_EESub[i] - WS.GetReservoir.FLT_Arr_PPSub[i]) * (1 - WS.GetParameters.FLT_kSub);
                if (i > 0)
                {
                    WS.GetReservoir.FLT_Arr_RSub[i] = WS.GetReservoir.FLT_Arr_RSub[i - 1] + WS.GetReservoir.FLT_Arr_EESub[i] - WS.GetReservoir.FLT_Arr_PPSub[i] - WS.GetReservoir.FLT_Arr_ESSub[i];
                }
                #endregion Aquifer Reservoir

                #region Channel Reservoir

                //Channel Reservoir
                WS.GetReservoir.FLT_Arr_EECan[i] = WS.GetInput.FLT_Arr_PrecipSeries[i] + WS.GetReservoir.FLT_Arr_ESSup[i] * (WS.GetParameters.FLT_AP / (1 - WS.GetParameters.FLT_AP - WS.GetParameters.FLT_AI));
                WS.GetReservoir.FLT_Arr_ERCan[i] = Math.Min((i > 0 ? WS.GetReservoir.FLT_Arr_RCan[i - 1] : RCan0) + WS.GetReservoir.FLT_Arr_EECan[i], WS.GetInput.FLT_Arr_EPSeries[i]);
                WS.GetReservoir.FLT_Arr_ESCan[i] = Math.Max((i > 0 ? WS.GetReservoir.FLT_Arr_RCan[i - 1] : RCan0) + WS.GetReservoir.FLT_Arr_EECan[i] - WS.GetReservoir.FLT_Arr_ERCan[i], 0) * (1 - WS.GetParameters.FLT_kCan);
                if (i > 0)
                {
                    WS.GetReservoir.FLT_Arr_RCan[i] = WS.GetReservoir.FLT_Arr_RCan[i - 1] + WS.GetReservoir.FLT_Arr_EECan[i] - WS.GetReservoir.FLT_Arr_ERCan[i] - WS.GetReservoir.FLT_Arr_ESCan[i];
                }
                #endregion Channel Reservoir

                #region Total Flow
                WS.GetOutput.FLT_Arr_QBas_Calc[i] = WS.GetReservoir.FLT_Arr_ESSub[i] * (WS.GetParameters.FLT_AD / (3.6 * WS.GetParameters.FLT_TimeStep));
                WS.GetOutput.FLT_Arr_QSup_Calc[i] = WS.GetInput.FLT_Arr_QtUpstream[i] + WS.GetReservoir.FLT_Arr_ESCan[i] * (WS.GetParameters.FLT_AD / (3.6 * WS.GetParameters.FLT_TimeStep)) * (1 - WS.GetParameters.FLT_AP - WS.GetParameters.FLT_AI);
                WS.GetOutput.FLT_Arr_Qt_Calc[i] = WS.GetOutput.FLT_Arr_QSup_Calc[i] + WS.GetOutput.FLT_Arr_QBas_Calc[i];

                #endregion Total Flow

            }


        }
    }
}
