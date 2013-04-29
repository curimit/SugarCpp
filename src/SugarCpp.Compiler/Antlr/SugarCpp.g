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

   Func_Def;

   Stmt_If;
   Stmt_While;

   Type_IDENT;
   Type_Tuple;

   Func_Args;

   Expr_Alloc;
   Expr_Alloc_Auto;

   Expr_Block;
   Expr_Cond;
   Expr_New_Type;
   Expr_New_Array;
   Expr_Bin;
   Expr_Return;
   
   Expr_Bin;
   Expr_Suffix;
   Expr_Prefix;

   Expr_Access;
   Expr_Dict;
   Expr_Call;

   Expr_Lambda;

   Expr_Tuple;
   Expr_Match_Tuple;
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
			if (Indents.Count > 0)
			{
				Emit(new CommonToken(DEDENT, "DEDENT"));
				Indents.Pop();
				CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
				base.NextToken();
				return tokens.Dequeue();
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
	: NEWLINE* (node)+ NEWLINE* EOF
	;

node
	: func_def
	;

type_name
	: IDENT -> ^(Type_IDENT IDENT)
	;

func_type_name
	: IDENT -> ^(Type_IDENT IDENT)
	| '(' func_type_name (',' func_type_name) ')' -> ^(Type_Tuple func_type_name+)
	;

generic_parameter
	: IDENT (','! IDENT)*
	;

func_args
	: stmt_alloc (',' stmt_alloc)* -> ^(Func_Args stmt_alloc*)
	;

func_def
	: func_type_name IDENT ('<' generic_parameter '>')? '(' func_args? ')' stmt_block
    ;

stmt_block
	: INDENT (NEWLINE+ stmt)* NEWLINE* DEDENT
	;

stmt
	: stmt_expr
	;

stmt_expr
	: stmt_alloc
	| stmt_return
	;

stmt_return
	: 'return' expr? -> ^(Expr_Return expr?)
	;

stmt_alloc
	: type_name IDENT ('=' expr)? -> ^(Expr_Alloc type_name IDENT expr?)
	| '|' IDENT '|' ('=' expr)? -> ^(Expr_Alloc_Auto IDENT expr?)
	;

expr
	: lambda_expr
	;

lambda_expr
	: '(' func_args ')' '=>' modify_expr -> ^(Expr_Lambda func_args modify_expr)
	| modify_expr
	;

modify_expr_op: '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=' ;
modify_expr
	: (a=cond_expr -> $a) (modify_expr_op b=cond_expr -> ^(Expr_Bin modify_expr_op $modify_expr $b))*
	;

cond_expr_item: cond_expr ;
cond_expr
	: (a=or_expr -> $a) ('?' a=cond_expr_item ':' b=cond_expr_item -> ^(Expr_Cond $cond_expr $a $b))?
	;

or_expr
	: (a=and_expr -> $a) ('||' b=and_expr -> ^(Expr_Bin '||' $or_expr $b))*
	;

and_expr
	: (a=bit_or -> $a) ('&&' b=bit_or -> ^(Expr_Bin '&&' $and_expr $b))*
	;

bit_or
	: (a=bit_xor -> $a) ('|' b=bit_xor -> ^(Expr_Bin '|' $bit_or $b))*
	;

bit_xor
	: (a=bit_and -> $a) ('^' b=bit_and -> ^(Expr_Bin '^' $bit_xor $b))*
	;

bit_and
	: (a=shift_expr -> $a) ('&' b=shift_expr -> ^(Expr_Bin '&' $bit_and $b))*
	;

cmp_equ_expr_op: '==' | '!=' ;
cmp_equ_expr
	: (a=cmp_expr -> $a) (cmp_equ_expr_op b=cmp_expr -> ^(Expr_Bin cmp_equ_expr_op $cmp_equ_expr $b))*
	;
	
cmp_expr_op: '<' | '<=' | '>' | '>=' ;
cmp_expr
	: (a=shift_expr -> $a) (cmp_expr_op b=shift_expr -> ^(Expr_Bin cmp_expr_op $cmp_expr $b))*
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
	: (a=prefix_expr -> $a) ( '->*' b=IDENT -> ^(Expr_Access '->*' $selector_expr $b)
						    | '.*'  b=IDENT -> ^(Expr_Access '.*'  $selector_expr $b)
						    )*
	;

prefix_expr_op: '!' | '~' | '++' | '--' | '-' | '+' | '*' | '&' ;
prefix_expr
	: (prefix_expr_op prefix_expr) -> ^(Expr_Prefix prefix_expr_op prefix_expr)
	| 'new' type_name ( '(' expr_list? ')' -> ^(Expr_New_Type  type_name expr_list?)
	                  | '[' expr_list? ']' -> ^(Expr_New_Array type_name expr_list?)
					  )
	| suffix_expr
	;
	
expr_list
	: expr (','! expr)*
	;

suffix_expr
	: (a=atom_expr -> $a) ( '++' -> ^(Expr_Suffix '++' $suffix_expr)
					      | '--' -> ^(Expr_Suffix '--' $suffix_expr)
						  | '.' IDENT -> ^(Expr_Access '.' $suffix_expr IDENT)
						  | '->' IDENT -> ^(Expr_Access '->' $suffix_expr IDENT)
						  | '::' IDENT -> ^(Expr_Access '::' $suffix_expr IDENT)
						  | '(' expr_list? ')' -> ^(Expr_Call $suffix_expr expr_list?)
						  | '[' expr ']' -> ^(Expr_Dict $suffix_expr expr)
					      )*
	;

atom_expr
	: INT
	| IDENT
	| STRING
	| { bool more_than_one = false; }
	 '(' expr (',' expr { more_than_one = true; } )* ')'
	 -> { more_than_one }? ^(Expr_Tuple expr+)
	 -> expr
	;

lvalue
	: IDENT
	//| '(' IDENT (',' IDENT)+ ')' -> ^(Expr_Match_Tuple IDENT*)
	;

// Lexer Rules

IDENT: ('a'..'z' | 'A'..'Z' | '_')+ ('0'..'9')*;

INT: '0'..'9'+ ;


STRING
	: '"' (~'"')* '"'
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
	: ('r'? '\n')+ SP?
	{
		int indent = $SP.text == null ? 0 : $SP.text.Length;
		if (indent > CurrentIndent)
		{
			Emit(new CommonToken(INDENT, "INDENT"));
			Emit(new CommonToken(NEWLINE, "NEWLINE"));
			Indents.Push(new Indentation(indent, CharIndex));
			CurrentIndent = indent;
		}
		else if (indent < CurrentIndent)
		{
			while (Indents.Count > 0 && indent < CurrentIndent)
			{
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

fragment SP: ' '+ ;

INDENT: {0==1}?=> ('\n') ;
DEDENT: {0==1}?=> ('\n') ;