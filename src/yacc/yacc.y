%{
#define YYSTYPE YYL_TOKEN

#include "../common.h"

extern "C"
{
	void yyerror(const char *s);
	extern int yylex(void);
	extern int yylineno;
	extern char* yytext;
}

%}

%token IF ELSE WHILE FOR UNLESS LOOP SWITCH TRY CATCH WHEN DICT RETURN WHERE
%token IMPORT CLASS ENUM THIS USING NAMESPACE
%token TO TIL DOWN_TO BY STEP DOT_DOT CAST
%token CONST SIGNED UNSIGNED PUBLIC PRIVATE VIRTUAL INLINE OVERRIDE LONG SHORT INT DOUBLE
%token INDENT DEDENT NEWLINE
%token COLON_EQ MINUS_GT_STAR DOT_STAR LT_LT GT_GT AND OR AS NEW
%token EQ_EQ NOT_EQ GT_EQ LT_EQ EQ_GT
%token PLUS_PLUS MINUS_MINUS UNARY_MINUS UNARY_PLUS SIZE_OF DELETE CAST_TYPE
%token MINUS_GT DOT SCOPING GROUPING LT_MINUS
%token PLUS_EQ MINUS_EQ MUL_EQ DIV_EQ MOD_EQ AND_EQ OR_EQ XOR_EQ LT_LT_EQ GT_GT_EQ

%token <STRING> IDENT
%token <STRING> NUMBER
%token <STRING> STRING

%type <STRING> attribute_item

%type <ROOT> root
%type <BLOCK> global_block stmt_block
%type <ATTRIBUTE> attribute
%type <CLASS_ARGUMENT> class_argument
%type <VECOTR_CLASS_ARGUMENT> class_args class_args_list
%type <FOR_ITEM> stmt_for_item
%type <VECTOR_FOR_ITEM> stmt_for_list
%type <STMT_MATCH_ITEM> stmt_match_item
%type <VECTOR_STMT_MATCH_ITEM> stmt_match_list
%type <AST_NODE> node enum_def stmt_if stmt_while stmt_try stmt_for stmt_return stmt stmt_match stmt_expr stmt_using
%type <COMMA_LIST> func_args func_args_list call_expr_args
%type <FUNC> func_def
%type <CLASS_DEF> class_def
%type <IMPORT> import
%type <TYPE_LIST> type_list class_inherit_list
%type <VECTOR_TYPE> generic_call_parameters
%type <STRING_LIST> generic_parameters string_list import_string_list import_block import_list enum_list

%type <SWITCH_ITEM> stmt_switch_item
%type <VECTOR_SWITCH_ITEM> stmt_switch_list
%type <AST_NODE> stmt_switch

%type <TYPE> type type_atom type_star type_template type_func

%type <EXPR> add_expr cond_expr modify_expr lambda_expr call_expr variable_def variable_def_no_comma
%type <EXPR> cast_expr prefix_expr suffix_expr
%type <EXPR> shift_expr cmp_expr
%type <EXPR> atom_expr
%type <EXPR> expr where_expr
%type <EXPR> bit_expr lvalue

%type <EXPR_CHAIN> cmp_expr_chain

%type <STRING> cmp_expr_op
%type <STRING> ident

%left '+' '-' '*' '/' '&'
%nonassoc UNARY

%nonassoc THEN
%nonassoc NEWLINE

%start root

%%

root:
	global_block
	{
		yyroot = new Root($1);
	}
	;

newline:
	newline NEWLINE
	| NEWLINE
	;

global_block
	:global_block newline node
	{
		$$->list.push_back($3);
	}
	| node
	{
		$$ = new Block($1);
	}
	;

stmt_block:
	stmt_block newline stmt
	{
		$$->list.push_back($3);
	}
	| stmt
	{
		$$ = new Block($1);
	}
	;

node
	: stmt_if 		{ $$ = $1; }
	| stmt_while 	{ $$ = $1; }
	| stmt_try 		{ $$ = $1; }
	| stmt_switch 	{ $$ = $1; }
	| stmt_for	  	{ $$ = $1; }
	| stmt_using	{ $$ = $1; }
	| import 		{ $$ = $1; }
	| class_def 	{ $$ = $1; }
	| enum_def		{ $$ = $1; }
	| func_def 		{ $$ = $1; }
	| variable_def 	{ $$ = new StmtExpr($1); }
	;

