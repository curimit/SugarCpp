# SugarCpp
SugarCpp is a programming language which compiles to C++11.

It adds lots of syntax sugar in to C++ and is 100% equivalent C++ code.

SugarCpp is still under development.
If you have any idea, please open a issue.

Try SugarCpp in your browser: http://curimit.com/project/SugarCpp/

## Features

* Indent-based code block.
* Go style defer/finally statement, easier way to handle exceptions.
* Multiple return values & parallel assignment.
* Inline function defination.
* Haskell style infix function.
* Prolog style query with C# LINQ syntax.
* C# style extension method.
* Scala style case class.
* Pattern matching.

## Examples

#### Hello world
```c++
import "stdio.h"

int main()
    printf("Hello world!") 
``` 

```c++
#include "stdio.h"

int main() {
    printf("Hello world!");
}
```

#### Calculate Sum
```c++
import "stdio.h"

int main()
    sum := 0
    for i <- 1 to 10
        sum = sum + i
    printf("sum = %d\n", sum)
```

```c++
#include "stdio.h"

int main() {
    auto sum = 0;
    for (auto i = 1; i <= 10; ++i) {
        sum = sum + i;
    }
    printf("sum = %d\n", sum);
}
```

#### Defer and Finally
Due to C++11 does not support C# style Finally syntax, it's difficult to guarantee resource be closed or pointer be deleted while exception happens.

SugarCpp provide two syntax: defer/finally
+ `defer` is fast, lightweight, the generate C++ code is highly readable.
It simply insert code before `return`/`continue`/`break` statements.
So when exception happens, the codes decleared by `defer` are **not** guarantee to be run.

+ `finally` is little heavier than `defer`.
It is behaved just like the finally syntax in C# or Java.
It use deconstructor to guarantee when exception happens, the codes decleared by `finally` will still be run.

##### This is an example of defer
Notice it has no extra cost and does not handle exceptions.
```c++
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
    if true
        print("2")
        defer print("defer2")
        if true
            print("3")
            defer print("defer3")
            print("4")
            return
        print("5")
    print("6")
    return
```

```c++
#include "fstream"

using namespace std;

void foo() {
    ofstream fout("output.txt");
    fout.write("Hello World!", 12);
    if ((false)) {
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
```

##### This is an example of finally
This syntax behaved like `finally` in Java or C# and even easier to use.
```c++
import "stdio.h"
       "functional"

void test()
    finally printf("Finally!\n")
    throw(0)

int main()
    try
        test()
    catch x: int
        printf("catch: %d\n", x)
```

```c++
#include "stdio.h"
#include "functional"

void test() {
    class _t_finally_0 {
    public:
        std::function<void()> finally;
        ~_t_finally_0() { finally(); }
    } _t_finally_0 = { [&]() { printf("Finally!\n"); } };
    throw(0);
}

int main() {
    try {
        test();
    } catch (int x) {
        printf("catch: %d\n", x);
    }
}
```

#### Generic Programming
```c++
T max<T>(x: T, y: T) = x > y ? x : y
```

```c++
template <typename T>
T max(T x, T y) {
    return x > y ? x : y;
}
```

#### Enumerated type
```c++
enum Color = RED | GREEN | BLUE
```

```c++
enum Color {
    RED = 0,
    GREEN,
    BLUE
};
```

#### Define new variable
```c++
// basic syntax
a := 1
a, b : int

// advanced syntax
a, b: int(1)
a, b := 1, 2
a, b := 1
```

```c++
auto a = 1;
int a, b;
int a(1);
int b(1);
auto a = 1;
auto b = 2;
auto a = 1;
auto b = 1;
```

#### Multiple return values && Parallel assignment
```c++
import "stdio.h"
       "tuple"

using std::tuple

tuple<T, T> sort<T>(a: T, b: T)
    return a < b ? (a, b) : (b, a)

int main()
    a := 10
    b := 1
    (a, b) = sort(a, b)
    printf("%d %d\n", a, b)
    (a, b) = (b, a)
    printf("%d %d\n", a, b)
```

```c++
#include "stdio.h"
#include "tuple"

using std::tuple;

template <typename T>
tuple<T, T> sort(T a, T b) {
    return a < b ? std::make_tuple(a, b) : std::make_tuple(b, a);
}

int main() {
    auto a = 10;
    auto b = 1;
    std::tie(a, b) = sort(a, b);
    printf("%d %d\n", a, b);
    std::tie(a, b) = std::make_tuple(b, a);
    printf("%d %d\n", a, b);
}
```

#### Scala style case class
```c++
class Expr
class Number<T>(value:T): Expr
class ExprBin(op:string, l:Expr, r:Expr): Expr
```

```
class Expr {
};

template <typename T>
class Number: public Expr {
public:
    T value;

    Number(T value) {
        this->value = value;
    }

    inline tuple<T> Unapply() {
        return std::make_tuple(value);
    }
};

class ExprBin: public Expr {
public:
    string op;
    Expr l;
    Expr r;

    ExprBin(string op, Expr l, Expr r) {
        this->op = op;
        this->l = l;
        this->r = r;
    }

    inline tuple<string, Expr, Expr> Unapply() {
        return std::make_tuple(op, l, r);
    }
};
```

#### Haskell style infix function
```C#
import "stdio.h"
       "algorithm"

using std::max

int main()
    a, b, c := 1, 2, 3
    x := a `max` b `max` c
    printf("%d\n", x)
``` 

```c++
#include "stdio.h"
#include "algorithm"

using std::max;

int main() {
    auto a = 1;
    auto b = 2;
    auto c = 3;
    auto x = max(max(a, b), c);
    printf("%d\n", x);
}
```

#### Attributes
##### 1. friend, public, private, static
```c++
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
```

