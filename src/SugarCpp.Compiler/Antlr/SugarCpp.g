grammar SugarCpp;

options
{
    output=AST;  
    ASTLabelType=CommonTree;  
    language=CSharp3;
}

tokens
{
   INDENT;
   DEDENT;
   
   Root;
   Block;
   Import;
   Enum;
   Class;
   Namespace;

   Attribute;
   Generic_Patameters;

   Func_Def;

   Global_Block;
   
   Stmt_Block;

   Stmt_Defer;
   
   Stmt_Using;
   Stmt_Typedef;

   Stmt_If;
   Stmt_While;
   Stmt_Loop;
   Stmt_For;
   Stmt_ForEach;
   Stmt_Try;

   Stmt_Return;

   Stmt_Linq;
   Linq_Prefix;
   Linq_From;
   Linq_Let;
   Linq_Where;

   Type_IDENT;
   Type_Ref;
   Type_Tuple;

   Func_Args;
   
   Expr_Alloc_Equal;
   Expr_Alloc_Bracket;

   Expr_Bracket;

   Expr_Not_Null;
   Expr_Cond_Not_Null;

   Expr_Cond;
   Expr_New_Type;
   Expr_New_Array;
   Expr_Bin;
   
   Expr_Bin;
   Expr_Suffix;
   Expr_Prefix;

   Expr_Access;
   Expr_Dict;
   Expr_Call;
   Expr_Call_With;

   Expr_Infix;

   Expr_Lambda;

   Expr_Tuple;

   Ident_List;
   Match_Tuple;
}

@lexer::header
{
	using System;
	using System.Collections;
    using System.Collections.Generic;
	using System.Linq;
}

@lexer::members
{
	class Indentation
	{
		public int Level;
		public int CharIndex;

		public Indentation(int Level, int CharIndex)
		{
			this.Level = Level;
			this.CharIndex = CharIndex;
		}
	}

	int CurrentIndent = 0;
	Stack<Indentation> Indents = new Stack<Indentation>();
	Stack<int>[] Bracket = new Stack<int>[3];

	Queue<IToken> tokens = new Queue<IToken>();

    public override void Emit(IToken token) 
    {
        state.token = token;
        tokens.Enqueue(token);
    }

    public override IToken NextToken()
    {
        base.NextToken();
        if (tokens.Count == 0)
		{
			if (Indents != null && Indents.Count > 0)
			{
				Emit(new CommonToken(NEWLINE, "NEWLINE"));
				Emit(new CommonToken(DEDENT, "DEDENT"));
				Indents.Pop();
				CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
				base.NextToken();
				return tokens.Dequeue();
			}
			if (Indents != null)
			{
				Indents = null;
				return new CommonToken(NEWLINE, "NEWLINE");
			}
            return new CommonToken(EOF, "EOF");
		}
        return tokens.Dequeue();
    }
} 

@lexer::init {
	CurrentIndent = 0;
	Bracket[0] = Stack<int>();
	Bracket[1] = Stack<int>();
	Bracket[2] = Stack<int>();
	Console.WriteLine("Init!");
}

@parser::header
{
	using System;
	using System.Collections;
    using System.Collections.Generic;
	using System.Linq;
}

@lexer  :: namespace { SugarCpp.Compiler }
@parser :: namespace { SugarCpp.Compiler }

public root
	: NEWLINE* global_block EOF -> ^(Root global_block)
	;

global_block
	: (node NEWLINE+)* -> ^(Global_Block node*)
	;

node
	: func_def
	| class_def
	| enum_def
	| global_alloc
	| global_using
	| global_typedef
	| import_def
	| namespace_def
	;
	
attribute_args
	: NUMBER
	| STRING
	| ident
	;

attribute_item
	: ident ('(' attribute_args (',' attribute_args)* ')')? -> ^(Attribute ident attribute_args*)
	| 'const' ('(' attribute_args (',' attribute_args)* ')')? -> ^(Attribute 'const' attribute_args*)
	;

attribute
	: ('[' attribute_item (',' attribute_item)* ']' NEWLINE+)+ -> attribute_item+
	;

global_alloc
	: attribute? ident_list ( ':' type_name ( ('=' | ':=') expr -> ^(Expr_Alloc_Equal attribute? type_name ident_list expr?)
	                                       | '(' expr_list? ')' -> ^(Expr_Alloc_Bracket attribute? type_name ident_list expr_list?)
								 		   | -> ^(Expr_Alloc_Equal attribute? type_name ident_list)
								  		   )
							| ':=' (modify_expr (',' modify_expr)*) -> ^(':=' attribute? ident_list modify_expr+)
							)
	;

global_using
	: stmt_using
	;

global_typedef
	: stmt_typedef
	;

