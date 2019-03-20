using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class BuildableUnboundColumn<RecordT,DataT> where RecordT : new() {
        public PersistedRemotelyUnboundColumn<RecordT, DataT> Persistable { get; }
        Func<int, DataT, Task<RecordT>> SaveOperation { get; }
        public Func<RecordT,int> ExtractId { get; }

        public BuildableUnboundColumn(
                PersistedRemotelyUnboundColumn<RecordT,DataT> persistable,
                Func<int, DataT, Task<RecordT>> saveOperation,
                Func<RecordT,int> extractId) {

            Persistable = persistable;
            SaveOperation = saveOperation;
            ExtractId = extractId;
        }
        
        public IDataGridColumn<RecordT> Build() {
            var lbl = Persistable.Editable.Transformable.ValueContaining.BaseUnbound.Label;
            var textVersionMode = Persistable.Editable.Transformable.ValueContaining.BaseUnbound.TextVersionMode;
            var valPrvd = Persistable.Editable.Transformable.ValueContaining.ValueProvider;
            var txtPrvd = Persistable.Editable.Transformable.ValueContaining.TextValueProvider;
            var valueExport = Persistable.Editable.Transformable.ValueContaining.ValueExporter;
            var transformation = Persistable.Editable.Transformable.Transformation;
            
            ItemPropertyChangedListener<RecordT> observableOrNull = null;

            if (Persistable.Editable.AsObservable != null) {
                observableOrNull = new ItemPropertyChangedListener<RecordT>(
                    Persistable.Editable.AsObservable,
                    Persistable.Editable.PropertyName);
            }
            
            return new DataGridColumn<RecordT,DataT>(
                new LocalValue<string>(lbl),
                textVersionMode,
                valPrvd,
                txtPrvd,
                valueExport,
                transformation,
                observableOrNull,
                Persistable.BuildEditor,
                Persistable.SetValue,
                SaveOperation,
                ExtractId,
                false,
                null,
                null);
        }
    }
}
