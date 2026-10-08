namespace Translator.Api.Models;

public sealed record TranslationRequest
{
    public string Text { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = "auto";
    public string TargetLanguage { get; init; } = "en";
    public string Tone { get; init; } = "Natural";
}
