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
	: ^(Root a=global_block)
	{
		$value = new Root(a);
	}
	;

global_block returns [GlobalBlock value]
@init
{
	$value = new GlobalBlock();
}
	: ^(Global_Block (a=node { foreach (var x in a) $value.List.Add(x); })*)
	;

node returns [List<AttrAstNode> value]
@init
{
	$value = new List<AttrAstNode>();
}
	: a = func_def { $value.Add(a); }
	| b = import_def { $value.Add(b); }
	| c = enum_def { $value.Add(c); }
	| d = class_def { $value.Add(d); }
	| e = global_alloc { foreach (var x in e) $value.Add(x); }
	| f = global_using { $value.Add(f); }
	| g = global_typedef { $value.Add(g); }
	| h = namespace_def { $value.Add(h); }
	;

global_using returns[GlobalUsing value]
	: a=stmt_using
	{
		$value = new GlobalUsing(a.List);
	}
	;

global_alloc returns [List<GlobalAlloc> value]
@init
{
	$value = new List<GlobalAlloc>();
}
	: ^(Expr_Alloc_Equal (attr=attribute)? a=type_name b=ident_list (c=expr_list)?)
	{
		$value.Add(new GlobalAlloc(a, b, c, attr, true));
	}
	| ^(Expr_Alloc_Bracket (attr=attribute)? a=type_name b=ident_list (c=expr_list)?)
	{
		$value.Add(new GlobalAlloc(a, b, c, attr, false));
	}
	| ^(':=' (attr=attribute)? d=ident_list e=expr_list)
	{
		int k = 0;
		for (int i = 0; i < d.Count(); i++)
		{
			$value.Add(new GlobalAlloc("auto", new List<string> { d[i] }, new List<Expr>{ e[k] }, attr, true));
			k = (k + 1) \% e.Count();
		}
	}
	;

global_typedef returns [GlobalTypeDef value]
	: a=stmt_typedef
	{
		$value = new GlobalTypeDef(a.Type, a.Name);
	}
	;

attribute_args returns [string value]
	: a=(NUMBER)
	{
		$value = a.Text;
	}
	| a=STRING
	{
		$value = a.Text.Substring(1, a.Text.Length - 2);
	}
	| b=ident
	{
		$value = b;
	}
	;

attribute_item returns [Attr value]
@init
{
	$value = new Attr();
}
	: ^(Attribute (a=ident { $value.Name = a; } | c='const' { $value.Name = "const"; }) (b=attribute_args { $value.Args.Add(b) ; })*)
	;

attribute returns [List<Attr> value]
@init
{
	$value = new List<Attr>();
}
	: (a=attribute_item { $value.Add(a); } )+
	;

namespace_def returns [Namespace value]
	: ^(Namespace a=ident b=global_block)
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
	: ^(Enum (attr=attribute)? a=ident b=ident_list)
	{
		$value = new Enum(a, b, attr);
	}
	;

class_def returns [Class value]
	: ^(Class (attr=attribute)? a=ident (b=generic_parameter)? (c=func_args)? (d=ident_list)? (e=global_block)?)
	{
		$value = new Class(a, b, c, d, e, attr);
	}
	;

type_name returns [string value]
@init
{
	$value = "";
}
	: ^( Type_IDENT
	     ('const' { $value += "const "; })?
		 ('unsigned' { $value += "unsigned "; })?
	     a=ident { $value+=a; }
	   ( '<' { $value+="<"; bool isFirst = true; }
	    (b=type_name
		{
			if (!isFirst) $value+=", ";
			isFirst = false;
			$value+=b;
		})*
		'>' { $value+=">"; })?
	  ( '*' { $value+="*"; }
	  | '[' ']' { $value+="[]"; }
	  | '&' { $value+="&"; }
	  )*)
	;

func_args returns [List<ExprAlloc> value]
@init
{
	$value = new List<ExprAlloc>();
}
	: ^(Func_Args (a=stmt_alloc
	{
		var b = (ExprAlloc)a;
		if (b.Type == "auto")
		{
			b.Type = "decltype";
		}
		$value.Add(b);
	})*)
	;

generic_parameter returns [List<string> value]
@init
{
	$value = new List<string>();
}
	: ^(Generic_Patameters (a=ident { $value.Add(a); })*)
	;

func_def returns [FuncDef value]
@init
{
	$value = new FuncDef();
}
	: ^(Func_Def (attr=attribute)? (a=type_name)? (deconstructor='~')? b=ident (x=generic_parameter )? (args=func_args { $value.Args = args; })?
	( e=stmt_block
	{
		if (attr != null) $value.Attribute = attr;
		$value.Type = a;
		$value.Name = b;
		if (deconstructor != null) 
		{
			$value.Name = "~" + $value.Name;
		}
		if (x != null)
		{
			$value.GenericParameter = x;
		}
		$value.Body = e;
	}
	| f=expr
	{
		if (attr != null) $value.Attribute = attr;
		$value.Type = a;
		$value.Name = b;
		if (deconstructor != null) 
		{
			$value.Name = "~" + $value.Name;
		}
		StmtBlock block = new StmtBlock();
		if (a == "void" || a == null)
		{
			block.StmtList.Add(new StmtExpr(f));
		}
		else
		{
			block.StmtList.Add(new StmtReturn(f));
		}
		if (x != null)
		{
			$value.GenericParameter = x;
		}
		$value.Body = block;
	}
	))
	;