enum_list:
	enum_list '|' ident { $$.push_back($3); }
	| ident { $$.push_back($1); }
	;

enum_def:
	ENUM ident '=' enum_list { $$ = new Enum($2, $4); }
	;

import_string_list:
	import_string_list newline STRING { $$.push_back($3); }
	| STRING { $$.push_back($1); }
	;

import_block:
	newline INDENT import_string_list DEDENT { $$ = $3; }
	;

import_list:
	STRING
	{
		$$.push_back($1);
	}
	| import_block
	{
		$$ = $1;
	}
	;

import:
	IMPORT import_list { $$ = new Import($2); }
	;

type:
	type_func
	;

type_func:
	type_star
	| type_star MINUS_GT type_star
	{
		TypeList* list = new TypeList();
		list->list.push_back($1);
		$$ = new TypeFunc(list, $3);
	}
	| type_star MINUS_GT '(' ')'
	{
		TypeList* list = new TypeList();
		list->list.push_back($1);
		$$ = new TypeFunc(list, new TypeVoid());
	}
	| '(' type_list ')' MINUS_GT '(' ')'
	{
		$$ = new TypeFunc($2, new TypeVoid());
	}
	| '(' type_list ')' MINUS_GT type_star
	{
		$$ = new TypeFunc($2, $5);
	}
	| '(' ')' MINUS_GT type_star
	{
		$$ = new TypeFunc(new TypeList(), $4);
	}
	| '(' ')' MINUS_GT '(' ')'
	{
		$$ = new TypeFunc(new TypeList(), new TypeVoid());
	}
	;

type_star:
	type_template
	| type_star '*' { $$ = new TypeSuffix("*", $1); }
	| type_star '&' { $$ = new TypeSuffix("&", $1); }
	;

type_template:
	type_atom
	| type_atom '<' type_list '>' { $$ = new TypeTemplate($1, $3); }
	;

type_atom:
	ident 					{ $$ = new TypeIdent($1); }
	| LONG 					{ $$ = new TypeIdent("long"); }
	| SHORT 				{ $$ = new TypeIdent("short"); }
	| INT 					{ $$ = new TypeIdent("int"); }
	| DOUBLE 				{ $$ = new TypeIdent("double"); }
	| LONG INT 				{ $$ = new TypePrefix("long", new TypeIdent("int")); }
	| LONG DOUBLE 			{ $$ = new TypePrefix("long", new TypeIdent("double")); }
	| LONG LONG 			{ $$ = new TypePrefix("long", new TypeIdent("long")); }
	| LONG LONG INT 		{ $$ = new TypePrefix("long", new TypePrefix("long", new TypeIdent("int"))); }
	| SHORT INT 			{ $$ = new TypePrefix("short", new TypeIdent("int")); }
	| UNSIGNED type_atom 	{ $$ = new TypePrefix("unsigned", $2); }
	| CONST type_atom 		{ $$ = new TypePrefix("const", $2); }
	| SIGNED type_atom 		{ $$ = new TypePrefix("signed", $2); }
	;

type_list:
	type_list ',' type 	{ $$->list.push_back($3); }
	| type 				{ $$ = new TypeList(); $$->list.push_back($1); }
	;

attribute_item:
	PUBLIC    { $$ = "public"; }
	| PRIVATE { $$ = "private"; }
	| VIRTUAL { $$ = "virtual"; }
	;

attribute:
	attribute attribute_item { $$[$2] = ""; }
	| attribute_item { $$[$1] = ""; }
	;

