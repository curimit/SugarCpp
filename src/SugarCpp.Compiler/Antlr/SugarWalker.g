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
    public string Alias(string op) 
    {
		if (op == "is") return "==";
		if (op == "isnt") return "!=";
		if (op == "not") return "!";
		if (op == "and") return "&&";
		if (op == "or") return "||";
		return op;
    }
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
@init
{
	$value = new GlobalUsing();
}
	: ^(Stmt_Using (attr=attribute { $value.Attribute = attr; } )? ( a=ident { $value.List.Add(a); }
																   | b='namespace' { $value.List.Add("namespace"); })*)
	;

global_alloc returns [List<GlobalAlloc> value]
@init
{
	$value = new List<GlobalAlloc>();
}
	: ^(Expr_Alloc_Equal (attr=attribute)? a=type_name b=ident_list (c=expr_list)?)
	{
		if (c != null)
		{
			$value.Add(new GlobalAlloc(a, b, c, attr, AllocType.Equal));
		}
		else
		{
			$value.Add(new GlobalAlloc(a, b, c, attr, AllocType.Declare));
		}
	}
	| ^(Expr_Alloc_Bracket (attr=attribute)? a=type_name b=ident_list (c=expr_list)?)
	{
		$value.Add(new GlobalAlloc(a, b, c, attr, AllocType.Bracket));
	}
	| ^(':=' (attr=attribute)? d=ident_list e=expr_list)
	{
		int k = 0;
		for (int i = 0; i < d.Count(); i++)
		{
			$value.Add(new GlobalAlloc(new AutoType(), d[i], e[k], attr, AllocType.Equal));
			k = (k + 1) \% e.Count();
		}
	}
	;

global_typedef returns [GlobalTypeDef value]
	: ^(Stmt_Typedef (attr=attribute)? a=type_name b=ident)
	{
		$value = new GlobalTypeDef(a, b);
		if (attr != null) $value.Attribute = attr;
	}
	;

attribute_args returns [string value]
	: a=NUMBER
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
	: ^(Namespace a=ident (b=global_block)?)
	{
		$value = new Namespace(a, b);
	}
	;

import_def returns [Import value]
@init
{
	$value = new Import();
}
	: ^(Import (attr=attribute { $value.Attribute = attr; } )? (a=STRING { $value.NameList.Add(a.Text); })*)
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

type_ident returns [SugarType value]
@init
{
	string type = "";
}
	: ^(Type_Ident ('const' {type+="const";})? ('unsigned' {type+="unsigned";})? a=ident {type+=a;})
	{
		$value = new IdentType(type);
	}
	;

type_template returns [SugarType value]
@init
{
	List<SugarType> list = new List<SugarType>();
}
	: ^(Type_Template a=type_name (b=type_name {list.Add(b);})*)
	{
		$value = new TemplateType(a, list);
	}
	;

type_array returns [SugarType value]
@init
{
	List<Expr> list = new List<Expr>();
}
	: ^(Type_Array a=type_name (b=expr { list.Add(b); })+)
	{
		$value = new ArrayType(a, list);
	}
	;

type_star returns [SugarType value]
	: ^(Type_Star a=type_name '*' { $value = new StarType(a); } ('*' { $value = new StarType($value); })*)
	;


type_ref returns [SugarType value]
	: ^(Type_Ref a=type_name)
	{
		$value = new RefType(a);
	}
	;

type_name returns [SugarType value]
	: a=type_array { $value = a; }
	| a=type_ref { $value = a; }
	| a=type_star { $value = a; }
	| a=type_template { $value = a; }
	| a=type_ident { $value = a; }
	;

