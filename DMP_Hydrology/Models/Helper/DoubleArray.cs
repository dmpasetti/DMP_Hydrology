using System;
using System.Linq;

namespace USP_Hydrology
{
    public static partial class Helper
    {
        public static Double Addition(this Double value, Double Value)
        {
            return value + Value;
        }
        public static Double[] Addition(this Double[] values, Double[] Values)
        {
            return values.Zip(Values, (x, y) => x + y).ToArray();
        }
        public static Double[] Addition(this Double[] values, Double[] Values1, Double[] Values2)
        {
            return values.Zip(Values1, (x, y) => x + y).Zip(Values2, (x, y) => x + y).ToArray();
        }
        public static Double[] Addition(this Double[] values, Double[] Values1, Double[] Values2, Double[] Values3)
        {
            return values.Zip(Values1, (x, y) => x + y).Zip(Values2, (x, y) => x + y).Zip(Values3, (x, y) => x + y).ToArray();
        }
        public static Double[] Addition(this Double[] values, Double[] Values1, Double[] Values2, Double[] Values3, Double[] Values4)
        {
            return values.Zip(Values1, (x, y) => x + y).Zip(Values2, (x, y) => x + y).Zip(Values3, (x, y) => x + y).Zip(Values4, (x, y) => x + y).ToArray();
        }
        public static Double[] Addition(this Double[] values, Double[] Values1, Double[] Values2, Double[] Values3, Double[] Values4, Double[] Values5)
        {
            return values.Zip(Values1, (x, y) => x + y).Zip(Values2, (x, y) => x + y).Zip(Values3, (x, y) => x + y).Zip(Values4, (x, y) => x + y).Zip(Values5, (x, y) => x + y).ToArray();
        }
        public static Double[][] Addition(this Double[][] values, Double[][] Values)
        {
            return values.Zip(Values, (x, y) => x.Zip(y, (X, Y) => X +  Y).ToArray()).ToArray();
        }

        public static Double Division(this Double value, Double Value)
        {
            return value / Value;
        }
        public static Double[] Division(this Double[] values, Double Value)
        {
            return values.Select(v => v / Value).ToArray();
        }
        public static Double[] Division(this Double[] values, Double[] Values)
        {
            return values.Zip(Values, (x, y) => x / y).ToArray();
        }

        public static Double Multiplication(this Double value, Double Value)
        {
            return value * Value;
        }
        public static Double[] Multiplication(this Double[] values, Double Value)
        {
            return values.Select(v => v * Value).ToArray();
        }
        public static Double[] Multiplication(this Double[] values, Double[] Values)
        {
            return values.Zip(Values, (x, y) => x * y).ToArray();
        }

        public static Double Subtraction(this Double value, Double Value)
        {
            return value - Value;
        }
        public static Double[] Subtraction(this Double[] values, Double Value)
        {
            return values.Select(v => v - Value).ToArray();
        }
        public static Double[] Subtraction(this Double[] values, Double[] Values)
        {
            return values.Zip(Values, (x, y) => x - y).ToArray();
        }

        public static Double[] Maximum(Double[] Values1, Double[] Values2)
        {
            return Values1.Zip(Values2, (x, y) => Math.Max(x, y)).ToArray();
        }
        public static Double[] Maximum(Double[] Values1, Double[] Values2, Double[] Values3)
        {
            return Values1.Zip(Values2, (x, y) => Math.Max(x, y)).Zip(Values3, (x, y) => Math.Max(x, y)).ToArray();
        }
        public static Double[] Maximum(Double[] Values1, Double[] Values2, Double[] Values3, Double[] Values4)
        {
            return Values1.Zip(Values2, (x, y) => Math.Max(x, y)).Zip(Values3, (x, y) => Math.Max(x, y)).Zip(Values4, (x, y) => Math.Max(x, y)).ToArray();
        }

        public static Double[] Minimum(Double[] Values1, Double[] Values2)
        {
            return Values1.Zip(Values2, (x, y) => Math.Min(x, y)).ToArray();
        }
        public static Double[] Minimum(Double[] Values1, Double[] Values2, Double[] Values3)
        {
            return Values1.Zip(Values2, (x, y) => Math.Min(x, y)).Zip(Values3, (x, y) => Math.Min(x, y)).ToArray();
        }
        public static Double[] Minimum(Double[] Values1, Double[] Values2, Double[] Values3, Double[] Values4)
        {
            return Values1.Zip(Values2, (x, y) => Math.Min(x, y)).Zip(Values3, (x, y) => Math.Min(x, y)).Zip(Values4, (x, y) => Math.Min(x, y)).ToArray();
        }
    }
}
