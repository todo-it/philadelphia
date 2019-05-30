using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// Helper class to create instances of <see cref="FixedIdUpdateServiceMethodCaller{ContT}"/> bound to services compatible 
    /// with <see cref="FixedIdUpdateServiceMethodCaller{ContT}"/>
    /// </summary>
    public class RemoteFieldBuilder<ContT> where ContT : new() {
        private readonly FixedIdUpdateServiceMethodCaller<ContT> _caller;
        private readonly Action<ContT,string> _postOperationConsumerOrNull;

        public RemoteFieldBuilder(
            FixedIdUpdateServiceMethodCaller<ContT> caller,
            Action<ContT,string> postOperationConsumerOrNull) {

            _caller = caller;
            _postOperationConsumerOrNull = postOperationConsumerOrNull;
        }
        
        private string GetFieldName<DataT>(Expression<Func<ContT,DataT>> getField) {
            var member = getField.Body as MemberExpression;

            if (member == null) {
                throw new ArgumentException("getField expression is not of expected type MemberExpression");
            }
            return member.Member.Name;
        }

        public ClassFieldRemoteMutator<DateTime?,DateTime?,ContT> BuildDateTimePicker(
                Expression<Func<ContT,DateTime?>> getField, DateTimePickerView view,
                params Validate<DateTime?>[] validators) {
            
            return Build(getField, view, validators);
        }

        public ClassFieldRemoteMutator<decimal,decimal,ContT> BuildDecimal(
                Expression<Func<ContT,decimal>> getField, InputView view, DecimalFormat format,
                params Validate<decimal>[] validators) {

            view.RaisesChangedOnKeyPressed = false;
            return Build(getField, view, x => I18n.Localize(x, format), x => I18n.ParseDecimal(x), validators);
        }
        
        public ClassFieldRemoteMutator<decimal,decimal,ContT> BuildDecimalWithPrecision(
            Expression<Func<ContT,decimal>> getField, InputView view, 
            Func<int> getPrecision,
            params Validate<decimal>[] validators) {

            view.RaisesChangedOnKeyPressed = false;
            return Build(
                getField, 
                view, 
                x => I18n.Localize(x, DecimalFormatExtensions.GetWithPrecision(getPrecision())), 
                x => I18n.ParseDecimalWithoutLoss(x, getPrecision()), 
                validators);
        }
        
        public ClassFieldRemoteMutator<decimal?,decimal?,ContT> BuildNullableDecimal(
                Expression<Func<ContT,decimal?>> getField, InputView view, DecimalFormat format,
                params Validate<decimal?>[] validators) {

            view.RaisesChangedOnKeyPressed = false;

            return Build(getField, view,
                x => x.HasValue ? I18n.Localize(x.Value, format) : "",
                x => x != "" ? I18n.ParseDecimal(x) : (decimal?)null,
                validators);
        }

        public ClassFieldRemoteMutator<int,int,ContT> BuildInt<WidgetT>(
                Expression<Func<ContT,int>> getField, IReadWriteValueView<WidgetT,string> view,
                params Validate<int>[] validators) {

            return Build(getField, view, x => I18n.Localize(x), x => I18n.ParseInt(x), validators);
        }
        
        public ClassFieldRemoteMutator<DateTime?,DateTime?,ContT> BuildNullableDateTime<WidgetT>(
                Expression<Func<ContT,DateTime?>> getField, IReadOnlyValueView<WidgetT,string> view, 
                DateTimeFormat format) {
            
            return Build(getField, view, x => !x.HasValue ? "" : I18n.Localize(x.Value, format));
        }

        public ClassFieldRemoteMutator<int,int,ContT> BuildInt<WidgetT>(
                Expression<Func<ContT,int>> getField, IReadOnlyValueView<WidgetT,string> view) {

            return Build(getField, view, x => I18n.Localize(x));
        }
        
        public ClassFieldRemoteMutator<int?,int?,ContT> BuildNullableIntBasedSelectDropdown<WidgetT>(
                Expression<Func<ContT,int?>> getField, IReadWriteValueView<WidgetT,Tuple<string,string>> view,
                params Validate<int?>[] validators) {
            
            return Build(
                getField, view,
                x => Tuple.Create(!x.HasValue ? "" : x.Value.ToString(), ""),//second param is irrelevant
                x => string.IsNullOrEmpty(x?.Item1) ? (int?)null : Convert.ToInt32(x.Item1),
                validators);
        }
        
        public ClassFieldRemoteMutator<ModelT,ModelT,ContT> Build<WidgetT,ModelT,ViewT>(
                Expression<Func<ContT,ModelT>> getField, IReadOnlyValueView<WidgetT,ViewT> view,
                Func<ModelT,ViewT> convertFromDomain) {

            return new ClassFieldRemoteMutator<ModelT,ModelT,ContT>(
                getField,
                x => x,
                x => x,
                x => _caller.SaveField(GetFieldName(getField), x),
                x => view.BindReadOnlyAndInitialize(x, convertFromDomain),
                _postOperationConsumerOrNull);
        }
        
        public ClassFieldRemoteMutator<ModelT,ModelT,ContT> Build<WidgetT,ModelT,ViewT>(
                Expression<Func<ContT,ModelT>> getField, IReadWriteValueView<WidgetT,ViewT> view,
                Func<ModelT,ViewT> convertFromDomain, Func<ViewT,ModelT> convertToDomain,
                params Validate<ModelT>[] validators) {

            return new ClassFieldRemoteMutator<ModelT,ModelT,ContT>(
                getField,
                x => x,
                x => x,
                x => _caller.SaveField(GetFieldName(getField), x),
                x => {
                    validators.ForEach(y => x.AddValidatorAndRevalidate(y));
                    view.BindReadWriteAndInitialize(x, convertFromDomain, convertToDomain);
                },
                _postOperationConsumerOrNull);
        }

        public ClassFieldRemoteMutator<ModelT,ModelT,ContT> Build<WidgetT,ModelT,ViewT>(
            Expression<Func<ContT,ModelT>> getField, IReadOnlyValueView<WidgetT,ViewT> view,
            Func<ModelT,ViewT> convertFromDomain,
            params Validate<ModelT>[] validators) {

            return new ClassFieldRemoteMutator<ModelT,ModelT,ContT>(
                getField,
                x => x,
                x => x,
                x => _caller.SaveField(GetFieldName(getField), x),
                x => {
                    validators.ForEach(y => x.AddValidatorAndRevalidate(y));
                    view.BindReadOnlyAndInitialize(x, convertFromDomain);
                },
                _postOperationConsumerOrNull);
        }
        
        public ClassFieldRemoteMutator<ModelT,ModelT,ContT> Build<WidgetT,ModelT>(
            Expression<Func<ContT,ModelT>> getField, IReadOnlyValueView<WidgetT,ModelT> view,
            params Validate<ModelT>[] validators) {

            return new ClassFieldRemoteMutator<ModelT,ModelT,ContT>(
                getField,
                x => x,
                x => x,
                x => _caller.SaveField(GetFieldName(getField), x),
                x => {
                    validators.ForEach(y => x.AddValidatorAndRevalidate(y));
                    view.BindReadOnlyAndInitialize(x);
                },
                _postOperationConsumerOrNull);
        }

        public ClassFieldRemoteMutator<LocalT,RemT,ContT> BuildSingleChoiceDropDown<WidgetT,LocalT,RemT>(
                Expression<Func<ContT,RemT>> getRemoteField, IReadWriteValueView<WidgetT,LocalT> view,
                Func<LocalT,RemT> toRemoteType, Func<RemT,LocalT> toLocalType,
                params Validate<LocalT>[] validators) {

            return new ClassFieldRemoteMutator<LocalT,RemT,ContT>(
                getRemoteField,
                toRemoteType,
                toLocalType,
                x => _caller.SaveField(GetFieldName(getRemoteField), x),
                x => {
                    validators.ForEach(y => x.AddValidatorAndRevalidate(y));
                    view.BindReadWriteAndInitialize(x);
                });
        }

        public ClassFieldRemoteMutator<LocalCollT,RemT,ContT> BuildMultiChoiceDropDown<WidgetT,LocalCollT,RemT>(
                Expression<Func<ContT,RemT>> getRemoteField,
                IRestrictedMultipleReadWriteValueView<WidgetT,LocalCollT> view,
                Func<LocalCollT,RemT> toRemoteType, Func<RemT,LocalCollT> toLocalType,
                params Validate<LocalCollT>[] validators) {

            return new ClassFieldRemoteMutator<LocalCollT,RemT,ContT>(
                getRemoteField,
                toRemoteType,
                toLocalType,
                x => _caller.SaveField(GetFieldName(getRemoteField), x),
                x => {
                    validators.ForEach(y => x.AddValidatorAndRevalidate(y));
                    view.BindReadWriteAndInitialize(x);
                });
        }

        public ClassFieldRemoteMutator<DataT,DataT,ContT> Build<WidgetT,DataT>(
                Expression<Func<ContT,DataT>> getField, IReadWriteValueView<WidgetT,DataT> view,
                params Validate<DataT>[] validators) {

            return new ClassFieldRemoteMutator<DataT,DataT,ContT>(
                getField,
                x => x,
                x => x,
                x => _caller.SaveField(GetFieldName(getField), x),
                x => {
                    validators.ForEach(y => x.AddValidatorAndRevalidate(y));
                    view.BindReadWriteAndInitialize(x);
                },
                _postOperationConsumerOrNull);
        }
        
        public static RemoteFieldBuilder<ContT> For(
                Func<int,string,string, Task<ContT>> saveOper, 
                Func<int> idProvider,
                Action<ContT,string> postOperationConsumerOrNull = null)  {

            return new RemoteFieldBuilder<ContT>(
                new FixedIdUpdateServiceMethodCaller<ContT>(saveOper, idProvider),
                postOperationConsumerOrNull);
        }
    }
}
