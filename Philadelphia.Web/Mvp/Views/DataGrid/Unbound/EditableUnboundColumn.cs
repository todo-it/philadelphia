using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class EditableUnboundColumn<RecordT,DataT> where RecordT : new() {
        public TransformableUnboundColumnBuilder<RecordT, DataT> Transformable { get; }
        public Func<RecordT, ISubscribeable> AsObservable { get; }
        public Func<RecordT, string> PropertyName { get; }

        public EditableUnboundColumn(
                TransformableUnboundColumnBuilder<RecordT,DataT> transformable,
                Func<RecordT,ISubscribeable> asObservable, 
                Func<RecordT,string> propertyName) {

            Transformable = transformable;
            AsObservable = asObservable;
            PropertyName = propertyName;
        }

        public PersistedRemotelyUnboundColumn<RecordT,DataT> NonEditable() {
            return new PersistedRemotelyUnboundColumn<RecordT,DataT>(this, null, null);
        }
        
        public PersistedRemotelyUnboundColumn<RecordT,DataT> Editable(
            Func<IReadWriteValueView<HTMLElement,DataT>> buildEditor,
            Action<RecordT,DataT> setValue) {

            return new PersistedRemotelyUnboundColumn<RecordT,DataT>(this, buildEditor, setValue);
        }
        
        public IDataGridColumn<RecordT> Build() {
            return NonEditable().NonPersisted().Build();
        }
    }
}
