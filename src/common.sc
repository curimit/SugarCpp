import
    "iostream"
    "string"
    "stdio.h"
    "string.h"
    "vector"
    "map"
    "stack"
    "AstCpp/cRender.h"
    "AstSugar/TargetCpp.h"
    "AstSugar/TargetCppHeader.h"
    "AstSugar/TargetCppImplementation.h"

using namespace std

public class YYL_TOKEN
    STRING_LIST: vector<string>
    STRING: string
    INT: int

    EXPR: Expr*
    EXPR_CHAIN: ExprChain*
    BLOCK: Block*
    ROOT: Root*
    STMT_IF: StmtIf*
    AST_NODE: AstNode*
    STMT_MATCH_ITEM: StmtMatchItem*
    VECTOR_STMT_MATCH_ITEM: vector<StmtMatchItem*>
    FOR_ITEM: ForItem*
    VECTOR_FOR_ITEM: vector<ForItem*>
    ATTRIBUTE: map<string, string>
    COMMA_LIST: CommaList*
    FUNC: Func*
    CLASS_ARGUMENT: ClassArgument*
    VECOTR_CLASS_ARGUMENT: vector<ClassArgument*>
    CLASS_DEF: Class*
    IMPORT: Import*
    TYPE_LIST: TypeList*
    VECTOR_TYPE: vector<Type*>
    SWITCH_ITEM: SwitchItem*
    VECTOR_SWITCH_ITEM: vector<SwitchItem*>
    TYPE: Type*

int yyparse()

extern yyroot: Root*
extern yyin: FILE*