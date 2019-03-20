using System;

namespace Philadelphia.Common {
    /// <summary>
    /// Not used by any framework machinery, intended to help forming well defined workflows
    /// </summary>
    public interface IFlow<WidgetT> {
        void Run(IFormRenderer<WidgetT> renderer, Action atExit);
    }

    public static class FlowExtensionMethods {
        public static void Run<T>(this IFlow<T> flow, IFormRenderer<T> renderer) => 
            flow.Run(renderer, LambdaUtil.DoNothingAction);
    }
}
