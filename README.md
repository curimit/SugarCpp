# SugarC
SugarC is a language which can compile to C/C++.
We all known that c/c++ provide very few syntactic sugar.Sometimes we have to write more code to achieve the same functionality.

SugarC is designed to write less code and provide lots of syntactic sugar to reduce the quantity of C/C++ code.It looks like CoffeeScript but can compile to C/C++.

Notice, since SugarC is compile to C/C++, so it doesn't include garbage collection.

## Examples

#### Hello world
```
main() = printf("Hello world!") 
``` 

#### Support `yield` (The same as C#)
```
getList() =
	for i in [1..10]
		yield i
main() =
	for i in getList()
		printf("%d\n", x)
```