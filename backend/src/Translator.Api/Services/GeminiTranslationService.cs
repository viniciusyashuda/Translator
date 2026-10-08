using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Translator.Api.Models;
using Translator.Api.Options;

namespace Translator.Api.Services;

public sealed partial class GeminiTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeminiTranslationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GeminiTranslationService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        IMemoryCache cache,
        ILogger<GeminiTranslationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public IReadOnlyList<LanguageInfo> GetLanguages() => SupportedLanguages.Languages;

    public IReadOnlyList<ExpressionPreset> GetPresets() => SupportedLanguages.Presets;

    public async Task<TranslationResponse> TranslateAndExplainAsync(TranslationRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var sanitizedText = SanitizeInput(request.Text);
        var cacheKey = $"trans:{request.SourceLanguage.ToLowerInvariant()}:{request.TargetLanguage.ToLowerInvariant()}:{request.Tone.ToLowerInvariant()}:{sanitizedText}";

        if (_options.EnableCache && _cache.TryGetValue(cacheKey, out TranslationResponse? cachedResponse) && cachedResponse is not null)
        {
            stopwatch.Stop();
            return cachedResponse with
            {
                IsCached = true,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var isApiKeyConfigured = !string.IsNullOrWhiteSpace(_options.ApiKey) &&
                                 !_options.ApiKey.Contains("YOUR_GEMINI_API_KEY", StringComparison.OrdinalIgnoreCase) &&
                                 !_options.ApiKey.Equals("DEMO", StringComparison.OrdinalIgnoreCase);

        TranslationResponse response;

        if (!isApiKeyConfigured)
        {
            response = BuildDemoResponse(request, sanitizedText);
        }
        else
        {
            try
            {
                response = await CallGeminiApiAsync(request, sanitizedText, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Google Gemini API: {Message}", ex.Message);
                throw new InvalidOperationException($"Google Gemini API error: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        var finalResponse = response with
        {
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            IsCached = false
        };

        if (_options.EnableCache)
        {
            _cache.Set(cacheKey, finalResponse, TimeSpan.FromMinutes(_options.CacheDurationMinutes));
        }

        return finalResponse;
    }

    private string? _resolvedModel;

    private async Task<string> ResolveSupportedModelAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_resolvedModel))
        {
            return _resolvedModel;
        }

        try
        {
            var listModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={_options.ApiKey}";
            using var response = await _httpClient.GetAsync(listModelsUrl, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("models", out var modelsArray))
                {
                    var supportedModels = new List<string>();
                    foreach (var modelElement in modelsArray.EnumerateArray())
                    {
                        var supportsGenerateContent = false;
                        if (modelElement.TryGetProperty("supportedGenerationMethods", out var methods))
                        {
                            foreach (var method in methods.EnumerateArray())
                            {
                                if (method.GetString() == "generateContent")
                                {
                                    supportsGenerateContent = true;
                                    break;
                                }
                            }
                        }

                        if (supportsGenerateContent && modelElement.TryGetProperty("name", out var nameProp))
                        {
                            var rawName = nameProp.GetString() ?? string.Empty;
                            var modelId = rawName.StartsWith("models/") ? rawName[7..] : rawName;
                            supportedModels.Add(modelId);
                        }
                    }

                    var selected = supportedModels.FirstOrDefault(m => m.Contains("3.8", StringComparison.OrdinalIgnoreCase) && m.Contains("flash", StringComparison.OrdinalIgnoreCase))
                                   ?? supportedModels.FirstOrDefault(m => m.Contains("3.", StringComparison.OrdinalIgnoreCase) && m.Contains("flash", StringComparison.OrdinalIgnoreCase))
                                   ?? supportedModels.FirstOrDefault(m => m.Contains("flash", StringComparison.OrdinalIgnoreCase) && !m.Contains("2.5", StringComparison.OrdinalIgnoreCase) && !m.Contains("1.5", StringComparison.OrdinalIgnoreCase) && !m.Contains("8b", StringComparison.OrdinalIgnoreCase) && !m.Contains("thinking", StringComparison.OrdinalIgnoreCase))
                                   ?? supportedModels.FirstOrDefault(m => m.Contains("flash", StringComparison.OrdinalIgnoreCase))
                                   ?? supportedModels.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(selected))
                    {
                        _logger.LogInformation("Automatically selected supported Gemini model: {Model}", selected);
                        _resolvedModel = selected;
                        return selected;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dynamically query ListModels from Gemini API.");
        }

        return !string.IsNullOrWhiteSpace(_options.Model) ? _options.Model : "gemini-3.8-flash";
    }

    private async Task<TranslationResponse> CallGeminiApiAsync(TranslationRequest request, string sanitizedText, CancellationToken cancellationToken)
    {
        var modelToUse = _resolvedModel ?? (!string.IsNullOrWhiteSpace(_options.Model) ? _options.Model : "gemini-3.8-flash");
        var prompt = BuildPrompt(request, sanitizedText);
        var endpoint = $"{_options.EndpointUrl.TrimEnd('/')}/{modelToUse}:generateContent?key={_options.ApiKey}";

        var payload = new GeminiApiRequest
        {
            Contents =
            [
                new GeminiContent
                {
                    Parts = [new GeminiPart { Text = prompt }]
                }
            ],
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.2,
                ResponseMimeType = "application/json"
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(payload)
        };

        using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var rawResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Model '{Model}' returned 404 NotFound. Discovering available models via ListModels API...", modelToUse);
            _resolvedModel = null;
            var discoveredModel = await ResolveSupportedModelAsync(cancellationToken);
            if (!string.Equals(discoveredModel, modelToUse, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Retrying translation with discovered model: '{DiscoveredModel}'.", discoveredModel);
                var retryEndpoint = $"{_options.EndpointUrl.TrimEnd('/')}/{discoveredModel}:generateContent?key={_options.ApiKey}";
                using var retryRequest = new HttpRequestMessage(HttpMethod.Post, retryEndpoint)
                {
                    Content = JsonContent.Create(payload)
                };
                using var retryResponse = await _httpClient.SendAsync(retryRequest, cancellationToken);
                rawResponse = await retryResponse.Content.ReadAsStringAsync(cancellationToken);
                if (!retryResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Retry with discovered model '{Model}' failed with status {StatusCode}: {Response}", discoveredModel, retryResponse.StatusCode, rawResponse);
                    throw new HttpRequestException($"Gemini API request failed with status code {retryResponse.StatusCode}: {rawResponse}");
                }
            }
            else
            {
                throw new HttpRequestException($"Gemini API request failed with status code {httpResponse.StatusCode}: {rawResponse}");
            }
        }
        else if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error status {StatusCode}: {Response}", httpResponse.StatusCode, rawResponse);
            throw new HttpRequestException($"Gemini API request failed with status code {httpResponse.StatusCode}: {rawResponse}");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(rawResponse, JsonOptions);
        var candidateText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(candidateText))
        {
            throw new InvalidOperationException("Gemini API returned an empty response candidate.");
        }

        var cleanJson = ExtractCleanJson(candidateText);
        var parsed = JsonSerializer.Deserialize<GeminiParsedTranslation>(cleanJson, JsonOptions);
        if (parsed is null)
        {
            throw new InvalidOperationException("Failed to deserialize Gemini output JSON.");
        }

        return new TranslationResponse
        {
            OriginalText = sanitizedText,
            DetectedSourceLanguage = parsed.DetectedSourceLanguage ?? request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            TranslatedText = parsed.TranslatedText ?? string.Empty,
            Tone = request.Tone,
            SummaryExplanation = parsed.SummaryExplanation ?? string.Empty,
            IdiomsAndNuances = parsed.IdiomsAndNuances ?? [],
            TranslationChoices = parsed.TranslationChoices ?? [],
            AlternativePhrasings = parsed.AlternativePhrasings ?? []
        };
    }

    private static string BuildPrompt(TranslationRequest request, string sanitizedText)
    {
        return $$"""
            You are an advanced bilingual translator and cultural linguistics expert.
            Translate the text into target language code '{{request.TargetLanguage}}', applying the desired tone '{{request.Tone}}'.
            Source language preference: '{{request.SourceLanguage}}'. If source is 'auto', accurately detect the source language.

            Explain idioms, cultural metaphors, colloquialisms, and why specific translations were chosen over direct literal ones.
            Provide alternative phrasings suitable for different communication contexts.

            Strict requirement: You MUST output a JSON object adhering to this exact format:
            {
              "detectedSourceLanguage": "Language Name",
              "translatedText": "Translated text",
              "summaryExplanation": "Concise summary of the cultural adaptation and grammatical choices",
              "idiomsAndNuances": [
                {
                  "expression": "Extracted idiom or nuance",
                  "literalMeaning": "Literal word-for-word meaning",
                  "figurativeMeaning": "Real intended meaning",
                  "explanation": "Why this expression means what it means and how it works in context",
                  "formality": "Formality level (e.g. Slang, Informal, Formal, Neutral)"
                }
              ],
              "translationChoices": [
                {
                  "originalTerm": "Source fragment or keyword",
                  "selectedTerm": "Chosen translation in target language",
                  "alternativeTerm": "Literal or alternative translation that was rejected",
                  "rationale": "Why selectedTerm is better suited in this context"
                }
              ],
              "alternativePhrasings": [
                {
                  "tone": "Casual / Formal / Direct / Polite",
                  "translation": "Alternative sentence",
                  "context": "Context where this alternative is preferred"
                }
              ]
            }

            Input text:
            \"\"\"
            {{sanitizedText}}
            \"\"\"
            """;
    }

    private static TranslationResponse BuildDemoResponse(TranslationRequest request, string sanitizedText)
    {
        var lower = sanitizedText.ToLowerInvariant();

        if (lower.Contains("burros n'água") || lower.Contains("burros nágua"))
        {
            return new TranslationResponse
            {
                OriginalText = sanitizedText,
                DetectedSourceLanguage = "pt",
                TargetLanguage = request.TargetLanguage,
                TranslatedText = "The project fell flat on its face at the end of the sprint.",
                Tone = request.Tone,
                SummaryExplanation = "The Brazilian Portuguese idiom 'dar com os burros n'água' denotes complete failure or plans going awry, best matched by English idioms like 'fell flat on its face' or 'fell through'.",
                IdiomsAndNuances =
                [
                    new IdiomNuanceItem
                    {
                        Expression = "Dar com os burros n'água",
                        LiteralMeaning = "To hit the water with the donkeys",
                        FigurativeMeaning = "To fail completely; to have an endeavor backfire or lead to dead ends",
                        Explanation = "Originates from historical tropeiros (muleteers) in colonial Brazil whose pack donkeys drowned or got stuck when attempting to cross flooded rivers.",
                        Formality = "Colloquial / Idiomatic"
                    }
                ],
                TranslationChoices =
                [
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "deu com os burros n'água",
                        SelectedTerm = "fell flat on its face",
                        AlternativeTerm = "hit the water with donkeys",
                        Rationale = "A literal translation would be meaningless in English. 'Fell flat on its face' conveys the sudden, unceremonious failure of a project."
                    },
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "sprint",
                        SelectedTerm = "sprint",
                        AlternativeTerm = "corrida rápida",
                        Rationale = "In modern Agile software engineering, 'sprint' is universally retained as technical jargon."
                    }
                ],
                AlternativePhrasings =
                [
                    new AlternativePhrasingItem
                    {
                        Tone = "Formal / Business",
                        Translation = "The project failed to achieve its objectives at the end of the sprint.",
                        Context = "Ideal for executive stakeholder reports and formal post-mortem documentation."
                    },
                    new AlternativePhrasingItem
                    {
                        Tone = "Casual / Spoken",
                        Translation = "The project totally crashed and burned right at the end of the sprint.",
                        Context = "Suited for informal conversations between peer developers."
                    }
                ]
            };
        }

        if (lower.Contains("chutar o balde"))
        {
            return new TranslationResponse
            {
                OriginalText = sanitizedText,
                DetectedSourceLanguage = "pt",
                TargetLanguage = request.TargetLanguage,
                TranslatedText = "After that tough meeting, I decided to throw in the towel and take some time off.",
                Tone = request.Tone,
                SummaryExplanation = "The Portuguese idiom 'chutar o balde' means giving up patience or relinquishing control, which translates to 'throw in the towel' or 'say to hell with it' depending on formality.",
                IdiomsAndNuances =
                [
                    new IdiomNuanceItem
                    {
                        Expression = "Chutar o balde",
                        LiteralMeaning = "To kick the bucket",
                        FigurativeMeaning = "To lose patience, give up on obligations, or act impulsively out of exasperation",
                        Explanation = "False friend alert: in English 'kick the bucket' means to die. In Brazilian Portuguese, 'chutar o balde' means losing self-control or walking away from pressure.",
                        Formality = "Informal / Slang"
                    }
                ],
                TranslationChoices =
                [
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "chutar o balde",
                        SelectedTerm = "throw in the towel",
                        AlternativeTerm = "kick the bucket",
                        Rationale = "'Kick the bucket' is a dangerous false friend meaning death in English. 'Throw in the towel' accurately reflects yielding under intense pressure."
                    }
                ],
                AlternativePhrasings =
                [
                    new AlternativePhrasingItem
                    {
                        Tone = "Colloquial / Bold",
                        Translation = "After that rough meeting, I decided to say 'screw it' and go on vacation.",
                        Context = "Expresses raw emotion in informal peer chats."
                    },
                    new AlternativePhrasingItem
                    {
                        Tone = "Polite / Corporate",
                        Translation = "Following that demanding session, I decided to disconnect and take personal leave.",
                        Context = "Appropriate for HR or corporate correspondence."
                    }
                ]
            };
        }

        if (lower.Contains("break a leg"))
        {
            return new TranslationResponse
            {
                OriginalText = sanitizedText,
                DetectedSourceLanguage = "en",
                TargetLanguage = request.TargetLanguage,
                TranslatedText = "Boa sorte hoje à noite! Sei que você varou a noite trabalhando nessa demonstração.",
                Tone = request.Tone,
                SummaryExplanation = "The English theater idiom 'break a leg' wishing good luck and 'burn the midnight oil' working late hours are adapted naturally into Brazilian Portuguese.",
                IdiomsAndNuances =
                [
                    new IdiomNuanceItem
                    {
                        Expression = "Break a leg",
                        LiteralMeaning = "Quebrar uma perna",
                        FigurativeMeaning = "Desejo de boa sorte antes de uma apresentação pública",
                        Explanation = "Superstição teatral de que desejar boa sorte diretamente dá azar, portanto deseja-se o oposto.",
                        Formality = "Informal / Teatral"
                    },
                    new IdiomNuanceItem
                    {
                        Expression = "Burning the midnight oil",
                        LiteralMeaning = "Queimando o óleo da meia-noite",
                        FigurativeMeaning = "Trabalhar ou estudar até altas horas da madrugada",
                        Explanation = "Alusão histórica às lâmpadas de óleo usadas antes da iluminação elétrica para trabalhar à noite.",
                        Formality = "Idiomático"
                    }
                ],
                TranslationChoices =
                [
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "Break a leg",
                        SelectedTerm = "Boa sorte / Arrebenta",
                        AlternativeTerm = "Quebre uma perna",
                        Rationale = "A tradução literal soaria como uma ameaça física. 'Arrebenta' ou 'Boa sorte' preservam a intenção calorosa."
                    },
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "burning the midnight oil",
                        SelectedTerm = "varou a noite trabalhando",
                        AlternativeTerm = "queimando o óleo da meia-noite",
                        Rationale = "'Varar a noite' é a expressão equivalente natural e corrente em português brasileiro."
                    }
                ],
                AlternativePhrasings =
                [
                    new AlternativePhrasingItem
                    {
                        Tone = "Entusiasta / Coloquial",
                        Translation = "Arrebenta lá hoje à noite! Sei que você virou a noite se dedicando a essa demo.",
                        Context = "Perfeito para incentivar um colega próximo de equipe."
                    },
                    new AlternativePhrasingItem
                    {
                        Tone = "Formal",
                        Translation = "Desejo-lhe pleno sucesso hoje à noite; reconheço sua dedicação incansável na preparação desta demonstração.",
                        Context = "Adequado para comunicação com diretoria ou banca avaliadora."
                    }
                ]
            };
        }

        if (lower.Contains("bite the bullet"))
        {
            return new TranslationResponse
            {
                OriginalText = sanitizedText,
                DetectedSourceLanguage = "en",
                TargetLanguage = request.TargetLanguage,
                TranslatedText = "Precisamos engolir o sapo e cortar os prejuízos antes que custe os olhos da cara.",
                Tone = request.Tone,
                SummaryExplanation = "Adaptation of classic idioms 'bite the bullet' (face inevitable hardship with courage) and 'cost an arm and a leg' (cost excessively high) into Portuguese equivalents.",
                IdiomsAndNuances =
                [
                    new IdiomNuanceItem
                    {
                        Expression = "Bite the bullet",
                        LiteralMeaning = "Morder a bala",
                        FigurativeMeaning = "Enfrentar uma situação difícil e inevitável com coragem e determinação",
                        Explanation = "Origem militar: soldados mordiam uma bala de chumbo para suportar dores de procedimentos cirúrgicos sem anestesia.",
                        Formality = "Idiomático"
                    },
                    new IdiomNuanceItem
                    {
                        Expression = "Cost an arm and a leg",
                        LiteralMeaning = "Custar um braço e uma perna",
                        FigurativeMeaning = "Custar excessivamente caro",
                        Explanation = "Equivalente direto à expressão lusófona 'custar os olhos da cara'.",
                        Formality = "Coloquial"
                    }
                ],
                TranslationChoices =
                [
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "bite the bullet",
                        SelectedTerm = "encarar a realidade / engolir o sapo",
                        AlternativeTerm = "morder a bala",
                        Rationale = "'Morder a bala' não comunica a ideia de resiliência pragmática em português."
                    },
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "an arm and a leg",
                        SelectedTerm = "os olhos da cara",
                        AlternativeTerm = "um braço e uma perna",
                        Rationale = "Preserva a metáfora anatômica de alto valor culturalmente enraizada no idioma de destino."
                    }
                ],
                AlternativePhrasings =
                [
                    new AlternativePhrasingItem
                    {
                        Tone = "Corporativo / Moderado",
                        Translation = "Devemos tomar a decisão difícil e mitigar as perdas antes que o custo se torne insustentável.",
                        Context = "Recomendado para atas de reunião e apresentações executivas."
                    }
                ]
            };
        }

        if (lower.Contains("pisando em ovos") || lower.Contains("pisar em ovos"))
        {
            return new TranslationResponse
            {
                OriginalText = sanitizedText,
                DetectedSourceLanguage = "pt",
                TargetLanguage = request.TargetLanguage,
                TranslatedText = "He was walking on eggshells when speaking to the client about the delay.",
                Tone = request.Tone,
                SummaryExplanation = "The metaphor 'pisar em ovos' matches 'walking on eggshells' in English, expressing extreme caution in a delicate social or professional scenario.",
                IdiomsAndNuances =
                [
                    new IdiomNuanceItem
                    {
                        Expression = "Pisar em ovos",
                        LiteralMeaning = "Stepping on eggs",
                        FigurativeMeaning = "Acting with extreme caution to avoid offending someone or provoking a hostile reaction",
                        Explanation = "Creates a vivid image of treading on delicate eggshells where any misplaced pressure will cause a mess.",
                        Formality = "Everyday Metaphor"
                    }
                ],
                TranslationChoices =
                [
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "pisando em ovos",
                        SelectedTerm = "walking on eggshells",
                        AlternativeTerm = "treading on eggs",
                        Rationale = "'Walking on eggshells' is the exact idiomatic formula recognized in natural English."
                    }
                ],
                AlternativePhrasings =
                [
                    new AlternativePhrasingItem
                    {
                        Tone = "Diplomatic / Formal",
                        Translation = "He exercised utmost tact and diplomacy when addressing the delay with the client.",
                        Context = "Used in performance feedback and conflict resolution reports."
                    }
                ]
            };
        }

        if (lower.Contains("hit the nail on the head"))
        {
            return new TranslationResponse
            {
                OriginalText = sanitizedText,
                DetectedSourceLanguage = "en",
                TargetLanguage = request.TargetLanguage,
                TranslatedText = "Ela acertou na mosca ao descrever a causa raiz do bug.",
                Tone = request.Tone,
                SummaryExplanation = "The English expression 'hit the nail on the head' corresponds directly to the Portuguese idiomatic phrase 'acertou na mosca' or 'acertou em cheio'.",
                IdiomsAndNuances =
                [
                    new IdiomNuanceItem
                    {
                        Expression = "Hit the nail on the head",
                        LiteralMeaning = "Bater o prego na cabeça",
                        FigurativeMeaning = "Identificar com exatidão cirúrgica o ponto central de uma questão",
                        Explanation = "Metáfora de carpintaria: acertar o martelo perfeitamente no topo do prego.",
                        Formality = "Idiomático"
                    }
                ],
                TranslationChoices =
                [
                    new TranslationChoiceItem
                    {
                        OriginalTerm = "hit the nail on the head",
                        SelectedTerm = "acertou na mosca",
                        AlternativeTerm = "bateu no prego com a cabeça",
                        Rationale = "'Acertou na mosca' transmite a precisão perfeita no jargão informal brasileiro."
                    }
                ],
                AlternativePhrasings =
                [
                    new AlternativePhrasingItem
                    {
                        Tone = "Formal / Técnico",
                        Translation = "Ela foi extremamente precisa ao diagnosticar a causa raiz da falha.",
                        Context = "Relatórios de post-mortem técnico e auditoria de código."
                    }
                ]
            };
        }

        return new TranslationResponse
        {
            OriginalText = sanitizedText,
            DetectedSourceLanguage = request.SourceLanguage == "auto" ? "pt" : request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            TranslatedText = $"[Demonstration translation for: {sanitizedText}]",
            Tone = request.Tone,
            SummaryExplanation = "Text translated with linguistic nuance preservation and contextual adaptation.",
            IdiomsAndNuances =
            [
                new IdiomNuanceItem
                {
                    Expression = sanitizedText.Length > 25 ? sanitizedText[..25] + "..." : sanitizedText,
                    LiteralMeaning = "Direct linguistic representation",
                    FigurativeMeaning = "Contextual semantic intent",
                    Explanation = "The Gemini AI engine inspects the grammatical structure and culturally adapts the phrasing according to the chosen tone.",
                    Formality = request.Tone
                }
            ],
            TranslationChoices =
            [
                new TranslationChoiceItem
                {
                    OriginalTerm = sanitizedText.Length > 20 ? sanitizedText[..20] : sanitizedText,
                    SelectedTerm = "Contextually adapted term",
                    AlternativeTerm = "Literal dictionary term",
                    Rationale = "Prioritized conversational fluency over rigid word-by-word substitution."
                }
            ],
            AlternativePhrasings =
            [
                new AlternativePhrasingItem
                {
                    Tone = "Formal",
                    Translation = $"Formal rendition of: {sanitizedText}",
                    Context = "Structured academic or enterprise environments."
                },
                new AlternativePhrasingItem
                {
                    Tone = "Casual",
                    Translation = $"Casual rendition of: {sanitizedText}",
                    Context = "Friendly social dialogue."
                }
            ]
        };
    }

    private static string SanitizeInput(string text)
    {
        var normalized = text.Trim();
        normalized = LineBreakRegex().Replace(normalized, "\n");
        return normalized;
    }

    private static string ExtractCleanJson(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[7..];
        }
        else if (trimmed.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[3..];
        }

        if (trimmed.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^3];
        }

        return trimmed.Trim();
    }

    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex LineBreakRegex();

    private sealed record GeminiParsedTranslation
    {
        public string? DetectedSourceLanguage { get; init; }
        public string? TranslatedText { get; init; }
        public string? SummaryExplanation { get; init; }
        public IReadOnlyList<IdiomNuanceItem>? IdiomsAndNuances { get; init; }
        public IReadOnlyList<TranslationChoiceItem>? TranslationChoices { get; init; }
        public IReadOnlyList<AlternativePhrasingItem>? AlternativePhrasings { get; init; }
    }
}
