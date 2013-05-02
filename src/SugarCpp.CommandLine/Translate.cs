using System;
using System.IO;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using SugarCpp.Compiler;

namespace SugarCpp.CommandLine
{
    class Translate
    {
        internal static void Main(string[] args)
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
                            Program.Panic("No file specified after " + arg);
                        }
                        if (outputFileName != null)
                        {
                            Program.Panic("You can not specify more than one output file");
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
                        Program.PrintHelp();
                        break;
                    default:
                        if (inputFileName != null)
                        {
                            Program.Panic("Multiple input file is not supported");
                        }
                        inputFileName = arg;
                        break;
                }
            }
            if (inputFileName == null)
            {
                Program.Panic("No input file is specified. Use --help for more information.");
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
            if (outputFileName == null)
            {
                Console.WriteLine(contents);
            }
            else
            {
                outputFile.WriteLine(contents);
            }
        }

        static string inputFileName = null;
        static string outputFileName = null;
        static bool printTokens = false;
        static bool printAST = false;
        static bool printCode = true;
        static StreamWriter outputFile = null;
    }
}
