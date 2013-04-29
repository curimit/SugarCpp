tree grammar SugarWalker ;

options
{
    tokenVocab=SugarCpp;   
    ASTLabelType=CommonTree;  
    language=CSharp3;
}

@header
{
	using System;
	using System.Collections;
	using System.Linq;
}

@members
{
}

@namespace { SugarCpp.Compiler }

public root returns [Root value]
@init
{
	$value = new Root();
}
	: (a = node  { $value.List.Add(a); } NEWLINE*)+
	;

node returns [AstNode value]
	: a = func_def { $value = a; }
	;

type_name returns [string value]
@init
{
	$value = "";
}
	: a=IDENT { $value+=a.Text; }
	  ('<' { $value+="<"; }
	  b=type_name { $value+=b; }
	  (',' b=type_name { $value+=", " + b; })*
	  '>' { $value+=">"; })*
	  ('*' { $value+="*"; })*
	| {bool isFirst = true; $value += "std::tuple<";} ^(Type_Tuple (b=type_name
	{
		if (!isFirst) $value += ",";
		isFirst = false;
		$value += b;
	})+)
	{
		$value += ">";
	}
	;

func_args returns [List<Stmt> value]
@init
{
	$value = new List<Stmt>();
}
	: ^(Func_Args (a=stmt { $value.Add(a); })*)
	;

func_def returns [FuncDef value]
@init
{
	$value = new FuncDef();
}
	: a=type_name b=IDENT ('<' (x=IDENT {$value.GenericParameter.Add(x.Text); })+ '>')? '(' (args=func_args { $value.Args = args; })? ')'
	( e=stmt_block
	{
		$value.Type = a;
		$value.Name = b.Text;
		$value.Body = e;
	})
	;

stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: INDENT (NEWLINE+ a=stmt { $value.StmtList.Add(a); })* NEWLINE* DEDENT
    ; 

stmt returns [Stmt value]
	: a=stmt_expr { $value = a; }
	;

stmt_expr returns [Stmt value]
	: a=stmt_alloc { $value = a; }
	| a=stmt_return { $value = a; }
	;

stmt_alloc returns [Stmt value]
	: a=alloc_expr { $value = a; }
	| a=alloc_expr_auto { $value = a; }
	;

stmt_return returns [Stmt value]
	: ^(Expr_Return (a=expr)?)
	{
		$value = new ExprReturn(a);
	}
	;

ident returns [string value]
	: a=IDENT { $value = a.Text; }
	;

ident_list returns [List<string> value]
@init
{
	$value = new List<string>();
}
	: a=ident { $value.Add(a); } ((',' a=ident { $value.Add(a); })+ ';')?
	;
	
alloc_expr returns [StmtAlloc value]
	: ^(Expr_Alloc a=type_name b=ident (c=expr)?)
	{
		$value = new StmtAlloc();
		$value.Type = a;
		$value.Name = b;
		$value.Expr = c;
	}
	;

alloc_expr_auto returns [StmtAlloc value]
	: ^(Expr_Alloc_Auto a=ident (b=expr)?)
	{
		$value = new StmtAlloc();
		$value.Type = "auto";
		$value.Name = a;
		$value.Expr = b;
	}
	;

new_expr returns [ExprNew value]
@init
{
	$value = new ExprNew();
}
	: ^(Expr_New a=IDENT { $value.ElemType = a.Text; } (b=expr { $value.Ranges.Add(b); })+)
	;

block_expr returns [ExprBlock value]
@init
{
	$value = new ExprBlock();
}
	: INDENT (NEWLINE+ a=stmt { $value.StmtList.Add(a); })* NEWLINE* DEDENT
    ; 

expr_tuple returns [ExprTuple value]
@init
{
	$value = new ExprTuple();
}
	: ^(Expr_Tuple (a=expr { $value.ExprList.Add(a); })+ )
	;

expr_match_tuple returns [MatchTuple value]
@init
{
	$value = new MatchTuple();
}
	: ^(Expr_Match_Tuple (a=IDENT { $value.VarList.Add(a.Text); })*)
	;

expr_list returns [List<Expr> value]
@init
{
	$value = new List<Expr>();
}
	: (a=expr { $value.Add(a); })*
	;

call_expr returns [Expr value]
	: ^(Expr_Call a=expr b=expr_list)
	{
		$value = new ExprCall(a, b);
	}
	;

dict_expr returns [Expr value]
	: ^(Expr_Dict a=expr b=expr)
	{
		$value = new ExprDict(a, b);
	}
	;

lambda_expr returns [ExprLambda value]
	: ^(Expr_Lambda b=func_args a=expr)
	{
		$value = new ExprLambda(a, b);
	}
	;

expr returns [Expr value]
    : tuple=expr_tuple
	{
		$value = tuple;
	}
	| call=call_expr
	{
		$value = call;
	}
	| dict=dict_expr
	{
		$value = dict;
	}
	| lambda=lambda_expr
	{
		$value = lambda;
	}
	|^(Expr_Cond a=expr b=expr c=expr)
	{
		$value = new ExprCond(a, b, c);
	}
	| ^(Expr_Access op=('.' | '::' | '->' | '->*' | '.*') a=expr text=IDENT)
	{
		$value = new ExprAccess(a, op.Text, text.Text);
	}
	| ^(Expr_Bin op=( '+' | '-' | '*' | '/'
	                | '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>='
					| '<' | '<=' | '>' | '>=' | '==' | '!='
					| '<<' | '>>'
					| '&' | '^' | '|'
					| '&&' | '||'
					) a=expr b=expr)
	{
		$value = new ExprBin(op.Text, a, b);
	}
	| ^(Expr_Suffix op=('++' | '--') a=expr)
	{
		$value = new ExprSuffix(op.Text, a);
	}
	| ^(Expr_Prefix op=('!' | '~' | '++' | '--' | '-' | '+' | '*' | '&') a=expr)
	{
		$value = new ExprPrefix(op.Text, a);
	}
	| text=(INT | DOUBLE | IDENT | STRING)
    {
        $value = new ExprConst(text.Text);
    }
	;
