using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ConvertingEditor<DataT,EditorT> : IConvertingEditor<DataT> {
        private readonly Func<IReadWriteValueView<HTMLElement, EditorT>> _buildEditor;
        private readonly Func<EditorT, DataT> _convertToDomain;
        private readonly Func<DataT, EditorT> _convertFromDomain;

        public ConvertingEditor(
            Func<IReadWriteValueView<HTMLElement,EditorT>> buildEditor,
            Func<DataT,EditorT> convertFromDomain,
            Func<EditorT,DataT> convertToDomain) {
            
            _buildEditor = buildEditor;
            _convertToDomain = convertToDomain;
            _convertFromDomain = convertFromDomain;
        }

        public IView<HTMLElement> Build(IReadWriteValue<DataT> model) {
            var v = _buildEditor();
            v.BindReadWriteAndInitialize(model, _convertFromDomain, _convertToDomain);
            return v;
        }
    }
}
