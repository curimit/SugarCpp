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
	: (a = node  { $value.List.Add(a); })+
	;

node returns [AstNode value]
	: a = func_def { $value = a; }
	| b = imports { $value = b; }
	;

imports returns [Import value]
@init
{
	$value = new Import();
}
	: 'import' (a = STRING { $value.NameList.Add(a.Text); })?
	  (INDENT (NEWLINE+ b = STRING { $value.NameList.Add(b.Text); })+ NEWLINE* DEDENT)? NEWLINE*
	;

func_def returns [FuncDef value]
	: a=IDENT b=IDENT '(' ')' c=stmt_block NEWLINE*
	{
		$value = new FuncDef(a.Text, b.Text, c);
	}
	;

stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: INDENT (NEWLINE+ a=stmt { $value.StmtList.Add(a); })+ NEWLINE* DEDENT
    ; 

stmt returns [Stmt value]
	: a=expr { $value = a; }
	| b=stmt_if { $value = b; }
	| c=stmt_while { $value = c; }
	;
	
stmt_if returns [StmtIf value]
	: 'if' a=expr b=stmt_block ('else' c=stmt_block)?
	{
		$value = new StmtIf();
		$value.Condition = a;
		$value.Body = b;
		$value.Else = c;
	}
	;

stmt_while returns [StmtWhile value]
	: 'while' a=expr b=stmt_block
	{
		$value = new StmtWhile();
		$value.Condition = a;
		$value.Body = b;
	}
	;

public alloc_expr returns [ExprAlloc value]
	: ^(Expr_Alloc a=IDENT b=IDENT (c=expr)?)
	{
		$value = new ExprAlloc();
		$value.Type = a.Text;
		$value.Name = b.Text;
		$value.Expr = c;
	}
	;

public expr returns [Expr value]
    : alloc=alloc_expr
	{
		$value = alloc;
	}
	| ^('=' a=expr b=expr)
	{
		$value = new ExprAssign(a, b);
	}
	| ^('+' a=expr b=expr)
	{
		$value = new ExprBin("+", a, b);
	}
	| ^('-' a=expr b=expr)
	{
		$value = new ExprBin("-", a, b);
	}
	| ^('*' a=expr b=expr)
	{
		$value = new ExprBin("*", a, b);
	}
	| ^('/' a=expr b=expr)
	{
		$value = new ExprBin("/", a, b);
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
