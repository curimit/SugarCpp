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
            if (args.Length == 0)
            {
                Program.Panic("You should specify a source file.");
            }
            string inputFileName = args[0];
            string compilerArgs = String.Empty;
            if (args.Length > 1)
            {
                Arguments arguments = new Arguments(Program.TrimArg(args), null);
                compilerArgs = arguments.ToString();
            }
            DetectCompiler();
            DoCompile(inputFileName, compilerArgs);
        }

        private static void DetectCompiler()
        {
            // Detect CXX from environment varibles
            compilerCommand = Environment.GetEnvironmentVariable("CXX");
            if (compilerCommand != null)
            {
                return;
            }
            // Try clang++ g++ and cl
            string suffix = ".exe";
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                suffix = string.Empty;
            }
            // clang++
            compilerCommand = GetFullPath("clang++" + suffix);
            if (compilerCommand != null)
            {
                return;
            }
            // g++
            compilerCommand = GetFullPath("g++" + suffix);
            if (compilerCommand != null)
            {
                compilerAdditionalArgs = "-std=c++0x";
                return;
            }
            // cl
            compilerCommand = GetFullPath("cl.exe");
            if (compilerCommand != null)
            {
                return;
            }
            Program.Panic("No compiler detected on your system. You should specify one by using environment varible CXX.");
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }
            string paths = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in paths.Split(Path.PathSeparator))
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        private static void DoCompile(string inputFileName, string arguments)
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
            proc.StartInfo.FileName = compilerCommand;
            proc.StartInfo.Arguments = cppFileName + " " + arguments + " " + compilerAdditionalArgs;
            proc.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            proc.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);
            proc.Start();
            proc.WaitForExit();
        }

        private static string compilerCommand = null;
        private static string compilerAdditionalArgs = String.Empty;
    }
}
