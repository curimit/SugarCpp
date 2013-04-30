using System;
using System.IO;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using SugarCpp.Compiler;

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
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                string nextArg = null;
                if (i < args.Length - 1)
                {
                    nextArg = args[i + 1];
                }

                switch (arg)
                {
                    case "--output":
                    case "-o":
                    case "/output":
                    case "/o":
                        if (nextArg == null)
                        {
                            Panic("No file specified after " + arg);
                        }
                        if (outputFileName != null)
                        {
                            Panic("You can not specify more than one output file");
                        }
                        outputFileName = nextArg;
                        i++;
                        break;
                    case "--token":
                    case "/token":
                        printTokens = true;
                        break;
                    case "--ast":
                    case "/ast":
                        printAST = true;
                        break;
                    case "--nocode":
                    case "/nocode":
                        printCode = false;
                        break;
                    case "--help":
                    case "-h":
                    case "/help":
                    case "/h":
                    case "/?":
                        PrintHelp();
                        break;
                    default:
                        if (inputFileName != null)
                        {
                            Panic("Multiple input file is not supported");
                        }
                        inputFileName = arg;
                        break;
                }
            }
            if (inputFileName == null)
            {
                Panic("No input file is specified. Use --help for more information.");
            }
            if (outputFileName != null)
            {
                outputFile = new StreamWriter(outputFileName);
            }
            Compile();
            if (outputFile != null)
            {
                outputFile.Close();
            }
        }

        /// <summary>
        /// Compile code from input.
        /// </summary>
        private static void Compile()
        {
            string input = File.ReadAllText(inputFileName);
            ANTLRStringStream inputStream = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            if (printTokens)
            {
                PrintTokens(tokens);
            }

            SugarCppParser parser = new SugarCppParser(tokens);
            AstParserRuleReturnScope<CommonTree, IToken> t = parser.root();
            CommonTree ct = (CommonTree)t.Tree;
            if (printAST)
            {
                PrintAST(ct);
            }

            if (printCode)
            {
                CommonTreeNodeStream nodes = new CommonTreeNodeStream(ct);
                SugarWalker walker = new SugarWalker(nodes);
                Root x = walker.root();
                TargetCpp cpp = new TargetCpp();
                string code = x.Accept(cpp).Render();
                Print(code);
            }
        }

        /// <summary>
        /// Print the abstract syntax tree.
        /// </summary>
        /// <param name="astNode">Node of abstract syntax tree.</param>
        /// <param name="depth">Depth of node from root.</param>
        private static void PrintAST(CommonTree astNode, int depth = 0)
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
        /// <param name="tokens">Tokens</param>
        private static void PrintTokens(CommonTokenStream tokens)
        {
            foreach (var token in tokens.GetTokens())
            {
                Print(token.ToString());
            }
        }

        /// <summary>
        /// Print text to console or output file.
        /// </summary>
        /// <param name="contents"></param>
        private static void Print(string contents)
        {
            if (outputFileName ==  null)
            {
                Console.WriteLine(contents);
            }
            else
            {
                outputFile.WriteLine(contents);
            }
        }

        /// <summary>
        /// Raise a panic and exit program.
        /// </summary>
        /// <param name="reason">Reason for exit.</param>
        private static void Panic(string reason)
        {
            Console.Error.WriteLine(reason);
            Environment.Exit(1);
        }

        /// <summary>
        /// Print help text.
        /// </summary>
        private static void PrintHelp()
        {
            string indent = "    ";
            Console.WriteLine("SugarCpp");
            Console.WriteLine("Version " + "0.0.1");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine(indent + "sugarcpp [filename] [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(indent + "--ast /ast               Print the abstract syntax tree.");
            Console.WriteLine(indent + "--help -h /help /h /?    Print this help text.");
            Console.WriteLine(indent + "--nocode /nocode         Do not print the generated code.");
            Console.WriteLine(indent + "--output -o /output /o [filename]");
            Console.WriteLine(indent + "                         Filename of output. If not specified, output");
            Console.WriteLine(indent + "                         will be printed to standard output.");
            Console.WriteLine(indent + "--token /token           Print the tokens.");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine(indent + "sugarcpp code.sug -o code.cpp");
            Environment.Exit(0);
        }

        static string inputFileName = null;
        static string outputFileName = null;
        static bool printTokens = false;
        static bool printAST = false;
        static bool printCode = true;
        static StreamWriter outputFile = null;
    }
}
