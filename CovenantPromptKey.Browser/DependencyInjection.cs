using Microsoft.Extensions.DependencyInjection;
using CovenantPromptKey.Browser.Implementations;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey.Browser;

public static class DependencyInjection
{
    public static IServiceCollection AddCovenantPromptKeyBrowser(this IServiceCollection services)
    {
        services.AddScoped<ILocalStorageService, LocalStorageService>();
        services.AddScoped<ISessionStorageService, SessionStorageService>();

        return services;
    }
}
