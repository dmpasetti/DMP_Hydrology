using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using MathNet.Numerics;
using Stats = MathNet.Numerics.Statistics;

namespace USP_Hydrology
{
    public partial class Buildup_Washoff
    {

        public enum MetricFunction
        {
            ArrayTotal,
            ArrayAverage,
            MonthlyAverage,
            AnnualAverage,
            MonthlyMaxToMin,
            MonthlyMaxToAverage,
            MonthlyMinToAverage,
            MovingAverage,
            FirstFlushPercentage,
            AverageEventWashoff,
            AverageWashoffDaysPerEvent,
            MaximumWashoffDays,
            TotalWashoffDays,
            WashoffToSurfaceFlowRatio
        }
        
        public static List<(MetricFunction, double?)> listFunctionsSensivitiyAnalysisBuildup = new List<(MetricFunction, double?)>
        {
            //{ (MetricFunction.ArrayAverage, null) },
            //{ (MetricFunction.ArrayTotal, null) },
            { (MetricFunction.MonthlyMaxToMin, null) },
            { (MetricFunction.MonthlyMaxToAverage, null) },
            { (MetricFunction.MonthlyMinToAverage, null) }
        };

        public static List<(MetricFunction, double?)> listFunctionsSensivitiyAnalysisWashoff = new List<(MetricFunction, double?)>
        {
            { (MetricFunction.ArrayAverage, null) },
            { (MetricFunction.ArrayTotal, null) },
            //{ (MetricFunction.MonthlyMaxToMin, null) },
            //{ (MetricFunction.MonthlyMaxToAverage, null) },
            //{ (MetricFunction.MonthlyMinToAverage, null) },
            { (MetricFunction.FirstFlushPercentage, null) },
            { (MetricFunction.AverageEventWashoff, null) },
            { (MetricFunction.AverageWashoffDaysPerEvent, null) },
            { (MetricFunction.MaximumWashoffDays, null) },
            { (MetricFunction.TotalWashoffDays, null) },
            { (MetricFunction.WashoffToSurfaceFlowRatio, null) }
        };

        public static List<(MetricFunction, double?)> listFunctionsScenarioAnalysisBuildup = new List<(MetricFunction, double?)>
        {
            { (MetricFunction.ArrayAverage, null) },
            { (MetricFunction.ArrayTotal, null) },
            { (MetricFunction.MonthlyMaxToMin, null) },
            { (MetricFunction.MonthlyMaxToAverage, null) },
            { (MetricFunction.MonthlyMinToAverage, null) },
            { (MetricFunction.MonthlyAverage, null) },
            { (MetricFunction.AnnualAverage, null) },
            { (MetricFunction.MovingAverage, 15D) }
        };

        public static List<(MetricFunction, double?)> listFunctionsScenarioAnalysisWashoff = new List<(MetricFunction, double?)>
        {
            { (MetricFunction.ArrayAverage, null) },
            { (MetricFunction.ArrayTotal, null) },
            { (MetricFunction.MonthlyMaxToMin, null) },
            { (MetricFunction.MonthlyMaxToAverage, null) },
            { (MetricFunction.MonthlyMinToAverage, null) },
            { (MetricFunction.MonthlyAverage, null) },
            { (MetricFunction.AnnualAverage, null) },
            { (MetricFunction.MovingAverage, 15D) },
            { (MetricFunction.FirstFlushPercentage, null) },
            { (MetricFunction.AverageEventWashoff, null) },
            { (MetricFunction.AverageWashoffDaysPerEvent, null) },
            { (MetricFunction.MaximumWashoffDays, null) },
            { (MetricFunction.TotalWashoffDays, null) },
            { (MetricFunction.WashoffToSurfaceFlowRatio, null) }

        };

