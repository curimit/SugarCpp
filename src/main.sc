import "AstSugar/Compiler.sc"

int main(argc: int, argv: char**)
    compiler := new Compiler()
    compiler->compile(argv[argc - 1])
