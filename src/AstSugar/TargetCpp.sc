import
    "AstNode.sc"

public class TargetCpp(): Visitor
    enum ScopeStyle = GlobalScope | ClassScope | FormalScope

    class_stack: stack<Class*>
    for_block: cAstNode*
    scope_style: ScopeStyle

    virtual cAstNode* visit(node: Root*)
        @scope_style = GlobalScope
        return node->block->accept(this)

    virtual cAstNode* visit(node: Block*)
        result := new cBlock()
        for x <- node->list
            result->list.push_back(x->accept(this))
        return result

    virtual cAstNode* visit(node: Import*) = new cNodeList(list) where
        list := [new cInclude(x) for x <- node->list] : vector<cAstNode*>

    virtual cAstNode* visit(node: Class*)
        @scope_style = ClassScope
        class_stack.push(node)

        astNode := new cClass()
        astNode->name = node->name
        astNode->genericParameter = node->genericParameter
        astNode->inherit = [new cInheritType("public", x->accept(this)) for x <- node->inherit->list] : vector<cAstNode*>
        block := new cBlock()
        // true:  public
        // false: private
        default_flag := match
            | node->attribute.count("public")  => true
            | node->attribute.count("private") => false
            | _ => false
        last_flag := false
        if node->args.size() > 0
            for x <- node->args
                if last_flag == false
                    last_flag = true
                    block->list.push_back(new cClassFlag("public"))
                block->list.push_back(new cStmtExpr(new cExprDeclare(x->type->accept(this), x->name)))
            // constructor
            func_args := new cCommaList()
            func_body := new cBlock()
            for x <- node->args
                func_args->list.push_back(new cExprDeclare(x->type->accept(this), x->name))
                func_body->list.push_back(new cStmtExpr(new cExprBin("=", new cExprBin("->", new cExprConst("this"), new cExprConst(x->name)), new cExprConst(x->name))))
            func := new cFunc()
            func->name = node->name
            func->args = func_args
            func->block = func_body
            func->funcType = cFunc::Constructor
            block->list.push_back(func)
        for x <- node->block->list
            current_flag := match
                | x->attribute.count("public")  => true
                | x->attribute.count("private") => false
                | _ => default_flag
            if current_flag != last_flag
                last_flag = current_flag
                block->list.push_back(new cClassFlag(current_flag?"public":"private"))
            block->list.push_back(x->accept(this))
        astNode->block = block

        class_stack.pop()
        return astNode

    virtual cAstNode* visit(node: Enum*) = new cEnum(name, list) where
        name := node->name
        list := node->list

    virtual cAstNode* visit(node: TypeIdent*) = new cTypeIdent(name) where
        name := node->name

    virtual cAstNode* visit(node: TypeTemplate*) = new cTypeTemplate(type, args) where
        type := node->type->accept(this)
        args := node->args->accept(this)

    virtual cAstNode* visit(node: TypeList*) = new cCommaList(list) where
        list := [x->accept(this) for x <- node->list] : vector<cAstNode*>

    virtual cAstNode* visit(node: TypePrefix*) = new cTypePrefix(prefix, type) where
        prefix := node->prefix
        type := node->type->accept(this)

    virtual cAstNode* visit(node: TypeSuffix*) = new cTypeSuffix(suffix, type) where
        suffix := node->suffix
        type := node->type->accept(this)

    virtual cAstNode* visit(node: TypeFunc*) = new cTypeFunc(args, type) where
        args := node->args->accept(this)
        type := node->type->accept(this)

    virtual cAstNode* visit(node: TypeScoping*) = new cTypeScoping(scope, type) where
        scope := node->scope
        type := node->type->accept(this)

    virtual cAstNode* visit(node: TypeVoid*) = new cTypeVoid()

    virtual cAstNode* visit(node: TypeAuto*) = new cTypeAuto()

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
        func->block = node->block->accept(this)
        return func

    virtual cAstNode* visit(node: StmtIf*) = new cStmtIf(expr, body_block, else_block) where
        expr := node->expr->accept(this)
        body_block := node->body_block->accept(this)
        else_block := node->else_block == nil ? nil : node->else_block->accept(this)

    virtual cAstNode* visit(node: StmtWhile*) = new cStmtWhile(expr, block) where
        expr := node->expr->accept(this)
        block := node->block->accept(this)

    virtual cAstNode* visit(node: StmtTry*) = new cStmtTry(expr, try_block, catch_block) where
        expr := node->expr->accept(this)
        try_block := node->try_block->accept(this)
        catch_block := node->catch_block->accept(this)

    virtual cAstNode* visit(node: SwitchItem*) = new cSwitchItem(expr, block) where
        expr := node->expr->accept(this)
        block := node->block->accept(this)

    virtual cAstNode* visit(node: StmtSwitch*) = new cStmtSwitch(expr, list) where
        expr := node->expr->accept(this)
        list := [x->accept(this) for x <- node->list] : vector<cAstNode*>

    virtual cAstNode* visit(node: ForItemEach*) = new cBlock(stmt) where
        stmt := new cStmtFor(iterate, block) where
            iterate := new cForItemIterate(name, expr) where
                name := node->name
                expr := node->expr->accept(this)
            block := @for_block

    virtual cAstNode* visit(node: ForItemCond*) = new cBlock(stmt) where
        stmt := new cStmtIf(expr, block, nil) where
            expr := node->expr->accept(this)
            block := @for_block

    virtual cAstNode* visit(node: ForItemMap*) = new cBraceBlock(block) where
        stmt := new cStmtExpr(new cExprDeclareAssign(type, name, expr)) where
            type := new cTypeAuto()
            name := node->name
            expr := node->expr->accept(this)
        block := new cBlock()
        block->list.push_back(stmt)
        block->list.push_back(@for_block)

    virtual cAstNode* visit(node: ForItemRange*) = new cBlock(stmt) where
        stmt := new cStmtFor(iterate, block) where
            iterate := new cForItemNormal(expr1, expr2, expr3) where
                expr1 := new cExprDeclareAssign(type, name, expr) where
                    type := new cTypeAuto()
                    name := node->name
                    expr := node->expr1->accept(this)
                expr2 := new cExprBin(op, expr1, expr2) where
                    op := match node->type
                        | ForItemRange::To      => "<="
                        | ForItemRange::Til     => "<"
                        | ForItemRange::Down_To => ">="
                    expr1 := new cExprConst(node->name)
                    expr2 := node->expr2->accept(this)
                expr3 := match returns cAstNode*
                    | node->expr3 == nil => new cExprPrefix(op, expr) where
                                                op := match node->type
                                                    | ForItemRange::To      => "++"
                                                    | ForItemRange::Til     => "++"
                                                    | ForItemRange::Down_To => "--"
                                                expr := new cExprConst(node->name)
                    | _ => new cExprBin("+=", expr1, expr2) where
                            expr1 := new cExprConst(node->name)
                            expr2 := node->expr3->accept(this)
            block := @for_block

    virtual cAstNode* visit(node: StmtFor*)
        @for_block = node->block->accept(this)
        // keng die
        for i <- int(node->list.size()) - 1 downto 0, x => node->list[i]
            @for_block = x->accept(this)
        return @for_block

    virtual cAstNode* visit(node: StmtUsing*) = new cStmtUsing(name, style) where
        name := node->name
        style := match node->style
            | StmtUsing::Symbol    => cStmtUsing::Symbol
            | StmtUsing::Namespace => cStmtUsing::Namespace

    virtual cAstNode* visit(node: StmtReturn*) = new cStmtReturn(expr) where
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: StmtMatch*)
        block := new cBlock()
        expr := node->expr->accept(this)
        for x <- node->list
            stmt := new cStmtExpr(new cExprDeclareAssign(new cTypeAuto(), x->name, new cExprBin(".", new cExprBracket(expr), new cExprConst(x->field))))
            block->list.push_back(stmt)
        return block

    virtual cAstNode* visit(node: StmtExpr*) = new cStmtExpr(expr) where
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprConst*) = new cExprConst(value) where
        value := node->value

    virtual cAstNode* visit(node: ExprPrefix*) = new cExprPrefix(op, expr) where
        op := node->op
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprSuffix*) = new cExprSuffix(op, expr) where
        op := node->op
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprBin*) = new cExprBin(op, expr1, expr2) where
        op := node->op
        expr1 := node->expr1->accept(this)
        expr2 := node->expr2->accept(this)

    virtual cAstNode* visit(node: ExprChain*)
        cout << "Assert faild!" << endl if node->list.size() > 1
        expr: cAstNode*
        for i <- 1 til node->list.size(), expr1 => node->list[i-1].expr->accept(this), expr2 => node->list[i].expr->accept(this), op => node->list[i].op
            if i == 1
                expr = new cExprBin(op, expr1, expr2)
                expr = new cExprBracket(expr) if node->list.size() > 2
            else
                expr = new cExprBin("&&", expr, new cExprBracket(new cExprBin(op, expr1, expr2)))
        return expr

    virtual cAstNode* visit(node: ExprCall*) = new cExprCall(expr, args, gp) where
        expr := node->expr->accept(this)
        args := node->args->accept(this)
        gp := [x->accept(this) for x <- node->gp]: vector<cAstNode*>

    virtual cAstNode* visit(node: ExprDict*) = new cExprDict(expr, args) where
        expr := node->expr->accept(this)
        args := node->args->accept(this)

    virtual cAstNode* visit(node: ExprInfix*) = new cExprCall(expr, args, gp) where
        expr := new cExprConst(node->name)
        args := new cCommaList()
        args->list.push_back(node->expr1->accept(this))
        args->list.push_back(node->expr2->accept(this))
        gp: vector<cAstNode*>

    virtual cAstNode* visit(node: ExprCond*) = new cExprCond(cond, expr1, expr2) where
        cond := node->cond->accept(this)
        expr1 := node->expr1->accept(this)
        expr2 := node->expr2->accept(this)

    virtual cAstNode* visit(node: ExprDeclare*) = new cExprDeclare(type, name) where
        type := node->type->accept(this)
        name := node->name

    virtual cAstNode* visit(node: ExprDeclareAssign*) = new cExprDeclareAssign(type, name, expr) where
        type := node->type->accept(this)
        name := node->name
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprCast*) = new cExprCast(type, expr) where
        type := node->type->accept(this)
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprSizeOf*) = new cExprSizeOf(expr) where
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprDelete*) = new cExprDelete(expr) where
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprNew*) = new cExprNew(type, args) where
        type := node->type->accept(this)
        args := node->args->accept(this)

    virtual cAstNode* visit(node: ExprBracket*) = new cExprBracket(expr) where
        expr := node->expr->accept(this)

    virtual cAstNode* visit(node: ExprLambda*) = new cExprLambda(args, block) where
        args := node->args->accept(this)
        block := new cBlock()
        block->list.push_back(new cStmtReturn(node->expr->accept(this)))

    virtual cAstNode* visit(node: ExprWhere*)
        block := new cExprBlock()
        block->list.push_back(node->block->accept(this))
        block->list.push_back(new cStmtExpr(node->expr->accept(this)))
        return block

    virtual cAstNode* visit(node: CommaList*) = new cCommaList(list) where
        list := [x->accept(this) for x <- node->list]: vector<cAstNode*>