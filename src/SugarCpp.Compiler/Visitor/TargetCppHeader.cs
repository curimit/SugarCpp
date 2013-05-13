using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class TargetCppHeader : TargetCpp
    {
        private int ClassLevel = 0;

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
                //if ((node is Import || node is GlobalUsing) && !node.Attribute.Exists(x => x.Name == "export")) continue;
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

        public override Template Visit(Class class_def)
        {
            ClassLevel++;
            var template = base.Visit(class_def);
            ClassLevel--;
            return template;
        }

        public override Template Visit(GlobalAlloc global_alloc)
        {
            if (this.ClassLevel > 0)
            {
                return base.Visit(global_alloc);
            }

            string type = global_alloc.Type;
            string name_prefix = "";
            string name_suffix = "";
            while (true)
            {
                if (type.EndsWith("*"))
                {
                    type = type.Substring(0, type.Length - 1);
                    name_prefix = "*" + name_prefix;
                    continue;
                }
                if (type.EndsWith("&"))
                {
                    type = type.Substring(0, type.Length - 1);
                    name_prefix = "&" + name_prefix;
                    continue;
                }
                if (type.EndsWith("[]"))
                {
                    type = type.Substring(0, type.Length - 2);
                    name_suffix = "[]" + name_suffix;
                    continue;
                }
                break;
            }

            string prefix = "";
            if (global_alloc.Attribute.Find(x => x.Name == "static") != null)
            {
                prefix += "static ";
            }

            if (global_alloc.Attribute.Find(x => x.Name == "const") != null)
            {
                prefix += "const ";
            }

            if (global_alloc.ExprList.Count() > 0)
            {
                List<Template> list = new List<Template>();
                foreach (var name in global_alloc.Name)
                {
                    Template stmt = null;
                    if (global_alloc.IsEqualSign)
                    {
                        stmt = new Template("extern <prefix><type> <name>;");
                    }
                    else
                    {
                        stmt = new Template("extern <prefix><type> <name>;");
                    }
                    stmt.Add("prefix", prefix);
                    if (type == "auto")
                    {
                        Template tmp = new Template("decltype(<expr; separator=\", \">)");
                        tmp.Add("expr", global_alloc.ExprList.Select(x => x.Accept(this)));
                        stmt.Add("type", tmp);
                    }
                    else
                    {
                        stmt.Add("type", type);
                    }
                    stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, name, name_suffix));
                    list.Add(stmt);
                }
                Template template = new Template("<list; separator=\"\n\">");
                template.Add("list", list);
                return template;
            }
            else
            {
                Template template = new Template("extern <prefix><type> <name; separator=\", \">;");
                template.Add("prefix", prefix);
                template.Add("type", type);
                template.Add("name", global_alloc.Name.Select(x => string.Format("{0}{1}{2}", name_prefix, x, name_suffix)));
                return template;
            }
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