import_def
	: 'import' STRING? (NEWLINE+ INDENT NEWLINE*  (STRING NEWLINE+)* DEDENT)? -> ^(Import STRING*)
	;

enum_def
	: attribute? 'enum' ident '=' (ident ('|' ident)*)? -> ^(Enum attribute? ident ^(Ident_List ident*))
	;

namespace_def
	: 'namespace' ident NEWLINE+ INDENT NEWLINE* global_block DEDENT -> ^(Namespace ident global_block)
	;

class_def
	:  attribute? 'class' ident (generic_parameter)? ('(' func_args ')')? (':' ident (',' ident)*)? (NEWLINE+ INDENT NEWLINE* global_block DEDENT)? -> ^(Class attribute? ident generic_parameter? func_args? (^(Ident_List ident*))? global_block?)
	;

type_name_op: '*' | '[' ']' | '&' ;
type_name
	: 'const'? 'unsigned'? ident ('<' (type_name (',' type_name)*)? '>')? type_name_op* -> ^(Type_IDENT 'const'? 'unsigned'? ident ('<' type_name* '>')?  type_name_op*)
	;

generic_parameter_inside
	: ident (',' ident)* -> ^(Generic_Patameters ident*)
	;

generic_parameter
	: '<' generic_parameter_inside '>' -> generic_parameter_inside
	;

func_args
	: func_args_item (',' func_args_item)* -> ^(Func_Args func_args_item*)
	;

func_args_item
	: ident_list ':' type_name ( ('=' | ':=') expr  -> ^(Expr_Alloc_Equal type_name ident_list expr?)
	                             | '(' expr_list? ')'  -> ^(Expr_Alloc_Bracket type_name ident_list expr_list?)
							     | -> ^(Expr_Alloc_Equal type_name ident_list)
							     )
	| ':='^  modify_expr
	;

func_def
	: attribute? type_name? '~'? ident generic_parameter? '(' func_args? ')' (NEWLINE+ stmt_block -> ^(Func_Def attribute? type_name? '~'? ident generic_parameter? func_args? stmt_block)
																			 | '=' expr  -> ^(Func_Def attribute? type_name? '~'? ident generic_parameter? func_args? expr))
    ;

stmt_block
	: INDENT NEWLINE*  (stmt NEWLINE+)* DEDENT -> ^(Stmt_Block stmt*)
	;

stmt
	: stmt_expr
	| stmt_if
	| stmt_for
	| stmt_while
	| stmt_try
	| stmt_linq
	| stmt_defer
	;

stmt_expr
	: (a=stmt_expr_item -> $a) ( 'if' expr -> ^(Stmt_If expr ^(Stmt_Block $stmt_expr))
							   | 'while' expr -> ^(Stmt_While expr ^(Stmt_Block $stmt_expr))
							   )?
	;

stmt_expr_item
	: stmt_alloc
	| stmt_return
	| stmt_using
	| stmt_typedef
	| stmt_modify
	;

stmt_defer
	: 'defer' stmt -> ^(Stmt_Defer stmt)
	;

stmt_typedef
	: 'typedef' ident '=' type_name -> ^(Stmt_Typedef type_name ident)
	;

stmt_using_item: ident | 'namespace';
stmt_using
	: 'using' stmt_using_item* -> ^(Stmt_Using stmt_using_item*)
	;

stmt_return
	: 'return' expr? -> ^(Stmt_Return expr?)
	;

stmt_if
	: 'if' expr stmt_block (NEWLINE* 'else' stmt_block)? -> ^(Stmt_If expr stmt_block stmt_block?)
	;

stmt_while
	: 'while' expr stmt_block -> ^(Stmt_While expr stmt_block)
	| 'loop' stmt_block -> ^(Stmt_Loop stmt_block)
	;

stmt_for
@init
{
	int type = 0;
}
	: 'for' '(' expr (';' expr ';' expr {type=0;} | 'in' expr {type=1;}) ')' stmt_block
	  -> {type==0}? ^(Stmt_For expr expr expr stmt_block)
	  -> ^(Stmt_ForEach expr expr stmt_block)
	;

stmt_try
	:	'try' stmt_block 'catch' expr stmt_block -> ^(Stmt_Try stmt_block expr stmt_block)
	;

linq_item
	: 'from' expr 'in' expr -> ^(Linq_From expr expr)
	| 'let' ident '=' expr -> ^(Linq_Let ident expr)
	| 'where' expr -> ^(Linq_Where expr)
	;

linq_prefix
	: (linq_item linq_item* NEWLINE+)+ -> ^(Linq_Prefix linq_item+)
	;

stmt_linq
	: linq_prefix stmt_block -> ^(Stmt_Linq linq_prefix stmt_block)
	;

ident_list
	: ident (',' ident)* -> ^(Ident_List ident+)
	;

