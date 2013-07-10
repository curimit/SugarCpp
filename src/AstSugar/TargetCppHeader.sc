import
    "TargetCpp.sc"

public class TargetCppHeader(fileNoExt: string): TargetCpp
    virtual cAstNode* visit(node: Root*)
        @scope_style = GlobalScope
        return node->block->accept(this)

    virtual cAstNode* visit(node: Block*)
        result := new cBlock()
        for x <- node->list
            result->list.push_back(x->accept(this))
        return result

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

        if node->attribute.count("virtual")
            func->prefix.push_back("virtual")
        func->genericParameter = node->genericParameter
        func->args = node->args->accept(this)
        func->isDeclare = true
        return func

     virtual cAstNode* visit(node: ExprDeclare*) = new cExprDeclare(type, name, isExtern) where
        type := node->type->accept(this)
        name := node->name
        isExtern := @scope_style == GlobalScope

    virtual cAstNode* visit(node: ExprDeclareAssign*) = new cExprDeclare(type, name, isExtern) where
        type := node->type->accept(this)
        name := node->name
        isExtern := @scope_style == GlobalScope

    virtual cAstNode* visit(node: Enum*) = new cEnum(name, list, true) where
        name := node->name
        list := node->list