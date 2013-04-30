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
	: a=overall_block NEWLINE*
	{
		$value = new Root(a);
	}
	;

overall_block returns [List<AstNode> value]
@init
{
	$value = new List<AstNode>();
}
	: (NEWLINE* a=node { $value.Add(a); } )+
	;

node returns [AstNode value]
	: a = func_def { $value = a; }
	| b = import_def { $value = b; }
	| c = enum_def { $value = c; }
	| d = class_def { $value = d; }
	| e = stmt_alloc { $value = e; }
	| f = namespace_def { $value = f; }
	| g = stmt_using { $value = g; }
	| h = stmt_typedef { $value = h; }
	;

namespace_def returns [Namespace value]
	: ^(Namespace a=ident b=overall_block)
	{
		$value = new Namespace(a, b);
	}
	;

import_def returns [Import value]
@init
{
	$value = new Import();
}
	: ^(Import (a=STRING { $value.NameList.Add(a.Text); })*)
	;

enum_def returns [Enum value]
@init
{
	$value = new Enum();
}
	: ^(Enum a=ident { $value.Name=a; } (a=ident { $value.Values.Add(a); })*)
	;

class_def returns [Class value]
	: ^(Class a=ident b=class_block)
	{
		$value = new Class(a, b);
	}
	;

class_block returns [List<ClassMember> value]
@init
{
	$value = new List<ClassMember>();
}
	: (NEWLINE* a=class_node { $value.Add(a); } )+
	;

attribute returns [List<string> value]
@init
{
	value = new List<string>();
}
	: ^(Attribute (a=ident { $value.Add(a); })+)
	;

class_node returns [ClassMember value]
@init
{
	HashSet<string> set = new HashSet<string>();
}
	: (a=attribute { foreach (var x in a) set.Add(x); } NEWLINE+)* b=node
	{
		$value = new ClassMember(b, set);
	}
	;

type_name returns [string value]
@init
{
	$value = "";
}
	: ^( Type_IDENT a=ident { $value+=a; }
	   ( '<' { $value+="<"; bool isFirst = true; }
	    (b=type_name
		{
			if (!isFirst) $value+=", ";
			isFirst = false;
			$value+=b;
		})*
		'>' { $value+=">"; })?
	  ( '*' { $value="shared_ptr<"+$value+">"; }
	  | '[' ']' { $value="vector<"+$value+">"; }
	  | '&' { $value+="&"; }
	  )*)
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
	: a=type_name b=ident ('<' x=ident { $value.GenericParameter.Add(x); } '>')? '(' (args=func_args { $value.Args = args; })? ')'
	( e=stmt_block
	{
		$value.Type = a;
		$value.Name = b;
		$value.Body = e;
	}
	| '=' f=expr
	{
		$value.Type = a;
		$value.Name = b;
		StmtBlock block = new StmtBlock();
		block.StmtList.Add(new ExprReturn(f));
		$value.Body = block;
	}
	)
	;

stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: ^(Stmt_Block (a=stmt { $value.StmtList.Add(a); })*)
    ; 

stmt returns [Stmt value]
	: a=stmt_expr { $value = a; }
	;

stmt_expr returns [Stmt value]
	: a=stmt_alloc { $value = a; }
	| a=stmt_return { $value = a; }
	| a=stmt_typedef { $value = a; }
	| a=stmt_if { $value = a; }
	| a=stmt_while { $value = a; }
	| a=stmt_for { $value = a; }
	| a=stmt_try { $value = a; }
	| b=expr { $value = b; }
	| c=stmt_using { $value = c; }
	;

stmt_using returns [StmtUsing value]
@init
{
	$value = new StmtUsing();
}
	: ^(Stmt_Using ( a=ident { $value.List.Add(a); }
				   | b='namespace' { $value.List.Add("namespace"); })*)
	;

stmt_typedef returns [Stmt value]
	: ^(Stmt_Typedef a=type_name b=ident)
	{
		$value = new StmtTypeDef(a, b);
	}
	;

stmt_alloc returns [Stmt value]
	: a=alloc_expr { $value = a; }
	;

stmt_if returns [Stmt value]
	: ^(Stmt_If a=expr b=stmt_block (c=stmt_block)?)
	{
		$value = new StmtIf(a, b, c);
	}
	;

stmt_while returns [Stmt value]
	: ^(Stmt_While a=expr b=stmt_block)
	{
		$value = new StmtWhile(a, b);
	}
	;

stmt_for returns [Stmt value]
	: ^(Stmt_For a=expr b=expr c=expr d=stmt_block)
	{
		$value = new StmtFor(a, b, c, d);
	}
	| ^(Stmt_ForEach a=expr b=expr d=stmt_block)
	{
		$value = new StmtForEach(a, b, d);
	}
	;

