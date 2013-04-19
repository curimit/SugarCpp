using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System.IO;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string input = File.ReadAllText("test.txt");
                ANTLRStringStream Input = new ANTLRStringStream(input);
                SugarCppLexer lexer = new SugarCppLexer(Input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);

                SugarCppParser parser = new SugarCppParser(tokens);

                AstParserRuleReturnScope<CommonTree, IToken> t = parser.root(); //取得语法树
                CommonTree ct = (CommonTree)t.Tree;

                Console.WriteLine("Tokens:");
                print(tokens);

                Console.WriteLine("AST:");
                print(ct);

                CommonTreeNodeStream nodes = new CommonTreeNodeStream(ct);
                SugarWalker walker = new SugarWalker(nodes);

                Root x = walker.root();
                TargetCpp cpp = new TargetCpp();
                Console.WriteLine("Program:");
                Console.WriteLine(x.Accept(cpp).Render());
            }
            catch (System.Exception ex)
            {
                Console.Write("出现错误:");
                Console.WriteLine(ex.Message);
            }
        }

        static void print(CommonTree T, int indent = 0)
        {
            for (int i = 0; i < indent; i++) Console.Write("  ");
            Console.WriteLine(T.Text);
            for (int i = 0; i < T.ChildCount; i++)
            {
                print(T.Children[i] as CommonTree, indent + 1);
            }
        }

        static void print(CommonTokenStream tokens)
        {
            foreach (var token in tokens.GetTokens())
            {
                Console.WriteLine(token);
            }
        }
    }
}
