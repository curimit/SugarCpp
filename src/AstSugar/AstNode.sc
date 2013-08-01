import
    "vector"
    "map"
    "stack"
    "../AstCpp/cAstNode.sc"

using namespace std

public class AstNode()
    attribute: map<string, string>
    virtual cAstNode* accept(visitor: Visitor*) = 0

public class Root(block: Block*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Block(list: vector<AstNode*>): AstNode
    this(x: AstNode*) = list.push_back(x)

    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Import(list: vector<string>): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Type(): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = 0

public class TypePrefix(prefix: string, type: Type*): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeSuffix(suffix: string, type: Type*): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeIdent(name: string): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeList(list: vector<Type*>): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeTemplate(type: Type*, args: TypeList*): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeFunc(args: TypeList*, type: Type*): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeScoping(scope: string, type: Type*): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeVoid(): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class TypeAuto(): Type
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Func(): AstNode
    enum FuncType = Normal | Constructor | Destructor

    type: Type*
    name: string
    genericParameter: vector<string>
    args: CommaList*
    block: Block* = new Block()
    funcType: FuncType

    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Enum(name: string, list: vector<string>): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ClassArgument(name: string, type: Type*)

public class Class(): AstNode
    name: string
    args: vector<ClassArgument*>
    genericParameter: vector<string>
    inherit: TypeList* = new TypeList()
    block: Block* = new Block()

    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtIf(expr: Expr*, body_block: Block*, else_block: Block*): AstNode
    this(expr: Expr*, body_block: Block*)
        @expr = expr
        @body_block = body_block
        @else_block = nil
    
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtWhile(expr: Expr*, block: Block*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtTry(expr: Expr*, try_block: Block*, catch_block: Block*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class SwitchItem(expr: Expr*, block: Block*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtSwitch(expr: Expr*, list: vector<SwitchItem*>): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ForItem(): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = 0

public class ForItemEach(name: string, expr: Expr*): ForItem
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ForItemCond(expr: Expr*): ForItem
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ForItemMap(name: string, expr: Expr*): ForItem
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ForItemRange(): ForItem
    enum Type = To | Til | Down_To

    name: string
    expr1, expr2, expr3: Expr* = nil
    type: Type

    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtFor(list: vector<ForItem*>, block: Block*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtReturn(expr: Expr*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtMatchItem()
    name: string
    field: string

    this(name: string, field: string)
        @name = name
        @field = field

public class StmtMatch(list: vector<StmtMatchItem*>, expr: Expr*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class StmtUsing(): AstNode
    enum Style = Namespace | Symbol

    name: string
    style: Style

    this(name: string, style: Style)
        @name = name
        @style = style

    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Expr(): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = 0

public class StmtExpr(expr: Expr*): AstNode
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprConst(value: string): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprPrefix(op: string, expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprLambda(args: CommaList*, expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprSuffix(op: string, expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprBin(op: string, expr1: Expr*, expr2: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprChainItem(op:string, expr: Expr*)

public class ExprChain(): Expr
    list: vector<ExprChainItem>

    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class CommaList(list: vector<Expr*>): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprCall(expr: Expr*, args: CommaList*, gp: vector<Type*>): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprWhere(expr: Expr*, block: Block*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprDict(expr: Expr*, args: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprNew(type: Type*, args: CommaList*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprInfix(name: string, expr1: Expr*, expr2: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprCast(type: Type*, expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprCond(cond: Expr*, expr1: Expr*, expr2: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprDeclare(type: Type*, name: string): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprDeclareAssign(type: Type*, name: string, expr: Expr*): ExprDeclare
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprSizeOf(expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprDelete(expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class ExprBracket(expr: Expr*): Expr
    virtual cAstNode* accept(visitor: Visitor*) = visitor->visit(this)

public class Visitor()
    virtual cAstNode* visit(node: Root*) = 0
    virtual cAstNode* visit(node: Block*) = 0

    virtual cAstNode* visit(node: TypeIdent*) = 0
    virtual cAstNode* visit(node: TypePrefix*) = 0
    virtual cAstNode* visit(node: TypeSuffix*) = 0
    virtual cAstNode* visit(node: TypeTemplate*) = 0
    virtual cAstNode* visit(node: TypeScoping*) = 0
    virtual cAstNode* visit(node: TypeFunc*) = 0

    virtual cAstNode* visit(node: TypeVoid*) = 0
    virtual cAstNode* visit(node: TypeAuto*) = 0

    virtual cAstNode* visit(node: TypeList*) = 0

    virtual cAstNode* visit(node: Func*) = 0
    virtual cAstNode* visit(node: Class*) = 0
    virtual cAstNode* visit(node: Enum*) = 0
    virtual cAstNode* visit(node: Import*) = 0
    virtual cAstNode* visit(node: StmtExpr*) = 0
    virtual cAstNode* visit(node: StmtIf*) = 0
    virtual cAstNode* visit(node: StmtWhile*) = 0
    virtual cAstNode* visit(node: StmtTry*) = 0

    virtual cAstNode* visit(node: SwitchItem*) = 0
    virtual cAstNode* visit(node: StmtSwitch*) = 0

    virtual cAstNode* visit(node: ForItemEach*) = 0
    virtual cAstNode* visit(node: ForItemCond*) = 0
    virtual cAstNode* visit(node: ForItemMap*) = 0
    virtual cAstNode* visit(node: ForItemRange*) = 0
    virtual cAstNode* visit(node: StmtFor*) = 0

    virtual cAstNode* visit(node: StmtReturn*) = 0

    virtual cAstNode* visit(node: StmtMatch*) = 0
    virtual cAstNode* visit(node: StmtUsing*) = 0

    virtual cAstNode* visit(node: ExprConst*) = 0
    virtual cAstNode* visit(node: ExprPrefix*) = 0
    virtual cAstNode* visit(node: ExprSuffix*) = 0
    virtual cAstNode* visit(node: ExprBin*) = 0
    virtual cAstNode* visit(node: ExprChain*) = 0
    virtual cAstNode* visit(node: ExprCall*) = 0
    virtual cAstNode* visit(node: ExprDict*) = 0
    virtual cAstNode* visit(node: ExprInfix*) = 0
    virtual cAstNode* visit(node: ExprCast*) = 0
    virtual cAstNode* visit(node: ExprCond*) = 0
    virtual cAstNode* visit(node: ExprDeclare*) = 0
    virtual cAstNode* visit(node: ExprDeclareAssign*) = 0
    virtual cAstNode* visit(node: ExprSizeOf*) = 0
    virtual cAstNode* visit(node: ExprDelete*) = 0
    virtual cAstNode* visit(node: ExprNew*) = 0
    virtual cAstNode* visit(node: ExprWhere*) = 0

    virtual cAstNode* visit(node: ExprLambda*) = 0

    virtual cAstNode* visit(node: ExprBracket*) = 0

    virtual cAstNode* visit(node: CommaList*) = 0