using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PrecipitationProcess
{
    class Program
    {
        public struct RainDataStation
        {
            public int ID_Station;
            public List<Data> DataStation;
        }
        public struct RainDataWS
        {
            public int ID_Watershed;
            public List<Data> DataWS;
        }

        public struct Data
        {
            public DateTime Date;
            public double? Value;
        }

        static void Main(string[] args)
        {
            string BasePath = @"D:\VisualStudio_Mestrado\DMP_Hydrology\PrecipitationProcess\RainFiles\";

            string[] PrecFiles =
            {
                "chuva_2347049.txt",
                "chuva_2347050.txt",
                "chuva_2347149.txt",
                "chuva_2348026.txt",
                "chuva_2348028.txt",
                "chuva_2348033.txt",
            };

            Dictionary<string, int> StationCodes = new Dictionary<string, int>
            {
                { "chuva_2347049.txt", 2347049 },
                { "chuva_2347050.txt", 2347050 },
                { "chuva_2347149.txt", 2347149 },
                { "chuva_2348026.txt", 2348026 },
                { "chuva_2348028.txt", 2348028 },
                { "chuva_2348033.txt", 2348033 },
            };

            int[] lstWatershed =
            {
                864981,
                864982,
                864983,
                864984,
                864985,
                864986,
                864987,
                864988,
                864989
            };


            Dictionary<Tuple<int, int>, double> Fractions = new Dictionary<Tuple<int, int>, double>
            {
                 { Tuple.Create(864981, 2348033), 1 },

                 { Tuple.Create(864982, 2347050), 0.167 },
                 { Tuple.Create(864982, 2348028), 0.057 },
                 { Tuple.Create(864982, 2348026), 0.582 },
                 { Tuple.Create(864982, 2348033), 0.194 },

                 { Tuple.Create(864983, 2348028), 0.647 },
                 { Tuple.Create(864983, 2348026), 0.007},
                 { Tuple.Create(864983, 2348033), 0.346 },

                 { Tuple.Create(864984, 2348028), 1 },

                 { Tuple.Create(864985, 2347050), 0.136 },
                 { Tuple.Create(864985, 2347149), 0.227 },
                 { Tuple.Create(864985, 2348028), 0.637 },

                 { Tuple.Create(864986, 2347050), 0.146 },
                 { Tuple.Create(864986, 2347149), 0.831 },
                 { Tuple.Create(864986, 2348028), 0.023 },

                 { Tuple.Create(864987, 2347050), 0.085 },
                 { Tuple.Create(864987, 2347149), 0.772 },
                 { Tuple.Create(864987, 2348028), 0.143 },

                 { Tuple.Create(864988, 2347050), 0.156 },
                 { Tuple.Create(864988, 2347149), 0.091 },
                 { Tuple.Create(864988, 2347049), 0.752 },

                 { Tuple.Create(864989, 2347149), 0.044 },
                 { Tuple.Create(864989, 2347049), 0.955 },
            };

            List<RainDataStation> lstRainDataStation = new List<RainDataStation>();
            List<RainDataWS> lstRainDataWS = new List<RainDataWS>();
            
            foreach(string _fileName in PrecFiles)
            {
                string fullPath = BasePath + _fileName;
                List<Data> StationInput = new List<Data>();
                using (var reader = new StreamReader(fullPath))
                {
                    
                    reader.ReadLine();
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        string[] RainData = line.Split();
                        StationInput.Add( new Data
                        {
                            Date = Convert.ToDateTime(RainData[0]),
                            Value = Convert.ToDouble(RainData[2].Replace(".",","))
                        });

                    }
                }

                lstRainDataStation.Add(new RainDataStation
                {
                    ID_Station = StationCodes[_fileName],
                    DataStation = StationInput
                });
            }


            
            foreach(int _ws in lstWatershed)
            {
                List<Tuple<int,int>> StationWSComb = (from obj in Fractions where obj.Key.Item1 == _ws select obj.Key).ToList();

                List<RainDataStation> StationWSData = lstRainDataStation.Where(x => StationWSComb.Any(y => x.ID_Station == y.Item2)).ToList();

                List<RainDataStation> StationWSMult = new List<RainDataStation>();

                foreach(RainDataStation _station in StationWSData)
                {
                    double Mult = Fractions[Tuple.Create(_ws, _station.ID_Station)];

                    List<Data> lstDataMult = new List<Data>();
                    foreach(Data _data in _station.DataStation)
                    {
                        lstDataMult.Add(new Data
                        {
                            Date = _data.Date,
                            Value = _data.Value * Mult
                        });
                    }

                    StationWSMult.Add(new RainDataStation
                    {
                        ID_Station = _station.ID_Station,
                        DataStation = lstDataMult
                    });
                }

                DateTime MinDate = new DateTime(1900, 1, 1);
                DateTime MaxDate = DateTime.Now;
                foreach(RainDataStation _station in StationWSData)
                {
                    DateTime Min = _station.DataStation.Min(x => x.Date);
                    DateTime Max = _station.DataStation.Max(x => x.Date);
                    if (Min > MinDate) MinDate = Min;
                    if (Max < MaxDate) MaxDate = Max;
                }

                List<Data> lstDataWS = new List<Data>();
                for(DateTime _dte = MinDate; _dte <= MaxDate; _dte = _dte.AddDays(1))
                {
                    List<Data> lstCurrentDate = new List<Data>();

                    foreach(RainDataStation _st in StationWSMult)
                    {
                        var input = _st.DataStation.Where(x => x.Date == _dte).FirstOrDefault();
                        if(input.Value != null)
                        {
                            lstCurrentDate.Add(input);
                        }
                        
                        
                    }

                    if(lstCurrentDate.Count == StationWSComb.Count)
                    {
                        lstDataWS.Add(new Data
                        {
                            Date = _dte,
                            Value = lstCurrentDate.Sum(x => x.Value) >= 0.2 ? lstCurrentDate.Sum(x => x.Value) : 0
                        });
                    }

                }

                lstRainDataWS.Add(new RainDataWS
                {
                    ID_Watershed = _ws,
                    DataWS = lstDataWS
                });
            }
            //Console.ReadKey();

            string PathOutputBase = @"D:\VisualStudio_Mestrado\DMP_Hydrology\PrecipitationProcess\RainFiles\";

            foreach (RainDataWS _ws in lstRainDataWS)
            {
                string fullPathOutput = PathOutputBase + _ws.ID_Watershed.ToString() + ".txt";

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(fullPathOutput))
                {
                    foreach(Data _data in _ws.DataWS)
                    {
                        file.WriteLine(_data.Date.ToShortDateString() + "\t" + _data.Value.ToString());
                    }

                }
            }
        }
    }
}