func_def
	: attribute type ident generic_parameters '(' func_args ')' newline INDENT stmt_block DEDENT
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Normal;
		$$->attribute = $1;
		$$->type = $2;
		$$->name = $3;
		$$->genericParameter = $4;
		$$->args = $6;
		$$->block = $10;
	}
	| type ident generic_parameters '(' func_args ')' newline INDENT stmt_block DEDENT
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Normal;
		$$->type = $1;
		$$->name = $2;
		$$->genericParameter = $3;
		$$->args = $5;
		$$->block = $9;
	}
	| attribute type ident generic_parameters '(' func_args ')' '=' expr
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Normal;
		$$->attribute = $1;
		$$->type = $2;
		$$->name = $3;
		$$->genericParameter = $4;
		$$->args = $6;
		$$->block = new Block((new StmtReturn($9)));
	}
	| type ident generic_parameters '(' func_args ')' '=' expr
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Normal;
		$$->type = $1;
		$$->name = $2;
		$$->genericParameter = $3;
		$$->args = $5;
		$$->block = new Block((new StmtReturn($8)));
	}

	| attribute THIS generic_parameters '(' func_args ')' newline INDENT stmt_block DEDENT
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Constructor;
		$$->attribute = $1;
		$$->genericParameter = $3;
		$$->args = $5;
		$$->block = $9;
	}
	| THIS generic_parameters '(' func_args ')' newline INDENT stmt_block DEDENT
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Constructor;
		$$->genericParameter = $2;
		$$->args = $4;
		$$->block = $8;
	}
	| attribute THIS generic_parameters '(' func_args ')' '=' expr
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Constructor;
		$$->attribute = $1;
		$$->genericParameter = $3;
		$$->args = $5;
		$$->block = new Block((new StmtReturn($8)));
	}
	| THIS generic_parameters '(' func_args ')' '=' expr
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Constructor;
		$$->genericParameter = $2;
		$$->args = $4;
		$$->block = new Block((new StmtReturn($7)));
	}

	| attribute '~' THIS generic_parameters '(' func_args ')' newline INDENT stmt_block DEDENT
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Destructor;
		$$->attribute = $1;
		$$->genericParameter = $4;
		$$->args = $6;
		$$->block = $10;
	}
	| '~' THIS generic_parameters '(' func_args ')' newline INDENT stmt_block DEDENT
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Destructor;
		$$->genericParameter = $3;
		$$->args = $5;
		$$->block = $9;
	}
	| attribute '~' THIS generic_parameters '(' func_args ')' '=' expr
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Destructor;
		$$->attribute = $1;
		$$->genericParameter = $4;
		$$->args = $6;
		$$->block = new Block((new StmtReturn($9)));
	}
	| '~' THIS generic_parameters '(' func_args ')' '=' expr
	{
		$$ = new Func();
		$$->funcType = Func::FuncType::Destructor;
		$$->genericParameter = $3;
		$$->args = $5;
		$$->block = new Block((new StmtReturn($8)));
	}
	;

func_args_list:
	func_args ',' variable_def_no_comma { $$->list.push_back($3); }
	| variable_def_no_comma
	{
		$$ = new CommaList();
		$$->list.push_back($1);
	}
	;

func_args:
	func_args_list { $$ = $1; }
	| { $$= new CommaList(); }
	;

string_list:
	string_list ',' ident { $$.push_back($3); }
	| ident { $$.push_back($1); }
	;

generic_parameters:
	'<' string_list '>' { $$ = $2; }
	| { $$.clear(); }
	;

class_inherit_list:
	':' type_list { $$ = $2; }
	| { $$ = new TypeList(); }
	;

class_argument:
	IDENT ':' type { $$ = new ClassArgument($1, $3); }
	;

class_args_list:
	class_args_list ',' class_argument { $$.push_back($3); }
	| class_argument { $$.push_back($1); }
	;

class_args:
	'(' class_args_list ')' { $$ = $2; }
	| '(' ')' { }
	| { }
	;

class_def
	: CLASS ident generic_parameters class_args class_inherit_list newline INDENT global_block DEDENT
	{
		$$ = new Class();
		$$->name = $2;
		$$->genericParameter = $3;
		$$->args = $4;
		$$->inherit = $5;
		$$->block = $8;
	}
	| attribute CLASS ident generic_parameters class_args class_inherit_list newline INDENT global_block DEDENT
	{
		$$ = new Class();
		$$->attribute = $1;
		$$->name = $3;
		$$->genericParameter = $4;
		$$->args = $5;
		$$->inherit = $6;
		$$->block = $9;
	}
	/*| CLASS ident generic_parameters class_args class_inherit_list
	{
		$$ = new Class();
		$$->name = $2;
		$$->genericParameter = $3;
		$$->args = $4;
		$$->inherit = $5;
		$$->block = new Block();
	}
	| attribute CLASS ident generic_parameters class_args class_inherit_list
	{
		$$ = new Class();
		$$->attribute = $1;
		$$->name = $3;
		$$->genericParameter = $4;
		$$->args = $5;
		$$->inherit = $6;
		$$->block = new Block();
	}*/
	;

stmt
	: stmt_expr
	| stmt_match
	| stmt_return
	| stmt_using
	;

