using SugarCpp.Compiler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SugarCpp.WinForm
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists("test.sc"))
            {
                this.Source.Text = File.ReadAllText("test.sc");
            }
        }

        private void Source_TextChanged(object sender, EventArgs e)
        {
            string input = this.Source.Text.Replace("\t", "    ");
            File.WriteAllText("test.sc", input);
            try
            {
                TargetCpp sugar_cpp = new TargetCpp();
                string output = sugar_cpp.Compile(input);
                this.Result.Text = output;
                File.WriteAllText("test.cpp", output);
            }
            catch (Exception ex)
            {
                string output = string.Format("Compile Error:\n{0}", ex.Message);
                this.Result.Text = output;
            }
        }
    }
}
