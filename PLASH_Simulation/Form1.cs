using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PLASH_Simulation
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        public string InputFileName;
        public string OutputFileName;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
                        
            OFD.Multiselect = false;
            OFD.Title = "Selecionar planilha de entrada";
            OFD.Filter = "Excel Document|*.xlsx;*.xls";
            OFD.ShowDialog();
            InputFileName = OFD.FileName;            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Title = "Selecionar pasta do arquivo de saída";
            SFD.Filter = "Excel Document|*.xlsx;*.xls";
            SFD.ShowDialog();
            OutputFileName = SFD.FileName;            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
