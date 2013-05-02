import "stdio.h"

[friend(Print)]
class Test
    [public]
    Test(x: int) = this->x = x

    [private]
    x: int

class Print
    [public, static]
    void print(a :Test&) = printf("%d", a.x)

int main()
    a := Test(123)
    Print::print(a)