        public static object ExecuteMetricFunction(MetricFunction func, double[] mainDataArray, double[] secondaryDataArray = null, DateTime[] datesArray = null, double? movingAverageParameter = null) {
            switch (func)
            {
                case MetricFunction.ArrayTotal:
                    return CalculateArraySum(mainDataArray);
                case MetricFunction.ArrayAverage:
                    return CalculateArrayAverage(mainDataArray);
                case MetricFunction.MonthlyAverage:
                    return CalculateArrayMonthlyAverage(datesArray, mainDataArray);
                case MetricFunction.AnnualAverage:
                    return CalculateArrayAnnualAverage(datesArray, mainDataArray);
                case MetricFunction.MonthlyMaxToMin:
                    return CalculateMonthlyAverageRatioMaxToMin(datesArray, mainDataArray);
                case MetricFunction.MonthlyMaxToAverage:
                    return CalculateMonthlyAverageRatioMaxToAverage(datesArray, mainDataArray);
                case MetricFunction.MonthlyMinToAverage:
                    return CalculateMonthlyAverageRatioMinToAverage(datesArray, mainDataArray);
                case MetricFunction.MovingAverage:
                    return CalculateMovingAverage(mainDataArray, (int)movingAverageParameter.Value);
                case MetricFunction.FirstFlushPercentage:
                    return CalculateAverageFirstFlushPercentage(mainDataArray);
                case MetricFunction.AverageEventWashoff:
                    return CalculateAverageEventWashoff(mainDataArray); 
                case MetricFunction.AverageWashoffDaysPerEvent:
                    return CalculateAverageEventLength(mainDataArray);
                case MetricFunction.MaximumWashoffDays:
                    return CalculateMaximumEventLength(mainDataArray);
                case MetricFunction.TotalWashoffDays:
                    return CalculateEventDaysTotal(mainDataArray);
                case MetricFunction.WashoffToSurfaceFlowRatio:
                    return CalculateWashoffToSurfaceFlowDaysRatio(mainDataArray, secondaryDataArray);
            }

            return null;

        }

        public static string GetMetricName(MetricFunction func)
        {
            switch (func)
            {
                case MetricFunction.ArrayTotal:
                    return "Somatório";
                case MetricFunction.ArrayAverage:
                    return "Valor médio";
                case MetricFunction.MonthlyAverage:
                    return "Médias mensais";
                case MetricFunction.AnnualAverage:
                    return "Médias anuais";
                case MetricFunction.MonthlyMaxToMin:
                    return "Razão máximo_mínimo";
                case MetricFunction.MonthlyMaxToAverage:
                    return "Razão máximo_média";
                case MetricFunction.MonthlyMinToAverage:
                    return "Razão mínimo_média";
                case MetricFunction.MovingAverage:
                    return "Média móvel";
                case MetricFunction.FirstFlushPercentage:
                    return "Taxa first flush";
                case MetricFunction.AverageEventWashoff:
                    return "Lavagem média dos eventos";
                case MetricFunction.AverageWashoffDaysPerEvent:
                    return "Média de dias de lavagem";
                case MetricFunction.MaximumWashoffDays:
                    return "Máximo de dias de lavagem";
                case MetricFunction.TotalWashoffDays:
                    return "Dias totais de lavagem";
                case MetricFunction.WashoffToSurfaceFlowRatio:
                    return "Razão lavagem_escoamento";
            }
            return null;
        }
        public static double CalculateArraySum(double[] dataArray)
        {
            return dataArray.Sum();
        }

        public static double CalculateArrayAverage(double[] dataArray)
        {
            return dataArray.Average();
        }

        public static double[] CalculateArrayMonthlyAverage(DateTime[] datesArray, double[] dataArray)
        {
            List<double> MonthlyAverages = new List<double>();

            for(DateTime i = datesArray[0]; i <= datesArray[datesArray.Length-1]; i = i.AddMonths(1))
            {
                int firstIndex = Array.IndexOf(datesArray, i);
                int lastIndex = Array.IndexOf(datesArray, i.AddMonths(1));
                if(lastIndex < firstIndex)
                {
                    lastIndex = datesArray.Length;
                }
                double average = dataArray.Skip(firstIndex).Take(lastIndex).Average();
                MonthlyAverages.Add(average);                
            }

            return MonthlyAverages.ToArray();
        }

        public static Ratio CalculateMonthlyAverageRatioMaxToMin(DateTime[] datesArray, double[] dataArray)
        {
            double[] monthlyAverages = CalculateArrayMonthlyAverage(datesArray, dataArray);
            double result = monthlyAverages.Max() / monthlyAverages.Min();
            if (!Double.IsInfinity(result))
            {
                return Ratio.FromDecimalFractions(result);
            }
            else
            {
                return Ratio.FromDecimalFractions(-1);
            }
        }

        public static Ratio CalculateMonthlyAverageRatioMaxToAverage(DateTime[] datesArray, double[] dataArray)
        {
            double[] monthlyAverages = CalculateArrayMonthlyAverage(datesArray, dataArray);

            return Ratio.FromDecimalFractions(monthlyAverages.Max() / monthlyAverages.Average());
        }

