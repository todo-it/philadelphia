using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ConditionalView<T> : IView<HTMLElement> {
        private readonly HTMLElement _container;
        private readonly IDictionary<T,Action<HTMLElement>> _options = new Dictionary<T, Action<HTMLElement>>();
        private T _value;

        public HTMLElement Widget => _container;
        public T Value {
            get => _value;
            set {
                if (!_options.TryGetValue(value, out var adder)) {
                    throw new Exception($"doesn't have view for item {value}");
                }

                _value = value;
                _container.RemoveAllChildren();
                adder(_container);
            }
        }

        public ConditionalView(T initialValue, Action<ConditionalView<T>> initializer) {
            _container = new HTMLDivElement();
            _container.ClassName = GetType().FullNameWithoutGenerics();

            initializer(this);

            Value = initialValue;
        }

        public void Register(T forValue, Action<HTMLElement> adder)  {
            _options.Add(forValue, adder);
        }

        public void Register(T forValue, HTMLElement showView)  {
            _options.Add(forValue, cnt => cnt.AppendChild(showView));
        }

        public void Register<ViewT>(T forValue, IView<ViewT> showView) where ViewT : HTMLElement {
            _options.Add(forValue, cnt => cnt.AppendChild(showView.Widget));
        }

        public static implicit operator RenderElem<HTMLElement>(ConditionalView<T> inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
