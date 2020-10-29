using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class PLASH
    {       
        public class UserInput
        {
            public double FLT_Area;
            public double FLT_Imperv;
            public double FLT_Perv;
            public double FLT_AvgCN;
            public double FLT_StreamLength;
            public double FLT_AvgSlope;
            public Input TimeSeries;
            
            public Parameters InputParameters;

        }

        

        public class Parameters
        {
            public double FLT_TimeStep;
            public double FLT_AD; // = 1589;    //Watershed Area (km2)
            public double FLT_AI; // = 0.02;     //Impervious Area Fraction (km2/km2)
            
            public double FLT_AP; // = 0.95;    //Pervious Area Fraction (km2/km2)


            public double FLT_DI; // = 5;      //Maximum Impervious Detention (mm)
            public double FLT_IP; // = 3;      //Maximum Interception (mm)
            public double FLT_DP; // = 6;      //Maximum Pervious Detention (mm)
            public double FLT_KSup; // = 120;  //Surface Reservoir Decay (h)
            public double FLT_CS; // = 1000;   //Soil Saturation Capacity (mm)
            public double FLT_CC; // = 0.3;    //Field Capacity (%)
            public double FLT_CR; // = 0.1;    //Recharge Capacity (%)
            public double FLT_PP; // = 0.02;   //Deep Percolation (mm/h)
            public double FLT_KSub; // = 360;  //Aquifer Reservoir Decay (d)
            public double FLT_KCan; // = 96;   //Channel Reservoir Decay (h)
            public double FLT_CH; // = 1;      //Hydraulic Conductivity (mm/h)
            public double FLT_FS; // = 500;    //Soil Capilarity Factor (mm)
            public double FLT_PS; // = 0.5;    //Soil Porosity (cm3/cm3)
            public double FLT_UI; // = 0.3;    //Initial Moisture (cm3/cm3)

            public double FLT_kSup { get => Math.Pow((Math.Pow(0.5, 1 / FLT_KSup)), FLT_TimeStep); }
            public double FLT_kSub { get => Math.Pow((Math.Pow(0.5, 1 / (FLT_KSub * 24))), FLT_TimeStep); }
            public double FLT_kCan { get => Math.Pow((Math.Pow(0.5, 1 / FLT_KCan)), FLT_TimeStep); }
            public double FLT_pp { get => FLT_PP * (FLT_TimeStep / 24); }

            public double FLT_CalibrationFraction;

            public bool BOOL_ValidSimulation;               
        }

        
        public class Input
        {
            public DateTime[] DTE_Arr_TimeSeries;
            public double[] FLT_Arr_PrecipSeries; //= { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.8, 0.2, 0.2, 0.8, 0.8, 1.4, 1, 5, 3.8, 2.6, 1.8, 1, 1, 1.8, 0.4, 1.6, 5.4, 7, 5.6, 3, 6.6, 5.2, 4.6, 5.4, 5, 6.8, 7.8, 8.4, 7.8, 8.6, 6.2, 3, 1.8, 6, 12.4, 6.2, 2.2, 2, 2.8, 1, 1, 0.6, 1.2, 0.2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public double[] FLT_Arr_EPSeries; //= { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };
            public double[] FLT_Arr_QtObsSeries; //= { 1.457, 1.174, 1.052, 1.054, 1.147, 1.159, 1.067, 1.131, 1.159, 1.248, 1.323, 1.381, 1.409, 1.368, 1.397, 1.427, 1.427, 1.414, 1.381, 1.422, 1.381, 1.381, 1.464, 1.562, 1.755, 1.848, 1.936, 2.14, 2.054, 2.23, 2.646, 4.876, 7.524, 9.872, 12.405, 15.05, 16.37, 17.808, 18.506, 20.065, 22.038, 24.759, 26.728, 27.982, 28.024, 25.961, 22.174, 21.46, 26.11, 29.513, 29.202, 26.018, 21.255, 17.54, 15.265, 12.94, 11.815, 9.886, 8.652, 7.921, 6.646, 5.869, 5.612, 4.946, 4.396, 4.265, 4.645, 5.239, 4.657, 4.326, 4.394, 4.14, 3.877, 3.132, 2.198, 2.565, 2.275, 2.311, 2.402, 2.208, 2.128, 2.168, 2.556, 2.669, 2.653, 2.745, 2.498, 2.33, 2.554, 2.588, 2.62, 2.323, 2.236, 2.404, 2.363, 2.586, 2.107, 2.013, 1.85, 1.727 };
            public double[] FLT_Arr_QtUpstream;
        }

        public class Reservoir
        {
            public double[] FLT_Arr_RImp; //Impervious Reservoir Level
            public double[] FLT_Arr_RInt; //Interception Reservoir Level
            public double[] FLT_Arr_RSup; //Surface Reservoir Level
            public double[] FLT_Arr_RSol; //Soil Reservoir Level
            public double[] FLT_Arr_RSub; //Aquifer Reservoir Level
            public double[] FLT_Arr_RCan; //Channel Reservoir Level

            //Impervious Detention Reservoir
            public double[] FLT_Arr_ERImp; //Real Evapotranspiration
            public double[] FLT_Arr_ESImp; //Downstream Flow

            //Pervious Interception Reservoir
            public double[] FLT_Arr_ERInt; //Real Evapotranspiration
            public double[] FLT_Arr_ESInt; // Downstream Flow

            //Pervious Detention Reservoir (Surface Reservoir)
            public double[] FLT_Arr_EPSup; //Potential Evapotranspiration
            public double[] FLT_Arr_EESup; //Upstream Flow
            public double[] FLT_Arr_ERSup; //Real Evapotranspiration
            public double[] FLT_Arr_ESSup; //Downstream Flow
            public double[] FLT_Arr_Infiltration; //Infiltration in Time Step
            public double[] FLT_Arr_Infiltration_Cumulative; //Total Infiltration
            public double[] FLT_Arr_IAE;
            public double[] FLT_Arr_TP;
            public double[] FLT_Arr_IAEAdim;
            public double[] FLT_Arr_TPAdim;
            public double[] FLT_Arr_SoilMoisture;

            //Shallow Soil Reservoir
            public double[] FLT_Arr_EPSol; //Potential Evapotranspiration
            public double[] FLT_Arr_EESol; //Upstream Flow (Infiltration)
            public double[] FLT_Arr_ERSol; //Real Evapotranspiration
            public double[] FLT_Arr_ESSol; //Downstream Flow

            //Aquifer Reservoir
            public double[] FLT_Arr_EESub; //Upstream Flow (Recharge)
            public double[] FLT_Arr_PPSub; //Deep Percolation
            public double[] FLT_Arr_ESSub; //Downstream Flow

            //Channel Reservoir
            public double[] FLT_Arr_EPCan; //Potential Evapotranspiration
            public double[] FLT_Arr_ERCan; //Real Evapotranspiration
            public double[] FLT_Arr_EECan; //Upstream Flow
            public double[] FLT_Arr_ESCan; //Downstream Flow
        }
        
        public class InitialConditions
        {
            public double RImp0;
            public double RInt0;
            public double RSup0;
            public double RCan0;
        }
        
        public class Output
        {
            public double[] FLT_Arr_QBas_Calc;
            public double[] FLT_Arr_QSup_Calc;
            public double[] FLT_Arr_Qt_Calc;
            public double[] FLT_Arr_Qt_Calibration;
            public double[] FLT_Arr_Qt_Up;
            public double[] FLT_Arr_Qt_Up_Obs;
        }

        public void InitReservoir()
        {
            GetReservoir = new Reservoir
            {
                FLT_Arr_RImp = new double[GetSimulationLength],
                FLT_Arr_RInt = new double[GetSimulationLength],
                FLT_Arr_RSup = new double[GetSimulationLength],
                FLT_Arr_RSol = new double[GetSimulationLength],
                FLT_Arr_RSub = new double[GetSimulationLength],
                FLT_Arr_RCan = new double[GetSimulationLength],

                FLT_Arr_ERImp = new double[GetSimulationLength],
                FLT_Arr_ESImp = new double[GetSimulationLength],

                FLT_Arr_ERInt = new double[GetSimulationLength],
                FLT_Arr_ESInt = new double[GetSimulationLength],

                FLT_Arr_EPSup = new double[GetSimulationLength],
                FLT_Arr_EESup = new double[GetSimulationLength],
                FLT_Arr_ERSup = new double[GetSimulationLength],
                FLT_Arr_ESSup = new double[GetSimulationLength],
                FLT_Arr_Infiltration = new double[GetSimulationLength],
                FLT_Arr_Infiltration_Cumulative = new double[GetSimulationLength],
                FLT_Arr_IAE = new double[GetSimulationLength],
                FLT_Arr_TP = new double[GetSimulationLength],
                FLT_Arr_IAEAdim = new double[GetSimulationLength],
                FLT_Arr_TPAdim = new double[GetSimulationLength],
                FLT_Arr_SoilMoisture = new double[GetSimulationLength],

                FLT_Arr_EPSol = new double[GetSimulationLength],
                FLT_Arr_EESol = new double[GetSimulationLength],
                FLT_Arr_ERSol = new double[GetSimulationLength],
                FLT_Arr_ESSol = new double[GetSimulationLength],

                FLT_Arr_EESub = new double[GetSimulationLength],
                FLT_Arr_PPSub = new double[GetSimulationLength],
                FLT_Arr_ESSub = new double[GetSimulationLength],

                FLT_Arr_EPCan = new double[GetSimulationLength],
                FLT_Arr_ERCan = new double[GetSimulationLength],
                FLT_Arr_EECan = new double[GetSimulationLength],
                FLT_Arr_ESCan = new double[GetSimulationLength]
            };
        }
        
        public void InitOutput()
        {
            GetOutput = new Output
            {
                FLT_Arr_QBas_Calc = new double[GetSimulationLength],
                FLT_Arr_QSup_Calc = new double[GetSimulationLength],
                FLT_Arr_Qt_Calc = new double[GetSimulationLength],
                FLT_Arr_Qt_Calibration = new double[GetSimulationLength]
            };
        }
    }
}
