using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eval.net;

namespace UnitTests
{
    [TestClass]
    public class EvalTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var config = EvalConfiguration.DoubleConfiguration;

            Assert.AreEqual(
                (double)Evaluator.Execute("12+45*10", config),
                (double)12 + 45 * 10);

            Assert.AreEqual(
                (double)Evaluator.Execute("12/4 * 5 + 45*13 - 72 * 598", config),
                (double)12 / 4 * 5 + 45 * 13 - 72 * 598);

            Assert.AreEqual(
                (double)Evaluator.Execute("345 / 23 * 124 / 41 * 12", config),
                (double)345 / 23 * 124 / 41 * 12);

            Assert.AreEqual(
                (double)Evaluator.Execute("345 / 23 >> 3 * 124 / 41 * 12", config),
                (double)((int)(345 / 23) >> 3 * 124 / 41 * 12));

            Assert.AreEqual(
                (double)Evaluator.Execute("345 / (23 >> 3) * 124 / 41 * 12", config),
                (double)345 / (23 >> 3) * 124 / 41 * 12);

            Assert.AreEqual(
                (double)Evaluator.Execute("345 / pow(5,12/9) * 124 / 41 * 12", config),
                (double)345 / Math.Pow(5.0, 12.0 / 9.0) * 124 / 41 * 12);

            Assert.AreEqual(
                (bool)Evaluator.Execute("-5&&2==7&&-4>=-5>>-8*-5", config),
                (bool)(-5 != 0 && 2 == 7 && -4 >= -5 >> -8 * -5));

            Assert.AreEqual(
                (bool)Evaluator.Execute("\"testing\" == \"testing\"", config),
                (bool)true);

            Assert.AreEqual(
                (string)Evaluator.Execute("\"testing\"", config),
                (string)"testing");

            Assert.AreEqual(
                (string)Evaluator.Execute("\"testing\" + 58.3", config),
                (string)"testing58.3");

            var withConsts = config.Clone();
            withConsts.SetConstant("x", 5.9);
            Assert.AreEqual(
                (double)Evaluator.Execute("x * 27 + (8>>2) / x", withConsts),
                (double)5.9 * 27 + (8 >> 2) / 5.9);

            Assert.AreEqual(
                (double)Evaluator.Execute("max(1,5,8.7)", config),
                (double)8.7);

            Assert.AreEqual(
                (double)Evaluator.Execute("30 * PI", config),
                (double)30 * Math.PI);
        }

        [TestMethod]
        public void CompileAndExecute100000Times()
        {
            var config = EvalConfiguration.DoubleConfiguration;

            for (int i = 0; i < 100000; i++)
            {
                Evaluator.Execute("12+45*10", config);
            }
        }

        [TestMethod]
        public void Execute100000Times()
        {
            var config = EvalConfiguration.DoubleConfiguration;

            var compiled = Evaluator.Compile("12+45*10", config);

            for (int i = 0; i < 100000; i++)
            {
                compiled.Execute();
            }
        }
    }
}
