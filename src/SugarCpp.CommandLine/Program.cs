using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SugarCpp.CommandLine
{
    class Program
    {
        /// <summary>
        /// Entry point of program.
        /// </summary>
        /// <param name="args">Arguments.</param>
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Panic("No arguments specified. Use --help for more information.");
            }
            switch (args[0])
            {
                case "compile":
                    return Compile.CompileSource(TrimArg(args));
                    break;
                case "run":
                    return Compile.RunSource(TrimArg(args));
                    break;
                default:
                    return Translate.Main(args);
                    break;
            }
        }

        public static string[] TrimArg(string[] args)
        {
            string[] trimmedArgs = new string[args.Length - 1];
            Array.Copy(args, 1, trimmedArgs, 0, args.Length - 1);
            return trimmedArgs;
        }

        /// <summary>
        /// Raise a panic and exit program.
        /// </summary>
        /// <param name="reason">Reason for exit.</param>
        internal static void Panic(string reason)
        {
            Console.Error.WriteLine(reason);
            Environment.Exit(1);
        }

        /// <summary>
        /// Print help text and exit program.
        /// </summary>
        internal static void PrintHelp()
        {
            // Get names and versions from assemblies.
            Assembly commandLineAssembly = Assembly.GetExecutingAssembly();
            AssemblyName commandLineAssemblyName = commandLineAssembly.GetName();
            AssemblyName compilerAssemblyName = GetCompilerAssemblyName(commandLineAssembly);

            string indent = "    ";
            Console.WriteLine("SugarCpp");
            Console.WriteLine("Compiler Version " + GetVersionString(compilerAssemblyName));
            Console.WriteLine("Command Line Interface Version " + GetVersionString(commandLineAssemblyName));
            Console.WriteLine();
            Console.WriteLine("Project website: https://github.com/curimit/SugarCpp");
            Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.WriteLine(indent + "sugarcpp [filename] <options>");
			Console.WriteLine(indent + "sugarcpp compile [filename] <compiler arguments>");
			Console.WriteLine(indent + "sugarcpp run [filename] <arguments>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(indent + "--ast /ast                  Output the abstract syntax tree.");
            Console.WriteLine(indent + "--token /token              Output the tokens.");
            Console.WriteLine(indent + "--help -h /help /h /?       Output this help text.");
            Console.WriteLine(indent + "--nocode /nocode            Do not print the generated code.");
            Console.WriteLine(indent + "--single /single            Translate into single cpp file.");
            Console.WriteLine(indent + "--output -o /output /o [output_path]");
            Console.WriteLine(indent + "                            Path of output. If not specified, output");
            Console.WriteLine(indent + "                            will be written into the directory");
            Console.WriteLine(indent + "                            where your source code at.");
            Console.WriteLine();
			Console.WriteLine("Examples:");
			Console.WriteLine(indent + "Translate into C++ code");
			Console.WriteLine(indent + indent + "sugarcpp code.sc");
			Console.WriteLine(indent + "Compile to binary by calling the default compiler");
			Console.WriteLine(indent + indent + "sugarcpp compile code.sc -o code.exe");
			Console.WriteLine(indent + "Compile and run");
			Console.WriteLine(indent + indent + "sugarcpp run code.sc");
            Environment.Exit(0);
        }

        /// <summary>
        /// Get and format version of assembly.
        /// </summary>
        /// <param name="name">AssemblyName</param>
        /// <returns></returns>
        internal static string GetVersionString(AssemblyName name)
        {
            return name.Version.Major + "." + name.Version.Minor + "." + name.Version.Build;
        }

        /// <summary>
        /// Get assembly name of compiler.
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns></returns>
        internal static AssemblyName GetCompilerAssemblyName(Assembly assembly)
        {
            foreach (var referenced in assembly.GetReferencedAssemblies())
            {
                if (referenced.Name == "SugarCpp.Compiler")
                {
                    return referenced;
                }
            }
            Panic("No SugarCpp.Compiler assembly found! This should not happen.");
            return null;
        }
    }
}
