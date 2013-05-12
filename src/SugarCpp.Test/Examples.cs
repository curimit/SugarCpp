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
            Assert.AreEqual(output, result, string.Format("Compile Result Error!\nInput: \n{0}\n\nResult: \n{1}\n\n\nOutput: \n{2}", input, result, output));
        }

        [Test]
        public void FriendAttribute()
        {
            Test();
        }

        [Test]
        public void FlagAttribute()
        {
            Test();
        }

        [Test]
        public void CaculateSum()
        {
            Test();
        }

        [Test]
        public void DefVar()
        {
            Test();
        }

        [Test]
        public void Enum()
        {
            Test();
        }

        [Test]
        public void ExtensionMethod()
        {
            Test();
        }

        [Test]
        public void HelloWorld()
        {
            Test();
        }

        [Test]
        public void InfixFunction()
        {
            Test();
        }

        [Test]
        public void Namespace()
        {
            Test();
        }

        [Test]
        public void Template()
        {
            Test();
        }

        [Test]
        public void Tuple()
        {
            Test();
        }

        [Test]
        public void Class()
        {
            Test();
        }

        [Test]
        public void Typedef()
        {
            Test();
        }

        [Test]
        public void ForLoop()
        {
            Test();
        }

        [Test]
        public void ToStringAttribute()
        {
            Test();
        }

        [Test]
        public void Defer()
        {
            Test();
        }

        [Test]
        public void PatternMatching()
        {
            Test();
        }
    }
}
