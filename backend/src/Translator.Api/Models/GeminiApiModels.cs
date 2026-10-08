using System.Text.Json.Serialization;

namespace Translator.Api.Models;

public sealed record GeminiApiRequest
{
    [JsonPropertyName("contents")]
    public IReadOnlyList<GeminiContent> Contents { get; init; } = [];

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig? GenerationConfig { get; init; }
}

public sealed record GeminiContent
{
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Role { get; init; }

    [JsonPropertyName("parts")]
    public IReadOnlyList<GeminiPart> Parts { get; init; } = [];
}

public sealed record GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}

public sealed record GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; init; } = 0.2;

    [JsonPropertyName("responseMimeType")]
    public string ResponseMimeType { get; init; } = "application/json";
}

public sealed record GeminiApiResponse
{
    [JsonPropertyName("candidates")]
    public IReadOnlyList<GeminiCandidate>? Candidates { get; init; }

    [JsonPropertyName("error")]
    public GeminiError? Error { get; init; }
}

public sealed record GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; init; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; init; }
}

public sealed record GeminiError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
