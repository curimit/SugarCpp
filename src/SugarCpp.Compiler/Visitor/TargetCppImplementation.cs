using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    public class TargetCppImplementation : TargetCpp
    {
        public string HeaderFileName;

        private string name_space = "";

        private void EnterNameSpace(string name)
        {
            if (name_space == "")
            {
                name_space = name;
            }
            else
            {
                name_space = name_space + "::" + name;
            }
        }

        private void PopNameSpace()
        {
            int k = name_space.IndexOf("::");
            if (k == -1)
            {
                name_space = "";
            }
            else
            {
                name_space = name_space.Substring(0, k);
            }
        }

        private string NameInNameSpace(string name)
        {
            if (name_space == "") return name;
            return name_space + "::" + name;
        }

        public override Template Visit(Root root)
        {
            Template template = new Template("#include \"<header>\"\n\n<body>");
            template.Add("header", this.HeaderFileName);
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
                // Generic Class/Function Should Defined in Header File
                if (node is FuncDef && ((FuncDef)node).GenericParameter.Count() > 0) continue;
                if (node is Class && ((Class)node).GenericParameter.Count() > 0) continue;

                if (node is Import || node is GlobalUsing || node is GlobalTypeDef || node is Enum) continue;
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
            Template template = template = new Template("<list; separator=\"\n\n\">");

            List<Template> list = new List<Template>();

            EnterNameSpace(class_def.Name);

            if (class_def.Args.Count() > 0)
            {
                {
                    FuncDef func = new FuncDef();
                    func.Type = null;
                    func.Name = class_def.Name;
                    func.Args = class_def.Args;
                    func.Body = new StmtBlock();
                    foreach (var item in class_def.Args)
                    {
                        string name = item.Name.First();
                        ExprAssign assign = new ExprAssign(new ExprAccess(new ExprConst("this", ConstType.Ident), "->", name),
                                                           new ExprConst(name, ConstType.Ident));
                        func.Body.StmtList.Add(new StmtExpr(assign));
                    }

                    list.Add(func.Accept(this));
                }
            }

            if (class_def.Attribute.Exists(x => x.Name == "case"))
            {
                FuncDef func = new FuncDef();
                func.Type = new IdentType("const char*");
                func.Name = "GetType";
                func.Args = new List<ExprAlloc>();
                func.Body = new StmtBlock();
                StmtReturn stmt = new StmtReturn(new ExprConst("\"" + class_def.Name + "\"", ConstType.String));
                func.Body.StmtList.Add(stmt);
                list.Add(func.Accept(this));
            }

            if (class_def.Block != null)
            {
                foreach (var node in class_def.Block.List)
                {
                    if (node is FuncDef)
                    {
                        list.Add(node.Accept(this));
                    }
                }
            }

            PopNameSpace();

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
            if (func_def.Attribute.Find(x => x.Name == "virtual") != null)
            {
                prefix += "virtual ";
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
                    template = new Template("<prefix><name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
                else
                {
                    template = new Template("<prefix><type> <name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                    template.Add("type", func_def.Type.Accept(this));
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
                    template.Add("type", func_def.Type.Accept(this));
                }
                template.Add("generics", func_def.GenericParameter.Select(x => string.Format("typename {0}", x)));
            }
            template.Add("prefix", prefix);
            template.Add("suffix", suffix);
            template.Add("name", NameInNameSpace(func_def.Name));
            template.Add("args", func_def.Args.Select(x => x.Accept(this)));
            template.Add("list", func_def.Body.Accept(this));
            return template;
        }
    }
}
