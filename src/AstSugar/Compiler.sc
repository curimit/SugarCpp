import
    "../common.h"

class Compiler
    public int compile()
        sFile := "file.txt"
        fp := fopen(sFile, "r")
        if fp == nil
            printf("cannot open %s\n", sFile)
            return -1
        defer fclose(fp)

        yyin = fp

        state := yyparse()

        cout << "State: " << state << endl

        if state != 0
            cout << "compile error!" << endl
            return state

        value := yyroot->accept(new TargetCpp())
        
        cout << "render..." << endl
        render := new cRender()
        value->accept(render)
        output := render->result()

        cout << output