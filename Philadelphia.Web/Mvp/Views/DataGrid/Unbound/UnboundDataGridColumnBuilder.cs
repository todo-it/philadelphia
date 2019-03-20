
namespace Philadelphia.Web {
    public class UnboundDataGridColumnBuilder {
        public static BaseUnboundColumnBuilder<RecordT> For<RecordT>(
                string label, TextType textMode = TextType.TreatAsText) where RecordT : new() {

            return new BaseUnboundColumnBuilder<RecordT>(label, textMode);
        }
    }
}
