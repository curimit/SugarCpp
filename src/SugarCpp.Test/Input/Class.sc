class Expr
class Number<T>(value:T): Expr
class ExprBin(op:string, l:Expr, r:Expr): Expr