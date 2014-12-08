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
   Stmt_Finally;

   Stmt_Using;
   Stmt_Typedef;

   Stmt_If;
   Stmt_Unless;

   Stmt_While;
   Stmt_Until;
   Stmt_Loop;
   Stmt_Try;

   Stmt_For;
   For_Item_Each;
   For_Item_When;
   For_Item_Map;
   For_Item_To;
   For_Item_Til;
   For_Item_Down_To;

   Stmt_Return;

   Stmt_Switch;
   Switch_Item;

   Type_List;
   Type_Func;

   Type_Array;
   Type_Ref;
   Type_Star;
   Type_Template;
   Type_Ident;

   Func_Args;

   Expr_List;
   Expr_Args;

   Expr_Cast;

   Expr_Alloc_Equal;
   Expr_Alloc_Bracket;

   Expr_Bracket;

   Expr_Not_Null;
   Expr_Cond_Not_Null;

   Expr_Chain;
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
   Expr_Where;
   Expr_Tuple;

   Expr_List_Generation;

   Match_Expr;
   Match_Expr_Item;

   Ident_List;
   Match_Tuple;

   Func_Declare;
}

@lexer::header
{
	using System;
	using System.Collections;
    using System.Collections.Generic;
	using System.Linq;
	using System.Text;
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
				Emit(this.CreateToken(NEWLINE, "NEWLINE"));
				Emit(this.CreateToken(DEDENT, "DEDENT"));
				Indents.Pop();
				CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
				base.NextToken();
				return tokens.Dequeue();
			}
			if (Indents != null)
			{
				Indents = null;
				return this.CreateToken(NEWLINE, "NEWLINE");
			}
            return this.CreateToken(EOF, "EOF");
		}
        return tokens.Dequeue();
    }

	public CommonToken CreateToken(int type, string text)
	{
		var x = new CommonToken(type, text);
		x.Line = this.Line;
		x.CharPositionInLine = this.CharPositionInLine;
		x.StartIndex = this.CharIndex;
		x.StopIndex = this.CharIndex;
		return x;
	}
}

@lexer::init {
	CurrentIndent = 0;
	Bracket[0] = Stack<int>();
	Bracket[1] = Stack<int>();
	Bracket[2] = Stack<int>();
	Console.WriteLine("Init!");
}

