using System;
using System.Collections.Generic;
using System.Linq;
using Philadelphia.Common;

namespace Philadelphia.Tests.Client.Model {
    public class TestModel : IHasSubscribeable {
        public enum ResulType {
            NotRun, Passed, Failure
        }

        private static IEnumerable<T> Singleton<T>(T x) { yield return x; }

        private static IEnumerable<object> ExplodeMsg(object msg) {
            IEnumerable<Exception> EnumerateAggr(AggregateException exception) {
                yield return exception;
                foreach (var innerException in exception.InnerExceptions) {
                    yield return innerException;
                }
            }

            IEnumerable<Exception> EnumerateExn(Exception exception) {
                yield return exception;
                while (exception.InnerException != null) {
                    yield return exception.InnerException;
                    exception = exception.InnerException;
                }
            }

            switch (msg) {
                case AggregateException a: return EnumerateAggr(a);
                case Exception ord: return EnumerateExn(ord);
                case IObjectWrapper w: return ExplodeMsg(w.Inner);
                default: return Singleton(msg);
            }
        }

        public class TestSession
        {
            private readonly List<object> _trace = new List<object>();
            public IEnumerable<object> Trace => _trace;
            public void WriteTrace(object msg) => _trace.Add(msg);

            public string TraceAsString => _trace.SelectMany(ExplodeMsg).ToJoinedString("\n");
        }

        public delegate void Test(TestSession session);

        private readonly Subscribeable _subscribeable = new Subscribeable();
        public ISubscribeable Subscribeable => _subscribeable;

        // NOTE: can't inline DefaultTest because bridge crashes if throw is used in lambda expression ...

        private string _log;
        private ResulType _outcome = ResulType.NotRun;

        public string Name { get; private set; }

        public Test Body { get; private set; }

        public ResulType Outcome {
            get => _outcome;
            private set {
                _outcome = value;
                _subscribeable.Notify(nameof(Outcome));
            }
        }

        public string Log {
            get => _log;
            private set {
                _log = value;
                _subscribeable.Notify(nameof(Log));
            }
        }

        public bool ExpectAssertionException { get; private set; }

        public void Run() {
            var sess = new TestSession();
            try
            {
                Body(sess);
                Outcome = ExpectAssertionException ? ResulType.Failure : ResulType.Passed;
                Log = sess.TraceAsString;
            }
            catch (AssertionException e) when (ExpectAssertionException)
            {
                Log = $"{sess.TraceAsString}\n\n{e}";
                Outcome = ResulType.Passed;
            }
            catch (Exception e) {
                Log = $"Test failed: {e}\n\n{sess.TraceAsString}";
                Outcome = ResulType.Failure;
            }
        }

        public static TestModel Create(string name, Test body, bool expectAssertionException) => 
            new TestModel { Name = name, Body = body, ExpectAssertionException = expectAssertionException };
    }
}