stmt_block returns [StmtBlock value]
@init
{
	$value = new StmtBlock();
}
	: ^(Stmt_Block (a=stmt { foreach (var x in a ) $value.StmtList.Add(x); })*)
    ; 

stmt returns [List<Stmt> value]
@init
{
	$value = new List<Stmt>();
}
	: a=stmt_expr { $value.Add(new StmtExpr(a)); }
	| a=stmt_return { $value.Add(a); }
	| a=stmt_if { $value.Add(a); }
	| a=stmt_while { $value.Add(a); }
	| a=stmt_for { $value.Add(a); }
	| a=stmt_try { $value.Add(a); }
	| a=stmt_linq { $value.Add(a); }
	| a=stmt_defer { $value.Add(a); }
	| b=stmt_translate { foreach (var x in b) $value.Add(x); }
	;

stmt_translate returns [List<Stmt> value]
@init
{
	$value = new List<Stmt>();
}
	: ^('?=' a=expr b=expr)
	{
		StmtBlock block = new StmtBlock();
		block.StmtList.Add(new StmtExpr(new ExprAssign(a, b)));
		StmtIf stmt_if = new StmtIf(new ExprBin("==", a, new ExprConst("nullptr", ConstType.Ident)), block, null);
		$value.Add(stmt_if);
	}
	| ^(':=' d=ident_list e=expr_list)
	{
		int k = 0;
		for (int i = 0; i < d.Count(); i++)
		{
			$value.Add(new StmtExpr(new ExprAlloc("auto", new List<string> { d[i] }, new List<Expr>{ e[k] }, true)));
			k = (k + 1) \% e.Count();
		}
	}
	;

stmt_defer returns [Stmt value]
	: ^(Stmt_Defer a=stmt)
	{
		$value = new StmtDefer(a[0]);
	}
	;

stmt_expr returns [Stmt value]
	: a=stmt_using { $value = a; }
	| b=expr { $value = b; }
	| c=stmt_typedef { $value = c; }
	;

stmt_using returns [StmtUsing value]
@init
{
	$value = new StmtUsing();
}
	: ^(Stmt_Using ( a=ident { $value.List.Add(a); }
				   | b='namespace' { $value.List.Add("namespace"); })*)
	;

stmt_typedef returns [StmtTypeDef value]
	: ^(Stmt_Typedef a=type_name b=ident)
	{
		$value = new StmtTypeDef(a, b);
	}
	;

stmt_alloc returns [Stmt value]
	: a=expr { $value = a; }
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
	| ^(Stmt_Loop b=stmt_block)
	{
		$value = new StmtWhile(new ExprConst("true", ConstType.Ident), b);
	}
	;

stmt_for returns [Stmt value]
	: ^(Stmt_For_To a=ident b=expr c=expr (d=expr)? e=stmt_block)
	{
		ExprConst variable = new ExprConst(a, ConstType.Ident);
		ExprAlloc start = new ExprAlloc("auto", new List<string>{ a }, new List<Expr> { b }, true);
		ExprBin condition = new ExprBin("<=", variable, c);
		Expr next = null;
		if (d == null)
			next = new ExprPrefix("++", variable);
		else next = new ExprAssign(variable, new ExprBin("+", variable, d));
		$value = new StmtFor(start, condition, next, e);
	}
	| ^(Stmt_For_Down_To a=ident b=expr c=expr (d=expr)? e=stmt_block)
	{
		ExprConst variable = new ExprConst(a, ConstType.Ident);
		ExprAlloc start = new ExprAlloc("auto", new List<string>{ a }, new List<Expr> { b }, true);
		ExprBin condition = new ExprBin(">=", variable, c);
		Expr next = null;
		if (d == null)
			next = new ExprPrefix("--", variable);
		else next = new ExprAssign(variable, new ExprBin("+", variable, d));
		$value = new StmtFor(start, condition, next, e);
	}
	| ^(Stmt_ForEach (own='&')? a=ident b=expr e=stmt_block)
	{
		if (own != null) a = "&" + a;
		$value = new StmtForEach(new ExprConst(a, ConstType.Ident), b, e);
	}
	;

stmt_try returns [Stmt value]
	: ^(Stmt_Try a=stmt_block b=expr c=stmt_block)
	{
		$value = new StmtTry(a, b, c);
	}
	;

stmt_return returns [Stmt value]
	: ^(Stmt_Return (a=expr)?)
	{
		$value = new StmtReturn(a);
	}
	;

