using System.Collections;
using System.Diagnostics;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{

    public class TargetCpp : Visitor
    {
        private Stack<Template> defer_stack = new Stack<Template>();
        public Stack<Class> class_stack = new Stack<Class>();
        private int stmt_finally_count = 0;

        public override Template Visit(Root root)
        {
            return root.Block.Accept(this);
        }

        public override Template Visit(IdentType type)
        {
            return new Template(type.Type);
        }

        public override Template Visit(DeclType type)
        {
            Template template = new Template("decltype(<expr>)");
            template.Add("expr", type.Expr.Accept(this));
            return template;
        }

        public override Template Visit(AutoType type)
        {
            return new Template("auto");
        }

        public override Template Visit(StarType type)
        {
            Template template = new Template("<type>*");
            template.Add("type", type.Type.Accept(this));
            return template;
        }

        public override Template Visit(RefType type)
        {
            Template template = new Template("<type>&");
            template.Add("type", type.Type.Accept(this));
            return template;
        }

        public override Template Visit(TemplateType type)
        {
            Template template = new Template("<type>\\<<list; separator=\", \">>");
            template.Add("type", type.Type.Accept(this));
            template.Add("list", type.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(FuncType type)
        {
            Template template = new Template("std::function\\<<return_type> (<list_type; separator=\", \">)>");
            if (type.Type != null)
            {
                template.Add("return_type", type.Type.Accept(this));
            }
            else
            {
                template.Add("return_type", "void");
            }
            template.Add("list_type", type.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ArrayType type)
        {
            Template template = new Template("<type><list>");
            template.Add("type", type.Type.Accept(this));
            List<Template> list = new List<Template>();
            foreach (var x in type.Args)
            {
                Template tmp = new Template("[<expr>]");
                tmp.Add("expr", x.Accept(this));
                list.Add(tmp);
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

        public override Template Visit(GlobalUsing global_using)
        {
            Template template = new Template("using <list; separator=\" \">;");
            template.Add("list", global_using.List);
            return template;
        }

        public override Template Visit(GlobalTypeDef global_typedef)
        {
            Template template = new Template("using <name> = <type>;");
            template.Add("type", global_typedef.Type.Accept(this));
            template.Add("name", global_typedef.Name);
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

            // Can declare inline
            if (global_alloc.Style == AllocType.Declare)
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
                            Template stmt = new Template("<prefix><type> <name> { <expr; separator=\", \"> } ;");
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

        public override Template Visit(ExprAlloc expr)
        {
            Template template = null;

            var type = expr.Type;

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

            // Can declare inline
            if (expr.Style == AllocType.Declare)
            {
                template = new Template("<type> <name; separator=\", \">");
                template.Add("type", type.Accept(this));
                template.Add("name", expr.Name.Select(x => string.Format("{0}{1}{2}", name_prefix, x, name_suffix)));
                return template;
            }

            List<Template> list = new List<Template>();
            foreach (var name in expr.Name)
            {
                switch (expr.Style)
                {
                    case AllocType.Equal:
                        {
                            Template stmt = new Template("<type> <name> = <expr; separator=\", \">");
                            stmt.Add("type", type.Accept(this));
                            stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, name, name_suffix));
                            stmt.Add("expr", expr.ExprList.Select(x => x.Accept(this)));
                            list.Add(stmt);
                            break;
                        }

                    case AllocType.Bracket:
                        {
                            Template stmt = new Template("<type> <name> { <expr; separator=\", \"> }");
                            stmt.Add("type", type.Accept(this));
                            stmt.Add("name", string.Format("{0}{1}{2}", name_prefix, name, name_suffix));
                            stmt.Add("expr", expr.ExprList.Select(x => x.Accept(this)));
                            list.Add(stmt);
                            break;
                        }
                }
            }

            template = new Template("<list; separator=\";\n\">");
            template.Add("list", list);
            return template;
        }

        public override Template Visit(Enum enum_def)
        {
            Template template = new Template("enum class <name> {\n    <list; separator=\",\n\">\n};<tostring>");
            template.Add("name", enum_def.Name);
            List<Template> list = new List<Template>();
            bool hasFlagAttribute = enum_def.Attribute.Find(x => x.Name == "FlagAttribute") != null;
            int i = 0;
            foreach (var item in enum_def.Values)
            {
                Template tp = new Template("<node><suffix>");
                tp.Add("node", item);
                if (i == 0 || hasFlagAttribute)
                {
                    tp.Add("suffix", string.Format(" = {0}", i));
                }
                else
                {
                    tp.Add("suffix", "");
                }
                list.Add(tp);
                if (hasFlagAttribute)
                {
                    i = i == 0 ? 1 : i * 2;
                }
                else
                {
                    i = i + 1;
                }
            }
            template.Add("list", list);

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
                Template node = new Template("\n\n<stmt>");
                node.Add("stmt", func.Accept(this));
                template.Add("tostring", node);
            }
            else
            {
                template.Add("tostring", "");
            }

            return template;
        }

        public override Template Visit(Import import)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            foreach (var name in import.NameList)
            {
                Template node = new Template("#include <name>");
                if (name.EndsWith(".sc\""))
                {
                    // Todo: recursive compilation
                    node.Add("name", name.Substring(0, name.Length - 4) + ".h\"");
                }
                else
                {
                    node.Add("name", name);
                }
                list.Add(node);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(StmtUsing stmt_using)
        {
            Template template = new Template("using <list; separator=\" \">");
            template.Add("list", stmt_using.List);
            return template;
        }

        public override Template Visit(StmtTypeDef stmt_typedef)
        {
            Template template = new Template("using <name> = <type>");
            template.Add("type", stmt_typedef.Type.Accept(this));
            template.Add("name", stmt_typedef.Name);
            return template;
        }

        public override Template Visit(StmtSwitchItem stmt_switch_item)
        {
            Template template = new Template("<list; separator=\"\n\">\n    {\n        <block>\n        break;\n    }");
            List<Template> list = new List<Template>();
            foreach (var x in stmt_switch_item.ExprList)
            {
                Template item = new Template("case <expr>:");
                item.Add("expr", x.Accept(this));
                list.Add(item);
            }
            template.Add("list", list);
            template.Add("block", stmt_switch_item.Block.Accept(this));
            return template;
        }

        public override Template Visit(StmtSwitch stmt_switch)
        {
            if (stmt_switch.Expr != null)
            {
                Template template = new Template("switch (<expr>) {\n<list; separator=\"\n\n\">\n}");
                template.Add("expr", stmt_switch.Expr.Accept(this));
                List<Template> list = stmt_switch.List.Select(x => x.Accept(this)).ToList();
                if (stmt_switch.DefalutBlock != null)
                {
                    Template node = new Template("defalult:\n    <block>");
                    node.Add("block", stmt_switch.DefalutBlock.Accept(this));
                    list.Add(node);
                }
                template.Add("list", list);
                return template;
            }
            else
            {
                Template template = new Template("<list; separator=\" \">");
                List<Template> list = new List<Template>();
                int ct = 0;
                foreach (var x in stmt_switch.List)
                {
                    Template node = null;
                    if (ct++ == 0)
                    {
                        node = new Template("if (<expr>) {\n    <block>\n}");
                    }
                    else
                    {
                        node = new Template("else if (<expr>) {\n    <block>\n}");
                    }

                    if (x.ExprList.Count() == 1)
                    {
                        node.Add("expr", x.ExprList.First().Accept(this));
                    }
                    else
                    {
                        Template tmp = new Template("<list; separator=\" && \">");
                        tmp.Add("list", x.ExprList.Select(i => (new ExprBracket(i)).Accept(this)));
                        node.Add("expr", tmp);
                    }
                    node.Add("block", x.Block.Accept(this));
                    list.Add(node);
                }
                if (stmt_switch.DefalutBlock != null)
                {
                    Template node = new Template("else {\n    <block>\n}");
                    node.Add("block", stmt_switch.DefalutBlock.Accept(this));
                    list.Add(node);
                }
                template.Add("list", list);
                return template;
            }
        }

        public override Template Visit(Class class_def)
        {
            this.class_stack.Push(class_def);
            Template template = null;
            if (class_def.GenericParameter.Count() == 0)
            {
                template = new Template("class <name><inherit> {\n<list; separator=\"\n\">\n};");
            }
            else
            {
                template = new Template("template \\<<generics; separator=\", \">>\nclass <name><inherit> {\n<list; separator=\"\n\">\n};");
                template.Add("generics", class_def.GenericParameter.Select(x => string.Format("typename {0}", x.Accept(this).Render())).ToArray());
            }
            template.Add("name", class_def.Name);
            if (class_def.Inherit.Count() > 0)
            {
                Template tmp = new Template(": <inherit; separator=\", \">");
                tmp.Add("inherit", class_def.Inherit.Select(x => string.Format("public {0}", x)));
                template.Add("inherit", tmp);
            }
            else
            {
                template.Add("inherit", "");
            }
            List<Template> list = new List<Template>();

            bool default_public = class_def.Attribute.Find(x => x.Name == "public") != null;

            string last = "private";

            bool last_flag = false;
            AstNode last_node = null;

            // friend class
            foreach (var attr in class_def.Attribute)
            {
                if (attr.Name == "friend")
                {
                    foreach (var name in attr.Args)
                    {
                        Template friend = new Template("    friend class <name>;");
                        friend.Add("name", name);
                        list.Add(friend);
                    }
                }
            }

            // Args
            if (class_def.Args.Count() > 0)
            {
                {
                    Template tmp = new Template("\npublic:\n    <nodes; separator=\"\n\">\n\n    <name>() = default;\n    <constructor>");
                    tmp.Add("name", class_def.Name);
                    List<Template> nodes = new List<Template>();
                    foreach (var item in class_def.Args)
                    {
                        GlobalAlloc alloc = new GlobalAlloc(item.Type, item.Name, null, null, AllocType.Declare);
                        nodes.Add(alloc.Accept(this));
                    }
                    tmp.Add("nodes", nodes);
                    list.Add(tmp);

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

                        tmp.Add("constructor", func.Accept(this));
                    }
                }

                last = "public";
                last_flag = true;
            }

            if (class_def.Attribute.Exists(x => x.Name == "case"))
            {
                Template tmp = null;
                if (last != "public")
                {
                    tmp = new Template("\npublic:\n    <get_type>");
                }
                else
                {
                    tmp = new Template("    <get_type>");
                }

                FuncDef func = new FuncDef();
                func.Type = new IdentType("const char*");
                func.Name = "GetType";
                func.Args = new List<ExprAlloc>();
                func.Body = new StmtBlock();
                func.Attribute.Add(new Attr { Name = "virtual" });
                StmtReturn stmt = new StmtReturn(new ExprConst("\"" + class_def.Name + "\"", ConstType.String));
                func.Body.StmtList.Add(stmt);
                tmp.Add("get_type", func.Accept(this));
                list.Add(tmp);

                last = "public";
                last_flag = true;
            }

            if (class_def.Block != null)
            {
                foreach (var node in class_def.Block.List)
                {
                    bool current = node is FuncDef || node is Class || node is Enum || node is Import || node is GlobalUsing || node is Namespace;
                    string modifier = null;
                    if (!default_public)
                    {
                        modifier = node.Attribute.Find(x => x.Name == "public") != null ? "public" : "private";
                    }
                    else
                    {
                        modifier = node.Attribute.Find(x => x.Name == "private") != null ? "private" : "public";
                    }

                    if (modifier != last)
                    {
                        Template member = new Template("\n<modifier>:\n    <expr>");
                        member.Add("modifier", modifier);
                        member.Add("expr", node.Accept(this));
                        list.Add(member);
                    }
                    else
                    {
                        if ((last_flag || current) && !(last_node is Import && node is Import))
                        {
                            Template member = new Template("\n    <expr>");
                            member.Add("expr", node.Accept(this));
                            list.Add(member);
                        }
                        else
                        {
                            Template member = new Template("    <expr>");
                            member.Add("expr", node.Accept(this));
                            list.Add(member);
                        }

                    }

                    last = modifier;
                    last_flag = current;
                    last_node = node;
                }
            }

            template.Add("list", list);
            this.class_stack.Pop();
            return template;
        }

        public override Template Visit(Namespace namespace_def)
        {
            if (namespace_def.Block == null || namespace_def.Block.List.Count() == 0)
            {
                return new Template(string.Format("namespace {0} {{ }}", namespace_def.Name));
            }
            Template template = new Template("namespace <name> {\n    <block>\n}");
            string name = namespace_def.Name;
            if (name.IndexOf("::") != -1)
            {
                int k = name.IndexOf("::");
                string prefix = name.Substring(0, k);
                string suffix = name.Substring(k + 2, name.Length - k - 2);
                template.Add("name", prefix);
                template.Add("block", Visit(new Namespace(suffix, namespace_def.Block)));
            }
            else
            {
                template.Add("name", namespace_def.Name);
                List<Template> list = new List<Template>();
                template.Add("block", namespace_def.Block.Accept(this));
            }
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
                template.Add("generics", func_def.GenericParameter.Select(x => string.Format("typename {0}", x.Accept(this).Render())).ToArray());
            }
            template.Add("prefix", prefix);
            template.Add("suffix", suffix);
            if (func_def.Name == "this")
                template.Add("name", this.class_stack.First().Name);
            else if (func_def.Name == "~this")
                template.Add("name", "~" + this.class_stack.First().Name);
            else
                template.Add("name", func_def.Name);
            template.Add("args", func_def.Args.Select(x => x.Accept(this)));
            template.Add("list", func_def.Body.Accept(this));
            return template;
        }

        public override Template Visit(StmtBlock block)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();

            int defer_count = 0;
            int scoped_finally_count = 0;
            bool contains_break = false;
            foreach (var node in block.StmtList)
            {
                if (node is StmtDefer)
                {
                    var stmt_defer = (StmtDefer)node;
                    defer_count++;
                    defer_stack.Push(stmt_defer.Stmt.Accept(this));
                    continue;
                }

                contains_break = contains_break || node is StmtReturn;

                if (defer_stack.Count() > 0 && node is StmtReturn)
                {
                    var stmt_return = (StmtReturn)node;
                    if (stmt_return.Expr == null)
                    {
                        foreach (var item in defer_stack)
                        {
                            list.Add(item);
                        }
                        list.Add(node.Accept(this));
                    }
                    else
                    {
                        Template expr = new Template("{ auto defer=<expr>; <list; separator=\" \"> return defer; }");
                        expr.Add("expr", stmt_return.Expr.Accept(this));
                        expr.Add("list", defer_stack.ToList());
                        list.Add(expr);
                    }
                    continue;
                }

                if (node is StmtFinally)
                {
                    var stmt_finally = (StmtFinally)node;
                    string name = string.Format("_t_finally_{0}", stmt_finally_count);
                    stmt_finally_count++;
                    scoped_finally_count++;
                    Template stmt =
                        new Template(
                            "class <name> {\npublic:\n    std::function\\<void()> finally;\n    ~<name>() { finally(); }\n} <name> = { [&]() { <expr> } };");
                    stmt.Add("name", name);
                    stmt.Add("expr", stmt_finally.Stmt.Accept(this));
                    list.Add(stmt);
                    continue;
                }

                if (node is StmtExpr)
                {
                    StmtExpr stmt = (StmtExpr)node;
                    if (stmt.Stmt is ExprConst)
                    {
                        string text = ((ExprConst)stmt.Stmt).Text;
                        if (text == "break" || text == "continue")
                        {
                            contains_break = true;
                            for (int i = 0; i < defer_count; i++)
                            {
                                list.Add(defer_stack.ElementAt(i));
                            }
                        }
                    }
                }

                list.Add(node.Accept(this));
            }

            for (int i = 0; i < defer_count; i++)
            {
                if (contains_break)
                {
                    defer_stack.Pop();
                }
                else
                {
                    list.Add(defer_stack.Pop());
                }
            }

            stmt_finally_count -= scoped_finally_count;

            template.Add("list", list);
            return template;
        }

        public override Template Visit(StmtDefer stmt_defer)
        {
            throw new Exception(string.Format("It's impossible to run this code."));
            return null;
        }

        public override Template Visit(StmtFinally stmt_finally)
        {
            throw new Exception(string.Format("It's impossible to run this code."));
            return null;
        }

        public override Template Visit(StmtIf stmt_if)
        {
            if (stmt_if.Else == null)
            {
                Template template = new Template("if (<cond>) {\n    <body>\n}");
                template.Add("cond", stmt_if.Condition.Accept(this));
                template.Add("body", stmt_if.Body.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("if (<cond>) {\n    <body1>\n} else {\n    <body2>\n}");
                template.Add("cond", stmt_if.Condition.Accept(this));
                template.Add("body1", stmt_if.Body.Accept(this));
                template.Add("body2", stmt_if.Else.Accept(this));
                return template;
            }
        }

        public override Template Visit(StmtWhile stmt_while)
        {
            Template template = new Template("while (<cond>) {\n    <body>\n}");
            template.Add("cond", stmt_while.Condition.Accept(this));
            template.Add("body", stmt_while.Body.Accept(this));
            return template;
        }

        public override Template Visit(StmtTry stmt_try)
        {
            Template template = new Template("try {\n    <body>\n} catch (<stmt>) {\n    <catch>\n}");
            template.Add("body", stmt_try.Body.Accept(this));
            template.Add("stmt", stmt_try.Stmt.Accept(this));
            template.Add("catch", stmt_try.Catch.Accept(this));
            return template;
        }

        public override Template Visit(StmtFor stmt_for)
        {
            Template template = new Template("<body>");
            template.Add("body", stmt_for.Body.Accept(this));
            Stack<ForItem> stack = new Stack<ForItem>();
            stmt_for.List.ForEach(x => stack.Push(x));
            foreach (var item in stack)
            {
                switch (item.Type)
                {
                    case ForItemType.Each:
                        {
                            var node = (ForItemEach)item;
                            Template tmp = new Template("for (auto <var> : <expr>) {\n    <body>\n}");
                            tmp.Add("var", node.Var);
                            tmp.Add("expr", node.Expr.Accept(this));
                            tmp.Add("body", template);
                            template = tmp;
                            break;
                        }

                    case ForItemType.Map:
                        {
                            var node = (ForItemMap)item;
                            Template tmp = new Template("{\n    auto _t_iterator = <expr>;\n    auto <var> = _t_iterator;\n    <body>\n}");
                            tmp.Add("var", node.Var);
                            tmp.Add("expr", node.Expr.Accept(this));
                            tmp.Add("body", template);
                            template = tmp;
                            break;
                        }

                    case ForItemType.To:
                        {
                            var node = (ForItemRange)item;
                            if (node.By == null)
                            {
                                Template tmp = new Template("for (auto <var> = <from>; <var> <op> <to>; <prefix_op><var>) {\n    <body>\n}");
                                tmp.Add("var", node.Var);
                                tmp.Add("from", node.From.Accept(this));
                                tmp.Add("to", node.To.Accept(this));
                                tmp.Add("body", template);

                                switch (node.Style)
                                {
                                    case ForItemRangeType.To:
                                        {
                                            tmp.Add("prefix_op", "++");
                                            tmp.Add("op", "<=");
                                            break;
                                        }

                                    case ForItemRangeType.DownTo:
                                        {
                                            tmp.Add("prefix_op", "--");
                                            tmp.Add("op", ">=");
                                            break;
                                        }

                                    case ForItemRangeType.Til:
                                        {
                                            tmp.Add("prefix_op", "++");
                                            tmp.Add("op", "!=");
                                            break;
                                        }
                                }
                                template = tmp;
                            }
                            else
                            {
                                Template tmp = new Template("for (auto <var> = <from>; <var> <op> <to>; <var> = <var> + <by>) {\n    <body>\n}");
                                tmp.Add("var", node.Var);
                                tmp.Add("from", node.From.Accept(this));
                                tmp.Add("to", node.To.Accept(this));
                                tmp.Add("by", node.By.Accept(this));
                                tmp.Add("body", template);

                                switch (node.Style)
                                {
                                    case ForItemRangeType.To:
                                        tmp.Add("op", "<=");
                                        break;

                                    case ForItemRangeType.DownTo:
                                        tmp.Add("op", ">=");
                                        break;

                                    case ForItemRangeType.Til:
                                        tmp.Add("op", "!=");
                                        break;
                                }
                                template = tmp;
                            }
                            break;
                        }

                    case ForItemType.When:
                        {
                            var node = (ForItemWhen)item;
                            Template tmp = new Template("if (<expr>) {\n    <body>\n}");
                            tmp.Add("expr", node.Expr.Accept(this));
                            tmp.Add("body", template);
                            template = tmp;
                            break;
                        }
                }
            }
            return template;
        }

        public override Template Visit(StmtForEach stmt_for_each)
        {
            if (stmt_for_each.Var is ExprConst)
            {
                ExprConst expr = (ExprConst)stmt_for_each.Var;
                Template template = new Template("for (auto <var> : <expr>) {\n    <body>\n}");
                template.Add("var", expr.Text);
                template.Add("expr", stmt_for_each.Target.Accept(this));
                template.Add("body", stmt_for_each.Body.Accept(this));
                return template;
            }
            else if (stmt_for_each.Var is ExprCall)
            {
                ExprCall expr = (ExprCall)stmt_for_each.Var;
                List<Stmt> stmt_list = new List<Stmt>();
                List<Expr> condition_list = new List<Expr>();
                int i = 0;
                foreach (var argument in expr.Args)
                {
                    ExprCall get = new ExprCall(new ExprConst("std::get", ConstType.Ident), new List<SugarType> { new IdentType(i.ToString()) },
                                                new List<Expr> { new ExprConst("_t_match", ConstType.Ident) });
                    i++;
                    if (argument is ExprConst && ((ExprConst)argument).Type == ConstType.Ident && !((ExprConst)argument).Text.StartsWith("@"))
                    {
                        ExprConst const_expr = (ExprConst)argument;
                        if (const_expr.Text == "_")
                        {
                            continue;
                        }
                        stmt_list.Add(new StmtExpr(new ExprAlloc(new AutoType(), const_expr.Text, get, AllocType.Equal)));
                    }
                    else
                    {
                        if (((ExprConst)argument).Text.StartsWith("@"))
                        {
                            ((ExprConst)argument).Text = ((ExprConst)argument).Text.Substring(1);
                        }
                        condition_list.Add(new ExprBin("==", get, argument));
                    }
                }
                StmtBlock block = new StmtBlock();
                foreach (var item in stmt_list)
                {
                    block.StmtList.Add(item);
                }
                foreach (var item in stmt_for_each.Body.StmtList)
                {
                    block.StmtList.Add(item);
                }
                if (condition_list.Count() > 0)
                {
                    StmtBlock if_body = new StmtBlock();
                    if_body.StmtList.Add(new StmtExpr(new ExprAlloc(new IdentType("auto&&"), "_t_match", new ExprCall(new ExprAccess(new ExprConst("_t_iterator", ConstType.Ident), ".", "Unapply"), null, null), AllocType.Equal)));
                    Expr condition = null;
                    foreach (var item in condition_list)
                    {
                        if (condition == null)
                        {
                            condition = item;
                            if (condition_list.Count() > 1)
                            {
                                condition = new ExprBracket(condition);
                            }
                        }
                        else
                        {
                            condition = new ExprBin("&&", condition, new ExprBracket(item));
                        }
                    }
                    StmtIf stmt_if = new StmtIf(condition, block, null);
                    if_body.StmtList.Add(stmt_if);
                    block = if_body;
                }
                else
                {
                    block.StmtList.Insert(0, new StmtExpr(new ExprAlloc(new IdentType("auto&&"), "_t_match", new ExprCall(new ExprAccess(new ExprConst("_t_iterator", ConstType.Ident), ".", "Unapply"), null, null), AllocType.Equal)));
                }
                StmtForEach for_each = new StmtForEach(new ExprConst("_t_iterator", ConstType.Ident), stmt_for_each.Target, block);
                return for_each.Accept(this);
            }
            else if (stmt_for_each.Var is ExprTuple)
            {
                ExprTuple expr = (ExprTuple)stmt_for_each.Var;
                List<Stmt> stmt_list = new List<Stmt>();
                List<Expr> condition_list = new List<Expr>();
                int i = 0;
                foreach (var argument in expr.ExprList)
                {
                    ExprCall get = new ExprCall(new ExprConst("get", ConstType.Ident), new List<SugarType> { new IdentType(i.ToString()) },
                                                new List<Expr> { new ExprConst("_t_match", ConstType.Ident) });
                    i++;
                    if (argument is ExprConst && ((ExprConst)argument).Type == ConstType.Ident)
                    {
                        ExprConst const_expr = (ExprConst)argument;
                        if (const_expr.Text == "_")
                        {
                            continue;
                        }
                        stmt_list.Add(new StmtExpr(new ExprAlloc(new IdentType("auto&&"), const_expr.Text, get, AllocType.Equal)));
                    }
                    else
                    {
                        condition_list.Add(new ExprBin("==", get, argument));
                    }
                }
                StmtBlock block = new StmtBlock();
                foreach (var item in stmt_list)
                {
                    block.StmtList.Add(item);
                }
                foreach (var item in stmt_for_each.Body.StmtList)
                {
                    block.StmtList.Add(item);
                }
                if (condition_list.Count() > 0)
                {
                    StmtBlock if_body = new StmtBlock();
                    Expr condition = null;
                    foreach (var item in condition_list)
                    {
                        if (condition == null)
                        {
                            condition = item;
                            if (condition_list.Count() > 1)
                            {
                                condition = new ExprBracket(condition);
                            }
                        }
                        else
                        {
                            condition = new ExprBin("&&", condition, new ExprBracket(item));
                        }
                    }
                    StmtIf stmt_if = new StmtIf(condition, block, null);
                    if_body.StmtList.Add(stmt_if);
                    block = if_body;
                }
                StmtForEach for_each = new StmtForEach(new ExprConst("_t_match", ConstType.Ident), stmt_for_each.Target, block);
                return for_each.Accept(this);
            }
            else
            {
                throw new Exception(string.Format("Iterators in foreach must be either variable or pattern matching"));
            }
        }

        public override Template Visit(StmtExpr stmt)
        {
            Template template = new Template("<stmt>;");
            template.Add("stmt", stmt.Stmt.Accept(this));
            return template;
        }

        public override Template Visit(ExprBracket expr)
        {
            Template template = new Template("(<expr>)");
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprCast expr)
        {
            Template template = new Template("((<type>)<expr>)");
            template.Add("type", expr.Type.Accept(this));
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprList expr)
        {
            Template template = new Template("{ <list; separator=\", \"> }");
            template.Add("list", expr.List.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprListGeneration expr)
        {
            Template template = new Template("({\n    <declare>;\n    <for>\n    <var>;\n})");
            ExprAlloc delcare = new ExprAlloc(expr.Type, "_t_return_value", null, AllocType.Declare);
            template.Add("declare", delcare.Accept(this));
            template.Add("var", "_t_return_value");

            Stmt stmt = null;
            if (expr.Type is TemplateType && (((TemplateType)expr.Type).Type is IdentType))
            {
                string type = ((IdentType)((TemplateType)expr.Type).Type).Type;
                if (type == "vector" || type == "list")
                {
                    stmt = new StmtExpr(new ExprCall(new ExprAccess(new ExprConst("_t_return_value", ConstType.Ident), ".", "push_back"), null, new List<Expr> { expr.Expr }));
                }
                if (type == "forward_list" || type == "deque")
                {
                    stmt = new StmtExpr(new ExprCall(new ExprAccess(new ExprConst("_t_return_value", ConstType.Ident), ".", "push_front"), null, new List<Expr> { expr.Expr }));
                }
                if (type == "queue" || type == "priority_queue" || type == "stack")
                {
                    stmt = new StmtExpr(new ExprCall(new ExprAccess(new ExprConst("_t_return_value", ConstType.Ident), ".", "push"), null, new List<Expr> { expr.Expr }));
                }
                if (type == "set" || type == "multiset" || type == "unordered_set" || type == "unordered_multiset" || type == "map" || type == "multimap" || type == "unordered_map" || type == "unordered_multimap")
                {
                    stmt = new StmtExpr(new ExprCall(new ExprAccess(new ExprConst("_t_return_value", ConstType.Ident), ".", "insert"), null, new List<Expr> { expr.Expr }));
                }
            }
            if (stmt == null)
            {
                string msg = string.Format("Type {0} is not supported in list generation.", expr.Type.Accept(this).Render());
                throw new Exception(msg);
            }

            expr.For.Body = new StmtBlock();
            expr.For.Body.StmtList.Add(stmt);
            template.Add("for", expr.For.Accept(this));

            return template;
        }

        public override Template Visit(MatchTuple match)
        {
            Template template = new Template("std::tie(<list; separator=\", \">)");
            template.Add("list", match.ExprList.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprAssign expr)
        {
            Template template = new Template("<left> = <right>");
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            return template;
        }

        public override Template Visit(ExprLambda expr)
        {
            Template template = new Template("([<ref>](<args; separator=\",\">)<type> {\n    <block>\n})");
            template.Add("args", expr.Args.Select(x => x.Accept(this)));
            template.Add("block", expr.Block.Accept(this));
            template.Add("ref", expr.IsRef ? "&" : "=");
            if (expr.Type == null)
            {
                template.Add("type", "");
            }
            else
            {
                Template type = new Template(" -> <type>");
                type.Add("type", expr.Type.Accept(this));
                template.Add("type", type);
            }
            return template;
        }

        public override Template Visit(ExprCurryLambda expr)
        {
            if (expr.Args.Count() <= 1)
            {
                return (new ExprLambda(expr.Block, expr.Args, expr.IsRef, expr.Type)).Accept(this);
            }

            Stack<ExprAlloc> stack = new Stack<ExprAlloc>();
            foreach (var x in expr.Args) stack.Push(x);

            Stack<string> capture_args = new Stack<string>();
            foreach (var x in expr.Args) capture_args.Push(x.Name.First());

            Template template = null;
            foreach (var x in stack)
            {
                Template node = null;
                if (template == null)
                {
                    node = new Template("([<ref><capture>](<arg>)<type> {\n    <block>\n})");
                    if (expr.Type == null)
                    {
                        node.Add("type", "");
                    }
                    else
                    {
                        Template type = new Template(" -> <type>");
                        type.Add("type", expr.Type.Accept(this));
                        node.Add("type", type);
                    }
                    node.Add("block", expr.Block.Accept(this));
                }
                else
                {
                    node = new Template("([<ref><capture>](<arg>) {\n    return <expr>;\n})");
                    node.Add("expr", template);
                }
                node.Add("ref", expr.IsRef ? "&" : "=");
                node.Add("arg", x.Accept(this));

                capture_args.Pop();
                string capture_by_value = "";
                foreach (var name in capture_args)
                {
                    capture_by_value = capture_by_value + ", " + name;
                }
                if (!expr.IsRef) capture_by_value = "";
                node.Add("capture", capture_by_value);
                template = node;
            }
            return template;
        }

        public override Template Visit(ExprTuple expr)
        {
            Template template = new Template("std::make_tuple(<list; separator=\", \">)");
            template.Add("list", expr.ExprList.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprCall expr)
        {
            if (expr.GenericParameter.Count() == 0)
            {
                Template template = new Template("<expr>(<args; separator=\", \">)");
                template.Add("expr", expr.Expr.Accept(this));
                template.Add("args", expr.Args.Select(x => x.Accept(this)));
                return template;
            }
            else
            {
                Template template = new Template("<expr>\\<<generics; separator=\", \">>(<args; separator=\", \">)");
                template.Add("expr", expr.Expr.Accept(this));
                template.Add("generics", expr.GenericParameter.Select(x => x.Accept(this)).ToArray());
                template.Add("args", expr.Args.Select(x => x.Accept(this)).ToArray());
                return template;
            }
        }

        public override Template Visit(ExprCond expr)
        {
            Template template = new Template("<cond> ? <expr1> : <expr2>");
            template.Add("cond", expr.Cond.Accept(this));
            template.Add("expr1", expr.Expr1.Accept(this));
            template.Add("expr2", expr.Expr2.Accept(this));
            return template;
        }

        public override Template Visit(StmtReturn expr)
        {
            if (expr.Expr != null)
            {
                Template template = new Template("return <expr>;");
                template.Add("expr", expr.Expr.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("return;");
                return template;
            }
        }

        public override Template Visit(ExprDict expr)
        {
            Template template = new Template("<expr><list>");
            template.Add("expr", expr.Expr.Accept(this));
            List<Template> list = new List<Template>();
            foreach (var index in expr.Index)
            {
                Template item = new Template("[<index>]");
                item.Add("index", index.Accept(this));
                list.Add(item);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(ExprNewType expr)
        {
            Template template = new Template("new <elem>(<args; separator=\", \">)");
            template.Add("elem", expr.ElemType.Accept(this));
            template.Add("args", expr.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprNewArray expr)
        {
            Template template = new Template("new <elem>[<list; separator=\", \">]");
            template.Add("elem", expr.ElemType.Accept(this));
            template.Add("list", expr.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprAccess expr)
        {
            Template template = new Template("<expr><op><name>");
            template.Add("expr", expr.Expr.Accept(this));
            template.Add("op", expr.Op);
            template.Add("name", expr.Name);
            return template;
        }

        public override Template Visit(ExprPrefix expr)
        {
            Template template = new Template("<op><expr>");
            template.Add("op", expr.Op);
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprSuffix expr)
        {
            Template template = new Template("<expr><op>");
            template.Add("op", expr.Op);
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprBin expr)
        {
            Template template = new Template("<left> <op> <right>");
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            template.Add("op", expr.Op);
            return template;
        }

        public override Template Visit(ExprInfix expr)
        {
            Template template = new Template("<func>(<left>, <right>)");
            template.Add("func", expr.Func);
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            return template;
        }

        public override Template Visit(ExprWhere expr)
        {
            Template template = new Template("({\n    <list; separator=\"\n\">\n})");
            List<Template> list = new List<Template>();
            foreach (var x in expr.StmtList) list.Add(x.Accept(this));
            list.Add((new StmtExpr(expr.Expr)).Accept(this));
            template.Add("list", list);
            return template;
        }

        public override Template Visit(ExprConst expr)
        {
            Template template = new Template("<expr>");
            template.Add("expr", expr.Text);
            return template;
        }

        public override Template Visit(ExprBlock block)
        {
            Template template = new Template("({\n    <list; separator=\"\n\">})");
            List<Template> list = new List<Template>();
            foreach (var node in block.StmtList)
            {
                Template expr = new Template("<expr>;");
                expr.Add("expr", node.Accept(this));
                list.Add(expr);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(ExprMatch expr)
        {
            Template template = new Template("([&]()<type>{\n    <list; separator=\" \">\n})()");
            if (expr.Type != null)
            {
                Template type = new Template(" -> <type> ");
                type.Add("type", expr.Type.Accept(this));
                template.Add("type", type);
            }
            else
            {
                template.Add("type", "");
            }
            List<Template> list = new List<Template>();

            Template match_expr = expr.Expr == null ? null : expr.Expr.Accept(this);
            bool isFirst = true;
            foreach (var x in expr.List)
            {
                if (x.Condition is ExprConst)
                {
                    ExprConst expr_const = (ExprConst)x.Condition;
                    if (expr_const.Type == ConstType.Ident && expr_const.Text == "_")
                    {
                        continue;
                    }
                }

                Template node = null;
                if (isFirst)
                {
                    node = new Template("if (<condition>) {\n    return <expr>;\n}");
                    isFirst = false;
                }
                else
                {
                    node = new Template("else if (<condition>) {\n    return <expr>;\n}");
                }

                if (match_expr == null)
                {
                    node.Add("condition", x.Condition.Accept(this));
                }
                else
                {
                    Template condition = new Template("<expr> == (<case>)");
                    condition.Add("expr", match_expr);
                    condition.Add("case", x.Condition.Accept(this));
                    node.Add("condition", condition);
                }
                node.Add("expr", x.Expr.Accept(this));
                list.Add(node);
            }

            foreach (var x in expr.List)
            {
                if (x.Condition is ExprConst)
                {
                    ExprConst expr_const = (ExprConst)x.Condition;
                    if (expr_const.Type == ConstType.Ident && expr_const.Text == "_")
                    {
                        var node = new Template("else {\n    return <expr>;\n}");
                        node.Add("expr", x.Expr.Accept(this));
                        list.Add(node);
                    }
                }
            }

            template.Add("list", list);
            return template;
        }
    }
}
