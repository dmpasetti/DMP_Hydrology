using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMP_Hydrology.PLASH
{
    public partial class PLASH
    {
        public (double, double, double) PLASHParamFromCN(double CN)
        {
            double CH = PLASHKFromCN(CN);
            double S = PLASHSFromCN(CN);
            double FS = Math.Pow(S, 2) / (2 * CH);

            return (CH, S, FS);
        }
        
        private double PLASHKFromCN(double CN)
        {
            if (CN <= 36D) return 47.07 - 0.82 * CN;
            if (CN <= 75D) return 31.39 - 0.39 * CN;
            return (100D - CN) / 12.42;
        } 

        private double PLASHSFromCN(double CN)
        {
            if (CN <= 65D) return 30.25 - 0.146 * CN;
            return (100D - CN) / 1.66;
        }




    }
}
