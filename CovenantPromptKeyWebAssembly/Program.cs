using CovenantPromptKey.Browser;
using CovenantPromptKey.Domain;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CovenantPromptKeyWebAssembly;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services
            // Domain DI now includes keyword/dictionary services (Slice 3).
            .AddCovenantPromptKeyDomain()
            .AddCovenantPromptKeyBrowser();

        await builder.Build().RunAsync();
    }
}
