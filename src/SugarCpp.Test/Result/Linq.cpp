#include "stdio.h"
#include "vector"

using namespace std;

int main() {
    vector<int> a, b;
    a.push_back(4);
    a.push_back(1);
    a.push_back(3);
    b.push_back(5);
    b.push_back(6);
    b.push_back(2);
    for (auto x : a) {
        for (auto y : b) {
            if (x + 1 == y) {
                auto sum = x + y;
                printf("%d + %d = %d\n", x, y, sum);
            }
        }
    }
}