stmt_using
	: USING ident 			{ $$ = new StmtUsing($2, StmtUsing::Style::Symbol); 	}
	| USING NAMESPACE ident { $$ = new StmtUsing($3, StmtUsing::Style::Namespace); }
	;

stmt_expr
	: variable_def 	{ $$ = new StmtExpr($1); }
	| lvalue		{ $$ = new StmtExpr($1); }
	;

stmt_match_item
	: IDENT ':' ident { $$ = new StmtMatchItem($3, $1); }
	| IDENT { $$ = new StmtMatchItem($1, $1); }
	;

stmt_match_list
	: stmt_match_list ',' stmt_match_item { $$.push_back($3); }
	| stmt_match_item { $$.push_back($1); }
	;

stmt_match
	: '{' stmt_match_list '}' COLON_EQ expr { $$ = new StmtMatch($2, $5); }
	;

lvalue
	: shift_expr
	| shift_expr '=' expr { $$ = new ExprBin("=", $1, $3); }
	;

variable_def
	: ident ':' type { $$ = new ExprDeclare($3, $1); }
	| ident ':' type '=' expr { $$ = new ExprDeclareAssign($3, $1, $5); }
	| ident COLON_EQ expr { $$ = new ExprDeclareAssign(new TypeAuto(), $1, $3); }
	;

variable_def_no_comma
	: ident ':' type { $$ = new ExprDeclare($3, $1); }
	| ident ':' type '=' modify_expr { $$ = new ExprDeclareAssign($3, $1, $5); }
	| ident COLON_EQ modify_expr { $$ = new ExprDeclareAssign(new TypeAuto(), $1, $3); }
	;

expr
	: where_expr
	;

where_expr
	: lambda_expr
	| lambda_expr WHERE newline INDENT stmt_block DEDENT { $$ = new ExprWhere($1, $5); }
	| lambda_expr WHERE stmt { $$ = new ExprWhere($1, new Block($3)); }
	;

lambda_expr:
	call_expr { $$ = $1; }
	| '(' func_args ')' MINUS_GT lambda_expr { $$ = new ExprLambda($2, $5); }
	;

call_expr:
	modify_expr    { $$ = $1; }
	| modify_expr generic_call_parameters '(' call_expr_args ')' { $$ = new ExprCall($1, $4, $2); }
	| modify_expr generic_call_parameters '(' ')' { $$ = new ExprCall($1, new CommaList(), $2); }
	| NEW type '(' call_expr_args ')' { $$ = new ExprNew($2, $4); }
	| NEW type '(' ')' { $$ = new ExprNew($2, new CommaList()); }
	| NEW type { $$ = new ExprNew($2, new CommaList()); }
	;

call_expr_args:
	modify_expr ',' call_expr_args
	{
		$$ = $3;
		$$->list.insert($$->list.begin(), $1);
	}
	| call_expr
	{
		$$ = new CommaList();
		$$->list.push_back($1);
	}
	;

generic_call_parameters:
	{ }
	// Todo: fix ambiguous, support get<int>(1)
	| '!' '(' type_list ')' { for (auto x : $3->list) $$.push_back(x); delete $3; }
	;

modify_expr:
	cond_expr
	| modify_expr '=' cond_expr			{ $$ = new ExprBin("=", $1, $3); }
	| modify_expr PLUS_EQ cond_expr		{ $$ = new ExprBin("+=", $1, $3); }
	| modify_expr MINUS_EQ cond_expr	{ $$ = new ExprBin("-=", $1, $3); }
	| modify_expr DIV_EQ cond_expr		{ $$ = new ExprBin("/=", $1, $3); }
	| modify_expr MOD_EQ cond_expr		{ $$ = new ExprBin("%=", $1, $3); }
	| modify_expr AND_EQ cond_expr		{ $$ = new ExprBin("&=", $1, $3); }
	| modify_expr OR_EQ cond_expr		{ $$ = new ExprBin("|=", $1, $3); }
	| modify_expr XOR_EQ cond_expr		{ $$ = new ExprBin("^=", $1, $3); }
	| modify_expr LT_LT_EQ cond_expr	{ $$ = new ExprBin("<<=", $1, $3); }
	| modify_expr GT_GT_EQ cond_expr	{ $$ = new ExprBin(">>=", $1, $3); }
	;