        public static Ratio CalculateMonthlyAverageRatioMinToAverage(DateTime[] datesArray, double[] dataArray)
        {
            double[] monthlyAverages = CalculateArrayMonthlyAverage(datesArray, dataArray);

            return Ratio.FromDecimalFractions(monthlyAverages.Min() / monthlyAverages.Average());
        }

        public static double[] CalculateArrayAnnualAverage(DateTime[] datesArray, double[] dataArray)
        {
            List<double> AnnualAverages = new List<double>();

            for (DateTime i = datesArray[0]; i <= datesArray[datesArray.Length]; i = i.AddYears(1))
            {
                int firstIndex = Array.IndexOf(datesArray, i);
                int lastIndex = Array.IndexOf(datesArray, i.AddYears(1));
                if (lastIndex < firstIndex)
                {
                    lastIndex = datesArray.Length;
                }
                double average = dataArray.Skip(firstIndex).Take(lastIndex).Average();
                AnnualAverages.Add(average);
            }

            return AnnualAverages.ToArray();
        }

        public static double[] CalculateMovingAverage(double[] dataArray, int interval = 15)
        {
            List<double> averageArray = new List<double>();
            for(int i = 0; i <= dataArray.Length - interval; i++)
            {
                double[] cutArray = dataArray.Skip(i).Take(i + interval - 1).ToArray();
                averageArray.Add(cutArray.Average());
            }
            return averageArray.ToArray();
        }

        public static Ratio CalculateFirstFlushPercentage(double[] eventArray)
        {
            double firstFlush = eventArray[0];
            return Ratio.FromDecimalFractions(firstFlush / eventArray.Sum());
        }

        public static int GetEventLength(double[] eventArray)
        {
            return eventArray.Count();
        }

        public static List<double[]> SplitEventArray(double[] timeSeries)
        {
            List<double[]> eventCollection = new List<double[]>();
            int cont = 0;
            for(int i = 0; i < timeSeries.Length; i++)
            {
                List<double> arrayEvent = new List<double>();
                cont = i;
                if(timeSeries[i] > 0)
                {   
                    while(cont < timeSeries.Length && timeSeries[cont] > 0)
                    {
                        arrayEvent.Add(timeSeries[cont]);
                        cont++;                        
                    }
                    eventCollection.Add(arrayEvent.ToArray());
                }
                i = cont;
                if (i == timeSeries.Length) {
                    return eventCollection;
                }
            }
            return eventCollection;
        }

        public static int[] GetEventLengthArray(List<double[]> splitEvents)
        {            
            List<int> eventLengthArray = new List<int>();

            foreach(double[] _event in splitEvents)
            {
                eventLengthArray.Add(GetEventLength(_event));
            }
            return eventLengthArray.ToArray();
        }

        public static Ratio CalculateAverageFirstFlushPercentage(double[] washoffTimeSeries)
        {
            List<double[]> splitEvents = SplitEventArray(washoffTimeSeries);
            double ratioSum = 0;
            List<Ratio> firstFlushPercentages = new List<Ratio>();
            foreach(double[] _event in splitEvents)
            {
                ratioSum += CalculateFirstFlushPercentage(_event).DecimalFractions;
            }
            return Ratio.FromDecimalFractions(ratioSum / splitEvents.Count());
        }

        public static double CalculateAverageEventWashoff(double[] washoffTimeSeries)
        {
            List<double[]> splitEvents = SplitEventArray(washoffTimeSeries);
            return splitEvents.Select(x => x.Sum()).Average();
        }

        public static double CalculateAverageEventLength(double[] timeSeries)
        {
            List<double[]> splitEvents = SplitEventArray(timeSeries);
            return GetEventLengthArray(splitEvents).Average();
        }

        public static double CalculateMaximumEventLength(double[] timeSeries)
        {
            List<double[]> splitEvents = SplitEventArray(timeSeries);
            return GetEventLengthArray(splitEvents).Max();
        }

        public static int CalculateEventDaysTotal(double[] timeSeries)
        {
            List<double[]> splitEvents = SplitEventArray(timeSeries);
            return GetEventLengthArray(splitEvents).Sum();
        }

        public static Ratio CalculateWashoffToSurfaceFlowDaysRatio(double[] washoffTimeSeries, double[] surfaceFlowTimeSeries)
        {
            int washoffDays = CalculateEventDaysTotal(washoffTimeSeries);
            int totalEventDays = CalculateEventDaysTotal(surfaceFlowTimeSeries);

            return Ratio.FromDecimalFractions((double)washoffDays / (double)totalEventDays);
        }
    }
}
