using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class ValueViewExtensions {
        private static ResultHolder<ToT> RunConvertingExceptionToFailure<FromT,ToT>(Func<FromT, ToT> toInvoke, FromT arg) {
            try {
                return ResultHolder<ToT>.CreateSuccess(toInvoke(arg));
            } catch (Exception ex) {
                Logger.Debug(typeof(ValueViewExtensions), "RunConvertingExceptionToFailure got exception {0}", ex);
                return ResultHolder<ToT>.CreateFailure(I18n.TranslateNoExtraction(ex.Message), ex);
            }
        }
			
        public static ValueChangedRich<DomainT> BindRead<WidgetT,ViewT,DomainT>(this IReadOnlyValueView<WidgetT,ViewT> view, IReadOnlyValue<DomainT> domain, Func<DomainT,ViewT> convertFromDomain) {
            ValueChangedRich<DomainT> handler = (sender, oldValue, newValue, rawErrors, isUserAction) => {
                var errors = rawErrors.ToList();
                var allErrors = new HashSet<string>(errors);
                var value = RunConvertingExceptionToFailure(convertFromDomain, newValue);
                var selfInvalidation = sender == view && errors.Any();

                if (!value.Success) {
                    allErrors.Add(value.ErrorMessage);
                }

                Logger.Debug(typeof(ValueViewExtensions), "BindRead handler called with value after conversion={0} errors={1} senderIsSelf={2} selfInvalidation={3}", value, sender == view, allErrors.PrettyToString(), selfInvalidation);

                if (!selfInvalidation) { 
                    //don't overwrite errorneus entry if entry origins from self so that user sees wrong value
                    view.Value = value.Result;
                }

                view.SetErrors(allErrors, isUserAction);
            };
            domain.Changed += handler;
            return handler;
        }

        public static ValueChangedRich<DomainT> BindReadOnlyAndInitialize<WidgetT,ViewT,DomainT>(
            this IReadOnlyValueView<WidgetT,ViewT> view, 
            IReadOnlyValue<DomainT> domain, 
            Func<DomainT,ViewT> convertFromDomain) {
            
            var handler = view.BindRead(domain, convertFromDomain);
            handler(domain, domain.Value, domain.Value, domain.Errors, false);

            return handler;
        }

        public static ValueChangedRich<T> BindReadOnlyAndInitialize<WidgetT,T>(this IReadOnlyValueView<WidgetT,T> view, IReadOnlyValue<T> domain) {
            return view.BindReadOnlyAndInitialize(domain, x => x);
        }

        public static ValueChangedSimple<ViewT> BindWrite<WidgetT,ViewT,DomainT>(this IReadWriteValueView<WidgetT,ViewT> view, IReadWriteValue<DomainT> domain, Func<ViewT,DomainT> convertToDomain) {
            ValueChangedSimple<ViewT> handler = async (newValue, userChange) => {
                var value = RunConvertingExceptionToFailure(convertToDomain, newValue);	
                Logger.Debug(typeof(ValueViewExtensions), "BindWrite handler called with value after conversion={0}", value);

                if (value.Success) {
                    view.SetErrors(new HashSet<string>(), userChange);
                    view.IsValidating = true;
                    //await Task.Delay(5000); //testing only
                    try {
                        await domain.DoChange(value.Result, userChange);    
                    } finally {
                        view.IsValidating = false;    
                    }
                } else {
                    view.SetErrors(new HashSet<string>{value.ErrorMessage}, userChange);
                }
            };
            view.Changed += handler;
            return handler;
        }

        public static ValueChangedSimple<T> BindWrite<WidgetT,T>(
                this IReadWriteValueView<WidgetT,T> view, IReadWriteValue<T> domain) {

            return view.BindWrite(domain, x => x);
        }

        public static Tuple<ValueChangedRich<DomainT>,ValueChangedSimple<ViewT>> BindReadWriteAndInitialize<WidgetT,ViewT,DomainT>(
            this IReadWriteValueView<WidgetT,ViewT> view, IReadWriteValue<DomainT> domain, 
            Func<DomainT,ViewT> convertFromDomain, Func<ViewT,DomainT> convertToDomain) {
            
            var readHandler = view.BindRead(domain, convertFromDomain);
            var writeHandler = view.BindWrite(domain, convertToDomain);
            readHandler(domain, domain.Value, domain.Value, domain.Errors, false);
            Logger.Debug(typeof(ValueViewExtensions), "BindReadWriteAndInitialize current value = {0} errors={1}", domain.Value, domain.Errors.PrettyToString());
            return new Tuple<ValueChangedRich<DomainT>, ValueChangedSimple<ViewT>>(readHandler, writeHandler);
        }

        public static Tuple<ValueChangedRich<T>,ValueChangedSimple<T>> BindReadWriteAndInitialize<WidgetT,T>(
            this IReadWriteValueView<WidgetT,T> view, IReadWriteValue<T> domain) {
            return view.BindReadWriteAndInitialize(domain, x => x, x => x);
        }
    }
}
