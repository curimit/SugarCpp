import
    "../common.h"

class Compiler
    public int compile(file: char*)
        cout << "[file.h]" << endl
        compile_h(file)
        cout << "[file.cpp]" << endl
        compile_cpp(file)

    int compile_h(file: char*)
        sFile := file
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

        value := yyroot->accept(new TargetCppHeader("file"))
        
        cout << "render..." << endl
        render := new cRender()
        value->accept(render)
        answer := render->result()

        out := fopen("file.h", "w")
        fprintf(out, "%s\n", answer.c_str())
        defer fclose(out)

        cout << answer << endl

    int compile_cpp(file: char*)
        sFile := file
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

        value := yyroot->accept(new TargetCppImplementation("file"))
        
        cout << "render..." << endl
        render := new cRender()
        value->accept(render)
        answer := render->result()

        out := fopen("file.cpp", "w")
        fprintf(out, "%s\n", answer.c_str())
        defer fclose(out)

        cout << answer << endl

