using System;

namespace Philadelphia.Common {
    public class ExternalEventsHandlers {
        /// <summary> supported on the web and console. Can be null.</summary>
        public Action OnUserCancel { get; }

        /// <summary> not supported on the web </summary>
        public (string, Action) OnFocusCycled { get; }
        public string Name { get; }

        public ExternalEventsHandlers(Action onUserCancel, (string, Action) onFocusCycled, string name) {
            OnUserCancel = onUserCancel;
            OnFocusCycled = onFocusCycled;
            Name = name;
        }

        public override string ToString() => Name;

        private static void Noop() { }
        public static readonly ExternalEventsHandlers Ignore = new ExternalEventsHandlers(null, ("", Noop), "Ignore");
        public static readonly ExternalEventsHandlers Default = Ignore;
        public static ExternalEventsHandlers Create(Action onUserCancel) => new ExternalEventsHandlers(onUserCancel, ("", Noop), "Custom");
    }
}
