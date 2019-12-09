using System;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class RemoteValueBuilder {
        public static RemoteValue<LocalT,RemOperResT> Build<WidgetT,LocalT,RemOperResT,ViewT>(
                LocalT initialValue, 
                Func<LocalT,Task<RemOperResT>> saveOper, 
                IReadWriteValueView<WidgetT,ViewT> view,
                Func<RemOperResT,LocalT> remToLocal,
                Func<LocalT, ViewT> convertFromDomain,
                Func<ViewT, LocalT> convertToDomain,
                params Validate<LocalT>[] validators) {

            return new RemoteValue<LocalT,RemOperResT>(
                initialValue, 
                saveOper,
                remToLocal,
                initialization: x => {
                    validators.ForEach(x.AddValidatorAndRevalidate);
                    view.BindReadWriteAndInitialize(x, convertFromDomain, convertToDomain); });
        }

        public static RemoteValue<DataT,RemOperResT> Build<WidgetT,DataT,RemOperResT>(
            DataT initialValue, 
            Func<DataT,Task<RemOperResT>> saveOper, 
            IReadWriteValueView<WidgetT,DataT> view,
            Func<RemOperResT,DataT> remToLocal,
            params Validate<DataT>[] validators) {

            return new RemoteValue<DataT,RemOperResT>(
                initialValue, 
                saveOper,
                remToLocal,
                initialization: x => {
                    validators.ForEach(x.AddValidatorAndRevalidate);
                    view.BindReadWriteAndInitialize(x); });
        }

        public static RemoteValue<DateTime?,RemOperResT> BuildDateTimePicker<RemOperResT>(
                DateTime? initialValue, 
                Func<DateTime?,Task<RemOperResT>> saveOper, 
                DateTimePickerView view, 
                Func<RemOperResT,DateTime?> remToLocal,
                params Validate<DateTime?>[] validators) {
            
            return new RemoteValue<DateTime?,RemOperResT>(
                initialValue, 
                saveOper,
                remToLocal,
                initialization: x => {
                    validators.ForEach(x.AddValidatorAndRevalidate);
                    view.BindReadWriteAndInitialize(x); });
        }
    }
}
