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
	: (a = func_def { $value.FuncList.Add(a); })+
	;

public func_def returns [FuncDef value]
	: a=IDENT b=IDENT c=stmt_block
	{
		$value = new FuncDef(a.Text,b.Text,c);
	}  
	;

public stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: (a=expr { $value.StmtList.Add(a); })*
    ;  

public expr returns [Expr value]  
    : ^('=' a=expr b=expr) { $value = new ExprAssign(a, b); }
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