stmt_alloc
	: ident_list ( ':' type_name ( ('=' | ':=') expr  -> ^(Expr_Alloc_Equal type_name ident_list expr?)
	                             | '(' expr_list? ')'  -> ^(Expr_Alloc_Bracket type_name ident_list expr_list?)
							     | -> ^(Expr_Alloc_Equal type_name ident_list)
							     )
				 | ':='  (modify_expr (',' modify_expr)*) -> ^(':=' ident_list modify_expr+))
	;

stmt_modify
	: lvalue ( modify_expr_op^ modify_expr
	         | '?='^ modify_expr)?
	;

expr
	: lambda_expr
	;

lambda_expr
	: '\\' '(' func_args? ')' '=>' lambda_expr -> ^(Expr_Lambda func_args? lambda_expr)
	| modify_expr
	;

modify_expr_op: '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=';
modify_expr
	: cond_expr ( (':=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=')^ cond_expr
				| ('='^ cond_expr)+)?
	;

cond_expr_item: or_expr ;
cond_expr
	: (a=or_expr -> $a) ('?' ( a=cond_expr_item ( ':' b=cond_expr_item -> ^(Expr_Cond $a $cond_expr $b)
											   | -> ^(Expr_Cond_Not_Null $cond_expr $a)
											  )
							 | -> ^(Expr_Not_Null $cond_expr)
							 ))?
	;

or_expr
	: (a=and_expr -> $a) ( '||' b=and_expr -> ^(Expr_Bin '||' $or_expr $b)
	                     | 'or' b=and_expr -> ^('or' $or_expr $b))*
	;

and_expr
	: (a=bit_or -> $a) ( '&&' b=bit_or -> ^(Expr_Bin '&&' $and_expr $b)
					   | 'and' b=bit_or -> ^(Expr_Bin 'and' $and_expr $b))*
	;

bit_or
	: (a=bit_xor -> $a) ('|' b=bit_xor -> ^(Expr_Bin '|' $bit_or $b))*
	;

bit_xor
	: (a=bit_and -> $a) ('^' b=bit_and -> ^(Expr_Bin '^' $bit_xor $b))*
	;

bit_and
	: (a=cmp_equ_expr -> $a) ('&' b=cmp_equ_expr -> ^(Expr_Bin '&' $bit_and $b))*
	;

cmp_equ_expr_op: '==' | 'is' | '!=' | 'isnt' ;
cmp_equ_expr
	: (a=cmp_expr -> $a) ( op=cmp_equ_expr_op b=cmp_expr -> ^(Expr_Bin $op $cmp_equ_expr $b) )?
	;
	
cmp_expr
	: (a=infix_expr -> $a) ( '<' b=infix_expr ( {b.Tree.Token.Type == IDENT}? ident* '>' '(' expr_list? ')' -> ^(Expr_Call $cmp_expr ^(Generic_Patameters $b ident*) expr_list?)
	                                          | -> ^(Expr_Bin '<' $cmp_expr $b))
	                       | '<=' b=infix_expr -> ^(Expr_Bin '<=' $cmp_expr $b)
						   | '>' b=infix_expr -> ^(Expr_Bin '>' $cmp_expr $b)
						   | '>=' b=infix_expr -> ^(Expr_Bin '>=' $cmp_expr $b))*
	;

infix_expr
	: (a=shift_expr -> $a) ( infix_func b=shift_expr  -> ^(Expr_Infix infix_func $infix_expr $b) )*
	;

shift_expr_op: '<<' | '>>' ;
shift_expr
	: (a=add_expr -> $a) (shift_expr_op b=add_expr -> ^(Expr_Bin shift_expr_op $shift_expr $b))*
	;

add_expr
	: (a=mul_expr -> $a) ( '+' b=mul_expr -> ^(Expr_Bin '+' $add_expr $b)
						 | '-' b=mul_expr -> ^(Expr_Bin '-' $add_expr $b)
						 )*
	;

mul_expr
	: (a=selector_expr -> $a) ( '*' b=selector_expr -> ^(Expr_Bin '*' $mul_expr $b)
						      | '/' b=selector_expr -> ^(Expr_Bin '/' $mul_expr $b)
						      | '%' b=selector_expr -> ^(Expr_Bin '%' $mul_expr $b)
						      )*
	;

selector_expr
	: (a=prefix_expr -> $a) ( '->*' b=ident -> ^(Expr_Access '->*' $selector_expr $b)
						    | '.*'  b=ident -> ^(Expr_Access '.*'  $selector_expr $b)
						    )*
	;

prefix_expr_op: '!' | '~' | '++' | '--' | '-' | '+' | '*' | '&';
prefix_expr
	: (prefix_expr_op prefix_expr) -> ^(Expr_Prefix prefix_expr_op prefix_expr)
	| 'new' type_name ( '(' expr_list? ')' -> ^(Expr_New_Type type_name expr_list?)
					  | '[' expr_list ']' -> ^(Expr_New_Array type_name expr_list))
	| suffix_expr
	;
	
