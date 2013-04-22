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
   Stmt_For;
   Stmt_While;

   Expr_Alloc;
   Expr_Block;
   Expr_Cond;
   Expr_New;
   Expr_Bin;
   Expr_Return;
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
	: imports
	| func_def
	| struct
	| enum
	;

imports
	: 'import' STRING? (INDENT (NEWLINE+ STRING)+ NEWLINE* DEDENT)? 
	;

enum
	: 'enum' IDENT '=' IDENT ('|' IDENT)*
	;

struct
	: 'struct' IDENT (INDENT (NEWLINE+ struct_stmt)+ DEDENT)
	;

struct_stmt
	: func_def
	| type_name IDENT ('=' expr)? -> ^(Expr_Alloc type_name IDENT expr?)
	;

type_name
	: IDENT ('[' ']')*
	;

generic_parameter
	: IDENT (','! IDENT)*
	;

func_args
	: stmt_alloc (',' stmt_alloc IDENT)*
	;

func_def
	: type_name IDENT ('[' generic_parameter ']')? '(' func_args? ')' ( stmt_block | '=' expr )
    ;

stmt_block
	: INDENT (NEWLINE+ stmt)* NEWLINE* DEDENT
	;

stmt
	: stmt_if
	| stmt_while
	| stmt_for
	| expr
	;

stmt_alloc
	: type_name IDENT ('=' atom_expr)? -> ^(Expr_Alloc type_name IDENT atom_expr?)
	;

stmt_if
	: 'if' '(' expr ')' stmt_block (NEWLINE* 'else' stmt_block)?
	;
	
stmt_while
	: 'while' '(' expr ')' stmt_block
	;

stmt_for
	: 'for' '(' expr (';' expr ';' expr | 'to' expr ('by' expr)?) ')' stmt_block
	;

expr
	: return_expr
	;

return_expr
	: 'return' expr? -> ^(Expr_Return expr?)
	| alloc_expr
	;

alloc_expr
	: type_name IDENT ('=' expr)? -> ^(Expr_Alloc type_name IDENT expr?)
	| assign_expr
	;

assign_expr
	: cond_expr ('='^ cond_expr)*
	;

Expr_Cond: '?' ;
cond_expr
	: logic_expr (Expr_Cond^ logic_expr ':'! logic_expr)?
	;

logic_expr
	: add_expr (('==' | '!=' | '>' | '<' | '>=' | '<=')^ add_expr)*
	;

add_expr
	: mul_expr (('+' | '-')^ mul_expr)*
	;

mul_expr
	: new_expr (('*' | '/')^ new_expr)*
	;

new_expr
	: 'new' IDENT ('[' expr ']')+ -> ^(Expr_New IDENT expr+)
	| prefix_expr
	;

prefix_expr
	: (('!' | '++' | '--' | '-')^)* call_expr
	;
	
Expr_Call: '(' ;
Expr_Dict: '[' ;

args_list
	: expr? (',' expr)* -> expr*
	;

call_expr
	: dot_expr (Expr_Call^ args_list ')'!
			   |Expr_Dict^ expr ']'!)*
	;

Expr_Dot
	: '.'
	;

dot_expr
	: atom_expr (Expr_Dot^ IDENT)*
	;

atom_expr
	: INT
	| IDENT
	| STRING
	| '('! expr ')'!
	| block_expr
	;

block_expr
	: INDENT (NEWLINE+ stmt)* DEDENT
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