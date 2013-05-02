using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Test
{
    internal class TestCase
    {
        public string Input { get; set; }
        public string Result { get; set; }

        public TestCase(string input, string result)
        {
            this.Input = input;
            this.Result = result;
        }
    }
}
