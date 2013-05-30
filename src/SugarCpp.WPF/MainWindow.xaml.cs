using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SugarCpp.Compiler;
using System.IO;

namespace SugarCpp.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Source.Options.ConvertTabsToSpaces = true;

            if (File.Exists("test.sc"))
            {
                this.Source.Text = File.ReadAllText("test.sc");
            }
        }

        private void Source_TextChanged_1(object sender, EventArgs e)
        {
            string input = this.Source.Text;
            File.WriteAllText("test.sc", input);
            try
            {
                TargetCpp sugar_cpp = new TargetCpp();
                var result = SugarCompiler.Compile(input, "test");
                this.Header.Text = result.Header;
                File.WriteAllText("test.h", this.Header.Text);
                this.Implementation.Text = result.Implementation;
                File.WriteAllText("test.cpp", this.Implementation.Text);
            }
            catch (Exception ex)
            {
                string output = string.Format("Compile Error:\n{0}", ex.Message);
                this.Header.Text = output;
            }
        }
    }
}
