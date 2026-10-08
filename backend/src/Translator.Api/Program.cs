using Scalar.AspNetCore;
using Translator.Api.Options;
using Translator.Api.Services;
using Translator.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var geminiSection = builder.Configuration.GetSection(GeminiOptions.SectionName);
builder.Services.Configure<GeminiOptions>(geminiSection);

var envApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
if (!string.IsNullOrWhiteSpace(envApiKey))
{
    builder.Services.PostConfigure<GeminiOptions>(opt => opt.ApiKey = envApiKey);
}

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TranslationRequestValidator>();

builder.Services.AddHttpClient<ITranslationService, GeminiTranslationService>((serviceProvider, client) =>
{
    var config = builder.Configuration.GetSection(GeminiOptions.SectionName).Get<GeminiOptions>() ?? new GeminiOptions();
    client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds > 0 ? config.TimeoutSeconds : 30);
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ContextLingo API - Documentation")
               .WithTheme(ScalarTheme.Moon);
    });
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
