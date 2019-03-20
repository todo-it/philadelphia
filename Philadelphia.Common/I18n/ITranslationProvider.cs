namespace Philadelphia.Common {
    public interface ITranslationProvider {
        bool IsDefault { get; }
        string CultureName { get; }
        TranslationItem[] Items { get; }
    }
}
