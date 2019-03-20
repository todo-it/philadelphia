using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TransformableUnboundColumnBuilder<RecordT,DataT> where RecordT : new() {
        public Func<ITransformationMediator,Tuple<HTMLElement,DataGridColumnControllerResult<DataT>>> Transformation { get; }
        public ValueContainingUnboundColumnBuilder<RecordT,DataT> ValueContaining { get; }

        public TransformableUnboundColumnBuilder(
                ValueContainingUnboundColumnBuilder<RecordT,DataT> valueContaining,
                Func<ITransformationMediator,Tuple<HTMLElement,DataGridColumnControllerResult<DataT>>> transformation) {

            Transformation = transformation;
            ValueContaining = valueContaining;
        }
        
        public EditableUnboundColumn<RecordT,DataT> Observes(
            Func<RecordT,ISubscribeable> asObservable, Func<RecordT,string> propertyName) {
            return new EditableUnboundColumn<RecordT,DataT>(this, asObservable, propertyName);
        }

        public EditableUnboundColumn<RecordT,DataT> DoesntObserve() {
            return new EditableUnboundColumn<RecordT,DataT>(this, null, null);
        }

        public IDataGridColumn<RecordT> Build() {
            return DoesntObserve().NonEditable().NonPersisted().Build();
        }
    }
}
