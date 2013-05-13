using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class TargetCppHeader : TargetCpp
    {
        public override Template Visit(Root root)
        {
            Template template = new Template("#pragma once\n\n<body>");
            template.Add("body", root.Block.Accept(this));
            return template;
        }

        public override Template Visit(GlobalBlock block)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            bool last = false;
            AstNode last_node = null;
            foreach (var node in block.List)
            {
                if (node is GlobalAlloc || node is FuncDef) continue;
                bool current = node is FuncDef || node is Class || node is Enum || node is Import || node is GlobalUsing || node is Namespace;
                if ((last || current) && !(last_node is Import && node is Import))
                {
                    Template tp = new Template("\n<node>");
                    tp.Add("node", node.Accept(this));
                    list.Add(tp);
                }
                else
                {
                    list.Add(node.Accept(this));
                }
                last = current;
                last_node = node;
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(FuncDef func_def)
        {
            string prefix = "";
            if (func_def.Attribute.Find(x => x.Name == "inline") != null)
            {
                prefix += "inline ";
            }
            if (func_def.Attribute.Find(x => x.Name == "static") != null)
            {
                prefix += "static ";
            }
            string suffix = "";
            if (func_def.Attribute.Find(x => x.Name == "const") != null)
            {
                suffix += " const";
            }

            Template template = null;
            if (func_def.GenericParameter.Count() == 0)
            {
                if (func_def.Type == null)
                {
                    template = new Template("<prefix><name>(<args; separator=\", \">)<suffix>;");
                }
                else
                {
                    template = new Template("<prefix><type> <name>(<args; separator=\", \">)<suffix>;");
                }
            }
            else
            {
                if (func_def.Type == null)
                {
                    template = new Template("template \\<<generics; separator=\", \">>\n<prefix><name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
                else
                {
                    template = new Template("template \\<<generics; separator=\", \">>\n<prefix><type> <name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
                template.Add("generics", func_def.GenericParameter.Select(x => string.Format("typename {0}", x)));
                template.Add("list", func_def.Body.Accept(this));
            }
            template.Add("prefix", prefix);
            template.Add("suffix", suffix);
            template.Add("type", func_def.Type);
            template.Add("name", func_def.Name);
            template.Add("args", func_def.Args.Select(x => x.Accept(this)));
            return template;
        }
    }
}