@parser::members
{
	public List<string> errors = new List<string>();
    public override void ReportError(RecognitionException e)
    {
        String hdr = GetErrorHeader(e);
        String msg = GetErrorMessage(e, tokenNames);
		errors.Add(hdr + " " + msg);
    }
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
	: (node WS* NEWLINE+)* -> ^(Global_Block node*)
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

attribute_args_list
	: '(' WS* attribute_args (WS* ',' WS* attribute_args)* WS* ')' -> attribute_args+
	;

attribute_item
	: ident (WS* attribute_args_list)? -> ^(Attribute ident attribute_args_list?)
	| 'const' (WS* attribute_args_list)? -> ^(Attribute 'const' attribute_args_list?)
	| 'static' (WS* attribute_args_list)? -> ^(Attribute 'static' attribute_args_list?)
	| 'public' (WS* attribute_args_list)? -> ^(Attribute 'public' attribute_args_list?)
	| 'virtual' (WS* attribute_args_list)? -> ^(Attribute 'virtual' attribute_args_list?)
	;

attribute
	: ('[' WS* attribute_item (WS* ',' WS* attribute_item)* WS* ']' WS* NEWLINE+)+ -> attribute_item+
	;

global_alloc
	: attribute? ('extern' WS*)? ident_list ( WS* ':' WS* type_name ( WS* ('=' | ':=') WS* where_expr -> ^(Expr_Alloc_Equal attribute? 'extern'? type_name ident_list ^(Expr_Args where_expr))
													                | WS* bracket_expr_list -> ^(Expr_Alloc_Bracket attribute? 'extern'? type_name ident_list bracket_expr_list)
								 					                | -> ^(Expr_Alloc_Equal attribute? 'extern'? type_name ident_list ^(Expr_Args))
								  					                )
									        | WS* ':=' WS* (where_expr (WS* ',' WS* where_expr)*) -> ^(':=' attribute? 'extern'? ident_list ^(Expr_Args where_expr+))
									        )
	;

global_using
	: attribute? 'using' (WS* stmt_using_item)* -> ^(Stmt_Using attribute? stmt_using_item*)
	;

global_typedef
	: attribute? 'typedef' WS* ident WS* '=' WS* type_name -> ^(Stmt_Typedef attribute? type_name ident)
	;

import_def
	: attribute? 'import' (WS* STRING)? (WS* NEWLINE+ INDENT NEWLINE* (STRING WS* NEWLINE+)* DEDENT)? -> ^(Import attribute? STRING*)
	;

enum_def
	: attribute? 'enum' WS* ident WS* '=' (WS* ident (WS* '|' WS* ident)*)? -> ^(Enum attribute? ident ^(Ident_List ident*))
	;

namespace_def
	: attribute? 'namespace' WS* ident (WS* NEWLINE+ INDENT NEWLINE* global_block DEDENT)? -> ^(Namespace attribute? ident global_block?)
	;

class_args
	: '(' ( WS* func_args WS* ')' -> func_args
		  | WS* ')' -> ^(Func_Args)
		  )
	;

class_def
	:  attribute? ('public' WS*)? ( 'class' WS* ident (generic_parameter)? (WS* ':' WS* ident (WS* ',' WS* ident)*)? (WS* NEWLINE+ INDENT NEWLINE* global_block DEDENT)? -> ^(Class 'public'? attribute? ident generic_parameter? class_args? (^(Ident_List ident*))? global_block?)
								  | 'case' WS* 'class' WS* ident (generic_parameter)? (WS* class_args)? (WS* ':' WS* ident (WS* ',' WS* ident)*)? (WS* NEWLINE+ INDENT NEWLINE* global_block DEDENT)? -> ^(Class 'case' 'public'? attribute? ident generic_parameter? class_args? (^(Ident_List ident*))? global_block?)
								  )
	;

type_list
	: type_name (WS* ','  WS* type_name)* -> ^(Type_List type_name*)
	;

type_name
	: type_single ( WS* '->' WS* (type_name | '(' WS* ')') -> ^(Type_Func ^(Type_List type_single) type_name?)
				  | -> type_single
				  )
	| '(' (WS* type_list)? WS* ')' WS* '->' WS* (type_name | '(' WS* ')') -> ^(Type_Func type_list? type_name?)
	;

type_single
	: type_star ( WS* '&' -> ^(Type_Ref type_star)
				| WS* '[' ( WS* expr (WS* ',' WS* expr)* WS* ']' -> ^(Type_Array type_star expr+)
				          | (WS* ',')* WS* ']' -> ^(Type_Array type_star expr+)
					      )
				| -> type_star
				)
	;

type_no_array
	: type_star ( WS* '&' -> ^(Type_Ref type_star)
				| -> type_star
				)
	;

type_star
	: type_template_type ( (WS* '*')+ -> ^(Type_Star type_template_type '*'+)
						 | -> type_template_type
						 )
	;

type_template_type
	: type_ident ( '<' (WS* type_name (WS* ',' WS* type_name)*)? WS* '>' -> ^(Type_Template type_ident type_name*)
				 | -> type_ident
				 )
	;

type_ident
	: ('static' WS*)? ('const' WS*)? ('struct' WS*)? ('long' WS*)? ('thread_local' WS*)? ident -> ^(Type_Ident 'static'? 'const'? 'struct'? 'long'? 'thread_local'? ident)
	;

generic_parameter_inside
	: type_name (WS* ',' WS* type_name)* -> ^(Generic_Patameters type_name*)
	;

generic_parameter
	: '<' WS* generic_parameter_inside WS* '>' -> generic_parameter_inside
	;

generic_parameter_ident
	: '<' WS* type_ident (WS* ',' WS* type_ident)* WS* '>' -> ^(Generic_Patameters type_ident*)
	;

func_args
	: func_args_item (WS* ',' WS* func_args_item)* -> ^(Func_Args func_args_item*)
	;

// TODO(curimit): Double check this
func_args_item
	: ident_list WS* ':' WS* type_name ( WS* ('=' | ':=') WS* expr  -> ^(Expr_Alloc_Equal type_name ident_list ^(Expr_Args expr))
	                                   | WS* bracket_expr_list  -> ^(Expr_Alloc_Bracket type_name ident_list bracket_expr_list)
							           | -> ^(Expr_Alloc_Equal type_name ident_list ^(Expr_Args))
							           )
	;

operator
	: '+' | '-' | '*' | '/'
	;

func_name
	: ident -> ident
	| '(' WS* operator WS* ')' -> operator
	;

func_type
	: type_name
	;

func_def
	: attribute? ('public' WS*)? ('virtual' WS*)? (func_type WS*)? ('~' WS*)? func_name (WS* generic_parameter_ident)? WS* '(' (WS* func_args)? WS* ')' ( WS* NEWLINE+ stmt_block -> ^(Func_Def 'public'? 'virtual'? attribute? func_type? '~'? func_name generic_parameter_ident? func_args? stmt_block)
																									                                                    | WS* '=' ( WS* where_expr -> ^(Func_Def 'public'? 'virtual'? attribute? func_type? '~'? func_name generic_parameter_ident? func_args? where_expr)
																											                                                      | WS* NEWLINE+ INDENT NEWLINE* (match_item WS* NEWLINE+)+ DEDENT -> ^(Func_Def 'public'? 'virtual'? attribute? func_type? '~'? func_name generic_parameter_ident? func_args? ^(Match_Expr match_item+))
																											                                                      )
																									                                                    | -> ^(Func_Def 'public'? 'virtual'? attribute? func_type? '~'? func_name generic_parameter_ident? func_args? Func_Declare)
																									                                                    )
    ;

stmt_block_item
	: stmt_complex WS* NEWLINE+ -> stmt_complex
	| stmt_simple WS* (NEWLINE+ | ';' NEWLINE*) -> stmt_simple
	;

stmt_block
	: INDENT NEWLINE* stmt_block_item* DEDENT -> ^(Stmt_Block stmt_block_item*)
	;

stmt
	: stmt_simple
	| stmt_complex
	;

stmt_simple
	: stmt_expr
	;

stmt_complex
	: stmt_if
	| stmt_for
	| stmt_while
	| stmt_try
	| stmt_switch
	| stmt_defer
	;

stmt_expr
	: (a=stmt_expr_item -> $a) ( WS* 'if' WS* expr -> ^(Stmt_If expr ^(Stmt_Block $stmt_expr))
							   | WS* 'unless' WS* expr -> ^(Stmt_Unless expr ^(Stmt_Block $stmt_expr))
							   | WS* 'while' WS* expr -> ^(Stmt_While expr ^(Stmt_Block $stmt_expr))
							   | WS* 'until' WS* expr -> ^(Stmt_Until expr ^(Stmt_Block $stmt_expr))
							   | WS* 'for' WS* for_item (WS* ',' WS* for_item)* -> ^(Stmt_For for_item* ^(Stmt_Block $stmt_expr))
							   )*
	;

stmt_expr_item
	: stmt_alloc
	| stmt_return
	| stmt_using
	| stmt_typedef
	| stmt_modify
	;

stmt_defer
	: 'defer' WS* stmt -> ^(Stmt_Defer stmt)
	| 'finally' WS* stmt -> ^(Stmt_Finally stmt)
	;

stmt_typedef
	: 'typedef' WS* ident WS* '=' WS* type_name -> ^(Stmt_Typedef type_name ident)
	;

stmt_using_item: ident | 'namespace';
stmt_using
	: 'using' (WS* stmt_using_item)* -> ^(Stmt_Using stmt_using_item*)
	;

stmt_return
	: 'return' (WS* expr)? -> ^(Stmt_Return expr?)
	;

inline_stmt_block
	: stmt_simple (WS* ';' WS* stmt_simple)* -> ^(Stmt_Block stmt_simple+)
	;

stmt_if
	: 'if' WS* expr ( WS* NEWLINE+ stmt_block (NEWLINE* 'else' NEWLINE+ stmt_block)? -> ^(Stmt_If expr stmt_block stmt_block?)
	                | WS* 'then' WS* inline_stmt_block -> ^(Stmt_If expr inline_stmt_block)
				    )
	| 'unless' WS* expr ( WS*NEWLINE+ stmt_block (NEWLINE* 'else' NEWLINE+ stmt_block)? -> ^(Stmt_Unless expr stmt_block stmt_block?)
	                    | WS* 'then' inline_stmt_block -> ^(Stmt_Unless expr inline_stmt_block)
				        )
	;

stmt_while
	: 'while' WS* expr ( WS* NEWLINE+ stmt_block -> ^(Stmt_While expr stmt_block)
			           | WS* 'then' WS* inline_stmt_block -> ^(Stmt_While expr inline_stmt_block)
				       )
	| 'until' expr ( WS* NEWLINE+ stmt_block -> ^(Stmt_Until expr stmt_block)
			       | WS* 'then' WS* inline_stmt_block -> ^(Stmt_Until expr inline_stmt_block)
				   )
	| 'loop' (WS* expr)? WS* NEWLINE+ stmt_block -> ^(Stmt_Loop expr? stmt_block)
	;

for_range
	: ident WS* '<-' WS* a=expr ( WS* 'to' WS* b=expr (WS* 'by' WS* c=expr)? -> ^(For_Item_To ident $a $b $c?)
						        | WS* 'til' WS* b=expr (WS* 'by' WS* c=expr)? -> ^(For_Item_Til ident $a $b $c?)
						        | WS* 'downto' WS* b=expr (WS* 'by' WS* c=expr)? -> ^(For_Item_Down_To ident $a $b $c?)
						        | -> ^(For_Item_Each ident $a)
						        )
	;

for_when
	: expr -> ^(For_Item_When expr)
	;

for_map
	: ident WS* '=>' WS* expr -> ^(For_Item_Map ident expr)
	;

for_item
	: for_range
	| for_when
	| for_map
	;

stmt_for
	: ('for' | 'let') ( WS* for_item (WS* ',' WS* for_item)* WS* NEWLINE+ stmt_block -> ^(Stmt_For for_item* stmt_block)
			        //| '(' expr ';' expr ';' expr ')' NEWLINE+ stmt_block -> ^(Stmt_For expr expr expr stmt_block)
			          )
	;

stmt_try
	: 'try' WS* NEWLINE+ stmt_block NEWLINE* 'catch' WS* stmt_alloc WS* NEWLINE+ stmt_block -> ^(Stmt_Try stmt_block stmt_alloc stmt_block)
	;

switch_item
	: 'when' WS* expr (WS* ',' WS* expr)* ( WS* NEWLINE+ stmt_block -> ^(Switch_Item ^(Expr_Args expr+) stmt_block)
	                                      | WS* 'then' WS* inline_stmt_block -> ^(Switch_Item ^(Expr_Args expr+) inline_stmt_block)
						                  )
	;

stmt_switch
	: 'switch' (WS* expr)? WS* NEWLINE+ INDENT NEWLINE* (switch_item WS* NEWLINE+)+ ('else' NEWLINE+ stmt_block NEWLINE*)? DEDENT -> ^(Stmt_Switch expr? switch_item* stmt_block?)
	;

ident_list
	: ident (WS* ',' WS* ident)* -> ^(Ident_List ident+)
	;

stmt_alloc
	: ident_list ( WS* ':' WS* type_name ( WS* ('=' | ':=') WS* where_expr  -> ^(Expr_Alloc_Equal type_name ident_list ^(Expr_Args where_expr))
	                                     | WS* bracket_expr_list  -> ^(Expr_Alloc_Bracket type_name ident_list bracket_expr_list)
							             | -> ^(Expr_Alloc_Equal type_name ident_list ^(Expr_Args))
							             )
				 | WS* ':=' (WS* where_expr (WS* ',' WS* where_expr)*) -> ^(':=' ident_list ^(Expr_Args where_expr*)))
	;

stmt_modify
	: lvalue ( WS* modify_expr_op WS* where_expr -> ^(modify_expr_op lvalue where_expr)
	         | WS* '?=' WS* where_expr -> ^('?=' lvalue where_expr)
             | WS* '<<' WS* where_expr -> ^(Expr_Bin '<<' lvalue where_expr)
             | WS* '>>' WS* where_expr -> ^(Expr_Bin '>>' lvalue where_expr)
			 | -> lvalue)
	;

where_item
	: stmt
	;

where_expr
	: (a=expr -> $a) ( WS* NEWLINE+ INDENT NEWLINE* 'where' ( WS* where_item ( NEWLINE* DEDENT -> ^(Expr_Where $where_expr where_item)
																	         | NEWLINE+ INDENT NEWLINE* (where_item WS* NEWLINE+)+ DEDENT NEWLINE* DEDENT -> ^(Expr_Where $where_expr where_item+)
																	         )
														    | WS* NEWLINE+ INDENT NEWLINE* (where_item WS* NEWLINE+)+ DEDENT NEWLINE* DEDENT -> ^(Expr_Where $where_expr where_item+)
														    )
					 | WS* 'where' WS* NEWLINE+ INDENT NEWLINE* (where_item WS* NEWLINE+)+ DEDENT -> ^(Expr_Where $where_expr where_item+)
			         | -> expr
		             )
	;

let_expr
	: 'let' WS* where_item ( WS* 'in' ( WS* expr -> ^(Expr_Where expr where_item+)
							          | WS* NEWLINE+ ( INDENT NEWLINE* expr WS* NEWLINE+ DEDENT -> ^(Expr_Where expr where_item+)
										             | WS* expr -> ^(Expr_Where expr where_item+)
										             )
							          )
					       | WS* NEWLINE+ INDENT NEWLINE* (where_item WS* NEWLINE+)+ WS* 'in' WS* expr WS* NEWLINE+ DEDENT -> ^(Expr_Where expr where_item+)
					       )
	;

match_item
	: '|' WS* expr WS* '=>' WS* where_expr -> ^(Match_Expr_Item expr where_expr)
	;

match_expr
	: 'match' (WS* expr)? (WS* 'returns' WS* type_name)? WS* NEWLINE+ INDENT NEWLINE* (match_item WS* NEWLINE+)+ DEDENT -> ^(Match_Expr expr? type_name? match_item+)
	;

expr
	: feed_expr
	| match_expr
	| let_expr
	;

feed_expr
	: (modify_expr WS* ('<|' | '|>') ) => (a=modify_expr -> $a) ( WS* '<|' WS* list_expr -> ^(Expr_Call $feed_expr ^(Expr_Args list_expr))
															    | WS* '|>' WS* list_expr -> ^(Expr_Call list_expr ^(Expr_Args $feed_expr))
															    )
	| list_expr
	;

list_expr
	: ('[' WS* feed_expr WS* 'for') => '[' WS* feed_expr WS* 'for' WS* for_item (WS* ',' WS* for_item)* WS* ']' WS* ':' WS* type_name  -> ^(Expr_List_Generation type_name? ^(Stmt_For for_item* ^(Stmt_Block)) feed_expr)
	| '[' ((WS | ',' | NEWLINE | INDENT | DEDENT)* feed_expr (WS* (',' | NEWLINE | INDENT | DEDENT)+ WS* feed_expr)*)? (WS | ',' | NEWLINE | INDENT | DEDENT)* ']' -> ^(Expr_List feed_expr*)
	| lambda_expr
	;

lambda_value
	: expr -> ^(Stmt_Block ^(Stmt_Return expr))
	| NEWLINE+ stmt_block -> stmt_block
	;

lambda_type
	: '(' WS* type_name WS* ')' -> type_name
	;

lambda_expr_op : '->' | '=>' | '-->' | '==>' ;
lambda_expr
	: '(' (WS* func_args)? WS* ')' (WS* lambda_type)? WS* lambda_expr_op WS* lambda_value -> ^(Expr_Lambda lambda_expr_op func_args? lambda_type? lambda_value)
	| modify_expr
	;

modify_expr_op: '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=';
modify_expr
	: cond_expr ( WS* (':=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '^=' | '|=' | '<<=' | '>>=')^ WS* cond_expr
				| (WS* '='^ WS* cond_expr)+
				)?
	;

cond_expr_item: or_expr ;
cond_expr
	: (a=or_expr -> $a) (WS* '?' ( WS* a=cond_expr_item ( WS* ':' WS* b=cond_expr_item -> ^(Expr_Cond $cond_expr $a $b)
											            | -> ^(Expr_Cond_Not_Null $cond_expr $a)
											            )
							     | -> ^(Expr_Not_Null $cond_expr)
							     )
				        )?
	;

or_op: '||' | 'or' ;
or_expr
	: (a=and_expr -> $a) (WS* op=or_op WS* b=and_expr -> ^(Expr_Bin $op $or_expr $b))*
	;

and_op: '&&' | 'and' ;
and_expr
	: (a=bit_or -> $a) (WS* op=and_op WS* b=bit_or -> ^(Expr_Bin $op $and_expr $b))*
	;

bit_or
	: (a=bit_xor -> $a) (WS* '|' WS* b=bit_xor -> ^(Expr_Bin '|' $bit_or $b))*
	;

bit_xor
	: (a=bit_and -> $a) (WS* '^' WS* b=bit_and -> ^(Expr_Bin '^' $bit_xor $b))*
	;

bit_and
	: (a=cmp_expr -> $a) (WS* '&' WS* b=cmp_expr -> ^(Expr_Bin '&' $bit_and $b))*
	;

chain_op
	: WS+ '<' WS* -> '<'
    | WS* '<=' WS* -> '<='
	| WS* '>' WS* -> '>'
	| WS* '>=' WS* -> '>='
	| WS* '!=' WS* -> '!='
	| WS* '==' WS* -> '=='
	| WS* 'is' WS* -> 'is'
	| WS* 'isnt' WS* -> 'isnt'
	;

chain_list: (chain_op shift_expr)+ ;
cmp_expr
	: (a=shift_expr -> $a) ( op=chain_op b=shift_expr ( chain_list -> ^(Expr_Chain  $cmp_expr $op $b chain_list)
													          | -> ^(Expr_Bin $op $cmp_expr $b)
													          )
						   )?
	;

shift_expr_op: '<<' | '>>' ;
shift_expr
	: (a=add_expr -> $a) (WS* shift_expr_op WS* b=add_expr -> ^(Expr_Bin shift_expr_op $shift_expr $b))*
	;

add_expr
	: (a=mul_expr -> $a) ( WS* '+' WS* b=mul_expr -> ^(Expr_Bin '+' $add_expr $b)
						 | WS* '-' WS* b=mul_expr -> ^(Expr_Bin '-' $add_expr $b)
						 )*
	;

mul_expr
	: (a=infix_expr -> $a) ( WS* '*' WS* b=infix_expr -> ^(Expr_Bin '*' $mul_expr $b)
						   | WS* '/' WS* b=infix_expr -> ^(Expr_Bin '/' $mul_expr $b)
						   | WS* '%' WS* b=infix_expr -> ^(Expr_Bin '%' $mul_expr $b)
						   )*
	;

infix_expr
	: (a=selector_expr -> $a) ( WS* infix_func WS* b=selector_expr  -> ^(Expr_Infix infix_func $infix_expr $b) )*
	;

selector_expr
	: (a=cast_expr -> $a) ( WS* '->*' WS* b=ident -> ^(Expr_Access '->*' $selector_expr $b)
						  | WS* '.*'  WS* b=ident -> ^(Expr_Access '.*'  $selector_expr $b)
						  )*
	;

cast_expr
	: ('(' WS* type_name WS* ')' WS* prefix_expr) => '(' WS* type_name WS* ')' WS* prefix_expr -> ^(Expr_Cast type_name prefix_expr)
	| prefix_expr
	;

prefix_expr_op: '!' | '~' | '++' | '--' | '-' | '+' | '*' | '&' | 'not';
prefix_expr
	: (prefix_expr_op WS* prefix_expr) -> ^(Expr_Prefix prefix_expr_op prefix_expr)
	| 'new' WS* type_no_array ( WS* bracket_expr_list -> ^(Expr_New_Type type_no_array bracket_expr_list)
						      | WS* square_expr_list -> ^(Expr_New_Array type_no_array square_expr_list)
						      )
	| suffix_expr
	;

square_expr_list
	: '[' WS* expr (WS* ',' WS* expr)* WS* ']' -> ^(Expr_Args expr*)
	;

bracket_expr_list
	: '(' (WS* expr (WS* ',' WS* expr)*)? ( WS* ')' -> ^(Expr_Args expr*)
							              | WS* NEWLINE+ ( INDENT NEWLINE* expr ((WS* ',' | WS* NEWLINE)+ WS* expr)* (WS* NEWLINE)* ( WS* ')' WS* NEWLINE* DEDENT
										                                                                                        | DEDENT NEWLINE* WS* ')'
																											                    ) -> ^(Expr_Args expr*)
										                 | (WS* expr ((WS*  ',' | WS*  NEWLINE)+ WS* expr)*)? WS* ')' -> ^(Expr_Args expr*)
										                 )
							              )
	;

suffix_expr
	: (a=atom_expr -> $a) ( WS* '++' -> ^(Expr_Suffix '++' $suffix_expr)
					      | WS* '--' -> ^(Expr_Suffix '--' $suffix_expr)
						  | WS* '.' WS* ident -> ^(Expr_Access '.' $suffix_expr ident)
						  | WS* '->' WS* ident -> ^(Expr_Access '->' $suffix_expr ident)
						  | WS* bracket_expr_list -> ^(Expr_Call $suffix_expr bracket_expr_list)
						  | generic_parameter WS* bracket_expr_list -> ^(Expr_Call $suffix_expr generic_parameter bracket_expr_list)
						  | WS* square_expr_list -> ^(Expr_Dict $suffix_expr square_expr_list)
						  | WS* '@' WS* ident WS* bracket_expr_list -> ^(Expr_Call_With $suffix_expr ident bracket_expr_list)
					      )*
	;

atom_expr
	: NUMBER
	| ident
	| STRING
	| '@' WS* ident -> ^('@' ident)
	| '(' WS* a=expr ( (WS* ',' WS* expr)+ WS* ')' -> ^(Expr_Tuple expr+)
	                 | WS* ')' -> ^(Expr_Bracket expr)
			         )
	;

lvalue_item
	: lvalue_prefix
	;

lvalue_prefix
	: (prefix_expr_op lvalue_prefix) -> ^(Expr_Prefix prefix_expr_op lvalue_prefix)
	| lvalue_suffix
	;

lvalue_suffix
	: (a=lvalue_atom -> $a) ( WS* '++' -> ^(Expr_Suffix '++' $lvalue_suffix)
					        | WS* '--' -> ^(Expr_Suffix '--' $lvalue_suffix)
						    | WS* '.' WS* ident -> ^(Expr_Access '.' $lvalue_suffix ident)
						    | WS* '->' WS* ident -> ^(Expr_Access '->' $lvalue_suffix ident)
						    | WS* bracket_expr_list -> ^(Expr_Call $lvalue_suffix bracket_expr_list)
						    | generic_parameter WS* bracket_expr_list -> ^(Expr_Call $lvalue_suffix generic_parameter bracket_expr_list)
						    | WS* square_expr_list -> ^(Expr_Dict $lvalue_suffix square_expr_list)
					        )*
	;

lvalue_atom
	: ident
	| '@' WS* ident -> ^('@' ident)
	;

lvalue
	: '(' WS* lvalue_item (WS* ',' WS* lvalue_item)+ WS* ')' -> ^(Match_Tuple lvalue_item*)
	| lvalue_item
	;

ident
	: IDENT (WS* '::' WS* IDENT)*
	;

infix_func
	: '`'! WS* ident WS* '`'!
	;

// Lexer Rules

DOT_DOT: '..' ;

IDENT
	: ('a'..'z' | 'A'..'Z' | '_') ('a'..'z' | 'A'..'Z' | '_' | '0'..'9')*
	(('-' ('a'..'z' | 'A'..'Z' | '_' | '0'..'9'))=> '-' ('a'..'z' | 'A'..'Z' | '_' | '0'..'9')+)*
	{
        StringBuilder sb = new StringBuilder();
        string[] list = Text.Split('-');
        bool first = true;
        foreach (var x in list)
        {
            if (x == "")
            {
                continue;
            }
            if (first)
            {
                sb.Append(x);
                first = false;
            }
            else
            {
                sb.Append("_");
                sb.Append(x);
            }
        }
        Text = sb.ToString();
	};

NUMBER: ( '0'..'9'+ ('.' '0'..'9'+)? ('e' '-'? '0'..'9'+)? ('f' | 'F' | 'u' ('l' 'l'?)? | 'l' 'l'? | 'U' ('L' 'L'?)? | 'L' 'L'?)?
        | '0' 'x' ('0'..'9' | 'a'..'f' | 'A' .. 'F')+
		)
		;

STRING
	: ( '"' ( options { greedy = false; } : (('\\') => '\\' . | . ) )* '"'
	  | '\'' ( options { greedy = false; } : (('\\') => '\\' . | . ) )* '\''
	  | '@\"""' ( options { greedy = false; } : (('"' '"') => '"' '"' | . ) )* '\"""'
	  )
	{
		StringBuilder sb = new StringBuilder();
		int erase = 0;
		bool at_string = false;
		if (Text[0] == '@')
		{
			at_string = true;
			Text = Text.Substring(3, Text.Length - 5);
		}
		int ct = 0;
		foreach (var c in Text)
		{
			ct++;
			if (c == '\n')
			{
				erase = CurrentIndent;
				sb.Append("\\n");
				continue;
			}
			if (erase == 0)
			{
				if (at_string)
				{
					sb.Append(c == '\\' ? "\\\\" : c == '"' && ct != 1 && ct != Text.Length ? "\\\"" : c.ToString());
				}
				else
				{
					sb.Append(c);
				}
			}
			else
			{
				erase--;
				continue;
			}
		}
		Text = sb.ToString();
	}
	;

Comment
	: '/*' ( options { greedy = false; } : . )* '*/' { $channel = Hidden; }
	;

LineComment
	: '//' (~('\n'|'\r'))* { $channel = Hidden; }
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
			Emit(this.CreateToken(DEDENT, "DEDENT"));
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
			Emit(this.CreateToken(NEWLINE, "NEWLINE"));
			Emit(this.CreateToken(INDENT, "INDENT"));
			Emit(this.CreateToken(NEWLINE, "NEWLINE"));
			Indents.Push(new Indentation(indent, CharIndex));
			CurrentIndent = indent;
		}
		else if (indent < CurrentIndent)
		{
			while (Indents.Count > 0 && indent < CurrentIndent)
			{
				Emit(this.CreateToken(NEWLINE, "NEWLINE"));
				Emit(this.CreateToken(DEDENT, "DEDENT"));
				Indents.Pop();
				CurrentIndent = Indents.Count == 0 ? 0 : Indents.First().Level;
			}
			Emit(this.CreateToken(NEWLINE, "NEWLINE"));
		}
		else
		{
			Emit(this.CreateToken(NEWLINE, "NEWLINE"));
			Skip();
		}
	}
	;

fragment SP: (' ' | '\t')+ ;

WS: ' ' ;

INDENT: {0==1}?=> ('\n') ;
DEDENT: {0==1}?=> ('\n') ;
