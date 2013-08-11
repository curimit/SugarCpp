import
    "TargetCpp.sc"
    "cstring"

public class TargetCppImplementation(fileNoExt: string): TargetCpp
    string NameInNameSpace(name: string)
        if class_stack.size() == 0
            return name
        else
            return class_stack.top()->name + "::" + name

    virtual cAstNode* visit(node: Root*)
        @scope_style = ScopeStyle::GlobalScope
        result := new cBlock()
        stmt := new cInclude(string("\"") + string(basename(fileNoExt.data())) + string(".h\""))
        result->list.push_back(stmt)
        result->list.push_back(node->block->accept(this))
        return result

    virtual cAstNode* visit(node: Block*)
        result := new cBlock()
        for x <- node->list
            // skip template
            let node => dynamic_cast!(Class*)(x), node != nullptr
                if node->genericParameter.size() > 0 then continue
            
            let node => dynamic_cast!(Func*)(x), node != nullptr
                if node->genericParameter.size() > 0 then continue

            result->list.push_back(x->accept(this))
        return result

    virtual cAstNode* visit(node: Class*)
        class_stack.push(node)

        block := node->block->accept(this)

        class_stack.pop()
        return block

    virtual cAstNode* visit(node: Func*)
        @scope_style = ScopeStyle::FormalScope
        func := new cFunc()
        switch node->funcType
            when Func::FuncType::Normal
                func->type = node->type->accept(this)
                func->name = node->name
                func->funcType = cFunc::FuncType::Normal

            when Func::FuncType::Constructor
                func->name = class_stack.top()->name
                func->funcType = cFunc::FuncType::Constructor

            when Func::FuncType::Destructor
                func->name = class_stack.top()->name
                func->funcType = cFunc::FuncType::Destructor

        func->name = NameInNameSpace(func->name)

        func->genericParameter = node->genericParameter
        func->args = node->args->accept(this)
        func->block = node->block->accept(this)
        return func

    virtual cAstNode* visit(node: StmtExpr*) = new cStmtExpr(expr) where
        expr := node->expr->accept(this)
