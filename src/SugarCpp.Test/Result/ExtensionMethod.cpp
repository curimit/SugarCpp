#include "stdio.h"
#include "math.h"

float sqr(float x) {
    return x * x;
}

int main() {
    auto a = 100;
    auto x = sqr(sqrt(a));
    printf("%f\n", x);
}