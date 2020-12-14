using System;
using System.Collections.Generic;
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
            Func<IReadWriteValueView<HTMLElement, DataT>> buildEditor = null; //to help choose overload
            return new PersistedRemotelyUnboundColumn<RecordT,DataT>(this, buildEditor, null);
        }
        
        public PersistedRemotelyUnboundColumn<RecordT,DataT> Editable(
                Func<IReadWriteValueView<HTMLElement,DataT>> buildEditor,
                Action<RecordT,DataT> setValue,
                params Validate<DataT>[] validators) {

            return new PersistedRemotelyUnboundColumn<RecordT,DataT>(
                this, buildEditor, setValue,
                validators);
        }

        public PersistedRemotelyUnboundColumn<RecordT,DataT> Editable(
            IConvertingEditor<DataT> editorBuilder,
            Action<RecordT,DataT> setValue,
            params Validate<DataT>[] validators) {

            return new PersistedRemotelyUnboundColumn<RecordT,DataT>(
                this, editorBuilder, setValue,
                validators);
        }

        public IDataGridColumn<RecordT> Build() {
            return NonEditable().NonPersisted().Build();
        }
    }
}
