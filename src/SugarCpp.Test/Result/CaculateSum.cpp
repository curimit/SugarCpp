#include "stdio.h"

int main() {
    auto sum = 0;
    for (auto i = 1; i < 10; i++) {
        sum = sum + i;
    }
    printf("sum = %d\n", sum);
}