linq_item returns [LinqItem value]
	: ^(Linq_From x=expr b=expr)
	{
		$value = new LinqFrom(x, b);
	}
	| ^(Linq_Let a=ident b=expr)
	{
		$value = new LinqLet(a, b);
	}
	| ^(Linq_Where b=expr)
	{
		$value = new LinqWhere(b);
	}
	;

linq_prefix returns [List<LinqItem> value]
@init
{
	$value = new List<LinqItem>();
}
	: ^(Linq_Prefix (a=linq_item { $value.Add(a); })+)
	;

stmt_linq returns [Stmt value]
	: ^(Stmt_Linq a=linq_prefix b=stmt_block)
	{
		$value = new StmtLinq(a, b);
	}
	;

ident returns [string value]
@init
{
	$value = "";
}
	: a=IDENT { $value += a.Text; } ('::' a=IDENT { $value += "::" + a.Text; })*
	;

ident_list returns [List<string> value]
@init
{
	$value = new List<string>();
}
	: ^(Ident_List (a=ident { $value.Add(a); })*)
	;
	
alloc_expr returns [ExprAlloc value]
	: ^(Expr_Alloc_Equal a=type_name b=ident_list (c=expr_list)?)
	{
		$value = new ExprAlloc(a, b, c, true);
	}
	| ^(Expr_Alloc_Bracket a=type_name b=ident_list (c=expr_list)?)
	{
		$value = new ExprAlloc(a, b, c, false);
	}
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
	: ^(Expr_Call a=expr (b=generic_parameter)? (c=expr_list)?)
	{
		$value = new ExprCall(a, b, c);
	}
	;

dict_expr returns [Expr value]
	: ^(Expr_Dict a=expr (b=expr_list)?)
	{
		$value = new ExprDict(a, b);
	}
	;

lambda_expr returns [ExprLambda value]
	: ^(Expr_Lambda '->' (b=func_args)? a=expr)
	{
		$value = new ExprLambda(a, b, true);
	}
	| ^(Expr_Lambda '=>' (b=func_args)? a=expr)
	{
		$value = new ExprLambda(a, b, false);
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
		$value = new ExprCall(new ExprConst(b, ConstType.Ident), null, Args);
	}
	;

expr returns [Expr value]
    : tuple=expr_tuple
	{
		$value = tuple;
	}
	| alloc=alloc_expr
	{
		$value = alloc;
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
	| ^(Expr_Cond_Not_Null a=expr b=expr)
	{
		$value = new ExprCond(new ExprBin("!=", a, new ExprConst("nullptr", ConstType.Ident)), a, b);
	}
	| ^(Expr_Not_Null a=expr)
	{
		$value = new ExprBin("!=", a, new ExprConst("nullptr", ConstType.Ident));
	}
	| ^(Expr_Access op=('.' | '::' | '->' | '->*' | '.*') a=expr ident_text=ident)
	{
		$value = new ExprAccess(a, op.Text, ident_text);
	}
	| ^(Expr_Bin op=( '+' | '-' | '*' | '/' | '%'
					| '<' | '<=' | '>' | '>=' | '==' | '!='
					| '<<' | '>>'
					| '&' | '^' | '|'
					| '&&' | '||'
					) a=expr b=expr)
	{
		$value = new ExprBin(op.Text, a, b);
	}
	| ^('and' a=expr b=expr)
	{
		$value = new ExprBin("&&", a, b);
	}
	| ^('or' a=expr b=expr)
	{
		$value = new ExprBin("||", a, b);
	}
	| ^('is' a=expr b=expr)
	{
		$value = new ExprBin("==", a, b);
	}
	| ^('isnt' a=expr b=expr)
	{
		$value = new ExprBin("!=", a, b);
	}
	| ^(op=('=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=') a=expr b=expr)
	{
		$value = new ExprBin(op.Text, a, b);
	}
	| ^('@' text_ident=ident)
	{
		$value = new ExprAccess(new ExprConst("this", ConstType.Ident), "->", text_ident);
	}
	| ^(Expr_Bracket a=expr)
	{
		$value = new ExprBracket(a);
	}
	| ^(Expr_Suffix op=('++' | '--') a=expr)
	{
		$value = new ExprSuffix(op.Text, a);
	}
	| ^(Expr_Prefix op=('!' | '~' | '++' | '--' | '-' | '+' | '*' | '&') a=expr)
	{
		$value = new ExprPrefix(op.Text, a);
	}
	| ^(':=' a=expr b=expr)
	{
		if (!(a is ExprConst))
		{
			throw new Exception("Assert failed.");
		}
		$value = new ExprAlloc("auto", new List<string> { ((ExprConst)a).Text }, new List<Expr> { b }, true);
	}
	| text_ident = ident
	{
		if (text_ident == "nil") text_ident = "nullptr";
		$value = new ExprConst(text_ident, ConstType.Ident);
	}
	| text=(NUMBER | DOUBLE)
    {
        $value = new ExprConst(text.Text, ConstType.Number);
    }
	| text = STRING
	{
        $value = new ExprConst(text.Text, ConstType.String);
	}
	;
