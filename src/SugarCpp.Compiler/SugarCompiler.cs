using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class TargetCppResult
    {
        public string Header;
        public string Implementation;
    }

    public class SugarCompiler
    {
        public static TargetCppResult Compile(string input, string file_name)
        {
            input = input.Replace("\r", "");
            ANTLRStringStream Input = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(Input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            SugarCppParser parser = new SugarCppParser(tokens);

            AstParserRuleReturnScope<CommonTree, IToken> t = parser.root();
            CommonTree ct = (CommonTree)t.Tree;

            if (parser.errors.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in parser.errors)
                {
                    sb.Append(error);
                    sb.Append("\n");
                }
                throw new Exception(sb.ToString());
            }

            CommonTreeNodeStream nodes = new CommonTreeNodeStream(ct);
            SugarWalker walker = new SugarWalker(nodes);

            Root ast = walker.root();

            TargetCppHeader header = new TargetCppHeader();
            TargetCppImplementation implementation = new TargetCppImplementation();
            implementation.HeaderFileName = string.Format("{0}.h", file_name);

            TargetCppResult result = new TargetCppResult();
            result.Header = ast.Accept(header).Render();
            result.Implementation = ast.Accept(implementation).Render();

            return result;
        }

        public static string Compile(string input)
        {
            input = input.Replace("\r", "");
            ANTLRStringStream Input = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(Input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            SugarCppParser parser = new SugarCppParser(tokens);

            AstParserRuleReturnScope<CommonTree, IToken> t = parser.root();
            CommonTree ct = (CommonTree)t.Tree;

            if (parser.errors.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in parser.errors)
                {
                    sb.Append(error);
                    sb.Append("\n");
                }
                throw new Exception(sb.ToString());
            }

            CommonTreeNodeStream nodes = new CommonTreeNodeStream(ct);
            SugarWalker walker = new SugarWalker(nodes);

            Root ast = walker.root();

            TargetCpp target_cpp = new TargetCpp();

            return ast.Accept(target_cpp).Render();
        }

        public static List<IToken> GetTokens(string input)
        {
            input = input.Replace("\r", "");
            ANTLRStringStream Input = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(Input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            return tokens.GetTokens();
        }

        public static CommonTree GetAst(string input)
        {

            input = input.Replace("\r", "");
            ANTLRStringStream Input = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(Input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            SugarCppParser parser = new SugarCppParser(tokens);

            AstParserRuleReturnScope<CommonTree, IToken> t = parser.root();
            CommonTree ct = (CommonTree)t.Tree;

            return ct;
        }
    }
}
