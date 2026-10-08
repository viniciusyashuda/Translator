namespace Translator.Api.Models;

public sealed record LanguageInfo(string Code, string Name, string NativeName, string Flag);

public sealed record ExpressionPreset(string Id, string SourceLanguage, string TargetLanguage, string Text, string Category);

public static class SupportedLanguages
{
    public static readonly IReadOnlyList<LanguageInfo> Languages =
    [
        new("auto", "Auto Detect", "Detectar idioma", "🌐"),
        new("en", "English", "English", "🇺🇸"),
        new("pt", "Portuguese", "Português", "🇧🇷"),
        new("es", "Spanish", "Español", "🇪🇸"),
        new("fr", "French", "Français", "🇫🇷"),
        new("de", "German", "Deutsch", "🇩🇪"),
        new("it", "Italian", "Italiano", "🇮🇹"),
        new("ja", "Japanese", "日本語", "🇯🇵"),
        new("zh", "Chinese", "中文", "🇨🇳")
    ];

    public static readonly IReadOnlyList<string> AllowedTones =
    [
        "Natural",
        "Casual",
        "Formal",
        "Business",
        "Colloquial",
        "Academic"
    ];

    public static readonly IReadOnlyList<ExpressionPreset> Presets =
    [
        new("preset-1", "pt", "en", "O projeto deu com os burros n'água no final do sprint.", "Idiom / Slang"),
        new("preset-2", "pt", "en", "Depois daquela reunião difícil, decidi chutar o balde e tirar férias.", "Idiom / Slang"),
        new("preset-3", "en", "pt", "Break a leg tonight! I know you've been burning the midnight oil for this demo.", "Idiom / Slang"),
        new("preset-4", "en", "pt", "We need to bite the bullet and cut our losses before it costs an arm and a leg.", "Business / Nuance"),
        new("preset-5", "pt", "en", "Ele estava pisando em ovos ao falar com o cliente sobre o atraso.", "Metaphor / Nuance"),
        new("preset-6", "en", "pt", "She hit the nail on the head when describing the root cause of the bug.", "Idiom / Slang")
    ];

    public static bool IsSupportedTargetLanguage(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Languages.Any(lang => lang.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsSupportedSourceLanguage(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return Languages.Any(lang => lang.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValidTone(string tone)
    {
        if (string.IsNullOrWhiteSpace(tone))
        {
            return false;
        }

        return AllowedTones.Any(t => t.Equals(tone, StringComparison.OrdinalIgnoreCase));
    }
}
