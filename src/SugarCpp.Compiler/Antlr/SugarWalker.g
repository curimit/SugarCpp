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
	| b = imports { $value = b; }
	| c = struct { $value = c; }
	| d = enum { $value= d; }
	;

imports returns [Import value]
@init
{
	$value = new Import();
}
	: 'import' (a = STRING { $value.NameList.Add(a.Text); })?
	  (INDENT (NEWLINE+ b = STRING { $value.NameList.Add(b.Text); })+ NEWLINE* DEDENT)? 
	;

enum returns [Enum value]
@init
{
	$value = new Enum();
}
	: 'enum' a=IDENT { $value.Name = a.Text; } '=' a=IDENT { $value.Values.Add(a.Text); } ('|' a=IDENT { $value.Values.Add(a.Text); })*
	;

struct returns [Struct value]
@init
{
	$value = new Struct();
}
	: 'struct' a=IDENT { $value.Name = a.Text; } (INDENT (NEWLINE+ b=struct_stmt { $value.List.Add(b); } )+ DEDENT)
	;

struct_stmt returns [AstNode value]
	: a=func_def { $value = a; }
	| b=alloc_expr { $value = b; }
	;

type_name returns [string value]
@init
{
	$value = "";
}
	: a=IDENT { $value+=a; } ('[' ']' { $value+="*"; })*
	;

func_def returns [FuncDef value]
@init
{
	$value = new FuncDef();
}
	: a=type_name b=IDENT ('<' (x=IDENT {$value.GenericParameter.Add(x.Text); })+ '>')? '(' (c=expr { $value.Args.Add(c); } (',' d=expr { $value.Args.Add(d); } IDENT)*)? ')'
	( e=stmt_block
	{
		$value.Type = a;
		$value.Name = b.Text;
		$value.Body = e;
	}
	| '=' f=expr
	{
		$value.Type = a;
		$value.Name = b.Text;
		StmtBlock tmp = new StmtBlock();
		if (a != "void")
			tmp.StmtList.Add(new ExprReturn(f));
		else tmp.StmtList.Add(f);
		$value.Body = tmp;
	}
	)
	;

stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: INDENT (NEWLINE+ a=stmt { $value.StmtList.Add(a); })* NEWLINE* DEDENT
    ; 

stmt returns [Stmt value]
	: a=expr { $value = a; }
	| b=stmt_if { $value = b; }
	| c=stmt_while { $value = c; }
	| d=stmt_for { $value = d; }
	;
	
stmt_if returns [StmtIf value]
	: 'if' '(' a=expr ')' b=stmt_block (NEWLINE* 'else' c=stmt_block)?
	{
		$value = new StmtIf();
		$value.Condition = a;
		$value.Body = b;
		$value.Else = c;
	}
	;

stmt_while returns [StmtWhile value]
	: 'while' '(' a=expr ')' b=stmt_block
	{
		$value = new StmtWhile();
		$value.Condition = a;
		$value.Body = b;
	}
	;

stmt_for returns [StmtFor value]
@init
{
	$value = new StmtFor();
}
	: 'for' '(' a=expr
			( ';' b=expr ';' c=expr 
			{
				$value.Start = a;
				$value.Condition = b;
				$value.Next = c;
			}
			| 'to' e=expr
			{
				ExprAlloc tmp = (ExprAlloc)a;
				
				$value.Start = a;
				$value.Condition = new ExprBin("!=", new ExprConst(tmp.Name), new ExprBin("+", e, new ExprConst("1")));
				$value.Next = new ExprBin("+=", new ExprConst(tmp.Name), new ExprConst("1"));
			}
			('by' h=expr
			{
				tmp = (ExprAlloc)a;
				
				$value.Start = a;
				$value.Condition = new ExprBin("!=", new ExprConst(tmp.Name), new ExprBin("+", e, h));
				$value.Next = new ExprBin("+=", new ExprConst(tmp.Name), h);
			})?
			) ')' d=stmt_block
			{
				$value.Body = d;
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

alloc_expr returns [ExprAlloc value]
	: ^(Expr_Alloc a=type_name b=ident (c=expr)?)
	{
		$value = new ExprAlloc();
		$value.Type = a;
		$value.Name = b;
		$value.Expr = c;
	}
	;
args_list returns [List<Expr> value]
@init
{
	$value = new List<Expr>();
}
	: (a=expr { $value.Add(a); })*
	;

call_expr returns [Expr value]
	: ^(Expr_Call a=expr b=args_list)
	{
		$value = new ExprCall(a, b);
	}
	| ^(Expr_Dict a=expr c=expr)
	{
		$value = new ExprDict(a, c);
	}
	;
	
dot_expr returns [ExprDot value]
	: ^(Expr_Dot a=expr b=IDENT)
	{
		$value = new ExprDot();
		$value.Expr = a;
		$value.Name = b.Text;
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


expr returns [Expr value]
    : alloc=alloc_expr
	{
		$value = alloc;
	}
	| call=call_expr
	{
		$value = call;
	}
	| dot=dot_expr
	{
		$value = dot;
	}
	| newExpr=new_expr
	{
		$value = newExpr;
	}
	| blockExpr=block_expr
	{
		$value = blockExpr;
	}
	| ^(Expr_Cond a=expr b=expr c=expr)
	{
		$value = new ExprCond(a, b, c);
	}
	| ^(Expr_Return (a=expr)?)
	{
		$value = new ExprReturn(a);
	}
	| ^('=' a=expr b=expr)
	{
		$value = new ExprAssign(a, b);
	}
	| ^('+' a=expr b=expr)
	{
		$value = new ExprBin("+", a, b);
	}
	| ^('-' a=expr (b=expr)?)
	{
		if (b != null)
		{
			$value = new ExprBin("-", a, b);
		}
		else
		{
			$value = new ExprPrefix("-", a);
		}
	}
	| ^('*' a=expr b=expr)
	{
		$value = new ExprBin("*", a, b);
	}
	| ^('/' a=expr b=expr)
	{
		$value = new ExprBin("/", a, b);
	}
	| ^('%' a=expr b=expr)
	{
		$value = new ExprBin("\%", a, b);
	}
	| ^('==' a=expr b=expr)
	{
		$value = new ExprBin("==", a, b);
	}
	| ^('>' a=expr b=expr)
	{
		$value = new ExprBin(">", a, b);
	}
	| ^('>=' a=expr b=expr)
	{
		$value = new ExprBin(">=", a, b);
	}
	| ^('<' a=expr b=expr)
	{
		$value = new ExprBin("<", a, b);
	}
	| ^('<=' a=expr b=expr)
	{
		$value = new ExprBin("<=", a, b);
	}
	| ^('!=' a=expr b=expr)
	{
		$value = new ExprBin("!=", a, b);
	}
	| ^('!' a=expr)
	{
		$value = new ExprPrefix("!", a);
	}
	| ^('&' a=expr)
	{
		$value = new ExprPrefix("&", a);
	}
	| INT
    {
        $value = new ExprConst($INT.Text);
    }
	| DOUBLE
	{
		$value = new ExprConst($DOUBLE.Text);
	}
	| IDENT
	{
		$value = new ExprConst($IDENT.Text);
	}
	| STRING
	{
		$value = new ExprConst($STRING.Text);
	}
	;
