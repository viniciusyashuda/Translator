using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Translator.Api.Models;
using Translator.Api.Options;
using Translator.Api.Services;
using Translator.Api.Validators;

namespace Translator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;
    private readonly TranslationRequestValidator _validator;
    private readonly GeminiOptions _options;

    public TranslationController(
        ITranslationService translationService,
        TranslationRequestValidator validator,
        IOptions<GeminiOptions> options)
    {
        _translationService = translationService;
        _validator = validator;
        _options = options.Value;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TranslationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = string.Join("; ", validation.Errors)
            };
            problemDetails.Extensions["errors"] = validation.Errors;
            return BadRequest(problemDetails);
        }

        try
        {
            var response = await _translationService.TranslateAndExplainAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Translation Service Error",
                Detail = ex.Message
            });
        }
    }

    [HttpGet("languages")]
    [ProducesResponseType(typeof(IReadOnlyList<LanguageInfo>), StatusCodes.Status200OK)]
    public IActionResult GetLanguages()
    {
        return Ok(_translationService.GetLanguages());
    }

    [HttpGet("presets")]
    [ProducesResponseType(typeof(IReadOnlyList<ExpressionPreset>), StatusCodes.Status200OK)]
    public IActionResult GetPresets()
    {
        return Ok(_translationService.GetPresets());
    }

    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        var hasApiKey = !string.IsNullOrWhiteSpace(_options.ApiKey) &&
                        !_options.ApiKey.Contains("YOUR_GEMINI_API_KEY", StringComparison.OrdinalIgnoreCase);

        return Ok(new
        {
            Status = "Healthy",
            Service = "ContextLingo - Gemini Translator & Explainer API",
            Model = _options.Model,
            ApiKeyConfigured = hasApiKey,
            CacheEnabled = _options.EnableCache,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
