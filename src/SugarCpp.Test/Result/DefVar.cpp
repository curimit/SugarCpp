auto a = 1;
int b;
int c = 0;
int d = 0;
vector<int> e(10);
vector<int> list1(100);
vector<int> list2(100);
vector<int> list3(100);
int *&f, *&g;

int test(decltype(123) a = 123, int b) {
    auto a = 1;
    int b;
    int c = 0;
    int d = 0;
    vector<int> e(10);
    vector<int> list1(100)
    vector<int> list2(100)
    vector<int> list3(100);
    int *&f, *&g;
}