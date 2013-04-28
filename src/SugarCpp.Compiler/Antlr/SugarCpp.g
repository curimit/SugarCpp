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

   Type_Tuple;

   Expr_Alloc;
   Expr_Alloc_Auto;

   Expr_Block;
   Expr_Cond;
   Expr_New;
   Expr_Bin;
   Expr_Return;
   
   Expr_Bin;
   Expr_Suffix;

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

@parser :: namespace { SugarCpp.Compiler }
@lexer  :: namespace { SugarCpp.Compiler }

public root
	: (node NEWLINE*)+ EOF
	;

node
	: func_def
	;

type_name
	: IDENT ('<' type_name (',' type_name)* '>')* ('*')*
	;

func_type_name
	: IDENT ('<' func_type_name (',' func_type_name)* '>')* ('*')*
	| '(' func_type_name (',' func_type_name) ')' -> ^(Type_Tuple func_type_name+)
	;

generic_parameter
	: IDENT (','! IDENT)*
	;

func_args
	: stmt_alloc (',' stmt_alloc IDENT)*
	;

func_def
	: func_type_name IDENT ('<' generic_parameter '>')? '(' func_args? ')' ( stmt_block | '=' expr )
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
	: add_expr
	;

add_expr
	: (a=mul_expr -> $a) ( '+' b=mul_expr -> ^(Expr_Bin '+' $add_expr $b)
						 | '-' b=mul_expr -> ^(Expr_Bin '-' $add_expr $b)
						 )*
	;

mul_expr
	: (a=suffix_expr -> $a) ( '*' b=suffix_expr -> ^(Expr_Bin '*' $mul_expr $b)
						    | '/' b=suffix_expr -> ^(Expr_Bin '/' $mul_expr $b)
						    | '%' b=suffix_expr -> ^(Expr_Bin '%' $mul_expr $b)
						    )*
	;

suffix_expr
	: (a=atom_expr -> $a) ( '++' -> ^(Expr_Suffix '++' $suffix_expr)
						  | '--' -> ^(Expr_Suffix '--' $suffix_expr)
						  )*
	;

atom_expr
	: INT
	| IDENT
	| STRING
	| { bool more_than_one = false; }
	 '(' expr (',' expr { more_than_one = true; Console.WriteLine("More Than One!"); } )* ')'
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