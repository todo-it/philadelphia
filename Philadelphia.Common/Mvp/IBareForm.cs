namespace Philadelphia.Common {
    /// <summary>
    /// this is a minimum required interface for forms. Works 'perfectly' for simple forms.
    /// It is likely that you want to use IForm instead for streamlined form ending handling.
    /// </summary>
    /// <typeparam name="WidgetT"></typeparam>
    public interface IBareForm<WidgetT> {
        string Title { get; }
        IFormView<WidgetT> View { get; }
        
        /// <summary>
        /// setups actions for optional externally raised events such as "user wants to close dialog"
        /// </summary>
        ExternalEventsHandlers ExternalEventsHandlers { get; }
    }
}
