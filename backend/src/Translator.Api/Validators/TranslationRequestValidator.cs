using Translator.Api.Models;

namespace Translator.Api.Validators;

public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static ValidationResult Success() => new(true, []);
    public static ValidationResult Failure(params string[] errors) => new(false, errors);
    public static ValidationResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}

public sealed class TranslationRequestValidator
{
    private const int MinTextLength = 1;
    private const int MaxTextLength = 2000;

    public ValidationResult Validate(TranslationRequest? request)
    {
        if (request is null)
        {
            return ValidationResult.Failure("Request body cannot be null.");
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            errors.Add("Text to translate cannot be empty.");
        }
        else
        {
            var trimmedLength = request.Text.Trim().Length;
            if (trimmedLength < MinTextLength || trimmedLength > MaxTextLength)
            {
                errors.Add($"Text length must be between {MinTextLength} and {MaxTextLength} characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            errors.Add("Target language is required.");
        }
        else if (!SupportedLanguages.IsSupportedTargetLanguage(request.TargetLanguage))
        {
            errors.Add($"Target language '{request.TargetLanguage}' is not supported.");
        }

        if (!string.IsNullOrWhiteSpace(request.SourceLanguage))
        {
            if (!SupportedLanguages.IsSupportedSourceLanguage(request.SourceLanguage))
            {
                errors.Add($"Source language '{request.SourceLanguage}' is not supported.");
            }
            else if (!request.SourceLanguage.Equals("auto", StringComparison.OrdinalIgnoreCase) &&
                     request.SourceLanguage.Equals(request.TargetLanguage, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Source language and target language cannot be the same.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Tone) && !SupportedLanguages.IsValidTone(request.Tone))
        {
            errors.Add($"Tone '{request.Tone}' is invalid. Allowed values: {string.Join(", ", SupportedLanguages.AllowedTones)}.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}