func_args returns [List<ExprAlloc> value]
@init
{
	$value = new List<ExprAlloc>();
}
	: ^(Func_Args (a=alloc_expr
	{
		$value.Add(a);
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
		if ((a is IdentType && ((IdentType)a).Type=="void") || a == null)
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
			$value.Add(new StmtExpr(new ExprAlloc(new AutoType(), d[i], e[k], AllocType.Equal)));
			k = (k + 1) \% e.Count();
		}
	}
	;

stmt_defer returns [Stmt value]
	: ^(Stmt_Defer a=stmt)
	{
		$value = new StmtDefer(a[0]);
	}
	| ^(Stmt_Finally a=stmt)
	{
		$value = new StmtFinally(a[0]);
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
	| ^(Stmt_Unless a=expr b=stmt_block (c=stmt_block)?)
	{
		$value = new StmtIf(new ExprPrefix("!", new ExprBracket(a)), b, c);
	}
	;

stmt_while returns [Stmt value]
	: ^(Stmt_While a=expr b=stmt_block)
	{
		$value = new StmtWhile(a, b);
	}
	| ^(Stmt_Until a=expr b=stmt_block)
	{
		$value = new StmtWhile(new ExprPrefix("!", new ExprBracket(a)), b);
	}
	| ^(Stmt_Loop (a=expr)? b=stmt_block)
	{
		if (a == null)
		{
			$value = new StmtWhile(new ExprConst("true", ConstType.Ident), b);
		}
		else
		{
			/*Expr iter = new ExprConst("_t_loop_iterator", ConstType.Ident);
			Expr start = new ExprAlloc(new AutoType(), "_t_loop_iterator", a, true);
			Expr condition = new ExprBin("!=", iter, new ExprConst("0", ConstType.Number));
			Expr next = new ExprPrefix("--", iter);
			$value = new StmtFor(start, condition, next, b);*/
			throw new Exception("Not Implement!");
		}
	}
	;

for_item returns [ForItem value]
	: ^(For_Item_To a=ident b=expr c=expr (d=expr)?)
	{
		$value = new ForItemTo(a, b, c, d);
	}
	| ^(For_Item_Down_To a=ident b=expr c=expr (d=expr)?)
	{
		$value = new ForItemDownTo(a, b, c, d);
	}
	| ^(For_Item_Each a=ident b=expr)
	{
		$value = new ForItemEach(a, b);
	}
	| ^(For_Item_When b=expr)
	{
		$value = new ForItemWhen(b);
	}
	;

for_item_list returns [List<ForItem> value]
@init
{
	$value = new List<ForItem>();
}
	: (a=for_item { $value.Add(a); } )+
	;

stmt_for returns [Stmt value]
	: ^(Stmt_For a=for_item_list b=stmt_block)
	{
		$value = new StmtFor(a, b);
	}
	;

stmt_try returns [Stmt value]
	: ^(Stmt_Try a=stmt_block b=stmt_alloc c=stmt_block)
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
		if (c != null)
		{
			$value = new ExprAlloc(a, b, c, AllocType.Equal);
		}
		else
		{
			$value = new ExprAlloc(a, b, c, AllocType.Declare);
		}
	}
	| ^(Expr_Alloc_Bracket a=type_name b=ident_list (c=expr_list)?)
	{
		$value = new ExprAlloc(a, b, c, AllocType.Bracket);
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

cast_expr returns [ExprCast value]
	: ^(Expr_Cast a=type_name b=expr)
	{
		$value = new ExprCast(a, b);
	}
	;

list_expr returns [ExprList value]
	: ^(Expr_List a=expr_list?)
	{
		$value = new ExprList(a);
	}
	;

chain_expr returns [Expr value]
@init
{
	Expr last;
}
	: ^(Expr_Chain
			a=expr
			{
				last=a;
			}
			op=('<' | '<=' | '>' | '>=' | '!=' | '==' | 'is' | 'isnt') a=expr
			{
				$value = new ExprBin(Alias(op.Text), last, a);
				last = a;
			}
			(
				op=('<' | '<=' | '>' | '>=' | '!=' | '==' | 'is' | 'isnt') a=expr
			{
				$value = new ExprBin("&&", $value, new ExprBin(Alias(op.Text), last, a));
				last = a;
			})*
	   )
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
	| cast=cast_expr
	{
		$value = cast;
	}
	| list=list_expr
	{
		$value = list;
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
	| chain = chain_expr
	{
		$value = chain;
	}
	| ^(Expr_Bin op=( '+' | '-' | '*' | '/' | '%'
					| '<' | '<=' | '>' | '>=' | '==' | '!='
					| '<<' | '>>'
					| '&' | '^' | '|'
					| '&&' | '||'
					| 'is' | 'isnt'
					| 'and' | 'or'
					) a=expr b=expr)
	{
		$value = new ExprBin(Alias(op.Text), a, b);
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
		$value = new ExprAlloc(new AutoType(), ((ExprConst)a).Text, b, AllocType.Equal);
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
