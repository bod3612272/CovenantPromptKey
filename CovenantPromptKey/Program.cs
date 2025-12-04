using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CovenantPromptKey.Components;
using CovenantPromptKey.Services.Implementations;
using CovenantPromptKey.Services.Interfaces;

namespace CovenantPromptKey
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to use HTTP only (本機使用，不需要 HTTPS)
            var httpPort = FindAvailablePort(5000);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, httpPort);
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Configure SignalR for large message support (FR-033: 支援最多 100,000 字元文本)
            // 預設 SignalR 訊息大小限制為 32KB，長文本輸入需要更大的緩衝區
            // 設定為 512KB 以支援最大 100,000 字元 UTF-8 文本（每字元最多 4 bytes）加上 JSON 封裝開銷
            builder.Services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 512 * 1024; // 512 KB
            });

            // Register application services
            // Singleton services (global state)
            builder.Services.AddSingleton<IDebugLogService, DebugLogService>();

            // Scoped services (per Circuit instance)
            builder.Services.AddScoped<ISessionStorageService, SessionStorageService>();
            builder.Services.AddScoped<IWorkSessionService, WorkSessionService>();
            builder.Services.AddScoped<IDictionaryService, DictionaryService>();

            // Transient services (stateless)
            builder.Services.AddTransient<IKeywordService, KeywordService>();
            builder.Services.AddTransient<IMarkdownService, MarkdownService>();
            builder.Services.AddTransient<IKeywordValidationService, KeywordValidationService>();
            builder.Services.AddTransient<ICsvService, CsvService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();

            // 使用 UseStaticFiles 以支援單一檔案發布
            // MapStaticAssets() 在單一檔案模式下無法找到 manifest 檔案
            app.UseStaticFiles();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Auto-open browser (FR-031) - 使用 HTTP
            OpenBrowser($"http://127.0.0.1:{httpPort}");

            app.Run();
        }

        /// <summary>
        /// Open the default browser with the specified URL.
        /// </summary>
        /// <param name="url">URL to open</param>
        private static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"無法自動開啟瀏覽器: {ex.Message}");
                Console.WriteLine($"請手動開啟: {url}");
            }
        }

        /// <summary>
        /// <summary>
        /// Find an available port starting from the specified port.
        /// </summary>
        /// <param name="startPort">Port to start searching from</param>
        /// <returns>Available port number</returns>
        private static int FindAvailablePort(int startPort = 5000)
        {
            for (int port = startPort; port <= 5050; port++)
            {
                if (IsPortAvailable(port))
                {
                    return port;
                }
            }
            
            // Fallback: let the OS assign a port
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }

        /// <summary>
        /// Check if a port is available for binding.
        /// </summary>
        /// <param name="port">Port to check</param>
        /// <returns>True if port is available</returns>
        private static bool IsPortAvailable(int port)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
