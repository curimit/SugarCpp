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
    
    select (a, b) from friends
    select Family(@a, f, _) from family
    select Family(@b, @f, _) from family
        printf("%s and %s has same father %s\n", a.c_str(), b.c_str(), f.c_str())