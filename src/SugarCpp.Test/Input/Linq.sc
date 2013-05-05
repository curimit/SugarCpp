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