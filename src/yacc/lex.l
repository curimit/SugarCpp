%{
#define YYSTYPE YYL_TOKEN

#include "../common.h"
#include "yacc.tab.h"

extern "C"
{
	int yywrap(void);
	int yylex(void);
}

stack<int> indents;
int indent_count = 0;
int dedent_count = 0;
int newline_count = 0;

//#define debug(x) { cout << "TOKEN:\t"; if (#x == "yytext") cout << x << endl; else cout << #x << endl; }
#define debug(x)

%}

%option yylineno

%x indent
%x dedent
%x newline
%s normal

digit       ([0-9])
integer     ({digit}+)
float       ({integer}\.{integer}(e(\-)?{integer})?)
suffix      (f|F|ul?l?|ll?|UL?L?|LL?)
number      ((\-)?({integer}|{float}|0x([0-9]|[a-f]|[A-F])+){suffix}?)
ident       ([a-zA-Z_][a-zA-Z0-9_]*)
blank_chars ([ \f\r\t\v]+)
operator ("+"|"-"|"*"|"/"|"%"|"|"|"("|")"|"["|"]"|"{"|"}"|","|"<"|"="|">"|"~"|"&"|"!"|"^"|"="|":"|"?"|"`"|"\\")

%%

"if"            { debug(yytext); return IF; }
"else"          { debug(yytext); return ELSE; }
"for"           { debug(yytext); return FOR; }
"while"         { debug(yytext); return WHILE; }
"where"         { debug(yytext); return WHERE; }
"switch"        { debug(yytext); return SWITCH; }
"when"          { debug(yytext); return WHEN; }
"try"           { debug(yytext); return TRY; }
"catch"         { debug(yytext); return CATCH; }
"then"          { debug(yytext); return THEN; }
"this"          { debug(yytext); return THIS; }

"using"         { debug(yytext); return USING; }
"namespace"     { debug(yytext); return NAMESPACE; }

"=="            { debug(yytext); return EQ_EQ; }
"!="            { debug(yytext); return NOT_EQ; }

"not"           { debug(yytext); return '!'; }
"and"           { debug(yytext); return AND; }
"or"            { debug(yytext); return OR; }
"is"            { debug(yytext); return EQ_EQ; }
"isnt"          { debug(yytext); return NOT_EQ; }

"&&"            { debug(yytext); return AND; }
"||"            { debug(yytext); return OR; }

"<="            { debug(yytext); return LT_EQ; }
">="            { debug(yytext); return GT_EQ; }
"<<"            { debug(yytext); return LT_LT; }
">>"            { debug(yytext); return GT_GT; }

".."            { debug(yytext); return DOT_DOT; }
"<-"            { debug(yytext); return LT_MINUS; }
"=>"            { debug(yytext); return EQ_GT; }

"->"            { debug(yytext); return MINUS_GT; }
"."             { debug(yytext); return DOT; }
"::"            { debug(yytext); return SCOPING; }

"++"            { debug(yytext); return PLUS_PLUS; }
"--"            { debug(yytext); return MINUS_MINUS; }

"!!"            { debug(yytext); return DICT; }

"new"           { debug(yytext); return NEW; }
"as"            { debug(yytext); return AS; }
"sizeof"        { debug(yytext); return SIZE_OF; }
"delete"        { debug(yytext); return DELETE; }
"cast"          { debug(yytext); return CAST; }

"return"        { debug(yytext); return RETURN; }

"to"            { debug(yytext); return TO; }
"til"           { debug(yytext); return TIL; }
"downto"        { debug(yytext); return DOWN_TO; }
"by"            { debug(yytext); return BY; }

"const"         { debug(yytext); return CONST; }
"public"        { debug(yytext); return PUBLIC; }
"private"       { debug(yytext); return PRIVATE; }
"virtual"       { debug(yytext); return VIRTUAL; }
"inline"        { debug(yytext); return INLINE; }
"unsigned"      { debug(yytext); return UNSIGNED; }
"signed"        { debug(yytext); return SIGNED; }
"long"          { debug(yytext); return LONG; }
"short"         { debug(yytext); return SHORT; }
"int"           { debug(yytext); return INT; }
"double"        { debug(yytext); return DOUBLE; }
"override"      { debug(yytext); return OVERRIDE; }

