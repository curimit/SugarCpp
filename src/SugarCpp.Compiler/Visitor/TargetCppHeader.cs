using System.Diagnostics;
using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class TargetCppHeader : TargetCpp
    {
        private int GenericCount = 0;

        public override Template Visit(Root root)
        {
            Template template = new Template("#pragma once\n\n<declare>\n\n<body>");
            string declare = Declare(root.Block).Render();
            if (declare == "")
            {
                template = new Template("#pragma once\n\n<declare><body>");
            }
            template.Add("declare", declare);
            template.Add("body", root.Block.Accept(this));
            return template;
        }

        public Template Declare(GlobalBlock block)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            foreach (var node in block.List)
            {
                if (node is Class)
                {
                    Template tp = new Template("class <node>;");
                    tp.Add("node", ((Class)node).Name);
                    list.Add(tp);
                }
            }
            template.Add("list", list);
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
                if (node is FuncDef && node.Attribute.Exists(x => x.Name == "static")) continue;
                if (node is GlobalAlloc && node.Attribute.Exists(x => x.Name == "static")) continue;
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
            this.class_stack.Push(class_def);
            if (class_def.GenericParameter.Count() > 0) GenericCount++;
            var template = base.Visit(class_def);
            if (class_def.GenericParameter.Count() > 0) GenericCount--;
            this.class_stack.Push(class_def);
            return template;
        }

        public override Template Visit(GlobalAlloc global_alloc)
        {
            if (global_alloc.Attribute.Exists(x => x.Name == "const"))
            {
                return base.Visit(global_alloc);
            }

            if (this.class_stack.Count() > 0)
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
                if (global_alloc.Attribute.Find(x => x.Name == "static") != null)
                {
                    prefix += "static ";
                }
                if (global_alloc.Attribute.Find(x => x.Name == "extern") != null)
                {
                    prefix += "extern ";
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
                if (global_alloc.Style == AllocType.Declare || global_alloc.Attribute.Exists(x => x.Name == "static"))
                {
                    template = new Template("<prefix><type> <name; separator=\", \">;");
                    template.Add("prefix", prefix);
                    template.Add("type", type.Accept(this));
                    template.Add("name", global_alloc.Name.Select(x => string.Format("{0}{1}{2}", name_prefix, x, name_suffix)));
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
                                stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, name, name_suffix));
                                stmt.Add("expr", global_alloc.ExprList.Select(x => x.Accept(this)));
                                list.Add(stmt);
                                break;
                            }

                        case AllocType.Bracket:
                            {
                                Template stmt = new Template("<prefix><type> <name> { <expr; separator=\", \"> };");
                                stmt.Add("prefix", prefix);
                                stmt.Add("type", type.Accept(this));
                                stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, name, name_suffix));
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
                if (global_alloc.Attribute.Find(x => x.Name == "static") != null)
                {
                    prefix += "static ";
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

                // Declare inline
                template = new Template("extern <prefix><type> <name; separator=\", \">;");
                template.Add("prefix", prefix);
                template.Add("type", type.Accept(this));
                template.Add("name", global_alloc.Name.Select(x => string.Format("{0}{1}{2}", name_prefix, x, name_suffix)));
                return template;
            }
        }

        public override Template Visit(FuncDef func_def)
        {
            if (this.GenericCount > 0 || func_def.GenericParameter.Count() > 0)
            {
                return base.Visit(func_def);
            }
            string prefix = "";
            if (func_def.Attribute.Find(x => x.Name == "inline") != null)
            {
                prefix += "inline ";
            }
            if (func_def.Attribute.Find(x => x.Name == "static") != null)
            {
                prefix += "static ";
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
                    template = new Template("<prefix><name>(<args; separator=\", \">)<suffix>;");
                }
                else
                {
                    template = new Template("<prefix><type> <name>(<args; separator=\", \">)<suffix>;");
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
                template.Add("list", func_def.Body.Accept(this));
            }
            template.Add("prefix", prefix);
            template.Add("suffix", suffix);
            if (func_def.Name == "this")
                template.Add("name", class_stack.First().Name);
            else if (func_def.Name == "~this")
                template.Add("name", "~" + class_stack.First().Name);
            else
                template.Add("name", func_def.Name);
            template.Add("args", func_def.Args.Select(x => x.Accept(this)));
            return template;
        }
    }
}
