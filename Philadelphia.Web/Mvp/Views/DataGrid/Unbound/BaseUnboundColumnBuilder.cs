using System;

namespace Philadelphia.Web {
    public class BaseUnboundColumnBuilder<RecordT>  where RecordT : new() {
        public string Label { get; }
        public TextType TextVersionMode { get; }

        public BaseUnboundColumnBuilder(string label, TextType textVersionMode = TextType.TreatAsText) {
            Label = label;
            TextVersionMode = textVersionMode;
        }

        public ValueContainingUnboundColumnBuilder<RecordT,string> WithValue(Func<RecordT,string> valueProvider) {
            return new ValueContainingUnboundColumnBuilder<RecordT,string>(this, valueProvider, x => x, (val,exp) => exp.Export(val));
        }

        public ValueContainingUnboundColumnBuilder<RecordT,DataT> WithValue<DataT>(
            Func<RecordT,DataT> valueProvider, 
            Func<DataT,string> textValueProvider,
            Action<DataT,ICellValueExporter> valueExporter) {
                
            return new ValueContainingUnboundColumnBuilder<RecordT,DataT>(this, valueProvider, textValueProvider, valueExporter);
        }

        /// <summary>
        /// for complicated DataT that cannot be reasonably represented in ICellExport and just-put-text-value should be used
        /// </summary>
        public ValueContainingUnboundColumnBuilder<RecordT,DataT> WithValueAsText<DataT>(
            Func<RecordT,DataT> valueProvider, 
            Func<DataT,string> textValueProvider) {
                
            return new ValueContainingUnboundColumnBuilder<RecordT,DataT>(this, valueProvider, textValueProvider, 
                (val,exp) => exp.Export(textValueProvider(val)));
        }
    }
}
