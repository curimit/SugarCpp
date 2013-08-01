import
    "iostream"
    "sstream"
    "vector"

using namespace std

class StringTemplate
    ss: stringstream
    indent_level: int
    is_newline: bool

    public StringTemplate()
        @ss.clear()
        @indent_level = 0
        @is_newline = false

    public void emit(x: string)
        ensure_newline()
        ss << x
    
    public void ensure_newline()
        if is_newline
            ss << "    " for i <- 1 to indent_level
            is_newline = false

    public void indent() = indent_level++

    public void dedent() = indent_level--

    public void newline()
        ss << endl
        is_newline = true

    public string result() = ss.str()
