namespace Translator.Api.Models;

public sealed record TranslationResponse
{
    public string OriginalText { get; init; } = string.Empty;
    public string DetectedSourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public string TranslatedText { get; init; } = string.Empty;
    public string Tone { get; init; } = string.Empty;
    public string SummaryExplanation { get; init; } = string.Empty;
    public IReadOnlyList<IdiomNuanceItem> IdiomsAndNuances { get; init; } = [];
    public IReadOnlyList<TranslationChoiceItem> TranslationChoices { get; init; } = [];
    public IReadOnlyList<AlternativePhrasingItem> AlternativePhrasings { get; init; } = [];
    public bool IsCached { get; init; }
    public long ExecutionTimeMs { get; init; }
}

public sealed record IdiomNuanceItem
{
    public string Expression { get; init; } = string.Empty;
    public string LiteralMeaning { get; init; } = string.Empty;
    public string FigurativeMeaning { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public string Formality { get; init; } = string.Empty;
}

public sealed record TranslationChoiceItem
{
    public string OriginalTerm { get; init; } = string.Empty;
    public string SelectedTerm { get; init; } = string.Empty;
    public string AlternativeTerm { get; init; } = string.Empty;
    public string Rationale { get; init; } = string.Empty;
}

public sealed record AlternativePhrasingItem
{
    public string Tone { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
}
