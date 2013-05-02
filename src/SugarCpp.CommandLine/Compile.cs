using System;
using System.IO;
using System.Text;

namespace SugarCpp.CommandLine
{
    class Compile
    {
        internal static void Main(string[] args)
        {
            if (args[0] != "compile")
            {
                Program.Panic("First argument should be 'compile'.");
            }
            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "--compiler" || arg == "/compiler")
                {
                    if (i + 1 == args.Length)
                    {
                        Program.Panic("No compiler specified after " + arg);
                    }
                    compiler = args[i + 1];
                    break;
                }
            }
            if (compiler == null)
            {
                GetCompiler();
            }
        }

        private static void GetCompiler()
        {
            // Environment varibles
            // Try clang++ g++ and cl
            Program.Panic("No compiler detected on your system. You should specify one by using --compiler or environment varible CXX.");
        }

        private static string compiler = null;
    }
}
