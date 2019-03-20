namespace Philadelphia.Common {
    public interface IFormView<WidgetT> {
        /// <summary>returns one or more instances of IView or UI specific string literals</summary>
        RenderElem<WidgetT>[] Render(WidgetT parentContainer);
        IView<WidgetT>[] Actions { get; }
    }
}
