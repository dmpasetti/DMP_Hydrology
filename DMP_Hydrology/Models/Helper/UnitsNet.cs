using System;
using System.Collections.Generic;
using System.Linq;
using UnitsNet;

namespace USP_Hydrology
{
    public static partial class Helper
    {
        // Addition
        public static Duration Addition(this Duration DurationT, Duration Duration)
        {
            return DurationT + Duration;
        }
        public static Length Addition(this Length LengthT, Length Length)
        {
            return LengthT + Length;
        }
        public static Ratio Addition(this Double Double, Ratio Ratio)
        {
            return Ratio.FromDecimalFractions(Double + Ratio.DecimalFractions);
        }
        public static Ratio Addition(this Ratio Ratio, Double Double)
        {
            return Ratio.FromDecimalFractions(Ratio.DecimalFractions + Double);
        }
        public static Ratio Addition(this Ratio RatioT, Ratio Ratio)
        {
            return RatioT + Ratio;
        }
        public static Volume Addition(this Volume VolumeT, Volume Volume)
        {
            return VolumeT + Volume;
        }
        public static VolumeFlow Addition(this VolumeFlow VolumeFlowT, VolumeFlow VolumeFlow)
        {
            return VolumeFlowT + VolumeFlow;
        }

        // Subtraction
        public static Duration Subtraction(this Duration DurationT, Duration Duration)
        {
            return DurationT - Duration;
        }
        public static Length Subtraction(this Length LengthT, Length Length)
        {
            return LengthT - Length;
        }
        public static Ratio Subtraction(this Double Double, Ratio Ratio)
        {
            return Ratio.FromDecimalFractions(Double - Ratio.DecimalFractions);
        }
        public static Ratio Subtraction(this Ratio Ratio, Double Double)
        {
            return Ratio.FromDecimalFractions(Ratio.DecimalFractions - Double);
        }
        public static Ratio Subtraction(this Ratio RatioT, Ratio Ratio)
        {
            return RatioT - Ratio;
        }
        public static Volume Subtraction(this Volume VolumeT, Volume Volume)
        {
            return VolumeT - Volume;
        }
        public static VolumeFlow Subtraction(this VolumeFlow VolumeFlowT, VolumeFlow VolumeFlow)
        {
            return VolumeFlowT - VolumeFlow;
        }

        // Multiplication
        public static Duration Multiplication(this Double Double, Duration Duration)
        {
            return Double * Duration;
        }
        public static Duration Multiplication(this Duration Duration, Double Double)
        {
            return Duration * Double;
        }
        public static Duration Multiplication(this Duration Duration, Ratio Ratio)
        {
            return Duration * Ratio.DecimalFractions;
        }
        public static Length Multiplication(this Length Length, Ratio Ratio)
        {
            return Length * Ratio.DecimalFractions;
        }
        public static Length Multiplication(this Speed Speed, Duration Duration)
        {
            return Speed * Duration;
        }
        public static Ratio Multiplication(this Ratio Ratio, Double Double)
        {
            return Ratio * Double;
        }
        public static Volume Multiplication(this Area Area, Length Length)
        {
            return Area * Length;
        }
        public static Volume Multiplication(this Length Length, Area Area)
        {
            return Length * Area;
        }
        public static Volume Multiplication(this Volume Volume, Double Double)
        {
            return Volume * Double;
        }
        public static Volume Multiplication(this Volume Volume, Ratio Ratio)
        {
            return Volume * Ratio.DecimalFractions;
        }
        public static Volume Multiplication(this VolumeFlow VolumeFlow, Duration Duration)
        {
            return VolumeFlow * Duration;
        }
        public static VolumeFlow Multiplication(this VolumeFlow VolumeFlow, Double Double)
        {
            return VolumeFlow * Double;
        }
        public static VolumeFlow Multiplication(this VolumeFlow VolumeFlow, Ratio Ratio)
        {
            return VolumeFlow * Ratio.DecimalFractions;
        }

