using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface IConvertingEditor<DataT> {
        IView<HTMLElement> Build(IReadWriteValue<DataT> inp);
    }
}
