# SugarCpp
SugarCpp is a language which can compile to C++.

## Examples

#### Hello world
```c++
int main() = printf("Hello world!") 
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

#### Generic Programming
```c++
T max<T>(x : T, y : T) = x if x > y else y
```

#### Enumerated type
```c++
enum Color = Red | Green | Blue
```

#### Define new variable
```c++
a := 1
b : int
c : int = 0
```

#### Multiple return values​​ && Parallel assignment
```c++
import "stdio.h"
       "tuple"

using std::tuple

tuple<T, T> sort<T>(a : T, b : T) = (a,b) if a < b else (b,a)

int main()
	a := 10
	b := 1
    (a, b) = sort(a, b)
    printf("%d %d\n", a, b)
    (a, b) = (b, a)
    printf("%d %d\n", a, b)
```

#### Haskell style infix function
```haskell
import "stdio.h"
       "algorithm"

using std::max

int main()
    a := 1
    b := 2
    x := a `max` b
    printf("%d\n", x)
``` 

#### C# style extension method
```c++
import "stdio.h"
       "math.h"

float sqr(x : float) = x * x

int main()
    a := 100
    x := a :sqrt() :sqr()
    printf("%f\n", x)
```

#### Namespace
```c++
namespace SugarCpp::AstNode::Expr
    struct ExprBin
        Left, Right : Expr
        Op : string
```

#### Typedef
```c++
typedef uint = unsigned int
```

#### Garbage collection
SugarCpp use `shared_ptr` for all pointers.
Every array types are `shared_ptr<vector>`.

###### Pointer definition

```c++
// SugarCpp Code
a := new int(10)

// C++ Code
auto a = shared_ptr<int>(new int(10));
```

###### Array definition
```c++
// SugarCpp Code
a := new int[n, m]

// C++ Code
auto a = shared_ptr<vector<vector<int>>>(new vector<vector<int>>(n, vector<int>(m)));
```

###### Array access
```c++
// SugarCpp Code
t := a[x, y, z]

// C++ Code
auto t = a->at(x)[y][z];
```

###### Example: Fibonacci numbers
```c++
import "cstdio"
       "memory"
       "vector"

using namespace std

int main()
    n := 10
    fib := new int[n]
    (fib[0], fib[1]) = (1, 1)
    for (i := 2; i < n; i++)
        fib[i] = fib[i-1] + fib[i-2]
    printf("%d\n", fib[n-1])
```
