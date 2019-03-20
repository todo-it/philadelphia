using System;
using Philadelphia.Tests.Client.Model;
// ReSharper disable UnusedMember.Global

namespace Philadelphia.Tests.Client.Tests
{
    public class TestRunnerTests
    {
        private readonly Action<object> _trace;
        public TestRunnerTests(Action<object> trace)
        {
            _trace = trace;
        }

        [Fact]
        public void exception_causes_failure()
        {
            var t = TestModel.Create("x", s => { throw new Exception("test"); }, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Failure);
            _trace(t.Log);
        }

        [Fact]
        public void exception_causes_failure_even_when_expecting_AssertionException()
        {
            var t = TestModel.Create("x", s => { throw new Exception("test"); }, true);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Failure);
            _trace(t.Log);
        }

        [Fact]
        public void AssertionException_causes_failure()
        {
            var t = TestModel.Create("x", s => { throw new AssertionException("test"); }, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Failure);
            _trace(t.Log);
        }

        [Fact]
        public void AssertionException_causes_pass_when_requested()
        {
            var t = TestModel.Create("x", s => { throw new AssertionException("test"); }, true);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Passed);
            _trace(t.Log);
        }

        [Fact]
        public void assert_throws__proper_exception_passes()
        {
            void Test(TestModel.TestSession s)
            {
                Assert.Throws<AggregateException>(() => {throw new AggregateException();});
            }

            var t = TestModel.Create("x", Test, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Passed);
            _trace(t.Log);
        }

        [Fact]
        public void assert_throws__inproper_exception_fails()
        {
            void Test(TestModel.TestSession s)
            {
                Assert.Throws<AggregateException>(() => {throw new ArithmeticException();});
            }

            var t = TestModel.Create("x", Test, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Failure);
            _trace(t.Log);
        }

        [Fact]
        public void assert_throws__no_exception_fails()
        {
            void Test(TestModel.TestSession s)
            {
                Assert.Throws<AggregateException>(() => { });
            }

            var t = TestModel.Create("x", Test, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Failure);
            _trace(t.Log);
        }

        [Fact]
        public void assert_throwsany__any_exception_passes()
        {
            void Test(TestModel.TestSession s)
            {
                Assert.ThrowsAny(() => {throw new AggregateException();});
                Assert.ThrowsAny(() => {throw new ArgumentException();});
            }

            var t = TestModel.Create("x", Test, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Passed);
            _trace(t.Log);
        }

        [Fact]
        public void assert_throwsany_no_exception_fails()
        {
            void Test(TestModel.TestSession s)
            {
                Assert.ThrowsAny(() => { });
            }

            var t = TestModel.Create("x", Test, false);
            t.Run();
            t.Outcome.Assert().Equal(TestModel.ResulType.Failure);
            _trace(t.Log);
        }

        // TODO: .Assertable() tests

        public class WithoutTrace
        {
            [Fact]
            public void OkScenario()
            {
            }

            [Fact(ExpectAssertionException = true)]
            public void ErrorScenario()
            {
                throw new AssertionException("test");
            }
        }

        public class ActionTrace
        {
            private readonly Action<string> _trace;

            public ActionTrace(Action<string> trace)
            {
                _trace = trace;
                trace("in constructor");
            }

            [Fact]
            public void Fact1()
            {
                _trace("in fact1");
            }

            [Fact]
            public void Fact2()
            {
                _trace("in fact2");
            }
        }

        public class WithTestSession
        {
            private readonly TestModel.TestSession _sess;

            public WithTestSession(TestModel.TestSession sess)
            {
                _sess = sess;
                sess.WriteTrace("in constructor");
            }

            [Fact]
            public void Fact1()
            {
                _sess.WriteTrace("in fact1");
            }

            [Fact]
            public void Fact2()
            {
                _sess.WriteTrace("in fact2");
            }
        }

        [Fact]
        public void exception_trace() {
            try {
                throw new Exception("outter", new Exception("inner"));
            }
            catch (Exception e) {
                _trace(e);
            }
        }
    }
}