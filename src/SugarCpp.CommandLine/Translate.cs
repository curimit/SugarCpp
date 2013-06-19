using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using SugarCpp.Compiler;

namespace SugarCpp.CommandLine
{
    class Translate
    {
        internal static int Main(string[] args)
        {
            Arguments arguments = new Arguments(args, new Dictionary<string, bool> {
                {"output", true},
                {"o", true},
                {"token", false},
                {"ast", false},
                {"nocode", false},
                {"single", false},
                {"help", false},
                {"h", false},
                {"?", false},
            });

            if (arguments.HasOption("help") || arguments.HasOption("h") || arguments.HasOption("?"))
            {
                Program.PrintHelp();
                return 0;
            }

            printTokens = arguments.HasOption("token");
            printAST = arguments.HasOption("ast");
            printCode = !arguments.HasOption("nocode");
            singleFile = arguments.HasOption("single");
            if (arguments.HasOption("o"))
            {
                outputPath = arguments.GetOption("o").Replace("\\", "/");
            }
            if (arguments.HasOption("output"))
            {
                outputPath = arguments.GetOption("output").Replace("\\", "/");
            }

            if (arguments.DirectArguments.Count == 0)
            {
                Program.Panic("No input file is specified. Use --help for more information.");
                return 0;
            }

            // multiple input file
            foreach (var fname in arguments.DirectArguments.Where(x => !x.StartsWith("-")))
            {
                string input = null;
                try
                {
                    input = File.ReadAllText(fname);
                }
                catch
                {
                    Console.WriteLine("Unable to read file: {0}", fname);
                }

                try
                {
                    if (printTokens)
                    {
                        PrintTokens(input);
                    }
                    else if (printAST)
                    {
                        PrintAST(input);
                    }
                    else
                    {
                        Compile(input, fname.Replace("\\", "/"));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Compile error with file: {0}", fname);
                    Console.WriteLine(e.Message);
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Compile code from input.
        /// </summary>
        private static void Compile(string input, string inputFileName)
        {
            int dot_pos = inputFileName.LastIndexOf(".");
            string file_no_ext = inputFileName.Substring(0, dot_pos);
            string header_name = file_no_ext + ".h";
            string implementation_name = file_no_ext + ".cpp";

            if (outputPath != null || outputPath == "")
            {
                if (!outputPath.EndsWith("/")) outputPath = outputPath + "/";
                int k = header_name.LastIndexOf("/");
                header_name = k == -1 ? header_name : header_name.Substring(k + 1);
                header_name = outputPath + header_name;

                k = implementation_name.LastIndexOf("/");
                implementation_name = outputPath + (k == -1 ? implementation_name : implementation_name.Substring(k + 1));
            }

            if (singleFile)
            {
                string code = SugarCompiler.Compile(input);

                if (printCode)
                {
                    Console.WriteLine(code);
                }

                File.WriteAllText(implementation_name, code);
            }
            else
            {
                var result = SugarCompiler.Compile(input, file_no_ext);

                if (printCode)
                {
                    Console.WriteLine(result.Header);

                    Console.WriteLine();

                    Console.WriteLine(result.Implementation);
                }

                File.WriteAllText(header_name, result.Header);
                File.WriteAllText(implementation_name, result.Implementation);
            }
        }

        /// <summary>
        /// Print the abstract syntax tree.
        /// </summary>
        private static void PrintAST(string input)
        {
            PrintAST(SugarCompiler.GetAst(input), 0);
        }

        /// <summary>
        /// Print the abstract syntax tree.
        /// </summary>
        /// <param name="astNode">Node of abstract syntax tree.</param>
        /// <param name="depth">Depth of node from root.</param>
        private static void PrintAST(CommonTree astNode, int depth)
        {
            string indent = "  ";
            for (int i = 0; i < depth; i++)
            {
                Console.Write(indent);
            }
            Print(astNode.Text);
            for (int i = 0; i < astNode.ChildCount; i++)
            {
                PrintAST(astNode.Children[i] as CommonTree, depth + 1);
            }
        }

        /// <summary>
        /// Print the tokens.
        /// </summary>
        /// <param name="input">SugarCpp source code</param>
        private static void PrintTokens(string input)
        {
            foreach (var token in SugarCompiler.GetTokens(input))
            {
                Print(token.ToString());
            }
        }

        /// <summary>
        /// Print text to console or output file.
        /// </summary>
        /// <param name="contents"></param>
        private static void Print(string contents, params object[] objs)
        {
            if (outputPath == null)
            {
                Console.WriteLine(contents, objs);
            }
            else
            {
                outputFile.WriteLine(contents, objs);
            }
        }

        private static string outputPath = null;
        private static bool printTokens = false;
        private static bool printAST = false;
        private static bool singleFile = false;
        private static bool printCode = true;
        private static StreamWriter outputFile = null;
    }
}
