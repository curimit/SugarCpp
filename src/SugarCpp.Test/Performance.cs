using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SugarCpp.Compiler;

namespace SugarCpp.Test
{
    public class Performance
    {
        private string _input1MB;
        private string _input100KB;

        [SetUp]
        public void Initialize()
        {
            this._input1MB = File.ReadAllText("./Performance/SpeedTest1MB.sc");
            this._input100KB = File.ReadAllText("./Performance/SpeedTest100KB.sc");
        }

        [Test]
        public void SpeedTest100KB()
        {
            var sugarCpp = new TargetCpp();
            sugarCpp.Compile(_input100KB);
        }

        [Test]
        public void SpeedTest1MB()
        {
            var sugarCpp = new TargetCpp();
            sugarCpp.Compile(_input1MB);
        }
    }
}
