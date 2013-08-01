import
    "../common.h"

class Compiler
    public int compile(file: char*)
        // parse file
        pre_compile(file)

        compile_h(file)
        compile_cpp(file)

    bool pre_compile(sFile: char*)
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

        cout << "parse success..." << endl

    int compile_h(file: char*)
        fname := strndup(file, strrchr(file, '.') - file)
        defer free(fname)
        header_name := (char*)malloc(strlen(fname) + strlen(".h") + 1)
        defer free(header_name)
        strcpy(header_name, fname)
        strcat(header_name, ".h")
        cout << "[" << header_name << "]" << endl

        value := yyroot->accept(new TargetCppHeader(fname))

        cout << "render..." << endl
        render := new cRender()
        value->accept(render)
        answer := render->result()

        out := fopen(header_name, "w")
        fprintf(out, "%s\n", answer.c_str())
        defer fclose(out)

        cout << answer << endl

    int compile_cpp(file: char*)
        fname := strndup(file, strrchr(file, '.') - file)
        defer free(fname)
        src_name := (char*)malloc(strlen(fname) + strlen(".cpp") + 1)
        defer free(src_name)
        strcpy(src_name, fname)
        strcat(src_name, ".cpp")
        cout << "[" << src_name << "]" << endl

        value := yyroot->accept(new TargetCppImplementation(fname))

        cout << "render..." << endl
        render := new cRender()
        value->accept(render)
        answer := render->result()

        out := fopen(src_name, "w")
        fprintf(out, "%s\n", answer.c_str())
        defer fclose(out)

        cout << answer << endl

