# SugarCpp
SugarCpp is a programming language which compiles to C++11.

It adds lots of syntax sugar in to C++ and is 100% equivalent C++ code.

SugarCpp is still under development.
If you have any idea, please open a issue.

Try SugarCpp in your browser: http://curimit.com/project/SugarCpp/

## Unit Test
#### Challenge the compiler on unit test page:
http://curimit.com/project/SugarCpp/unit_test.html

1. Add new test cases.
2. Edit/Remove exist test cases.
3. Run all unit tests.

## Features

* Indent-based code block.
* [Borrow lots of syntax sugar from CoffeeScript.](#syntax-sugar-borrow-from-coffeescript)
* [Go style defer/finally statement, be able to handle exceptions.](#defer-and-finally)
* [Powerful for loop.](#powerful-for-loop)
* [Multiple return values & parallel assignment.](#multiple-return-values--parallel-assignment)
* [C# style lambda expression.](#c-style-lambda-expression)
* Inline function definition.
* [Haskell style infix function.](#haskell-style-infix-function)
* [Switch case.](#switch-case)
* [Scala style case class.](#scala-style-case-class)

## Plugins
Sublime-Text:
https://github.com/curimit/Sublime-SugarCpp

vim:
https://github.com/ppwwyyxx/vim-SugarCpp


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

#### Powerful for loop
```c++
import "cstdio"
       "cstdlib"
       "vector"

using namespace std

int main()
    for i <- 1 to 10, j <- 1 to 10, i + j == 10
        printf("%d + %d = %d\n", i, j, i + j)

    sum := 0
    for i <- 4 downto 0 by -1, i != 2, x <- a[i]
        sum += x

    // i => i * i
    // means map i into i*i
    for i <- 1 to 100, i % 3 == 0, i => i * i
        printf("%d\n", i)

    // notice the type of i can be also changed
    for i <- ["12", "21"], i => atoi(i), i => i * 2
        printf("%d\n", i)
```

```c++
#include "cstdio"
#include "cstdlib"
#include "vector"

using namespace std;

int main() {
    for (auto i = 1; i <= 10; ++i) {
        for (auto j = 1; j <= 10; ++j) {
            if (i + j == 10) {
                printf("%d + %d = %d\n", i, j, i + j);
            }
        }
    }
    auto sum = 0;
    for (auto i = 4; i >= 0; i = i + -1) {
        if (i != 2) {
            for (auto x : a[i]) {
                sum += x;
            }
        }
    }
    for (auto i = 1; i <= 100; ++i) {
        if (i % 3 == 0) {
            {
                auto _t_iterator = i * i;
                auto i = _t_iterator;
                printf("%d\n", i);
            }
        }
    }
    for (auto i : { "12", "21" }) {
        {
            auto _t_iterator = atoi(i);
            auto i = _t_iterator;
            {
                auto _t_iterator = i * 2;
                auto i = _t_iterator;
                printf("%d\n", i);
            }
        }
    }
}
```

#### Multi-dimensional array
```c++
int main()
    a: int[2,3]
    a[1,2] = 2
```

```c++
int main() {
    int a[2][3];
    a[1][2] = 2;
}
```

#### Types
```c++
import "cstdint"
       "functional"

a: bool
b: char
c: uchar
d: schar
e: int
f: int8
g: int16
h: int32
i: int64
j: uint
k: uint8
l: uint16
m: uint32
n: uint64
o: float
p: double
q: long double

// func type
r: (int, int) -> int
s: () -> int
t: int -> ()
u: () -> ()

int main()
    a = true
```

```c++
#include "cstdint"
#include "functional"

bool a;
char b;
unsigned char c;
signed char d;
int e;
int8_t f;
int16_t g;
int32_t h;
int64_t i;
unsigned int j;
uint8_t k;
uint16_t l;
uint32_t m;
uint64_t n;
float o;
double p;
long double q;
std::function<int (int, int)> r;
std::function<int ()> s;
std::function<void (int)> t;
std::function<void ()> u;

int main() {
    a = true;
}
```

#### Multiple line function call
```c++
int main()
    glVertexAttribPointer(
        attribute_coord2d, // attribute
        2,                 // number of elements per vertex, here (x,y)
        GL_FLOAT,          // the type of each element
        GL_FALSE,          // take our values as-is
        0,                 // no extra data between each position
        triangle_vertices  // pointer to the C array
    )

    test(
        1,2,3
        4,5,6
    )

    test(
        1
        2
        3)

    test(1,2
         3,4)

    test(1,2
    3,4)
```

```c++
int main() {
    glVertexAttribPointer(attribute_coord2d, 2, GL_FLOAT, GL_FALSE, 0, triangle_vertices);
    test(1, 2, 3, 4, 5, 6);
    test(1, 2, 3);
    test(1, 2, 3, 4);
    test(1, 2, 3, 4);
}
```

#### Multiple line string
```c++
int main()
    a = @"""
    int main() {
        printf("adf")
    }
    """

    b = "test
    it"

    c = "test
        it"
```

```c++
int main() {
    a = "\nint main() {\n    printf(\"adf\")\n}\n";
    b = "test\nit";
    c = "test\n    it";
}
```

#### Operator overloading
Currently support + - * /, other operators are on todo list.
```c++
Test (+) (a: const Test&, b: const Test&)
    // something

Test (/) (a: const Test&, b: const Test&)
    // something
```

```c++
Test operator+(const Test &a, const Test &b) {
}

Test operator/(const Test &a, const Test &b) {
}
```

#### Curried lambda functions
```c++
import "iostream"

using namespace std

void example1()
    t := 0
    
    plus := (x: int, y: int) --> x + y + t
    
    f := plus(2)
    // print 5, 6
    cout << f(3) << " " << f(4) << endl
    
    x := 5
    fx := plus(x)
    // print 8, 9
    cout << fx(3) << " " << fx(4) << endl
    
    t = 10
    // print 18, 19
    cout << fx(3) << " " << fx(4) << endl

void example2()
    t := 0
    
    plus := (x: int, y: int) ==> x + y + t
    
    f := plus(2)
    // print 5, 6
    cout << f(3) << " " << f(4) << endl
    
    x := 5
    fx := plus(x)
    // print 8, 9
    cout << fx(3) << " " << fx(4) << endl
    
    t = 10
    // print 8, 9
    cout << fx(3) << " " << fx(4) << endl

int main()
    example1()
    example2()
```

```c++
#include "iostream"

using namespace std;

void example1() {
    auto t = 0;
    auto plus = ([&](int x) {
        return ([&, x](int y) {
        return x + y + t;
    });
    });
    auto f = plus(2);
    cout << f(3) << " " << f(4) << endl;
    auto x = 5;
    auto fx = plus(x);
    cout << fx(3) << " " << fx(4) << endl;
    t = 10;
    cout << fx(3) << " " << fx(4) << endl;
}

void example2() {
    auto t = 0;
    auto plus = ([=](int x) {
        return ([=](int y) {
        return x + y + t;
    });
    });
    auto f = plus(2);
    cout << f(3) << " " << f(4) << endl;
    auto x = 5;
    auto fx = plus(x);
    cout << fx(3) << " " << fx(4) << endl;
    t = 10;
    cout << fx(3) << " " << fx(4) << endl;
}

int main() {
    example1();
    example2();
}
```

#### [Beta] List Generation
```c++
import "iostream"
       "cstdio"
       "vector"
       "list"
       "forward_list"
       "queue"
       "stack"
       "set"
       "unordered_set"
       "map"
       "unordered_map"
       "tuple"

using namespace std

int main()
    a := [i for i <- 1 to 10] : vector<int>
    
    b := [i for i <- 1 to 10] : list<int>
    c := [i for i <- 1 to 10] : forward_list<int>
    
    d := [i for i <- 1 to 10] : queue<int>
    e := [i for i <- 1 to 10] : deque<int>
    f := [i for i <- 1 to 10] : priority_queue<int>
    
    g := [i for i <- 1 to 10] : stack<int>
    
    h := [i for i <- 1 to 10] : set<int>
    i := [i for i <- 1 to 10] : multiset<int>
    j := [i for i <- 1 to 10] : unordered_set<int>
    k := [i for i <- 1 to 10] : unordered_multiset<int>
    
    l := [[i, i*i] for i <- 1 to 10] : map<int, int>
    m := [[i, i*i] for i <- 1 to 10] : multimap<int, int>
    n := [[i, i*i] for i <- 1 to 10] : unordered_map<int, int>
    o := [[i, i*i] for i <- 1 to 10] : unordered_multimap<int, int>
    
    // combine list generation with for loop
    printf("%d\n", i) for i <- [i * i for i <- 1 to 100, (i & 1) is 0, (i % 3) is 0] : vector<int>
```

```c++
#include "iostream"
#include "cstdio"
#include "vector"
#include "list"
#include "forward_list"
#include "queue"
#include "stack"
#include "set"
#include "unordered_set"
#include "map"
#include "unordered_map"
#include "tuple"

using namespace std;

int main() {
    auto a = ({
        vector<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push_back(i);
        }
        _t_return_value;
    });
    auto b = ({
        list<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push_back(i);
        }
        _t_return_value;
    });
    auto c = ({
        forward_list<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push_front(i);
        }
        _t_return_value;
    });
    auto d = ({
        queue<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push(i);
        }
        _t_return_value;
    });
    auto e = ({
        deque<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push_front(i);
        }
        _t_return_value;
    });
    auto f = ({
        priority_queue<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push(i);
        }
        _t_return_value;
    });
    auto g = ({
        stack<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.push(i);
        }
        _t_return_value;
    });
    auto h = ({
        set<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert(i);
        }
        _t_return_value;
    });
    auto i = ({
        multiset<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert(i);
        }
        _t_return_value;
    });
    auto j = ({
        unordered_set<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert(i);
        }
        _t_return_value;
    });
    auto k = ({
        unordered_multiset<int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert(i);
        }
        _t_return_value;
    });
    auto l = ({
        map<int, int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert({ i, i * i });
        }
        _t_return_value;
    });
    auto m = ({
        multimap<int, int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert({ i, i * i });
        }
        _t_return_value;
    });
    auto n = ({
        unordered_map<int, int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert({ i, i * i });
        }
        _t_return_value;
    });
    auto o = ({
        unordered_multimap<int, int> _t_return_value;
        for (auto i = 1; i <= 10; ++i) {
            _t_return_value.insert({ i, i * i });
        }
        _t_return_value;
    });
    for (auto i : ({
        vector<int> _t_return_value;
        for (auto i = 1; i <= 100; ++i) {
            if ((i & 1) == 0) {
                if ((i % 3) == 0) {
                    _t_return_value.push_back(i * i);
                }
            }
        }
        _t_return_value;
    })) {
        printf("%d\n", i);
    }
}
```

#### [Beta] Where Expression
```c++
int unit(a: Point) = Point(a.x / k, a.y / k, a.z / k) where
    k := len(a)
```

```c++
int unit(Point a) {
    return ({
        auto k = len(a);
        Point(a.x / k, a.y / k, a.z / k);
    });
}
```

#### [Beta] Implicit switch
```c++
int gcd(a: int, b: int) =
    | a == 0 => b
    | _      => gcd(b % a, a)
```

```c++
int gcd(int a, int b) {
    return ({
        decltype(b) match;
        if (a == 0) {
            match = b;
        } else {
            match = gcd(b % a, a);
        }
        match;
    });
}
```

#### Defer and Finally
Due to the fact that C++11 does not support C# style Finally syntax, it's difficult to guarantee resource be closed or pointer be deleted while exception happens.

SugarCpp provide two syntax: defer/finally
+ `defer` is fast, lightweight, the generated C++ code is highly readable.
It simply insert code before `return`/`continue`/`break` statements.
So when exception happens, the codes decleared by `defer` are **not** guarantee to be run.

+ `finally` is little heavier than `defer`.
It is behaved just like the finally syntax in C# or Java.
It uses a destructor to guarantee when exception happens, the codes decleared by `finally` will still be run.

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

using namespace std

void test()
    x := new int[100]
    finally delete(x)

    x[0] = 1
    // maybe open file failed
    // maybe divided by zero
    // anyway, an exception occurs
    // we want to avoid memory leak
    throw(0)

int main()
    try
        test()
    catch x:int
        printf("catch: %d\n", x)
```

```c++
#include "stdio.h"
#include "functional"

using namespace std;

void test() {
    auto x = new int[100];
    class _t_finally_0 {
    public:
        std::function<void()> finally;
        ~_t_finally_0() { finally(); }
    } _t_finally_0 = { [&]() { delete(x); } };
    x[0] = 1;
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

#### Syntax sugar borrow from CoffeeScript

##### Existential Operator
```c++
int main()
    // ?= operator
    tree->left ?= new Node()

    // ? operator
    footprints = yeti ? "bear"
```

```c++
int main() {
    if (tree->left == nullptr) {
        tree->left = new Node();
    }
    footprints = yeti != nullptr ? yeti : "bear";
}
```

##### Chained Comparisons
```c++
int main()
    a = 1 <= x <= y <= 10

    b = 1 < x != 10
```

```c++
int main() {
    a = 1 <= x && x <= y && y <= 10;
    b = 1 < x && x != 10;
}
```

##### @ syntax represent this ponter
```coffeescript
[public]
class Point
    x, y := 0

    void set(x: int, y: int)
        @x = x
        @y = y
```

```c++
class Point {
public:
    auto x = 0;
    auto y = 0;

    void set(int x, int y) {
        this->x = x;
        this->y = y;
    }
};
```

##### Suffix If/While/Until/For
```c++
import "stdio.h"
       "initializer_list"

int main()
    // suffix if
    printf("haha!") if score > 90
    return 0 if score == 0

    // suffix while/until
    buy()  while supply > demand
    sell() until supply > demand

    // suffix for
    a[i] = 0 for i <- 1 to 10, i % 2 == 0
    printf("%s\n", food) for food <- ["toast", "cheese", "wine"]

    // combine together
    a[i] = i if i % 2== 0 for i <- list
```

```c++
#include "stdio.h"
#include "initializer_list"

int main() {
    if (score > 90) {
        printf("haha!");
    }
    if (score == 0) {
        return 0;
    }
    while (supply > demand) {
        buy();
    }
    while (!(supply > demand)) {
        sell();
    }
    for (auto i = 1; i <= 10; ++i) {
        if (i % 2 == 0) {
            a[i] = 0;
        }
    }
    for (auto food : { "toast", "cheese", "wine" }) {
        printf("%s\n", food);
    }
    for (auto i : list) {
        if (i % 2 == 0) {
            a[i] = i;
        }
    }
}
```

##### Arrays Initialization
```c++
grid:int[3, 3] = [
    [1, 2, 3]
    [4, 5, 6]
    [7, 8, 0]
]

list: int[9] = [
    1, 2, 3
    4, 5, 6
    7, 8, 9
]

line: int[3] = [1, 2, 3]
```

```c++
int grid[3][3] = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } };
int list[9] = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
int line[3] = { 1, 2, 3 };
```

##### Operators and Aliases
<table>
    <tr>
        <th>SugarCpp</th>
        <th>C++</td>
    </tr>
    <tr>
        <td>is</td>
        <td>==</td>
    </tr>

    <tr>
        <td>isnt</td>
        <td>!=</td>
    </tr>

    <tr>
        <td>not</td>
        <td>!</td>
    </tr>

    <tr>
        <td>and</td>
        <td>&&</td>
    </tr>

    <tr>
        <td>or</td>
        <td>||</td>
    </tr>
</table>

#### C# style lambda expression
```c++
import "stdio.h"

int main()
    x := 1

    // capture by reference
    f := () -> ++x

    // capture by value
    f := () => ++x
```

```c++
#include "stdio.h"

int main() {
    auto x = 1;
    auto f = ([&]() { return ++x; });
    auto f = ([=]() { return ++x; });
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

#### Scala style case class
```c++
case class Expr
case class Number<T>(value:T): Expr
case class ExprBin(op:string, l:Expr, r:Expr): Expr

void Test(expr: Expr*)
    // reflection get actual type.
    cout << expr->GetType() << endl

int main()
    expr: Expr
    number: Number<int>
    expr_bin: ExprBin

    Test(&expr)
    Test(&number)
    Test(&expr_bin)
```

```
class Expr {
public:
    virtual const char* GetType();
};

template <typename T>
class Number: public Expr {
public:
    T value;

    Number() = default;
    Number(T value) {
        this->value = value;
    }
    virtual const char* GetType() {
        return "Number";
    }
};

class ExprBin: public Expr {
public:
    string op;
    Expr l;
    Expr r;

    ExprBin() = default;
    ExprBin(string op, Expr l, Expr r);
    virtual const char* GetType();
};

const char* Expr::GetType() {
    return "Expr";
}

ExprBin::ExprBin(string op, Expr l, Expr r) {
    this->op = op;
    this->l = l;
    this->r = r;
}

const char* ExprBin::GetType() {
    return "ExprBin";
}

void Test(Expr *expr) {
    cout << expr->GetType() << endl;
}

int main() {
    Expr expr;
    Number<int> number;
    ExprBin expr_bin;
    Test(&expr);
    Test(&number);
    Test(&expr_bin);
}
```

#### Haskell style infix function
```coffeescript
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

#### Switch case
```c++
int main()
    // switch table
    // auto break, auto add {}
    switch x
        when '0'
            i := 0
            printf("%d", i)
        when '1', '2'
            i := 4
            printf("%d", i)
        else
            printf("unknown")

    // if elsif syntax
    switch
        when 0 < i < 10
            printf("case1")
        when i <= 0
            printf("case2")
        else
            printf("else")
```

```c++
#include "test.h"

int main() {
    switch (x) {
    case '0':
        {
            auto i = 0;
            printf("%d", i);
            break;
        }

    case '1':
    case '2':
        {
            auto i = 4;
            printf("%d", i);
            break;
        }

    defalult:
        printf("unknown");
    }
    if (0 < i && i < 10) {
        printf("case1");
    } else if (i <= 0) {
        printf("case2");
    } else {
        printf("else");
    }
}
```

#### Attributes
##### 1. friend, public, private, static, inline, virtual
```c++
import "stdio.h"

[friend(Print), public]
class Test
    Test(x: int) = this->x = x

    [private]
    x: int

[public]
class Print
    [inline, static]
    void print(a :Test&) = printf("%d", a.x)

    [virtual, const]
    int get() = 1

int main()
    a := Test(123)
    Print::print(a)
```

```c++
#include "stdio.h"

class Test {
    friend class Print;

public:
    Test(int x);

private:
    int x;
};

class Print {
public:
    inline static void print(Test &a);

    virtual int get() const;
};

Test::Test(int x) {
    this->x = x;
}

inline void Print::print(Test &a) {
    printf("%d", a.x);
}

int Print::get() const {
    return 1;
}

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
    name := day @ToString()
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

const char* ToString(const Day& _t_value) {
    switch (_t_value) {
    case Sunday:
        {
            return "Sunday";
            break;
        }

    case Monday:
        {
            return "Monday";
            break;
        }

    case Tuesday:
        {
            return "Tuesday";
            break;
        }

    case Wednesday:
        {
            return "Wednesday";
            break;
        }

    case Thursday:
        {
            return "Thursday";
            break;
        }

    case Friday:
        {
            return "Friday";
            break;
        }

    case Saturday:
        {
            return "Saturday";
            break;
        }

    defalult:
        throw("Not Found");
    }
}

int main() {
    auto day = Sunday;
    auto name = ToString(day);
    printf("%s\n", name);
}
```

#### Function Type
```c++
import "iostream"
       "functional"

using namespace std

int apply(f: () -> int) = f()
int apply(f: (int, int) -> int, a: int, b: int) = f(a, b)

int main()
	a := 10
	b := 20
	ans1 := apply(() -> 100)
	cout << ans1 << endl
	ans2 := apply((a: int, b: int) -> a * b, a, b)
	cout << ans2 << endl
```

```c++
#include "iostream"
#include "functional"

using namespace std;

int apply(function<int ()> f) {
    return f();
}

int apply(function<int (int, int)> f, int a, int b) {
    return f(a, b);
}

int main() {
    auto a = 10;
    auto b = 20;
    auto ans1 = apply(([&]() { return 100; }));
    cout << ans1 << endl;
    auto ans2 = apply(([&](int a,int b) { return a * b; }), a, b);
    cout << ans2 << endl;
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
type u_int32 = uint
```

```c++
using u_int32 = unsigned int;
```

## Contributors
  * [BYVoid](https://github.com/BYVoid)
    * Implement command line interface.
    * Suggest the defer syntax.
  * [ppwwyyxx](https://github.com/ppwwyyxx)
    * Vim plugin for SugarCpp.

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
        Translate into C++ code, will generate .h and .cpp two files.
            sugarcpp code.sc
        Compile to binary by calling the default compiler
            sugarcpp compule code.sc -o code.exe
        Compile and run
            sugarcpp run code.sc

