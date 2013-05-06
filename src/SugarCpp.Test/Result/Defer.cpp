#include "fstream"

using namespace std;

void foo() {
    ofstream fout("output.txt");
    fout.write("Hello World!", 12);
    if (1 == 2) {
        fout.close();
        return;
    }
    fout.close();
}

int main() {
    foo();
}