namespace Philadelphia.Web {
    public enum TextType {
        /// <summary>secure TextContent setter - looses html and whitespace</summary>
        TreatAsText,

        /// <summary>insecure InnerHTML setter</summary>
        TreatAsHtml,

        /// <summary>whitespace: pre</summary>
        TreatAsPreformatted 
    }
}
