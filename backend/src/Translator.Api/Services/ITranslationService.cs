using Translator.Api.Models;

namespace Translator.Api.Services;

public interface ITranslationService
{
    Task<TranslationResponse> TranslateAndExplainAsync(TranslationRequest request, CancellationToken cancellationToken = default);
    IReadOnlyList<LanguageInfo> GetLanguages();
    IReadOnlyList<ExpressionPreset> GetPresets();
}
