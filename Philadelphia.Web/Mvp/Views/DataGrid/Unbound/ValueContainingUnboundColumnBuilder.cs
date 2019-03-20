using System;
using Bridge.Html5;

namespace Philadelphia.Web {
    public class ValueContainingUnboundColumnBuilder<RecordT,DataT> where RecordT : new() {
        public BaseUnboundColumnBuilder<RecordT> BaseUnbound { get; }
        public Func<RecordT, DataT> ValueProvider { get; }
        public Func<DataT, string> TextValueProvider { get; }
        public Action<DataT,ICellValueExporter> ValueExporter { get; }

        public ValueContainingUnboundColumnBuilder(
                BaseUnboundColumnBuilder<RecordT> baseUnbound,
                Func<RecordT,DataT> valueProvider, 
                Func<DataT,string> textValueProvider,
                Action<DataT,ICellValueExporter> valueExporter) {

            BaseUnbound = baseUnbound;
            ValueProvider = valueProvider;
            TextValueProvider = textValueProvider;
            ValueExporter = valueExporter;
        }

        public TransformableUnboundColumnBuilder<RecordT,DataT> Transformable(
                Func<ITransformationMediator,Tuple<HTMLElement,DataGridColumnControllerResult<DataT>>> transforms) {

            return new TransformableUnboundColumnBuilder<RecordT,DataT>(this, transforms);
        }
        
        public TransformableUnboundColumnBuilder<RecordT,DataT> TransformableAsText() {
            return new TransformableUnboundColumnBuilder<RecordT,DataT>(this, 
                x => DataGridColumnController.ForTypeTreatedAsString(TextValueProvider, x));
        }

        public TransformableUnboundColumnBuilder<RecordT,DataT> NonTransformable() {
            return new TransformableUnboundColumnBuilder<RecordT,DataT>(this, x => 
                new Tuple<HTMLElement,DataGridColumnControllerResult<DataT>>(
                    new HTMLElement(),
                    new DataGridColumnControllerResult<DataT> {
                        GroupingImpl = null,
                        AggregationImpl = null,
                        SortingImpl = null,
                        FilterImpl = null
                    }));
        }

        public IDataGridColumn<RecordT> Build() { 
            return NonTransformable().DoesntObserve().Build();
        }
    }
}
