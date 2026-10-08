namespace Translator.Api.Options;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.8-flash";
    public string EndpointUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableCache { get; set; } = true;
    public int CacheDurationMinutes { get; set; } = 60;
}
