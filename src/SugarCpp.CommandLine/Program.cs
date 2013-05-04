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
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Panic("No arguments specified. Use --help for more information.");
            }
            switch (args[0])
            {
                case "compile":
                    Compile.Main(TrimArg(args));
                    break;
                case "run":
                    DoRun(TrimArg(args));
                    break;
                default:
                    Translate.Main(args);
                    break;
            }
        }

        private static string[] TrimArg(string[] args)
        {
            string[] trimmedArgs = new string[args.Length - 1];
            Array.Copy(args, 1, trimmedArgs, 0, args.Length - 1);
            return trimmedArgs;
        }

        private static void DoRun(string[] args)
        {
            throw new NotImplementedException();
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
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(indent + "--ast /ast               Output the abstract syntax tree.");
            Console.WriteLine(indent + "--help -h /help /h /?    Output this help text.");
            Console.WriteLine(indent + "--nocode /nocode         Do not print the generated code.");
            Console.WriteLine(indent + "--output -o /output /o [filename]");
            Console.WriteLine(indent + "                         Filename of output. If not specified, output");
            Console.WriteLine(indent + "                         will be printed to standard output.");
            Console.WriteLine(indent + "--token /token           Output the tokens.");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine(indent + "sugarcpp code.sc -o code.cpp");
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
