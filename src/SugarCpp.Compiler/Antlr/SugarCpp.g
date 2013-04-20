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
   Expr_Call = '(';
   Expr_Bin;
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
	: node+ EOF
	;

node
	: imports
	| func_def
	;

imports
	: 'import' STRING? (INDENT (NEWLINE+ STRING)+ NEWLINE* DEDENT)? NEWLINE*
	;

type_name
	: IDENT
	;

func_def
	: type_name IDENT '(' ')' stmt_block NEWLINE*
    ;

stmt_block
	: INDENT (NEWLINE+ stmt)+ DEDENT
	;

stmt
	: stmt_if
	| stmt_while
	| stmt_for
	| expr
	;

stmt_if
	: 'if' expr stmt_block ('else' stmt_block)?
	;
	
stmt_while
	: 'while' expr stmt_block
	;

stmt_for
	: 'for' '(' expr ';' expr ';' expr ')' stmt_block
	;

expr
	: alloc_expr
	;

alloc_expr
	: type_name IDENT ('=' expr)? -> ^(Expr_Alloc type_name IDENT expr?)
	| logic_expr
	;

logic_expr
	: assign_expr (('==' | '!=' | '>' | '<' | '>=' | '<=')^ assign_expr)*
	;

assign_expr
	: add_expr ('='^ add_expr)*
	;

add_expr
	: mul_expr (('+' | '-')^ mul_expr)*
	;

mul_expr
	: call_expr (('*' | '/')^ call_expr)*
	;

Expr_Call
	: '('
	;

args_list
	: expr? (',' expr)* -> expr*
	;

call_expr
	: atom_expr (Expr_Call^ args_list ')'!)*
	;

atom_expr
	: INT
	| DOUBLE
	| IDENT
	| STRING
	| '('! expr ')'!
	;

// Lexer Rules

IDENT: 'a'..'z'+ ;

INT: '0'..'9'+ ;

DOUBLE
	: ('0'..'9')+ '.' ('0'..'9')* EXPONENT?
    | '.' ('0'..'9')+ EXPONENT?
    | ('0'..'9')+ EXPONENT     
    ;

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