using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Demo.Client {
    public static class Observable {
        public delegate void Subscriber<in T>(T e);

        public interface IObservable<out T> {
            void Add(Subscriber<T> subscriber);
        }

        public class Publisher<T> : IObservable<T> {
            private readonly List<Subscriber<T>> _subscribers = new List<Subscriber<T>>();
            public void Fire(T evt) => _subscribers.ForEach(h => h(evt));
            void IObservable<T>.Add(Subscriber<T> h) => _subscribers.Add(h);
        }

        public class Delegated<T> : IObservable<T> {
            private readonly Action<Subscriber<T>> _add;
            public Delegated(Action<Subscriber<T>> add) => _add = add;
            public void Add(Subscriber<T> subscriber) => _add(subscriber);
        }

        public static IObservable<B> Map<A, B>(this IObservable<A> a, Func<A, B> map) => 
            new Delegated<B>(subscriber => a.Add(e => map(e)));

        public static IObservable<A> Filter<A>(this IObservable<A> a, Predicate<A> accepts) {
            var src = new Publisher<A>();
            a.Add(ea => {
                if (accepts(ea)) src.Fire(ea);
            });
            return src;
        }

        public static IObservable<A> FromCliEvent<A>(Action<Subscriber<A>> add) => 
            new Delegated<A>(add);

        public static IObservable<A> FromCliEventHandler<A>(Action<EventHandler<A>> add) 
            => new Delegated<A>(subscriber => add((sender, a) => subscriber(a)));
    }
    
    // Poll below is something so obvious that perhaps it is not needed...
    public static class Poll {
        public delegate Answer Respondent<in Question, out Answer>(Question e);

        public interface IRegistrator<out Question, in Answer> {
            void Add(Respondent<Question, Answer> respondent);
        }

        public class Runner<Question, Answer>: IRegistrator<Question, Answer> {
            private readonly List<Respondent<Question, Answer>> _respondents = new List<Respondent<Question, Answer>>();
            public IEnumerable<Answer> Ask(Question q) => _respondents.Select(r => r(q));

            void IRegistrator<Question, Answer>.Add(Respondent<Question, Answer> h) => _respondents.Add(h);
        }
    }

    //TODO state aware validation
    //that is: a validator that can opt out from decision basing on any
    //subsequent validators that were fired
    //kind of fold-like behavior 
    public static class ExampleUsage {
        public class SimpleIntModel {
            private int _data = 0;

            private readonly Observable.Publisher<(int previous, int current)> _changed = new Observable.Publisher<(int, int)>();
            public Observable.IObservable<(int previous, int current)> Changed => _changed;

            public int Data {
                get => _data;
                set {
                    var oldValue = _data;
                    _data = value;
                    _changed.Fire((oldValue, value));
                }
            }
        }

        public class IntWithValidation {
            private readonly SimpleIntModel _data = new SimpleIntModel();
            private readonly Poll.Runner<(int previous, int current), bool> _validation = new Poll.Runner<(int previous, int current), bool>();
            public Poll.IRegistrator<(int previous, int current), bool> Validation => _validation;

            public int Data {
                get => _data.Data;
                set {
                    if (_validation.Ask((_data.Data, value)).All(x=>x)) {
                        _data.Data = value;
                    }
                }
            }
        }

        //public class CliEventExample {
        //    private int _data = 0;
            
        //    public event Observable.Subscriber<Unit> Created;
        //    public event EventHandler<(int previous, int current)> Changing;
        //    public event Action<(int previous, int current)> Changed;

        //    public CliEventExample() {
        //        Created?.Invoke(Unit.Instance);
        //    }

        //    public int Data {
        //        get => _data;
        //        set {
        //            var oldValue = _data;
        //            Changing?.Invoke(this, (oldValue, value));
        //            _data = value;
        //            Changed?.Invoke((oldValue, value));
        //        }
        //    }

        //    public Observable.IObservable<(int previous, int current)> ChangingObj =>
        //        Observable.FromCliEventHandler<(int, int)>(h => Changing += h);

        //    public Observable.IObservable<(int previous, int current)> ChangedObj =>
        //        Observable.FromCliEvent<(int, int)>(handler => Changed += tuple => handler(tuple));

        //    // will not compile
        //    //public Observable.IObservable<(int previous, int current)> ChangedObj2 =>
        //    //    Observable.FromCliEvent<(int, int)>(handler => Changed += handler);

        //    public Observable.IObservable<Unit> CreatedOj =>
        //        Observable.FromCliEvent<Unit>(handler => Created += handler);
        //}

        //public class CliEventIsolateBug {
        //    private int _data = 0;
            
        //    public event EventHandler<(int previous, int current)> Changing;

        //    public int Data {
        //        get => _data;
        //        set {
        //            var oldValue = _data;
        //            Changing?.Invoke(this, (oldValue, value));
        //            _data = value;
        //        }
        //    }

        //    //public Observable.IObservable<(int previous, int current)> ChangingObj => 
        //    //    Observable.FromCliEventHandler<(int, int)>(h => Changing += h);

        //    //public void Aqq() {

        //    //}
        //}
    }
}