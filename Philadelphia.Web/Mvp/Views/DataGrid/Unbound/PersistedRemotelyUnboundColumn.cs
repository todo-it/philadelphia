using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PersistedRemotelyUnboundColumn<RecordT,DataT> where RecordT : new() {
        public EditableUnboundColumn<RecordT,DataT> Editable {get; }
        public Func<IReadWriteValueView<HTMLElement,DataT>> BuildEditor { get; }
        public Func<IEnumerable<Validate<DataT>>> Validators { get; }
        public Action<RecordT, DataT> SetValue { get; }

        public PersistedRemotelyUnboundColumn(
                EditableUnboundColumn<RecordT,DataT> editable,
                Func<IReadWriteValueView<HTMLElement,DataT>> buildEditor,
                Action<RecordT, DataT> setValue,
                Func<IEnumerable<Validate<DataT>>> validators) {

            Editable = editable;
            BuildEditor = buildEditor;
            SetValue = setValue;
            Validators = validators;
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