cond_expr:
	bit_expr { $$ = $1; }
	| bit_expr '?' bit_expr ':' bit_expr { $$ = new ExprCond($1, $3, $5); }
	;

bit_expr:
	cmp_expr { $$ = $1; }
	| bit_expr '|' cmp_expr	{ $$ = new ExprBin("|", $1, $3); }
	| bit_expr '&' cmp_expr	{ $$ = new ExprBin("&", $1, $3); }
	| bit_expr '^' cmp_expr	{ $$ = new ExprBin("^", $1, $3); }
	| bit_expr OR cmp_expr	{ $$ = new ExprBin("||", $1, $3); }
	| bit_expr AND cmp_expr	{ $$ = new ExprBin("&&", $1, $3); }
	;

cmp_expr_op:
	'<' 		{ $$ = "<"; }
	| '>' 		{ $$ = ">"; }
	| EQ_EQ 	{ $$ = "=="; }
	| NOT_EQ 	{ $$ = "!="; }
	| LT_EQ 	{ $$ = "<="; }
	| GT_EQ 	{ $$ = ">="; }
	;

cmp_expr_chain:
	cmp_expr_chain cmp_expr_op shift_expr { $$->list.push_back(ExprChainItem($2, $3)); }
	| shift_expr
	{
		$$ = new ExprChain();
		$$->list.push_back(ExprChainItem("", $1));
	}
	;

cmp_expr:
	cmp_expr_chain
	{
		if ($1->list.size() == 1)
			$$ = $1->list[0].expr;
		else $$ = $1;
	}
	;

shift_expr:
	add_expr	{ $$ = $1; }
	| shift_expr LT_LT add_expr	{ $$ = new ExprBin("<<", $1, $3); }
	| shift_expr GT_GT add_expr { $$ = new ExprBin(">>", $1, $3); }
	;

add_expr:
	cast_expr	{ $$ = $1; }
	| add_expr '+' cast_expr 			{ $$ = new ExprBin("+", $1, $3);   }
	| add_expr '-' cast_expr			{ $$ = new ExprBin("-", $1, $3);   }
	| add_expr '*' cast_expr			{ $$ = new ExprBin("*", $1, $3);   }
	| add_expr '/' cast_expr			{ $$ = new ExprBin("/", $1, $3);   }
	| add_expr '%' cast_expr			{ $$ = new ExprBin("%", $1, $3);   }
	| add_expr DOT cast_expr			{ $$ = new ExprBin(".", $1, $3);   }
	| add_expr MINUS_GT cast_expr		{ $$ = new ExprBin("->", $1, $3);  }
	| add_expr '`' ident '`' cast_expr 	{ $$ = new ExprInfix($3, $1, $5);  }
	| add_expr MINUS_GT_STAR cast_expr	{ $$ = new ExprBin("->*", $1, $3); }
	| add_expr DOT_STAR cast_expr		{ $$ = new ExprBin(".*", $1, $3);  }
	;

cast_expr:
	prefix_expr { $$ = $1; }
	| CAST type prefix_expr { $$ = new ExprCast($2, $3); }
	;

prefix_expr:
	suffix_expr { $$ = $1; }
	| '(' PLUS_PLUS suffix_expr ')'	  { $$ = new ExprPrefix("++", $3); }
	| '(' MINUS_MINUS suffix_expr ')' { $$ = new ExprPrefix("--", $3); }
	| '(' '!' suffix_expr ')'	{ $$ = new ExprPrefix("!", $3);  }
	| '(' '+' suffix_expr ')'	{ $$ = new ExprPrefix("+", $3);  }
	| '(' '-' suffix_expr ')'	{ $$ = new ExprPrefix("-", $3);  }
	| '(' '*' suffix_expr ')' 	{ $$ = new ExprPrefix("*", $3);  }
	| '(' '&' suffix_expr ')'	{ $$ = new ExprPrefix("&", $3);  }
	| SIZE_OF suffix_expr 		{ $$ = new ExprSizeOf($2); }
	| DELETE suffix_expr 		{ $$ = new ExprDelete($2); }
	;

suffix_expr:
	atom_expr { $$ = $1; }
	| suffix_expr PLUS_PLUS					{ $$ = new ExprSuffix("++", $1); }
	| suffix_expr MINUS_MINUS 				{ $$ = new ExprSuffix("--", $1); }
	| suffix_expr '[' call_expr_args ']'	{ $$ = new ExprDict($1, $3); }
	;

