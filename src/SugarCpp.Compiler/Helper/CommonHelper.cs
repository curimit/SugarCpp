using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    internal class CommonHelper
    {
        private static Random _rd = new Random();

        public static int GetRandomInt()
        {
            return _rd.Next();
        }
    }
}
