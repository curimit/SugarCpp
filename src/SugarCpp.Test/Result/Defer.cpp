#include "fstream"

using namespace std;

void foo() {
    ofstream fout("output.txt");
    fout.write("Hello World!", 12);
    if (false) {
        fout.close();
        return;
    }
    fout.close();
}

void bar() {
    print("1");
    if (true) {
        print("2");
        if (true) {
            print("3");
            print("4");
            print("defer3");
            print("defer2");
            print("defer1");
            return;
        }
        print("5");
        print("defer2");
    }
    print("6");
    print("defer1");
    return;
}