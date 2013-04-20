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

public node returns [AstNode value]
	: a = func_def { $value = a; }
	| b = imports { $value = b; }
	;

public imports returns [Import value]
@init
{
	$value = new Import();
}
	: 'import' (a = STRING { $value.NameList.Add(a.Text); })?
	  (INDENT (b = STRING { $value.NameList.Add(b.Text); })+ DEDENT)?
	;

public func_def returns [FuncDef value]
	: a=IDENT b=IDENT c=stmt_block
	{
		$value = new FuncDef(a.Text, b.Text, c);
	}  
	;

public stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: INDENT (a=stmt { $value.StmtList.Add(a); })+ DEDENT
    ; 

public stmt returns [Stmt value]
	: a=expr { $value = a; }
	;

public expr returns [Expr value]
    : ^('=' a=expr b=expr)
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