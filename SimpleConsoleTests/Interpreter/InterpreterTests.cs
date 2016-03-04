using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SimpleConsole.Tests
{
    [TestClass]
    public class InterpreterTests
    {
        private readonly IInterpreter _itpr = InterpreterFactory.Create(Console.In, Console.Out);

        private void Test(string expect, string input)
        {
            string str;
            var r = _itpr.Input(input, out str).ToString();
            Assert.AreEqual(expect, r);
            Console.WriteLine($"{str} => {r}");
        }

        [TestMethod]
        public void TestBase()
        {
            Test("[0]", "0");
            Test("[-1]", "-1");
            Test("[5.7]", "5.7");
            Test("[13]", "5 + 8");
            Test("[-2]", "12-14");
            Test("[8]", "1+3-5+9");
            Test("[65]", "3+7*9-1");
            Test("[-164558]", "2*(5+8)-5*2-123*(3453-777)/2");
            Test("[66]", "(20+2)*(6/2)");
            Test("[312]", "2*(6+2*(3+6*(6+6)))");
            Test("[312]", "(((6+6)*6+3)*2+6)*2");
            Test("[4]", "a=b=4");
            Test("[4]", "a");
            Test("[4]", "b");
            Test("[0]", "a-b");
        }

        [TestMethod]
        public void TestType()
        {
            Test("[0]", "a=0");
            Test("List :: Long", "type a");
            Test("Builtin", "type builtin");
            Test("[1]", "fn a=>1");
            Test("Function", "type a");
            Test("[1.4]", "a=1.4");
            Test("List :: Double", "type a");
        }

        [TestMethod]
        public void TestFunc()
        {
            Test("[2]", "fn a=>2");
            Test("[2]", "a");
            Test("[5.7]", "fn a=>5.7");
            Test("[5.7]", "a");
            Test("[]", "fn k x=>x*x");
            Test("[]", "fn a x y z=>(k x)+(k y)-(k z)");
            Test("[0]", "a 3 4 5");
            Test("[]", "fn b x y z=>x+y-z");
            Test("[0]", "b (k 3) ((k 4)) (((k 5)))");
            Test("[54]", "5+k+7");
            Test("[54]", "5+k-7");
            Test("[]", "fnx k x=>1+x");
            Test("[2, 3, 4, 5]", "k 1 2 3 4");
            Test("[]", "fnx k x xs=>x");
            Test("[1]", "k 1 2 3 4");
        }

        [TestMethod]
        public void TestBool()
        {
            Test("[True]", "bool 6");
            Test("[True]", "not not bool 6.6");
            Test("[9]", "(if 1 2) | (match 0 6 7)");
            Test("[True]", "is_empty empty");
            Test("[]", "empty");
            Test("[True]", "is_single 7");
            Test("[False]", "is_many 6");
            Test("[True]", "is_many 5 6");
        }

        [TestMethod]
        public void TestReduce()
        {
            Test("Module :: Math loaded.", "load Math");
            Test("[]", "fn fib n => lazy match lte n 2 n-1 sum fib n-1 fib n-2");
            Test("[]", "fn N n => lazy match lte n 1 1 sum 1 N n-1");
            Test("[]", "fn fib_sum n => lazy if gt n 0 sum fib n fib_sum n-1");
            Test("[1]", "N -1");
            Test("[20]", "N 20");
            Test("[5]", "fib 6");
            Test("[13]", "fib 8");
            Test("[33]", "fib_sum 8");
            Test("[232]", "fib_sum 12");
        }

        [TestMethod]
        public void TestMath()
        {
            Test("Module :: Math loaded.", "load Math");
            Test("Function", "type sin");
            Test("[]", "sin");
            Test("[4, 5, 1, -4, -5]", "round (5 * sin 1 2 3 4 5)");
            Test("[120]", "product 1 2 3 4 5");
            Test("[0]", "sum 1 -1 2 -2");
        }
    }
}