"import"        { debug(yytext); return IMPORT; }
"class"         { debug(yytext); return CLASS; }
"enum"          { debug(yytext); return ENUM; }

"+="            { debug(yytext); return PLUS_EQ; }
"-="            { debug(yytext); return MINUS_EQ; }
"*="            { debug(yytext); return MUL_EQ; }
"/="            { debug(yytext); return DIV_EQ; }
"%="            { debug(yytext); return MOD_EQ; }
"&="            { debug(yytext); return AND_EQ; }
"|="            { debug(yytext); return OR_EQ; }
"^="            { debug(yytext); return XOR_EQ; }
"<<="           { debug(yytext); return LT_LT_EQ; }
">>="           { debug(yytext); return GT_GT_EQ; }

":="            { debug(yytext); return COLON_EQ; }

"->*"           { debug(yytext); return MINUS_GT_STAR; }
".*"            { debug(yytext); return DOT_STAR; }

\".*\"          { debug(yytext); yylval.STRING=strdup(yytext); return STRING; }
\'.*\'          { debug(yytext); yylval.STRING=strdup(yytext); return STRING; }

{ident}         { debug(yytext); yylval.STRING=strdup(yytext); return IDENT; }
{number}        { debug(yytext); yylval.STRING=strdup(yytext); return NUMBER; }
{operator}      { debug(yytext); return *yytext; }
(\r?\n)+[ ]*    {
                    int len = 0;
                    for (int i = 0; i < yyleng; i++) len += yytext[i] != char(13) && yytext[i] != char(10);
                    if (len > 0 && (indents.size() == 0 || indents.top() < len)) {
                        indent_count = 1;
                        indents.push(len);
                        // cout << indents.size() << endl;
                        newline_count = 1;
                        BEGIN(newline);
                    } else if (indents.size() > 0 && indents.top() > len) {
                        dedent_count = 0;
                        // cout << indents.size() << endl;
                        while (indents.size() > 0 && indents.top() > len) {
                            indents.pop();
                            dedent_count++;
                        }
                        //cout << indents.size() << endl;
                        if (dedent_count > 0) {
                            newline_count = 1;
                            BEGIN(dedent);
                        }
                    } else {
                        newline_count = 1;
                        BEGIN(newline);
                    }
                }

{blank_chars}	{ }
\/\/.*          { }

<<EOF>>     {
                if (indents.size() > 0) {
                    indents.pop();
                    debug(DEDENT);
                    return DEDENT;
                }
                if (dedent_count > 0) {
                    dedent_count--;
                    debug(DEDENT);
                    return DEDENT;
                }
                yyterminate();
            }

<indent>.  {
                indent_count--;
                if (indent_count == 0) {
                    if (indent_count > 0)
                        BEGIN(indent);
                    else if (dedent_count > 0)
                        BEGIN(dedent);
                    else if (newline_count > 0)
                        BEGIN(newline);
                    else BEGIN(INITIAL);
                }
                unput(*yytext);
                debug(INDENT);
                return INDENT;
            }

<dedent>.  {
                dedent_count--;
                if (dedent_count == 0) {
                    if (indent_count > 0)
                        BEGIN(indent);
                    else if (dedent_count > 0)
                        BEGIN(dedent);
                    else if (newline_count > 0)
                        BEGIN(newline);
                    else BEGIN(INITIAL);
                }
                unput(*yytext);
                debug(DEDENT);
                return DEDENT;
            }

<newline>.  {
                newline_count--;
                if (newline_count == 0) {
                    if (indent_count > 0)
                        BEGIN(indent);
                    else if (dedent_count > 0)
                        BEGIN(dedent);
                    else if (newline_count > 0)
                        BEGIN(newline);
                    else BEGIN(INITIAL);
                }
                unput(*yytext);
                debug(NEWLINE);
                return NEWLINE;
            }

%%

int yywrap(void)
{
	return 1;
}