expr_list
	: expr (','! expr)*
	;

suffix_expr
	: (a=atom_expr -> $a) ( '++' -> ^(Expr_Suffix '++' $suffix_expr)
					      | '--' -> ^(Expr_Suffix '--' $suffix_expr)
						  | '.' ident -> ^(Expr_Access '.' $suffix_expr ident)
						  | '->' ident -> ^(Expr_Access '->' $suffix_expr ident)
						  | '(' expr_list? ')' -> ^(Expr_Call $suffix_expr expr_list?)
						  | '[' expr_list? ']' -> ^(Expr_Dict $suffix_expr expr_list?)
						  //| ':' ident '(' expr_list? ')' -> ^(Expr_Call_With $suffix_expr ident expr_list?)
					      )*
	;

atom_expr
	: NUMBER
	| ident
	| STRING
	| '(' expr ( (',' expr)+ ')' -> ^(Expr_Tuple expr+)
	           | ')' -> ^(Expr_Bracket expr)
			   )
	;

lvalue_item
	: (a=lvalue_atom -> $a) ( '++' -> ^(Expr_Suffix '++' $lvalue_item)
					        | '--' -> ^(Expr_Suffix '--' $lvalue_item)
						    | '.' ident -> ^(Expr_Access '.' $lvalue_item ident)
						    | '->' ident -> ^(Expr_Access '->' $lvalue_item ident)
						    | generic_parameter? '(' expr_list? ')' -> ^(Expr_Call $lvalue_item generic_parameter? expr_list?)
						    | '[' expr_list? ']' -> ^(Expr_Dict $lvalue_item expr_list?)
					        )*
	;

lvalue_atom
	: ident
	;

lvalue
	: '(' lvalue_item (',' lvalue_item)+ ')' -> ^(Match_Tuple lvalue_item*)
	| lvalue_item
	;

ident
	: IDENT ('::' IDENT)*
	;

infix_func
	: '`'! ident '`'!
	;

// Lexer Rules

IDENT: ('a'..'z' | 'A'..'Z' | '_')+ ('0'..'9')*;

NUMBER: ( '0'..'9'+ ('.' '0'..'9'+)? ('e' '-'? '0'..'9'+)? ('ll' | 'f')?
        | '0' 'x' ('0'..'9' | 'a'..'f' | 'A' .. 'F')+
		)
		;

STRING
	: '"' (~'"')* '"'
	| '\'' (~'\'') '\''
	;

Comment
	: '/*' ( options { greedy = false; } : . )* '*/' { $channel = Hidden; }
	;

LineComment
	: '//' ~ ('\n'|'\r')* '\r'? '\n' { $channel = Hidden; }
	;

fragment
EXPONENT :
    ('e'|'E') ('+'|'-')? ('0'..'9')+
    ;


Left_Bracket
	: '(' | '[' | '{'
	{
		int k = $text == "(" ? 0 : $text == "[" ? 1 : 2;
		if (Bracket[k] == null) Bracket[k] = new Stack<int>();
		Bracket[k].Push(CharIndex);
	}
	;

Right_Bracket
	: ')' | ']' | '}'
	{
		int k = $text == "(" ? 0 : $text == "[" ? 1 : 2;
		int pos = Bracket[k].Pop();
		while (Indents.Count > 0 && pos < Indents.First().CharIndex)
		{
			Emit(new CommonToken(DEDENT, "DEDENT"));
			Indents.Pop();
			CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
		}
	}
	;

NEWLINE
	: ('\r'? '\n')+ SP?
	{
		int indent = $SP.text == null ? 0 : $SP.text.Length;
		if (indent > CurrentIndent)
		{
			Emit(new CommonToken(NEWLINE, "NEWLINE"));
			Emit(new CommonToken(INDENT, "INDENT"));
			Emit(new CommonToken(NEWLINE, "NEWLINE"));
			Indents.Push(new Indentation(indent, CharIndex));
			CurrentIndent = indent;
		}
		else if (indent < CurrentIndent)
		{
			while (Indents.Count > 0 && indent < CurrentIndent)
			{
				Emit(new CommonToken(NEWLINE, "NEWLINE"));
				Emit(new CommonToken(DEDENT, "DEDENT"));
				Indents.Pop();
				CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
			}
			Emit(new CommonToken(NEWLINE, "NEWLINE"));
		}
		else
		{
			Emit(new CommonToken(NEWLINE, "NEWLINE"));
			Skip();
		}
	}
	;

fragment SP: (' ' | '\t')+ ;

INDENT: {0==1}?=> ('\n') ;
DEDENT: {0==1}?=> ('\n') ;