        // Division
        public static Double Division(this Ratio RatioT, Ratio Ratio)
        {
            return RatioT / Ratio;
        }
        public static Duration Division(this Duration Duration, Double Double)
        {
            return Duration / Double;
        }
        public static Length Division(this Area Area, Length Length)
        {
            return Area / Length;
        }
        public static Length Division(this Length Length, Double Double)
        {
            return Length / Double;
        }
        public static Length Division(this Length Length, Ratio Ratio)
        {
            return Length / Ratio.DecimalFractions;
        }
        public static Length Division(this Volume Volume, Area Area)
        {
            return Volume / Area;
        }
        public static Ratio Division(this Duration DurationT, Duration Duration)
        {
            return Ratio.FromDecimalFractions(DurationT / Duration);
        }
        public static Ratio Division(this Length LengthT, Length Length)
        {
            return Ratio.FromDecimalFractions(LengthT / Length);
        }
        public static Ratio Division(this Volume VolumeT, Volume Volume)
        {
            return Ratio.FromDecimalFractions(VolumeT / Volume);
        }
        public static Ratio Division(this VolumeFlow VolumeFlowT, VolumeFlow VolumeFlow)
        {
            return Ratio.FromDecimalFractions(VolumeFlowT / VolumeFlow);
        }
        public static Speed Division(this VolumeFlow VolumeFlow, Area Area)
        {
            return Speed.FromMetersPerSecond(VolumeFlow.CubicMetersPerSecond / Area.SquareMeters);
        }
        public static VolumeFlow Division(this Volume Volume, Duration Duration)
        {
            return Volume / Duration;
        }
        public static VolumeFlow Division(this VolumeFlow VolumeFlow, Double Double)
        {
            return VolumeFlow / Double;
        }

        // Power
        public static Ratio Power(this Ratio Ratio, Double Double)
        {
            return Ratio.FromDecimalFractions(Math.Pow(Ratio.DecimalFractions, Double));
        }
        public static Ratio Power(this Ratio RatioT, Ratio Ratio)
        {
            return Ratio.FromDecimalFractions(Math.Pow(RatioT.DecimalFractions, Ratio.DecimalFractions));
        }
        public static Area Power2(this Length Length)
        {
            return Length * Length;
        }

        // Minimum
        public static Ratio Minimum(Ratio Ratio1, Ratio Ratio2)
        {
            return Ratio.FromDecimalFractions(Math.Min(Ratio1.DecimalFractions, Ratio2.DecimalFractions));
        }
        public static Volume Minimum(Volume Volume1, Volume Volume2)
        {
            return Volume.FromCubicMeters(Math.Min(Volume1.CubicMeters, Volume2.CubicMeters));
        }
        public static VolumeFlow Minimum(VolumeFlow VolumeFlow1, VolumeFlow VolumeFlow2)
        {
            return VolumeFlow.FromCubicMetersPerSecond(Math.Min(VolumeFlow1.CubicMetersPerSecond, VolumeFlow2.CubicMetersPerSecond));
        }

        // Maximum
        public static Length Maximum(this Length LengthT, Length Length)
        {
            return Length.FromMeters(Math.Max(LengthT.Meters, Length.Meters));
        }
        public static Ratio Maximum(Ratio Ratio1, Ratio Ratio2)
        {
            return Ratio.FromDecimalFractions(Math.Max(Ratio1.DecimalFractions, Ratio2.DecimalFractions));
        }
        public static Volume Maximum(Volume Volume1, Volume Volume2)
        {
            return Volume.FromCubicMeters(Math.Max(Volume1.CubicMeters, Volume2.CubicMeters));
        }
        public static VolumeFlow Maximum(VolumeFlow VolumeFlow1, VolumeFlow VolumeFlow2)
        {
            return VolumeFlow.FromCubicMetersPerSecond(Math.Max(VolumeFlow1.CubicMetersPerSecond, VolumeFlow2.CubicMetersPerSecond));
        }
        public static VolumeFlow Maximum(VolumeFlow VolumeFlow1, VolumeFlow VolumeFlow2, VolumeFlow VolumeFlow3)
        {
            return VolumeFlow.FromCubicMetersPerSecond(Math.Max(Math.Max(VolumeFlow1.CubicMetersPerSecond, VolumeFlow2.CubicMetersPerSecond), VolumeFlow3.CubicMetersPerSecond));
        }

        // Average
        public static Length Average(this IEnumerable<Length> Length)
        {
            var C = Length.Count();
            var S = 0D;
            for (int c = 0; c < C; c++)
                S += Length.ElementAt(c).Meters;
            return UnitsNet.Length.FromMeters(S / C);
        }
        public static Ratio Average(this IEnumerable<Ratio> Ratio)
        {
            var C = Ratio.Count();
            var S = 0D;
            for (int c = 0; c < C; c++)
                S += Ratio.ElementAt(c).DecimalFractions;
            return UnitsNet.Ratio.FromDecimalFractions(S / C);
        }

        // Sum
        public static Length Sum(this IEnumerable<Length> Length)
        {
            var C = Length.Count();
            var S = 0D;
            for (int c = 0; c < C; c++)
                S += Length.ElementAt(c).Meters;
            return UnitsNet.Length.FromMeters(S);
        }
    }
}
