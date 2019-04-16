using System;
using System.Text.RegularExpressions;
using Bridge;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class LocalValueFieldBuilder {
        public static LocalValue<T> Build<WidgetT,T>(
                IReadWriteValueView<WidgetT,T> view, params Validate<T>[] validators) {

            var result = new LocalValue<T>(default(T), default(T));
            view.BindReadWriteAndInitialize(result);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<T> Build<WidgetT,T>(T defaultValue,
            IReadWriteValueView<WidgetT,T> view, params Validate<T>[] validators) {

            var result = new LocalValue<T>(defaultValue, default(T));
            view.BindReadWriteAndInitialize(result);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }
        
        public static LocalValue<DateTime?> BuildDateTimePicker(
                DateTimePickerView view, DateTime? defaultValue = null, 
                params Validate<DateTime?>[] validators) {
            
            var result = new LocalValue<DateTime?>(defaultValue);
            
            view.BindReadWriteAndInitialize(result);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        /// <param name="domainToView">doesn't need to handle nulls/empty values</param>
        /// <param name="viewToDomain">doesn't need to handle nulls/empty values</param>
        public static LocalValue<DataT> BuildNullableDropdown<WidgetT,DataT>(
                Func<DataT,string> domainToView,
                Func<string,DataT> viewToDomain,
                // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<DataT>[] validators)
                    where DataT:class {
            
            return Build(null, view,
                x => Tuple.Create(x == null ? "" : domainToView(x), ""), //2nd param is irrelevant
                x => string.IsNullOrEmpty(x.Item1) ? null : viewToDomain(x.Item1),
                validators);
        }
        
        public static LocalValue<DataT> BuildChoice<WidgetT,DataT>(
                DataT defaultVal,
                Func<string,DataT> viewToDomain,
                // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<DataT>[] validators) {
            
            return Build(defaultVal, view,
                x => Tuple.Create(x.ToString(), ""), //2nd param is irrelevant
                x => {
                    if (string.IsNullOrEmpty(x.Item1)) {
                        throw new Exception("cannot have null value in non nullable enum dropdown");
                    }
                    return viewToDomain(x.Item1);
                },
                validators);
        }
        
        //T should be enum but impossible to give such constraint. Using:
        //http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
        //but thrown out IConvertible doesn't seem to work in bridge.net
        public static LocalValue<EnumT> BuildEnumBasedChoice<WidgetT,EnumT>(
                EnumT defaultVal,
                // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<EnumT>[] validators) 
                    where EnumT:struct {
            
            return Build(defaultVal, view,
                x => Tuple.Create(x.ToString(), ""), //2nd param is irrelevant
                x => {
                    if (string.IsNullOrEmpty(x.Item1)) {
                        throw new Exception("cannot have null value in non nullable enum dropdown");
                    }
                    return (EnumT)(object)Convert.ToInt32(x.Item1);
                },
                validators);
        }

        //T should be enum but impossible to give such constraint. Using:
        //http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
        //but thrown out IConvertible doesn't seem to work in bridge.net
        public static LocalValue<EnumT?> BuildNullableEnumBasedDropdown<WidgetT,EnumT>(
            // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<EnumT?>[] validators) 
                    where EnumT:struct {

            return Build(null, view,
                x => Tuple.Create(x.ToString(), ""), //2nd param is irrelevant
                x => string.IsNullOrEmpty(x.Item1) ? (EnumT?)null : (EnumT)(object)Convert.ToInt32(x.Item1),
                validators);
        }
        
        //T should be enum but impossible to give such constraint. Using:
        //http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
        //but thrown out IConvertible doesn't seem to work in bridge.net
        public static LocalValue<EnumT?> BuildNullableEnumBasedDropdown<WidgetT,EnumT>(
                EnumT defaultVal,
                // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<EnumT?>[] validators) 
                    where EnumT:struct {
            
            return Build(defaultVal, view,
                x => Tuple.Create(x.ToString(), ""), //2nd param is irrelevant
                x => string.IsNullOrEmpty(x.Item1) ? (EnumT?)null : (EnumT)(object)Convert.ToInt32(x.Item1),
                validators);
        }

        public static LocalValue<int> BuildIntBasedDropdown<WidgetT>(
                int defaultValue, 
                // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, 
                params Validate<int>[] validators) {

            return Build(defaultValue, view,
                x => Tuple.Create(x.ToString(), ""), //2nd param is irrelevant
                x => x.Item1 == "" ? -1 : Convert.ToInt32(x.Item1),
                validators);
        }
        
        public static LocalValue<string> BuildStringBasedDropdown<WidgetT>(
            // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<string>[] validators) {

            return Build(null, view,
                x => Tuple.Create(x != null ? x : "", ""), //2nd param is irrelevant
                x => string.IsNullOrEmpty(x?.Item1) ? null : x.Item1,
                validators);
        }

        public static LocalValue<int?> BuildNullableIntBasedDropdown<WidgetT>(
            // REVIEW: make view be of some new class or make it ValueTuple with element names
                IReadWriteValueView<WidgetT,Tuple<string,string>> view, params Validate<int?>[] validators) {

            return Build(null, view,
                x => Tuple.Create(x.HasValue ? x.Value.ToString() : "", ""), //2nd param is irrelevant
                x => string.IsNullOrEmpty(x?.Item1) ? (int?)null : Convert.ToInt32(x.Item1),
                validators);
        }

        public static LocalValue<ModelT> Build<WidgetT,ModelT,ViewT>(
                ModelT defaultValue, IReadWriteValueView<WidgetT,ViewT> view, 
                Func<ModelT,ViewT> convertFromDomain, Func<ViewT,ModelT> convertToDomain, 
                params Validate<ModelT>[] validators) {

            var result = new LocalValue<ModelT>(defaultValue);
            view.BindReadWriteAndInitialize(result, convertFromDomain, convertToDomain);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }
        
        public static LocalValue<ModelT> Build<WidgetT,ModelT,ViewT>(
                ModelT defaultValue, IReadOnlyValueView<WidgetT,ViewT> view, 
                Func<ModelT,ViewT> convertFromDomain,
                params Validate<ModelT>[] validators) {

            var result = new LocalValue<ModelT>(defaultValue);
            view.BindReadOnlyAndInitialize(result, convertFromDomain);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }
        
        public static LocalValue<ModelT> Build<WidgetT,ModelT,ViewT>(
                IReadOnlyValueView<WidgetT,ViewT> view, Func<ModelT,ViewT> convertFromDomain,
                params Validate<ModelT>[] validators) {

            var result = new LocalValue<ModelT>(default(ModelT));
            view.BindReadOnlyAndInitialize(result, convertFromDomain);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<DataT> Build<WidgetT,DataT>(
                DataT defaultValue, IReadOnlyValueView<WidgetT,DataT> view,
                params Validate<DataT>[] validators) {

            var result = new LocalValue<DataT>(defaultValue);
            view.BindReadOnlyAndInitialize(result, x => x);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<DataT> Build<WidgetT,DataT>(
                IReadOnlyValueView<WidgetT,DataT> view, params Validate<DataT>[] validators) {

            var result = new LocalValue<DataT>(default(DataT));
            view.BindReadOnlyAndInitialize(result, x => x);
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<decimal> BuildDecimal<WidgetT>(
                IReadOnlyValueView<WidgetT,string> view, DecimalFormat format, params Validate<decimal>[] validators) {
            
            var result = new LocalValue<decimal>(0);
            view.BindReadOnlyAndInitialize(result, 
                x => I18n.Localize(x, format));
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<decimal> BuildDecimal(
                InputView view, DecimalFormat format, params Validate<decimal>[] validators) {
            
            view.RaisesChangedOnKeyPressed = false;

            var result = new LocalValue<decimal>(0);
            view.BindReadWriteAndInitialize(result, 
                x => I18n.Localize(x, format),
                x => I18n.ParseDecimal(x));
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<decimal?> BuildNullableDecimal(
                InputView view, DecimalFormat format, params Validate<decimal?>[] validators) {
            
            view.RaisesChangedOnKeyPressed = false;

            var result = new LocalValue<decimal?>(null);
            
            view.BindReadWriteAndInitialize(result, 
                x => !x.HasValue ? "" : I18n.Localize(x.Value, format),
                x => string.IsNullOrEmpty(x) ? (decimal?)null : I18n.ParseDecimal(x));
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }
        
        public static LocalValue<decimal> BuildDecimalWithPrecision<WidgetT>(
                IReadOnlyValueView<WidgetT,string> view, decimal defaultValue, Func<int> getPrecision, 
                params Validate<decimal>[] validators) {
             
            var result = new LocalValue<decimal>(defaultValue);
            
            view.BindReadOnlyAndInitialize(result, 
                x => I18n.Localize(x, DecimalFormatExtensions.GetWithPrecision(getPrecision())));

            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }
        
        public static LocalValue<decimal> BuildDecimalWithPrecision(
                InputView view, decimal defaultValue, Func<int> getPrecision, 
                params Validate<decimal>[] validators) {
            
            var result = new LocalValue<decimal>(defaultValue);
            view.RaisesChangedOnKeyPressed = false;

            view.BindReadWriteAndInitialize(result, 
                x => I18n.Localize(x, DecimalFormatExtensions.GetWithPrecision(getPrecision())),
                x => I18n.ParseDecimalWithoutLoss(x, getPrecision()));

            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<decimal?> BuildNullableDecimalWithPrecision(
                InputView view, Func<int> getPrecision, params Validate<decimal?>[] validators) {
            
            view.RaisesChangedOnKeyPressed = false;
             
            var result = new LocalValue<decimal?>(null);
            
            view.BindReadWriteAndInitialize(result, 
                x => !x.HasValue ? 
                        "" 
                    : 
                        I18n.Localize(x.Value, DecimalFormatExtensions.GetWithPrecision(getPrecision())),
                x => string.IsNullOrEmpty(x) ? 
                        (decimal?)null 
                    :
                        I18n.ParseDecimalWithoutLoss(x, getPrecision()));

            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<int?> BuildNullableInt<WidgetT>(
                int? defaultValue, IReadWriteValueView<WidgetT,string> view, 
                params Validate<int?>[] validators) {

            var result = new LocalValue<int?>(defaultValue);
            view.BindReadWriteAndInitialize(result, 
                x => !x.HasValue ? "" : I18n.Localize(x.Value),
                x => string.IsNullOrEmpty(x) ? (int?)null : I18n.ParseInt(x));
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<int?> BuildNullableInt<WidgetT>(
                IReadWriteValueView<WidgetT,string> view, 
                params Validate<int?>[] validators) {

            return BuildNullableInt(null, view, validators);
        }

        public static LocalValue<int> BuildInt<WidgetT>(
                IReadOnlyValueView<WidgetT,string> view, params Validate<int>[] validators) {

            var result = new LocalValue<int>(0);
            view.BindReadOnlyAndInitialize(result, x => I18n.Localize(x));
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<int> BuildInt<WidgetT>(
                IReadWriteValueView<WidgetT,string> view, params Validate<int>[] validators) {
            
            return BuildInt(0, view, validators);
        }

        public static LocalValue<int> BuildInt<WidgetT>(
                int defaultvalue, IReadWriteValueView<WidgetT,string> view, params Validate<int>[] validators) {

            var result = new LocalValue<int>(defaultvalue);
            view.BindReadWriteAndInitialize(result, x => I18n.Localize(x), x => {
                try {
                    var val = I18n.ParseInt(x);
                    return val;
                } catch (Exception ex) {
                    Logger.Error(typeof(LocalValueFieldBuilder), "BuildInt converter got exception {0}", ex);
                    throw new Exception(
                        I18n.Translate("Wrong integer format entered. Remove letters or special characters"));
                }
            });
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }

        public static LocalValue<T> BuildNonTrivial<WidgetT, T, UiDataT>(
            T defaultValue, 
            IReadWriteValueView<WidgetT,UiDataT> view, 
            Func<UiDataT, T> fromUi,
            Func<T, UiDataT> toUi,
            string basicValidationMsg,
            params Validate<T>[] validators) {

            var result = new LocalValue<T>(defaultValue);
            view.BindReadWriteAndInitialize(result, toUi, x => {
                try {
                    return fromUi(x);
                } catch (Exception ex) {
                    Logger.Error(typeof(LocalValueFieldBuilder), "converter got exception {0}", ex);
                    throw new Exception(basicValidationMsg);
                }
            });
            validators.ForEach(y => result.AddValidatorAndRevalidate(y));
            return result;
        }
    }
}
