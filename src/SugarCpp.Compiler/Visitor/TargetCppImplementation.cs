using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                if (node is GlobalAlloc && node.Attribute.Exists(x => x.Name == "const") && !node.Attribute.Exists(x => x.Name == "static")) continue;
                if (node.Attribute.Exists(x => x.Name == "extern")) continue;

                if (node is Import || node is GlobalUsing || node is GlobalTypeDef || (node is Enum && node.Attribute.All(x => x.Name != "ToString"))) continue;
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
            Template template = new Template("<list; separator=\"\n\n\">");

            List<Template> list = new List<Template>();

            EnterNameSpace(class_def.Name);
            this.class_stack.Push(class_def);

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

                    if (node is GlobalAlloc && node.Attribute.Exists(x => x.Name == "static") && !node.Attribute.Exists(x => x.Name == "const"))
                    {
                        list.Add(node.Accept(this));
                    }
                }
            }

            this.class_stack.Pop();
            PopNameSpace();

            template.Add("list", list);
            return template;
        }

        public override Template Visit(Enum enum_def)
        {
            if (enum_def.Attribute.Find(x => x.Name == "ToString") != null)
            {
                Attr attr = enum_def.Attribute.Find(x => x.Name == "ToString");

                FuncDef func = new FuncDef();
                func.Type = new IdentType("const char*");
                func.Name = attr.Args.Count() == 0 ? "ToString" : attr.Args.First();
                func.Args.Add(new ExprAlloc(new IdentType("const " + enum_def.Name + "&"), "_t_value", null, AllocType.Declare));
                List<StmtSwitchItem> switch_list = new List<StmtSwitchItem>();
                foreach (var item in enum_def.Values)
                {
                    StmtBlock block = new StmtBlock();
                    block.StmtList.Add(new StmtReturn(new ExprConst("\"" + item + "\"", ConstType.String)));
                    switch_list.Add(new StmtSwitchItem(new List<Expr> { new ExprConst(enum_def.Name + "::" + item, ConstType.Ident) }, block));
                }

                StmtBlock default_block = new StmtBlock();
                {
                    default_block.StmtList.Add(new StmtExpr(new ExprCall(new ExprConst("throw", ConstType.Ident), null, new List<Expr> { new ExprConst("\"Not Found\"", ConstType.String) })));
                }

                StmtSwitch stmt_switch = new StmtSwitch(new ExprConst("_t_value", ConstType.Ident), switch_list, default_block);
                StmtBlock body = new StmtBlock();
                body.StmtList.Add(stmt_switch);
                func.Body = body;
                return func.Accept(this);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override Template Visit(FuncDef func_def)
        {
            string prefix = "";
            if (func_def.Attribute.Find(x => x.Name == "inline") != null)
            {
                prefix += "inline ";
            }
            if (this.class_stack.Count() == 0)
            {
                if (func_def.Attribute.Find(x => x.Name == "static") != null)
                {
                    prefix += "static ";
                }
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
            if (func_def.Name == "this")
                template.Add("name", NameInNameSpace(class_stack.First().Name));
            else if (func_def.Name == "~this")
                template.Add("name", NameInNameSpace("~" + class_stack.First().Name));
            else
                template.Add("name", NameInNameSpace(func_def.Name));

            List<Template> args_list = new List<Template>();
            foreach (var x in func_def.Args)
            {
                ExprAlloc alloc = new ExprAlloc(x.Type, x.Name, null, AllocType.Declare);
                args_list.Add(alloc.Accept(this));
            }
            template.Add("args", args_list);
            template.Add("list", func_def.Body.Accept(this));
            return template;
        }

        public override Template Visit(GlobalAlloc global_alloc)
        {
            Template template = null;

            var type = global_alloc.Type;

            string name_prefix = "";
            string name_suffix = "";
            while (true)
            {
                if (type is StarType)
                {
                    name_prefix = "*" + name_prefix;
                    type = ((StarType)type).Type;
                    continue;
                }
                if (type is RefType)
                {
                    name_prefix = "&" + name_prefix;
                    type = ((RefType)type).Type;
                    continue;
                }
                if (type is ArrayType)
                {
                    Template tmp = new Template("<type_list>");
                    List<Template> type_list = new List<Template>();
                    foreach (var x in ((ArrayType)type).Args)
                    {
                        Template item = new Template("[<expr>]");
                        item.Add("expr", x.Accept(this));
                        type_list.Add(item);
                    }
                    tmp.Add("type_list", type_list);
                    name_suffix = tmp.Render() + name_suffix;
                    type = ((ArrayType)type).Type;
                    continue;
                }
                break;
            }

            string prefix = "";
            if (this.class_stack.Count() == 0)
            {
                if (global_alloc.Attribute.Find(x => x.Name == "static") != null)
                {
                    prefix += "static ";
                }
            }
            if (global_alloc.Attribute.Find(x => x.Name == "const") != null)
            {
                prefix += "const ";
            }

            if (type is AutoType)
            {
                // Todo: Check ExprList.Count()
                Debug.Assert(global_alloc.ExprList.Count() == 1);
                type = new DeclType(global_alloc.ExprList.First());
            }

            // Can declare inline
            if (global_alloc.Style == AllocType.Declare)
            {
                template = new Template("<prefix><type> <name; separator=\", \">;");
                template.Add("prefix", prefix);
                template.Add("type", type.Accept(this));
                template.Add("name", global_alloc.Name.Select(x => string.Format("{0}{1}{2}", name_prefix, NameInNameSpace(x), name_suffix)).ToList());
                return template;
            }

            List<Template> list = new List<Template>();
            foreach (var name in global_alloc.Name)
            {
                switch (global_alloc.Style)
                {
                    case AllocType.Equal:
                        {
                            Template stmt = new Template("<prefix><type> <name> = <expr; separator=\", \">;");
                            stmt.Add("prefix", prefix);
                            stmt.Add("type", type.Accept(this));
                            stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, NameInNameSpace(name), name_suffix));
                            stmt.Add("expr", global_alloc.ExprList.Select(x => x.Accept(this)));
                            list.Add(stmt);
                            break;
                        }

                    case AllocType.Bracket:
                        {
                            Template stmt = new Template("<prefix><type> <name> { <expr; separator=\", \"> };");
                            stmt.Add("prefix", prefix);
                            stmt.Add("type", type.Accept(this));
                            stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, NameInNameSpace(name), name_suffix));
                            stmt.Add("expr", global_alloc.ExprList.Select(x => x.Accept(this)));
                            list.Add(stmt);
                            break;
                        }
                }
            }

            template = new Template("<list; separator=\"\n\">");
            template.Add("list", list);
            return template;
        }
    }
}
