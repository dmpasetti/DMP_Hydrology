using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USP_Hydrology
{
    public partial class Tree
    {
        //Funcao que preenche uma aba de planilha para uma matriz de Nw e Kw.
        //Nome da aba e o valor de kb
        //A planilha e para uma certa metrica
        //Numa pasta com certo valor de escMax
        //Para cada bacia

        /*Fluxograma
         * Defino EscMax
         * Dentro do Loop, acho Kb
         * Em outro loop, simulo para diferentes Nw e Kw
         * Em mais um loop (dentro de um enum, ou dicionario), Percorro metricas selecionadas
         * Para cada metrica, calculo seu valor para as simulacoes feitas com cada Nw e Kw, preencho uma aba da planilha 
         * Isso para buildup e washoff
         * Vario Kb
         */

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ArrayNw"></param>
        /// <param name="ArrayKw"></param>
        /// <param name="Matrix"> Dicionario para output. 
        /// Key = (double, double) = (nw, kw).
        /// Value = double = Metrica sendo impressa
        /// </param>
        /// <returns></returns>
        public static List<object[]> FillSpreadsheetWashoffMatrix(double[] ArrayNw, double[] ArrayKw, Dictionary<(double, double), double> Matrix)
        {
            List<object> firstRow = new List<object>();
            firstRow.Add(null);
            firstRow.Add("Kw");
            List<object> secondRow = new List<object>();
            secondRow.Add("Nw");
            foreach (double _kw in ArrayKw)
            {
                secondRow.Add(_kw);
            }
            List<object[]> dataContent = new List<object[]>();
            dataContent.Add(firstRow.ToArray());
            dataContent.Add(secondRow.ToArray());
            for(int i = 0; i < ArrayNw.Length; i++)
            {
                List<object> dataRow = new List<object>();
                double _nw = ArrayNw[i];
                dataRow.Add(_nw);
                for(int j = 0; j < ArrayKw.Length; j++)
                {
                    double _kw = ArrayKw[j];
                    (double, double) key = (_nw, _kw);
                    dataRow.Add(Matrix[key]);
                }
                dataContent.Add(dataRow.ToArray());
            }
            return dataContent;
        }

        

    }
}
