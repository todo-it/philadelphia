using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TabContentAccumulator : ITabContent {
        private readonly Action<object, ISet<string>> _updateObserver;
        private readonly List<HTMLElement> _widgets = new List<HTMLElement>();

        public IEnumerable<HTMLElement> Widgets => _widgets;
            
        public TabContentAccumulator(Action<object,ISet<string>> updateObserver) {
            _updateObserver = updateObserver;
        }

        public void Add<T>(IReadOnlyValueView<HTMLElement, T> itm) {
            var err = itm.Errors;

            Logger.Debug(GetType(), "Add for itm {0} has initial result={1}", itm.Widget.Id, err.Count);
            itm.ErrorsChanged += (sender, errors) => _updateObserver(sender, errors);
            _updateObserver(itm, err); //initialization
            _widgets.Add(itm.Widget);
        }
        
        public void Add<T>(IReadOnlyValueView<HTMLElement, T> observed, HTMLElement view) {
            var err = observed.Errors;

            Logger.Debug(GetType(), "Add for itm {0} has initial result={1}", observed.Widget.Id, err.Count);
            observed.ErrorsChanged += (sender, errors) => _updateObserver(sender, errors);
            _updateObserver(observed, err); //initialization
            _widgets.Add(view);
        }

        public void Add(HTMLElement itm) {
            _widgets.Add(itm);
        }
    }
}