```c++
#include "stdio.h"

class Test {
    friend class Print;

public:
    Test(int x) {
        this->x = x;
    }

private:
    int x;
};

class Print {
public:
    static void print(Test& a) {
        printf("%d", a.x);
    }
};

int main() {
    auto a = Test(123);
    Print::print(a);
}
```

##### 2. FlagAttribute
```c++
[FlagAttribute]
enum MyFlags = None | Flag1 | Flag2 | Flag3 | Flag4
```

```c++
enum MyFlags {
    None = 0,
    Flag1 = 1,
    Flag2 = 2,
    Flag3 = 4,
    Flag4 = 8
};
```

##### 3. ToString
```c++
import "stdio.h"

[ToString]
enum Day = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday

int main()
    day := Sunday
    name := day:ToString()
    printf("%s\n", name)
```

```c++
#include "stdio.h"

enum Day {
    Sunday = 0,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
};

const char* ToString(const Day& a) {
    switch (a) {
    case Sunday:
        return "Sunday";

    case Monday:
        return "Monday";

    case Tuesday:
        return "Tuesday";

    case Wednesday:
        return "Wednesday";

    case Thursday:
        return "Thursday";

    case Friday:
        return "Friday";

    case Saturday:
        return "Saturday";
    }
}

int main() {
    auto day = Sunday;
    auto name = ToString(day);
    printf("%s\n", name);
}
```

#### Namespace
```c++
namespace SugarCpp::AstNode::Expr
    class ExprBin
        Left, Right : Expr
        Op : string
```

```c++
namespace SugarCpp {
    namespace AstNode {
        namespace Expr {
            class ExprBin {
                Expr Left, Right;
                string Op;
            };
        }
    }
}
```

#### Typedef
```c++
typedef u_int32 = unsigned int
```

```c++
typedef unsigned int u_int32;
```

#### C# style extension method
This feature currently not available.
```c++
import "stdio.h"
       "string"

using namespace std

string ToString(n: int, base:=10)
    a:string := "0"
    a[0] = n % base + 48
    return a if n < base else ToString(n / base, base) + a

int main()
    a := 100
    base10 := a:ToString()
    base2 := a:ToString(2)
    printf("%s %s\n", base10.c_str(), base2.c_str())
```

```c++
#include "stdio.h"
#include "string"

using namespace std;

string ToString(int n, decltype(10) base = 10) {
    string a = "0";
    a[0] = n % base + 48;
    return n < base ? a : ToString(n / base, base) + a;
}

int main() {
    auto a = 100;
    auto base10 = ToString(a);
    auto base2 = ToString(a, 2);
    printf("%s %s\n", base10.c_str(), base2.c_str());
}
```

#### Prolog style query with C# LINQ syntax
This feature currently not available.
```c++
import "stdio.h"
       "iostream"
       "tuple"
       "vector"

using namespace std

class Family(child: string, father: string, mother: string)

int main()
    // Two way to unpacking object
    // 1. Using case class
    family: vector<Family>
    family.push_back(Family("a", "b", "c"))
    family.push_back(Family("d", "b", "f"))
    family.push_back(Family("e", "g", "h"))
    
    // 2. Using tuple
    friends: vector<tuple<string, string> >
    friends.push_back(("a", "d"))
    friends.push_back(("a", "e"))
    
    // @ means not define new variable
    from (a, b) in friends
    from Family(@a, f, _) in family
    from Family(@b, @f, _) in family
        printf("%s and %s has same father %s\n", a.c_str(), b.c_str(), f.c_str())
```

```c++
#include "stdio.h"
#include "iostream"
#include "tuple"
#include "vector"

using namespace std;

class Family {
public:
    string child;
    string father;
    string mother;

    Family(string child, string father, string mother) {
        this->child = child;
        this->father = father;
        this->mother = mother;
    }

    inline tuple<string, string, string> Unapply() {
        return std::make_tuple(child, father, mother);
    }
};

int main() {
    vector<Family> family;
    family.push_back(Family("a", "b", "c"));
    family.push_back(Family("d", "b", "f"));
    family.push_back(Family("e", "g", "h"));
    vector<tuple<string, string>> friends;
    friends.push_back(std::make_tuple("a", "d"));
    friends.push_back(std::make_tuple("a", "e"));
    for (auto _t_match : friends) {
        auto a = get<0>(_t_match);
        auto b = get<1>(_t_match);
        for (auto _t_iterator : family) {
            auto &&_t_match = _t_iterator.Unapply();
            if (std::get<0>(_t_match) == a) {
                auto f = std::get<1>(_t_match);
                for (auto _t_iterator : family) {
                    auto &&_t_match = _t_iterator.Unapply();
                    if ((std::get<0>(_t_match) == b) && (std::get<1>(_t_match) == f)) {
                        printf("%s and %s has same father %s\n", a.c_str(), b.c_str(), f.c_str());
                    }
                }
            }
        }
    }
}
```

## Command Line Usage

    SugarCpp
    Compiler Version 1.0.0
    Command Line Interface Version 1.0.2

    Project website: https://github.com/curimit/SugarCpp

    Usage:
        sugarcpp [filename] <options>
        sugarcpp compile [filename] <compiler arguments>
        sugarcpp run [filename] <arguments>

    Options:
        --ast /ast                  Output the abstract syntax tree.
        --help -h /help /h /?       Output this help text.
        --nocode /nocode            Do not print the generated code.
        --output -o /output /o [filename]
                                    Filename of output. If not specified, output
                                    will be printed to standard output.
        --token /token              Output the tokens.

    Examples:
        Translate into C++ code
            sugarcpp code.sc -o code.cpp
        Compile to binary by calling the default compiler
            sugarcpp compule code.sc -o code.exe
        Compile and run
            sugarcpp run code.sc

