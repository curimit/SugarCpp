# SugarCpp
SugarCpp is a language which can compile to C++.

SugarCpp is still under development.If you have any idea, you can open an issue.

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
    RED,
    GREEN,
    BLUE
};
```

#### Define new variable
```c++
a := 1
b : int
c : int = 0
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
       "math.h"

float sqr(x: float) = x * x

int main()
    a := 100
    x := a :sqrt() :sqr()
    printf("%f\n", x)
```

```c++
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
```

#### Attributes
```haskell
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
