namespace Philadelphia.Web {
    // REVIEW: why not put this as nested type inside ConfirmOperationForm<T>?

    /// <summary>
    /// it is not defined within type as otherwise it would be generic and thus have needlesly looong signature
    /// </summary>
    public enum ConfirmOperationFormOutcome {
        Success,
        FailureOrCanceled
    }
}
