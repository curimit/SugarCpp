import "fstream"

using namespace std

void foo()
    fout: ofstream("output.txt")
    defer fout.close()
    fout.write("Hello World!", 12)
    if (false)
        return

// A more complex example to show the execution order of defer
void bar()
    print("1")
    defer print("defer1")
    if (true)
        print("2")
        defer print("defer2")
        if (true)
            print("3")
            defer print("defer3")
            print("4")
            return
        print("5")
    print("6")
    return