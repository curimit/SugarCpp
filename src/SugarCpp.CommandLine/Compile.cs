using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SugarCpp.Compiler;

namespace SugarCpp.CommandLine
{
    class Compile
	{
		internal static void CompileSource(string[] args)
		{
			if (args.Length == 0)
			{
				Program.Panic("You should specify a source file.");
			}
			string inputFileName = args[0];
			string compilerArgs = string.Empty;
			if (args.Length > 1)
			{
				Arguments arguments = new Arguments(Program.TrimArg(args), null);
				compilerArgs = arguments.ToString();
			}
			DetectCompiler();
			DoCompile(inputFileName, compilerArgs);
		}
		
		internal static void RunSource(string[] args)
		{
			if (args.Length == 0)
			{
				Program.Panic("You should specify a source file.");
			}
			string inputFileName = args[0];
			string runArgs = string.Empty;
			if (args.Length > 1)
			{
				Arguments arguments = new Arguments(Program.TrimArg(args), null);
				runArgs = arguments.ToString();
			}
			DetectCompiler();
			string exeFileName = Path.GetTempFileName();
			File.Delete(exeFileName);
			exeFileName += ".exe";
			DoCompile(inputFileName, compiler.OutputArg + " " + exeFileName);
			RunCommand(exeFileName, runArgs);
		}

        private static void DetectCompiler()
        {
            // Detect CXX from environment varibles
            string compilerCommand = Environment.GetEnvironmentVariable("CXX");
            if (compilerCommand != null)
            {
				string outputArg = Environment.GetEnvironmentVariable("OUTPUTARG");
				if (outputArg == null)
				{
					outputArg = "-o";
				}
				compiler = new CompilerInfo {
					Command=compilerCommand,
					AdditionalArgs="",
					OutputArg=outputArg
				};
                return;
            }
            // Try clang++ g++ and cl
            string suffix = ".exe";
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                suffix = string.Empty;
            }
			foreach (var compilerInfo in compilers)
			{
				compilerCommand = GetFullPath(compilerInfo.Command + suffix);
				if (compilerCommand != null)
				{
					compiler = compilerInfo;
					return;
				}
			}
            Program.Panic("No compiler detected on your system. You should specify one by using environment varible CXX.");
        }

        internal static string GetFullPath(string fileName)
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

		internal static void DoCompile(string inputFileName, string arguments)
        {
            string cppFileName = Path.GetTempFileName();

            string input = File.ReadAllText(inputFileName);
            TargetCppResult result = null;
            try
            {
                TargetCpp sugarCpp = new TargetCpp();
                result = sugarCpp.Compile(input, cppFileName);
            }
            catch (Exception ex)
            {
                Program.Panic(string.Format("Compile Error:\n{0}", ex.Message));
            }
            // Write to temperory file
            File.Delete(cppFileName);
            File.WriteAllText(cppFileName + ".h", result.Header);
            File.WriteAllText(cppFileName + ".cpp", result.Implementation);

            // Execute compiler
			RunCommand(compiler.Command, cppFileName + ".cpp" + " " + arguments + " " + compiler.AdditionalArgs);
        }

		private static void RunCommand(string command, string arguments)
		{
			Process proc = new Process();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.FileName = command;
			proc.StartInfo.Arguments = arguments;
			proc.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
			proc.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);
			proc.Start();
			proc.WaitForExit();
		}

		private class CompilerInfo
		{
			public string Command;
			public string AdditionalArgs;
			public string OutputArg;
		}

		private static CompilerInfo[] compilers = new CompilerInfo[]{
			new CompilerInfo{Command="clang++", AdditionalArgs="-std=c++11", OutputArg="-o"},
			new CompilerInfo{Command="g++", AdditionalArgs="-std=c++0x", OutputArg="-o"},
			new CompilerInfo{Command="cl", AdditionalArgs="", OutputArg="/o"}
		};

		private static CompilerInfo compiler = null;
    }
}
