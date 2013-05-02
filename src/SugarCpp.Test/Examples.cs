using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using SugarCpp.Compiler;

namespace SugarCpp.Test
{
    [TestFixture]
    public class Examples
    {
        private Dictionary<string, TestCase> source = new Dictionary<string, TestCase>();

        [SetUp]
        public void Initialize()
        {
            foreach (var fileName in Directory.GetFiles("./Input", "*.sc"))
            {
                string caseName = new FileInfo(fileName).Name;
                caseName = caseName.Substring(0, caseName.Length - 3);
                string input = File.ReadAllText(string.Format("./Input/{0}.sc", caseName));
                string result = File.ReadAllText(string.Format("./Result/{0}.cpp", caseName));
                this.source[caseName] = new TestCase(input, result);
            }
        }

        public string Compile(string input)
        {
            var sugarCpp = new TargetCpp();
            return sugarCpp.Compile(input);
        }

        public void Test()
        {
            string caseName = (new StackTrace(1, true)).GetFrame(0).GetMethod().Name;
            string input = source[caseName].Input;
            string result = source[caseName].Result;
            string output = this.Compile(input);
            Assert.AreEqual(output, result, "Compile Result Error!");
        }

        [Test]
        public void HelloWorld()
        {
            Test();
        }

        [Test]
        public void CaculateSum()
        {
            Test();
        }
    }
}
