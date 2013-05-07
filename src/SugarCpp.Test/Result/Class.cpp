class Expr {
};

template <typename T>
class Number: public Expr {
public:
    T value;

    Number(T value) {
        this->value = value;
    }

    inline tuple<T> Unapply() {
        return std::make_tuple(value);
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

    inline tuple<string, Expr, Expr> Unapply() {
        return std::make_tuple(op, l, r);
    }
};