using BibleData;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CovenantPromptKey.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddCovenantPromptKeyDomain(this IServiceCollection services)
    {
        services.AddSingleton<IDebugLogService, DebugLogService>();
        services.AddSingleton<BibleIndex>();

        services.AddTransient<IKeywordValidationService, KeywordValidationService>();

        services.AddScoped<IDictionaryService, DictionaryService>();
        services.AddScoped<IWorkSessionService, WorkSessionService>();

        services.AddScoped<IBibleSettingsService, BibleSettingsService>();
        services.AddScoped<IBiblePageStateService, BiblePageStateService>();
        services.AddScoped<IBibleBookmarkService, BibleBookmarkService>();
        services.AddScoped<IBibleSearchService, BibleSearchService>();
        services.AddScoped<IBibleReadingService, BibleReadingService>();
        services.AddScoped<IBibleExportService, BibleExportService>();
        services.AddScoped<IBibleGameService, BibleGameService>();

        services.AddTransient<IMarkdownService, MarkdownService>();
        services.AddTransient<ICsvService, CsvService>();
        services.AddTransient<IKeywordService, KeywordService>();
        return services;
    }
}
