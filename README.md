# SugarCpp
SugarCpp is a language which can compile C++11.

The generated code is of very high quality and can be comparable with the handwritten code.

SugarCpp is still under development.
If you have any idea, please open the issue.

Try SugarCpp in browser: http://curimit.com/project/SugarCpp/

## Examples

#### Hello world
```c++
import "stdio.h"
void main() = printf("Hello world!") 
``` 

```c++
#include "stdio.h"

void main() {
    printf("Hello world!");
}
```

#### Calculate Sum
```c++
import "stdio.h"

int main()
    sum := 0
    for (i := 1; i < 10; i++)
        sum = sum + i
    printf("sum = %d\n", sum)
```

```c++
#include "stdio.h"

int main() {
    auto sum = 0;
    for (auto i = 1; i < 10; i++) {
        sum = sum + i;
    }
    printf("sum = %d\n", sum);
}
```

#### Generic Programming
```c++
T max<T>(x: T, y: T) = x if x > y else y
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
a := 1
b : int
c : int := 0
```

```c++
auto a = 1;
int b;
int c = 0;
```

#### Multiple return values && Parallel assignment
```c++
import "stdio.h"
       "tuple"

using std::tuple

tuple<T, T> sort<T>(a: T, b: T) = (a,b) if a < b else (b,a)

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

#### LINQ
```c++
import "stdio.h"
       "vector"

using namespace std

int main()
    a, b : vector<int>
    a.push_back(4)
    a.push_back(1)
    a.push_back(3)
    
    b.push_back(5)
    b.push_back(6)
    b.push_back(2)
    
    from x in a
    from y in b
    where x + 1 == y
    let sum = x + y
        printf("%d + %d = %d\n", x, y, sum)
```

```c++
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
};
```

#### Haskell style infix function
```C#
import "stdio.h"
       "algorithm"

using std::max

int main()
    a := 1
    b := 2
    x := a `max` b
    printf("%d\n", x)
``` 

```c++
#include "stdio.h"
#include "algorithm"

using std::max;

int main() {
    auto a = 1;
    auto b = 2;
    auto x = max(a, b);
    printf("%d\n", x);
}
```

#### C# style extension method
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
enum MyFlags = Flag1 | Flag2 | Flag3 | Flag4
```

```c++
enum MyFlags {
    Flag1 = 0,
    Flag2 = 1,
    Flag3 = 2,
    Flag4 = 4
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
typedef int_ptr = int*
```

```c++
typedef int* int_ptr;
```
