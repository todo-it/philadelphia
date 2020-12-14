using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PersistedRemotelyUnboundColumn<RecordT,DataT> where RecordT : new() {
        public EditableUnboundColumn<RecordT,DataT> Editable {get; }
        public Func<IReadWriteValue<DataT>,IView<HTMLElement>> BuildEditor { get; }
        
        public IEnumerable<Validate<DataT>> Validators { get; }
        public Action<RecordT, DataT> SetValue { get; }
        
        public PersistedRemotelyUnboundColumn(
                EditableUnboundColumn<RecordT,DataT> editable,
                Func<IReadWriteValueView<HTMLElement,DataT>> buildEditor,
                Action<RecordT, DataT> setValue,
                params Validate<DataT>[] validators) {

            Editable = editable;
            SetValue = setValue;
            Validators = validators;

            BuildEditor = m => {
                var result = buildEditor();
                result.BindReadWriteAndInitialize(m);
                return result;
            };
        }

        public PersistedRemotelyUnboundColumn(
                EditableUnboundColumn<RecordT,DataT> editable,
                IConvertingEditor<DataT> editorBuilder,
                Action<RecordT, DataT> setValue,
                params Validate<DataT>[] validators) {

            Editable = editable;
            SetValue = setValue;
            Validators = validators;
            
            BuildEditor = editorBuilder.Build;
        }

        public BuildableUnboundColumn<RecordT,DataT> NonPersisted() {
            return new BuildableUnboundColumn<RecordT,DataT>(this, null, null);
        }
        
        public BuildableUnboundColumn<RecordT,DataT> Persisted(
                Func<int, DataT, Task<RecordT>> saveOperation, 
                Func<RecordT,int> extractId) {

            return new BuildableUnboundColumn<RecordT,DataT>(this, saveOperation, extractId);
        }

        public IDataGridColumn<RecordT> Build() {
            return NonPersisted().Build();
        }
    }
}