atom_expr:
	NUMBER			{ $$ = new ExprConst($1); }
	| THIS			{ $$ = new ExprConst("this"); }
	| ident 		{ $$ = new ExprConst($1); }
	| STRING 		{ $$ = new ExprConst($1); }
	| '(' expr ')'	{ $$ = new ExprBracket($2); }
	;

stmt_if:
	IF expr THEN stmt
	{
		$$ = new StmtIf($2, new Block($4));
	}
	| IF expr THEN stmt ELSE stmt
	{
		$$ = new StmtIf($2, new Block($4), new Block($6));
	}
	| IF expr newline INDENT stmt_block DEDENT %prec THEN
	{
		$$ = new StmtIf($2, $5);
	}
	| IF expr newline INDENT stmt_block DEDENT newline ELSE newline INDENT stmt_block DEDENT
	{
		$$ = new StmtIf($2, $5, $11);
	}
	;

stmt_while:
	WHILE expr newline INDENT stmt_block DEDENT
	{
		$$ = new StmtWhile($2, $5);
	}
	| WHILE expr THEN stmt
	{
		$$ = new StmtWhile($2, new Block($4));
	}
	;

stmt_try:
	TRY newline INDENT stmt_block DEDENT newline CATCH expr newline INDENT stmt_block DEDENT
	{
		$$ = new StmtTry($8, $4, $11);
	}
	;

stmt_switch_item:
	WHEN expr newline INDENT stmt_block DEDENT
	{
		$$ = new SwitchItem($2, $5);
	}
	| '|' expr newline INDENT stmt_block DEDENT
	{
		$$ = new SwitchItem($2, $5);
	}
	;

stmt_switch_list:
	stmt_switch_list newline stmt_switch_item { $$.push_back($3); }
	| stmt_switch_item { $$.push_back($1); }
	;

stmt_switch:
	SWITCH expr newline INDENT stmt_switch_list DEDENT
	{
		$$ = new StmtSwitch($2, $5);
	}
	;

stmt_for_item:
	IDENT LT_MINUS modify_expr { $$ = new ForItemEach($1, $3); }
	| IDENT LT_MINUS call_expr TO modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::To;
		item->expr1 = $3;
		item->expr2 = $5;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr DOT_DOT modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::To;
		item->expr1 = $3;
		item->expr2 = $5;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr TIL modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::Til;
		item->expr1 = $3;
		item->expr2 = $5;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr DOWN_TO modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::Down_To;
		item->expr1 = $3;
		item->expr2 = $5;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr TO call_expr BY modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::To;
		item->expr1 = $3;
		item->expr2 = $5;
		item->expr3 = $7;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr DOT_DOT call_expr BY modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::To;
		item->expr1 = $3;
		item->expr2 = $5;
		item->expr3 = $7;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr TIL call_expr BY modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::Til;
		item->expr1 = $3;
		item->expr2 = $5;
		item->expr3 = $7;
		$$ = item;
	}
	| IDENT LT_MINUS call_expr DOWN_TO call_expr BY modify_expr
	{
		auto item = new ForItemRange();
		item->name = $1;
		item->type = ForItemRange::Type::Down_To;
		item->expr1 = $3;
		item->expr2 = $5;
		item->expr3 = $7;
		$$ = item;
	}
	| IDENT EQ_GT modify_expr
	{
		$$ = new ForItemMap($1, $3);
	}
	| modify_expr
	{
		$$ = new ForItemCond($1);
	}
	;

stmt_for_list:
	stmt_for_list ',' stmt_for_item { $$.push_back($3); }
	| stmt_for_item { $$.push_back($1); }
	;

stmt_for:
	FOR stmt_for_list newline INDENT stmt_block DEDENT
	{
		$$ = new StmtFor($2, $5);
	}
	| FOR stmt_for_list THEN stmt
	{
		$$ = new StmtFor($2, new Block($4));
	}
	;

stmt_return:
	RETURN expr
	{
		$$ = new StmtReturn($2);
	}
	;

ident:
	IDENT { $$ = $1; }
	| IDENT SCOPING ident { $$ = $1 + "::" + $3; }
	;

%%

Root* yyroot;

void yyerror(const char *message)
{
	fprintf(stderr, "error at line %d: '%s', after '%s'\n", yylineno, message, yylval.STRING.c_str());
//	cerr << "error: " << s << endl;
}
