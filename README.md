# SugarCpp
SugarCpp is a language which can compile to C++.

## Examples

#### Hello world
```
void main() = printf("Hello world!") 
``` 

#### Calculate Sum
```
import "iostream"
       "stdio.h"

int main()
    int sum = 0
    for (int i = 1 to n)
        sum = sum + i
    printf("sum = %d\n", sum)
```

#### Generic Programming
```
T max<T>(T x, T y) = x > y ? x : y
```

#### Enumerated type
```
enum Color = Red | Green | Blue
```

#### Multiple return values​​ && Parallel assignment
```
import "stdio.h"
       "tuple"

(T, T) sort<T>(T a, T b) = a<b? (a,b) : (b,a)

int main()
    |a| = 10
    |b| = 1
    (a, b) = sort(a, b)
    printf("%d %d\n", a, b)
    (a, b) = (b, a)
    printf("%d %d\n", a, b)
```