stmt_try returns [Stmt value]
	: ^(Stmt_Try a=stmt_block b=expr c=stmt_block)
	{
		$value = new StmtTry(a, b, c);
	}
	;

stmt_return returns [Stmt value]
	: ^(Expr_Return (a=expr)?)
	{
		$value = new ExprReturn(a);
	}
	;

ident returns [string value]
@init
{
	$value = "";
}
	: a=IDENT { $value = a.Text; } ('::' a=IDENT { $value += "::" + a.Text; })*
	;

ident_list returns [List<Expr> value]
@init
{
	$value = new List<Expr>();
}
	: ^(Ident_List (a=ident { $value.Add(new ExprConst(a)); })+)
	;
	
alloc_expr returns [ExprAlloc value]
	: ^(Expr_Alloc a=type_name b=ident_list (c=expr)?)
	{
		$value = new ExprAlloc(a, b, c);
	}
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

match_tuple returns [MatchTuple value]
@init
{
	$value = new MatchTuple();
}
	: ^(Match_Tuple (a=expr { $value.ExprList.Add(a); })*)
	;

expr_list returns [List<Expr> value]
@init
{
	$value = new List<Expr>();
}
	: (a=expr { $value.Add(a); })+
	;

call_expr returns [ExprCall value]
@init
{
	$value = new ExprCall();
}
	: ^(Expr_Call a=expr { $value.Expr=a; } ('<' (x=ident { $value.GenericParameter.Add(x); })* '>')? (b=expr_list { $value.Args=b; })?)
	;

dict_expr returns [Expr value]
	: ^(Expr_Dict a=expr (b=expr_list)?)
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

new_expr returns [Expr value]
	: ^(Expr_New_Type a=type_name b=expr_list?)
	{
		$value = new ExprNewType(a, b);
	}
	| ^(Expr_New_Array a=type_name b=expr_list)
	{
		$value = new ExprNewArray(a, b);
	}
	;

call_with_expr returns [ExprCall value]
	: ^(Expr_Call_With a=expr b=ident c=expr_list?)
	{
		List<Expr> Args = new List<Expr>();
		Args.Add(a);
		if (c != null)
		{
			foreach (var item in c)
			{
				Args.Add(item);
			}
		}
		$value = new ExprCall();
		$value.Expr = new ExprConst(b);
		$value.Args = Args;
	}
	;

expr returns [Expr value]
    : tuple=expr_tuple
	{
		$value = tuple;
	}
	| match=match_tuple
	{
		$value = match;
	}
	| call=call_expr
	{
		$value = call;
	}
	| call_with=call_with_expr
	{
		$value = call_with;
	}
	| dict=dict_expr
	{
		$value = dict;
	}
	| lambda=lambda_expr
	{
		$value = lambda;
	}
	| expr_new=new_expr
	{
		$value = expr_new;
	}
	| ^(Expr_Infix ident_text=ident a=expr b=expr)
	{
		$value = new ExprInfix(ident_text, a, b);
	}
	| ^(Expr_Cond a=expr b=expr c=expr)
	{
		$value = new ExprCond(a, b, c);
	}
	| ^(Expr_Access op=('.' | '::' | '->' | '->*' | '.*') a=expr ident_text=ident)
	{
		$value = new ExprAccess(a, op.Text, ident_text);
	}
	| ^(Expr_Bin op=( '+' | '-' | '*' | '/'
					| '<' | '<=' | '>' | '>=' | '==' | '!='
					| '<<' | '>>'
					| '&' | '^' | '|'
					| '&&' | '||'
					) a=expr b=expr)
	{
		$value = new ExprBin(op.Text, a, b);
	}
	| ^(op=('=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=') a=expr b=expr)
	{
		$value = new ExprBin(op.Text, a, b);
	}
	| ^(':=' a=expr b=expr)
	{
		$value = new ExprAlloc("auto", new List<Expr> { a }, b);
	}
	| ^(Expr_Suffix op=('++' | '--') a=expr)
	{
		$value = new ExprSuffix(op.Text, a);
	}
	| ^(Expr_Prefix op=('!' | '~' | '++' | '--' | '-' | '+' | '*' | '&') a=expr)
	{
		$value = new ExprPrefix(op.Text, a);
	}
	| text_ident = ident
	{
		$value = new ExprConst(text_ident);
	}
	| text=(NUMBER | DOUBLE | STRING)
    {
        $value = new ExprConst(text.Text);
    }
	;
