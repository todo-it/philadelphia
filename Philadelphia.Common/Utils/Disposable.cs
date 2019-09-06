using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common
{
    public class Disposable : IDisposable
    {
        private readonly Action _onDispose;

        public Disposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }

        public static void DisposeBestEffort(IEnumerable<IDisposable> disposables)
        {
            Exception TryOne(IDisposable d)
            {
                try
                {
                    d.Dispose();
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }

            var errors = disposables.Select(TryOne).Where(x => x != null).ToList();
            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }
        }

        public static IDisposable OfSeqBestEffort(IEnumerable<IDisposable> disposables) 
            => new Disposable(() => DisposeBestEffort(disposables));
    }
}