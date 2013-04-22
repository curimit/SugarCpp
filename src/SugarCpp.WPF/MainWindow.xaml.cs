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
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
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
                string output = sugar_cpp.Compile(input);
                this.Result.Text = output;
                File.WriteAllText("test.cpp", output);
            }
            catch (Exception ex)
            {
                string output = string.Format("编译错误:\n{0}", ex.Message);
                this.Result.Text = output;
            }
        }
    }
}
