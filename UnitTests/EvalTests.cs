using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eval.net;
using System.Threading;
using System.Threading.Tasks;

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
                (double)Evaluator.Execute("2*5!+3", config),
                (double)243);

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

            Assert.AreEqual(
                (double)Evaluator.Execute("-4^(7**2)**-2", config),
                (double)-4);

            Assert.AreEqual(
                (double)Evaluator.Execute("-4^7**(2**-2)", config),
                (double)-3);

            Assert.AreEqual(
                (double)Evaluator.Execute("-4^7**2**-2", config),
                (double)-3);

            Assert.AreEqual(
                (double)Evaluator.Execute("\"-4\"^7**\"2\"**-2", config),
                (double)-3);

            Assert.AreEqual(
                Evaluator.Execute("\"abc\"+5", config),
                "abc5");

            Assert.AreEqual(
                Evaluator.Execute("\"5\"+5", config),
                "55");

            Assert.AreEqual(
                Evaluator.Execute("5+\"5\"", config),
                "55");

            Assert.AreEqual(
                (double)Evaluator.Execute("12e5", config),
                1200000);

            Assert.AreEqual(
                (double)Evaluator.Execute("12e+5", config),
                1200000);

            Assert.AreEqual(
                Evaluator.Compile("CALL(a,b,)", config).GetAllVariablesInfo().Count,
                2);
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

        [TestMethod]
        public async System.Threading.Tasks.Task Execute100000TimesAsync()
        {
            var config = EvalConfiguration.DoubleConfiguration;

            var compiled = Evaluator.Compile("12+45*10", config);

            for (int i = 0; i < 100000; i++)
            {
                await compiled.ExecuteAsync(CancellationToken.None);
            }
        }

        [TestMethod]
        public void LazyFunctionExec()
        {
            var config = EvalConfiguration.DoubleConfiguration;
            config.SetFunction("DoNothing", (_, args) =>
            {
                return "OK";
            }, true);

            Assert.AreEqual(
                Evaluator.Execute("DoNothing(1/0)", config),
                "OK");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task LazyFunctionExecAsync()
        {
            var config = EvalConfiguration.DoubleConfiguration;
            config.SetFunction("DoNothing", (cancellationToken, _, args) =>
            {
                return Task.FromResult((object)"OK");
            }, true);

            Assert.AreEqual(
                await Evaluator.ExecuteAsync("DoNothing(1/0)", config, CancellationToken.None),
                "OK");

            Assert.AreEqual(
                Evaluator.Execute("DoNothing(1/0)", config),
                "OK");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task LazyFunctionExecAsync2()
        {
            var config = EvalConfiguration.DoubleConfiguration;
            config.SetFunction("Ret", (cancellationToken, _, args) =>
            {
                return (args[0] as EvalConfiguration.ArgResolverAsync)();
            }, true);

            Assert.AreEqual(
                await Evaluator.ExecuteAsync("Ret(123)", config, CancellationToken.None),
                "123");

            Assert.AreEqual(
                Evaluator.Execute("Ret(123)", config),
                "123");
        }

        [TestMethod]
        public void NonLazyFunctionExec()
        {
            var config = EvalConfiguration.DoubleConfiguration;
            config.SetFunction("DoNothing", (_, args) =>
            {
                return "OK";
            }, false);

            var didThrow = false;

            try
            {
                Evaluator.Execute("DoNothing(1/0)", config);
            }
            catch
            {
                didThrow = true;
            }

            Assert.AreEqual(didThrow, true);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task NonLazyFunctionExecAsync()
        {
            var config = EvalConfiguration.DoubleConfiguration;
            config.SetFunction("DoNothing", (cancellationToken, _, args) =>
            {
                return Task.FromResult((object)"OK");
            }, false);

            var didThrow = false;

            try
            {
                await Evaluator.ExecuteAsync("DoNothing(1/0)", config, CancellationToken.None);
            }
            catch
            {
                didThrow = true;
            }

            Assert.AreEqual(didThrow, true);
        }
    }
}
