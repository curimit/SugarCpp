using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SugarCpp.Compiler;

namespace SugarCpp.CommandLine
{
    class Compile
    {
        internal static void Main(string[] args)
        {
            if (args[0] != "compile")
            {
                Program.Panic("First argument should be 'compile'.");
            }
            GetCompiler();
            DoCompile(args[1]);
        }

        private static void GetCompiler()
        {
            // Environment varibles
            // Try clang++ g++ and cl
            compiler = "g++";
            return;
            Program.Panic("No compiler detected on your system. You should specify one by using --compiler or environment varible CXX.");
        }

        private static void DoCompile(string inputFileName)
        {
            string input = File.ReadAllText(inputFileName);
            string output = String.Empty;
            try
            {
                TargetCpp sugarCpp = new TargetCpp();
                output = sugarCpp.Compile(input);
            }
            catch (Exception ex)
            {
                Program.Panic(string.Format("Compile Error:\n{0}", ex.Message));
            }
            // Write to temperory file
            string cppFileName = Path.GetTempFileName();
            File.Delete(cppFileName);
            cppFileName += ".cpp";
            File.WriteAllText(cppFileName, output);
            // Execute compiler
            Process proc = new Process();
            // Redirect the output stream of the child process.
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = compiler;
            proc.StartInfo.Arguments = cppFileName + " -std=c++0x";
            proc.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            proc.Start();
            proc.WaitForExit();
        }

        private static string compiler = null;
    }
}
