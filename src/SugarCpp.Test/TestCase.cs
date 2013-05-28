using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Test
{
    internal class TestCase
    {
        public string Input { get; set; }
        public string Header { get; set; }
        public string Implementation { get; set; }

        public TestCase(string input, string header, string impelementation)
        {
            this.Input = input;
            this.Header = header;
            this.Implementation = impelementation;
        }
    }
}
