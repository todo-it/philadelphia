using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Tests.Client.Model
{
    public interface IObjectWrapper {
        object Inner { get; }
    }
    public struct Assertable<T> : IObjectWrapper
    {
        public T Item { get; }
        public bool UseAssertionException { get; }

        public Assertable(T item, bool useAssertionException)
        {
            Item = item;
            UseAssertionException = useAssertionException;
        }

        public Exception Exception(string msg) =>
            UseAssertionException 
                ? new AssertionException(msg) 
                : new Exception(msg);

        public override string ToString() => Item?.ToString();

        public Assertable<TT> Map<TT>(Func<T, TT> map) => new Assertable<TT>(map(Item), UseAssertionException);

        public Assertable<T> With(Action<T> assert) {
            assert(Item);
            return this;
        }

        public static implicit operator Assertable<T>(T x) => new Assertable<T>(x, true);
        object IObjectWrapper.Inner => Item;
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) {}
        public AssertionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class AssertableExt
    {
        public static Assertable<T> Assert<T>(this T x, bool useAssertionException = true) => new Assertable<T>(x, useAssertionException);
    }

    public static class Assert
    {
        public static Assertable<T> Equal<T>(this Assertable<T> a, T expected)
        {
            if (!object.Equals(a.Item, expected))
            {
                throw a.Exception($"Objects are not equal: Actual({a.Item?.ToString()}) <> Expected({expected?.ToString()})");
            }

            return a;
        }

        public static Assertable<T> NotEqual<T>(this Assertable<T> a, T notExpected)
        {
            if (object.Equals(a.Item, notExpected))
            {
                throw a.Exception($"Objects are equal: Actual({a.Item?.ToString()}) == Expected({notExpected?.ToString()})");
            }

            return a;
        }

        public static Assertable<T> NotNull<T>(this Assertable<T> a)
        {
            if (a.Item == null)
                throw a.Exception("Object is null, Expected=not null");
            return a;
        }

        public static Assertable<T> Null<T>(this Assertable<T> a)
        {
            if (a.Item != null)
                throw a.Exception("Object is not null, Expected=null");
            return a;
        }

        public static Assertable<T> True<T>(this Assertable<T> a, Func<T, bool> shouldBeTrue, string errorMsg)
        {
            if (!shouldBeTrue(a.Item))
                throw a.Exception($"Condition '{errorMsg}' is false, Expected=true");
            return a;
        }

        private static Assertable<T> SingleImpl<T, TT>(Assertable<TT> a, IReadOnlyList<T> sample)
        where TT : IEnumerable<T>
        {
            if (sample.Count == 0)
            {
                throw a.Exception("Collection is empty, expected exactly one item");
            }

            if (sample.Count > 1)
            {
                var coll = string.Join(", ", a.Item.Select(x => x.ToString()));
                throw a.Exception($"Collection has more than one element, expected exactly one item. Collection: \n{coll}");
            }

            return new Assertable<T>(sample[0], a.UseAssertionException);
        }

        public static Assertable<T> Single<T>(this Assertable<IEnumerable<T>> a)
        {
            var sample = a.Item
                .Select((v, i) => new {v, i})
                .TakeWhile(x => x.i <= 1)
                .Select(x => x.v)
                .ToArray();

            return SingleImpl(a, sample);
        }

        public static Assertable<T> Single<T>(this Assertable<T[]> a)
        {
            return SingleImpl(a, a.Item);
        }

        public static Assertable<T> Throws<T>(Action action) where T : Exception
        {
            string Msg(string msg) => 
                $"Supposed to throw exception type {typeof(T)}, instead {msg}";

            try
            {
                action();
            }
            catch (T e)
            {
                return new Assertable<T>(e, true);
            }
            catch (Exception exception)
            {
                throw new AssertionException(Msg("no exception thrown"), exception);
            }

            throw new AssertionException(Msg("no exception thrown"));
        }

        public static Assertable<Exception> ThrowsAny(Action action) => Throws<Exception>(action);
    }
}