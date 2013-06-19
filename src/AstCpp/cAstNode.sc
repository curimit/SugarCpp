import "StringTemplate.sc"

public class cAstNode()
    virtual void accept(visitor: cVisitor*) = 0

public class cType(): cAstNode
    virtual void accept(visitor: cVisitor*) = 0

public class cTypePrefix(prefix: string, type: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeSuffix(suffix: string, type: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeIdent(name: string): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeTemplate(type: cAstNode*, args: cAstNode*): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeFunc(args: cAstNode*, type: cAstNode*): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeScoping(scope: string, type: cAstNode*): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeVoid(): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cTypeAuto(): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cInheritType(prefix: string, type: cAstNode*): cType
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cBlock(list: vector<cAstNode*>): cAstNode
    this(x: cAstNode*) = list.push_back(x)

    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cBraceBlock(block: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cClass(): cAstNode
    name: string
    genericParameter: vector<string>
    inherit: vector<cAstNode*>
    block: cAstNode* = new cAstNode()

    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cClassFlag(name: string): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cEnum(name: string, list: vector<string>): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cFunc(): cAstNode
    enum FuncType = Normal | Constructor | Destructor

    type: cAstNode*
    name: string
    genericParameter: vector<string>
    prefix: vector<string>
    args: cAstNode*
    block: cAstNode* = new cBlock()
    funcType: FuncType

    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cInclude(name: string): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cNodeList(list: vector<cAstNode*>): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtIf(expr: cAstNode*, body_block: cAstNode*, else_block: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtWhile(expr: cAstNode*, block: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtTry(expr: cAstNode*, try_block: cAstNode*, catch_block: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtSwitch(expr: cAstNode*, list: vector<cAstNode*>): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cSwitchItem(expr: cAstNode*, block: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtFor(iterate: cAstNode*, block: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cForItemNormal(expr1: cAstNode*, expr2: cAstNode*, expr3: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cForItemIterate(name: string, expr: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtReturn(expr: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtUsing(): cAstNode
    enum Style = Namespace | Symbol

    name: string
    style: Style

    this(name: string, style: Style)
        @name = name
        @style = style

    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cStmtExpr(expr: cAstNode*): cAstNode
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExpr: cAstNode
    virtual void accept(visitor: cVisitor*) = 0

public class cExprBin(op: string, expr1: cAstNode*, expr2: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprPrefix(op: string, expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprSuffix(op: string, expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprCall(expr: cAstNode*, args: cAstNode*, gp: vector<cAstNode*>): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprDict(expr: cAstNode*, args: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cCommaList(list: vector<cAstNode*>): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprCond(cond: cAstNode*, expr1: cAstNode*, expr2: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprDeclare(type: cAstNode*, name: string): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprDeclareAssign(type: cAstNode*, name: string, expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprCast(type: cAstNode*, expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprConst(v: string): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprLambda(args: cAstNode*, block: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprSizeOf(expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprDelete(expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprNew(type: cAstNode*,args: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprBracket(expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprBrace(expr: cAstNode*): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cExprBlock(list: vector<cAstNode*>): cExpr
    virtual void accept(visitor: cVisitor*) = visitor->visit(this)

public class cVisitor()
    virtual void visit(node: cBlock*) = 0
    virtual void visit(node: cBraceBlock*) = 0

    virtual void visit(node: cTypeIdent*) = 0
    virtual void visit(node: cTypePrefix*) = 0
    virtual void visit(node: cTypeSuffix*) = 0
    virtual void visit(node: cTypeTemplate*) = 0
    virtual void visit(node: cTypeScoping*) = 0
    virtual void visit(node: cTypeFunc*) = 0

    virtual void visit(node: cTypeVoid*) = 0
    virtual void visit(node: cTypeAuto*) = 0

    virtual void visit(node: cInheritType*) = 0

    virtual void visit(node: cClassFlag*) = 0

    virtual void visit(node: cClass*) = 0
    virtual void visit(node: cEnum*) = 0
    virtual void visit(node: cFunc*) = 0
    virtual void visit(node: cNodeList*) = 0
    virtual void visit(node: cInclude*) = 0
    virtual void visit(node: cStmtExpr*) = 0
    virtual void visit(node: cStmtIf*) = 0
    virtual void visit(node: cStmtWhile*) = 0
    virtual void visit(node: cStmtTry*) = 0
    
    virtual void visit(node: cSwitchItem*) = 0
    virtual void visit(node: cStmtSwitch*) = 0

    virtual void visit(node: cForItemNormal*) = 0
    virtual void visit(node: cForItemIterate*) = 0
    virtual void visit(node: cStmtFor*) = 0

    virtual void visit(node: cStmtReturn*) = 0
    virtual void visit(node: cStmtUsing*) = 0

    virtual void visit(node: cExprBin*) = 0
    virtual void visit(node: cExprConst*) = 0
    virtual void visit(node: cExprPrefix*) = 0
    virtual void visit(node: cExprSuffix*) = 0
    virtual void visit(node: cExprDict*) = 0
    virtual void visit(node: cExprCall*) = 0
    virtual void visit(node: cExprCond*) = 0
    virtual void visit(node: cExprDeclare*) = 0
    virtual void visit(node: cExprDeclareAssign*) = 0
    virtual void visit(node: cExprCast*) = 0
    virtual void visit(node: cExprSizeOf*) = 0
    virtual void visit(node: cExprNew*) = 0
    virtual void visit(node: cExprDelete*) = 0

    virtual void visit(node: cExprBlock*) = 0
    virtual void visit(node: cExprLambda*) = 0

    virtual void visit(node: cExprBracket*) = 0
    virtual void visit(node: cExprBrace*) = 0
    virtual void visit(node: cCommaList*) = 0