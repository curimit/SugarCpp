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
	Hashtable memory = new Hashtable();
}

@namespace { SugarCpp.Compiler }

public root returns [Root value]
@init
{
	$value = new Root();
}
	: a = func_def { $value.FuncList.Add(a); }
	;

public func_def returns [FuncDef value]
	: ^(Func_Def a=stmt_block) { $value = new FuncDef(); $value.VarList = a; }  
	;

public stmt_block returns [List<string> value]
@init
{
	value = new List<string>();
}
	: a=stmt { $value.Add(a); }  
    ;  

public stmt returns [string value]  
    : INT
    {
        $value = $INT.text;
    }  
	;

