using System;
using System.Collections.Generic;
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
            Arguments arguments = new Arguments(args, new Dictionary<string, bool> {
                {"output", true},
                {"o", true},
                {"token", false},
                {"ast", false},
                {"nocode", false},
                {"help", false},
                {"h", false},
                {"?", false},
            });

            if (arguments.HasOption("help") || arguments.HasOption("h") || arguments.HasOption("?"))
            {
                Program.PrintHelp();
            }

            printTokens = arguments.HasOption("token");
            printAST = arguments.HasOption("ast");
            printCode = !arguments.HasOption("nocode");
            outputFileName = arguments.GetOption("o");
            outputFileName = arguments.GetOption("output");

            if (arguments.DirectArguments.Count == 0)
            {
                Program.Panic("No input file is specified. Use --help for more information.");
            }
            // TODO support multiple input
            inputFileName = arguments.DirectArguments[0];

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
            int dot_pos = inputFileName.LastIndexOf(".");
            string header_name = inputFileName.Substring(0, dot_pos) + ".h";
            string implementation_name = inputFileName.Substring(0, dot_pos) + ".cpp";
            ANTLRStringStream inputStream = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            if (printTokens)
            {
                PrintTokens(tokens);
            }

            SugarCppParser parser = new SugarCppParser(tokens);
            AstParserRuleReturnScope<CommonTree, IToken> t = parser.root();
            if (parser.errors.Count > 0)
            {
                foreach (var error in parser.errors)
                {
                    Console.WriteLine(error);
                }
                return;
            }
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
                TargetCppHeader header = new TargetCppHeader();
                TargetCppImplementation implementation = new TargetCppImplementation();
                string include_name = header_name;
                if (include_name.LastIndexOf('/') != -1) include_name = include_name.Substring(include_name.LastIndexOf('/') + 1);
                if (include_name.LastIndexOf('\\') != -1) include_name = include_name.Substring(include_name.LastIndexOf('\\') + 1);
                implementation.HeaderFileName = include_name;
                string header_code = x.Accept(header).Render();
                string implementation_code = x.Accept(implementation).Render();
                File.WriteAllText(header_name, header_code);
                File.WriteAllText(implementation_name, implementation_code);
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
