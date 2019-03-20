using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class OperationBatchBuilder {
        private readonly List<Func<Task<Unit>>> _operations = new List<Func<Task<Unit>>>();

        public OperationBatchBuilder(Action<OperationBatchBuilder> initializer) => initializer(this);
        public IEnumerable<Func<Task<Unit>>> Build() => _operations;

        //
        // parameterless
        //
        public void Add<T>(Func<Task<T>> oper, Action<T> forwardOutcomeOrNull = null) {
            _operations.Add(async () => {
                var outcome = await oper();

                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.Exec(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        public void Add<T>(Func<Task<T>> oper, Func<T,Task> forwardOutcomeOrNull) {
            _operations.Add(async () => {
                var outcome = await oper();

                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.ExecAsync(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }
        
        //
        // one param
        //
        public void Add<InpT,OutT>(
                Func<InpT> input, Func<InpT,Task<OutT>> oper, Action<OutT> forwardOutcomeOrNull = null) {

            _operations.Add(async () => {
                var outcome = await oper(input());
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.Exec(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        public void Add<InpT,OutT>(
                Func<InpT> input, Func<InpT,Task<OutT>> oper, Func<OutT,Task> forwardOutcomeOrNull) {

            _operations.Add(async () => {
                var outcome = await oper(input());
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.ExecAsync(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        //
        // two params
        //
        public void Add<InpT1,InpT2,OutT>(
                Func<Tuple<InpT1,InpT2>> input, Func<InpT1,InpT2,Task<OutT>> oper, 
                Action<OutT> forwardOutcomeOrNull = null) {

            _operations.Add(async () => {
                var pars = input();
                var outcome = await oper(pars.Item1, pars.Item2);
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.Exec(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        public void Add<InpT1,InpT2,OutT>(
                Func<Tuple<InpT1,InpT2>> input, Func<InpT1,InpT2,Task<OutT>> oper, 
                Func<OutT,Task> forwardOutcomeOrNull) {

            _operations.Add(async () => {
                var pars = input();
                var outcome = await oper(pars.Item1, pars.Item2);
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.ExecAsync(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        //
        // three params
        //
        public void Add<InpT1,InpT2,InpT3,OutT>(
                Func<Tuple<InpT1,InpT2,InpT3>> input,     
                Func<InpT1,InpT2,InpT3,Task<OutT>> oper, 
                Action<OutT> forwardOutcomeOrNull = null) {

            _operations.Add(async () => {
                var pars = input();
                var outcome = await oper(pars.Item1, pars.Item2, pars.Item3);
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.Exec(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        public void Add<InpT1,InpT2,InpT3,OutT>(
                Func<Tuple<InpT1,InpT2,InpT3>> input,     
                Func<InpT1,InpT2,InpT3,Task<OutT>> oper, 
                Func<OutT,Task> forwardOutcomeOrNull) {

            _operations.Add(async () => {
                var pars = input();
                var outcome = await oper(pars.Item1, pars.Item2, pars.Item3);
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.ExecAsync(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        //
        // four params
        //
        public void Add<InpT1,InpT2,InpT3,InpT4,OutT>(
                Func<Tuple<InpT1,InpT2,InpT3,InpT4>> input,     
                Func<InpT1,InpT2,InpT3,InpT4,Task<OutT>> oper, 
                Action<OutT> forwardOutcomeOrNull = null) {

            _operations.Add(async () => {
                var pars = input();
                var outcome = await oper(pars.Item1, pars.Item2, pars.Item3, pars.Item4);
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.Exec(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }

        public void Add<InpT1,InpT2,InpT3,InpT4,OutT>(
                Func<Tuple<InpT1,InpT2,InpT3,InpT4>> input,     
                Func<InpT1,InpT2,InpT3,InpT4,Task<OutT>> oper, 
                Func<OutT,Task> forwardOutcomeOrNull) {

            _operations.Add(async () => {
                var pars = input();
                var outcome = await oper(pars.Item1, pars.Item2, pars.Item3, pars.Item4);
                
                if (forwardOutcomeOrNull != null) {
                    await ExecOnUiThread.ExecAsync(() => forwardOutcomeOrNull(outcome));
                }

                return Unit.Instance;
            });
        }
    }
}
