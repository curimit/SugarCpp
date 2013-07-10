import
    "TargetCpp.sc"

public class TargetCppImplementation(fileNoExt: string): TargetCpp
    string NameInNameSpace(name: string)
        if class_stack.size() == 0
            return name
        else
            return class_stack.top()->name + "::" + name

    virtual cAstNode* visit(node: Root*)
        @scope_style = GlobalScope
        result := new cBlock()
        stmt := new cInclude(string("\"") + fileNoExt + string(".sc\""))
        result->list.push_back(stmt)
        result->list.push_back(node->block->accept(this))
        return result

    virtual cAstNode* visit(node: Block*)
        result := new cBlock()
        for x <- node->list
            result->list.push_back(x->accept(this))
        return result

    virtual cAstNode* visit(node: Class*)
        class_stack.push(node)

        block := node->block->accept(this)

        class_stack.pop()
        return block

    virtual cAstNode* visit(node: Func*)
        @scope_style = FormalScope
        func := new cFunc()
        switch node->funcType
            when Func::Normal
                func->type = node->type->accept(this)
                func->name = node->name
                func->funcType = cFunc::Normal

            when Func::Constructor
                func->name = class_stack.top()->name
                func->funcType = cFunc::Constructor

            when Func::Destructor
                func->name = class_stack.top()->name
                func->funcType = cFunc::Destructor

        func->name = NameInNameSpace(func->name)

        func->genericParameter = node->genericParameter
        func->args = node->args->accept(this)
        func->block = node->block->accept(this)
        return func

    virtual cAstNode* visit(node: StmtExpr*) = new cStmtExpr(expr) where
        expr := node->expr->accept(this)
