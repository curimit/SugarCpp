import
    "cAstNode.sc"
    "assert.h"

public class cRender(): cVisitor
    st: StringTemplate

    string result() = @st.result()

    virtual void visit(node: cBlock*)
        for x <- node->list
            x->accept(this)
            @st.newline()

    virtual void visit(node: cBraceBlock*)
        @st.emit("{")
        @st.newline()

        @st.indent()
        node->block->accept(this)
        @st.dedent()
        
        @st.emit("}")        

    virtual void visit(node: cTypeIdent*)
        @st.emit(node->name)

    virtual void visit(node: cTypePrefix*)
        @st.emit(node->prefix)
        @st.emit(" ")
        node->type->accept(this)

    virtual void visit(node: cTypeTemplate*)
        node->type->accept(this)
        @st.emit("<")
        node->args->accept(this)
        @st.emit(">")

    virtual void visit(node: cTypeFunc*)
        @st.emit("std::function<")
        node->type->accept(this)
        @st.emit("(")
        node->args->accept(this)
        @st.emit(")")
        @st.emit(">")        

    virtual void visit(node: cTypeScoping*)
        @st.emit(node->scope)
        @st.emit("::")
        node->type->accept(this)

    virtual void visit(node: cTypeVoid*) = @st.emit("void")

    virtual void visit(node: cTypeAuto*) = @st.emit("auto")

    virtual void visit(node: cTypeSuffix*)
        node->type->accept(this)
        @st.emit(node->suffix)

    virtual void visit(node: cInheritType*)
        @st.emit(node->prefix)
        @st.emit(" ")
        node->type->accept(this)

    virtual void visit(node: cClassFlag*)
        @st.dedent()
        @st.emit(node->name)
        @st.emit(":")
        @st.indent()

    virtual void visit(node: cClass*)
        if node->genericParameter.size() > 0
            @st.emit("template<")
            isFirst := true
            for x <- node->genericParameter
                @st.emit(", ") if not isFirst
                @st.emit("typename ")
                @st.emit(x)
                isFirst = false
            @st.emit(">")
            @st.newline()
        @st.emit("class ")
        @st.emit(node->name)
        if node->inherit.size() > 0
            @st.emit(": ")
            isFirst := true
            for x <- node->inherit
                @st.emit(", ") if not isFirst
                x->accept(this)
                isFirst = false
        @st.emit(" {")
        @st.newline()
        
        @st.indent()
        node->block->accept(this)
        @st.dedent()
        @st.emit("};")

    virtual void visit(node: cEnum*)
        @st.emit("enum ")
        @st.emit(node->name)

        if not node->isDeclare
            @st.emit(" {")
            @st.newline()

            @st.indent()
            for i <- 0 til node->list.size(), x => node->list[i]
                @st.emit(x)
                @st.emit(",") if i != node->list.size() - 1
                @st.newline()
            @st.dedent()

            @st.emit("}")

        @st.emit(";")

    virtual void visit(node: cInclude*)
        @st.emit("#include ")
        @st.emit(node->name)

    virtual void visit(node: cNodeList*)
        isFirst := true
        for x <- node->list
            @st.newline() if not isFirst
            x->accept(this)
            isFirst = false

    virtual void visit(node: cFunc*)
        if node->genericParameter.size() > 0
            @st.emit("template<")
            isFirst := true
            for x <- node->genericParameter
                @st.emit(", ") if not isFirst
                @st.emit("typename ")
                @st.emit(x)
                isFirst = false
            @st.emit(">")
            @st.newline()
        for x <- node->prefix
            @st.emit(x)
            @st.emit(" ")
        switch node->funcType
            when cFunc::Normal
                node->type->accept(this)
                @st.emit(" ")

            when cFunc::Constructor
                /* empty */

            when cFunc::Destructor
                @st.emit("~")

        @st.emit(node->name)
        @st.emit("(")
        node->args->accept(this)
        @st.emit(")")
        if not node->isDeclare
            @st.emit(" {")
            @st.newline()

            @st.indent()
            node->block->accept(this)
            @st.dedent()

            @st.emit("}")
        else
            @st.emit(";")

    virtual void visit(node: cStmtIf*)
        @st.emit("if ")
        @st.emit("(")
        node->expr->accept(this)
        @st.emit(")")

        @st.emit(" {")
        @st.newline()

        @st.indent()
        node->body_block->accept(this)
        @st.dedent()

        @st.emit("}")

        if node->else_block != nil
            @st.emit(" else ")
            @st.emit("{")
            @st.newline()

            @st.indent()
            node->else_block->accept(this)
            @st.dedent()
            @st.emit("}")

    virtual void visit(node: cStmtWhile*)
        @st.emit("while ")
        @st.emit("(")
        node->expr->accept(this)
        @st.emit(")")

        @st.emit(" {")
        @st.newline()

        @st.indent()
        node->block->accept(this)
        @st.dedent()

        @st.emit("}")

    virtual void visit(node: cStmtTry*)
        @st.emit("try {")
        @st.newline()

        @st.indent()
        node->try_block->accept(this)
        @st.dedent()

        @st.emit("}")
        @st.emit(" catch ")
        @st.emit("(")
        node->expr->accept(this)
        @st.emit(")")
        @st.emit(" {")
        @st.newline()

        @st.indent()
        node->catch_block->accept(this)
        @st.dedent()

        @st.emit("}")

    virtual void visit(node: cSwitchItem*)
        @st.emit("case ")
        node->expr->accept(this)
        @st.emit(":")
        @st.newline()
        @st.emit("{")
        @st.newline()

        @st.indent()
        node->block->accept(this)
        @st.dedent()
        @st.emit("}")
        @st.newline()

    virtual void visit(node: cStmtSwitch*)
        @st.emit("switch ")
        @st.emit("(")
        node->expr->accept(this)
        @st.emit(")")
        @st.emit(" {")
        @st.newline()
        
        @st.indent()
        x->accept(this) for x <- node->list
        @st.dedent()
        
        @st.emit("}")

    virtual void visit(node: cForItemNormal*)
        @st.emit("for (")
        node->expr1->accept(this)
        @st.emit("; ")
        node->expr2->accept(this)
        @st.emit("; ")
        node->expr3->accept(this)
        @st.emit(")")

    virtual void visit(node: cForItemIterate*)
        @st.emit("for (auto ")
        @st.emit(node->name)
        @st.emit(" : ")
        node->expr->accept(this)
        @st.emit(")")

    virtual void visit(node: cStmtFor*)
        node->iterate->accept(this)
        @st.emit(" {")
        @st.newline()

        @st.indent()
        node->block->accept(this)
        @st.dedent()

        @st.emit("}")

    virtual void visit(node: cStmtReturn*)
        @st.emit("return ")
        node->expr->accept(this)
        @st.emit(";")

    virtual void visit(node: cStmtUsing*)
        @st.emit("using ")
        if node->style == cStmtUsing::Namespace
            @st.emit("namespace ")
        @st.emit(node->name)
        @st.emit(";")

    virtual void visit(node: cStmtExpr*)
        node->expr->accept(this)
        @st.emit(";")

    virtual void visit(node: cExprBin*)
        node->expr1->accept(this)
        space := node->op != "->" and node->op != "."
        @st.emit(" ") if space
        @st.emit(node->op)
        @st.emit(" ") if space
        node->expr2->accept(this)

    virtual void visit(node: cExprPrefix*)
        @st.emit(node->op)
        node->expr->accept(this)

    virtual void visit(node: cExprSuffix*)
        node->expr->accept(this)
        @st.emit(node->op)

    virtual void visit(node: cExprCall*)
        node->expr->accept(this)
        if node->gp.size() > 0
            @st.emit("<")
            isFirst := true
            for x <- node->gp
                @st.emit(", ") if not isFirst
                x->accept(this)
                isFirst = false
            @st.emit(">")
        @st.emit("(")
        node->args->accept(this)
        @st.emit(")")

    virtual void visit(node: cExprDeclare*)
        if node->isExtern
            @st.emit("extern ")
        node->type->accept(this)
        @st.emit(" ")
        @st.emit(node->name)

    virtual void visit(node: cExprDeclareAssign*)
        if node->isExtern
            @st.emit("extern ")
            assert(false)
        node->type->accept(this)
        @st.emit(" ")
        @st.emit(node->name)

    virtual void visit(node: cExprCast*)
        @st.emit("(")
        node->type->accept(this)
        @st.emit(")")
        node->expr->accept(this)

    virtual void visit(node: cExprDict*)
        node->expr->accept(this)
        @st.emit("[")
        node->args->accept(this)
        @st.emit("]")

    virtual void visit(node: cCommaList*)
        isFirst := true
        for x <- node->list
            @st.emit(", ") if not isFirst
            x->accept(this)
            isFirst = false

    virtual void visit(node: cExprSizeOf*)
        @st.emit("sizeof")
        @st.emit(" ")
        node->expr->accept(this)

    virtual void visit(node: cExprDelete*)
        @st.emit("delete")
        @st.emit(" ")
        node->expr->accept(this)

    virtual void visit(node: cExprNew*)
        @st.emit("new")
        @st.emit(" ")
        node->type->accept(this)
        @st.emit("(")
        node->args->accept(this)
        @st.emit(")")

    virtual void visit(node: cExprCond*)
        node->cond->accept(this)
        @st.emit(" ? ")
        node->expr1->accept(this)
        @st.emit(" : ")
        node->expr2->accept(this)

    virtual void visit(node: cExprConst*)
        @st.emit(node->v)

    virtual void visit(node: cExprLambda*)
        @st.emit("([&](")
        node->args->accept(this)
        @st.emit(") {")
        @st.newline()

        @st.indent()
        node->block->accept(this)
        @st.dedent()

        @st.emit("})")

    virtual void visit(node: cExprBracket*)
        @st.emit("(")
        node->expr->accept(this)
        @st.emit(")")

    virtual void visit(node: cExprBrace*)
        @st.emit("{")
        node->expr->accept(this)
        @st.emit("}")

    virtual void visit(node: cExprBlock*)
        @st.emit("({")
        @st.newline()

        @st.indent()
        for x <- node->list
            x->accept(this)
            @st.newline()
        @st.dedent()

        @st.emit("})")