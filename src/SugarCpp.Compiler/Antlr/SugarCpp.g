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

   Block;
   Func_Def;
   Root;
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

@parser :: namespace { SugarCpp.Compiler }
@lexer  :: namespace { SugarCpp.Compiler }

public root
	: func_def+ EOF+ -> ^(Root func_def+)
	;

func_def
	: Var stmt_block -> ^(Func_Def stmt_block)
    ;

stmt_block
	: INDENT! stmt+ DEDENT!
	;

stmt
	: INT
	| func_def
	| '('! stmt ')'!
	;

// Lexer Rules

INDENT: {0==1}?=> ('\n') ;
DEDENT: {0==1}?=> ('\n') ;

Var: 'a'..'z'+ ;

INT: '0'..'9'+ ;

WS: (' ')+ { Skip(); } ;

// '('
Left_Round_Bracket
	: '('
	{
		if (Bracket[0] == null) Bracket[0] = new Stack<int>();
		Bracket[0].Push(CharIndex);
	}
	;

// ')'
Right_Round_Bracket
	: ')'
	{
		int pos = Bracket[0].Pop();
		while (Indents.Count > 0 && pos < Indents.First().CharIndex)
		{
			Emit(new CommonToken(DEDENT, "DEDENT"));
			Indents.Pop();
			CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
		}
	}
	;

// '['
Left_Square_Bracket
	: '['
	{
		if (Bracket[1] == null) Bracket[1] = new Stack<int>();
		Bracket[1].Push(CharIndex);
	}
	;

// ']'
Right_Square_Bracket
	: ']'
	{
		int pos = Bracket[1].Pop();
		while (Indents.Count > 0 && pos < Indents.First().CharIndex)
		{
			Emit(new CommonToken(DEDENT, "DEDENT"));
			Indents.Pop();
			CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
		}
	}
	;

// '{'
Left_Curly_Bracket
	: '{'
	{
		if (Bracket[2] == null) Bracket[2] = new Stack<int>();
		Bracket[2].Push(CharIndex);
	}
	;

// '}'
Right_Curly_Bracket
	: '}'
	{
		int pos = Bracket[2].Pop();
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
		}
		else
		{
			Skip();
		}
	}
	;

fragment SP: ' '+ ;