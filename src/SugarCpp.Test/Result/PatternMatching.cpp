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