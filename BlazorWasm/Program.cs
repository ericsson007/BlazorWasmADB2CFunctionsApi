using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorWasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!!  builder.HostEnvironment.Environment {builder.HostEnvironment.Environment}");

            var settings = new Settings();
            builder.Configuration.Bind(settings);
            System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!!  AzureAdB2C.Authority {settings?.AzureAdB2C?.Authority}");
            System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!!  AzureAdB2C.ClientId {settings?.AzureAdB2C?.ClientId}");
            System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!!  AzureAdB2C.Scope {settings?.AzureAdB2C?.Scope}");
            System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!!  AzureAdB2C.SecureWebApiEndpoint {settings?.AzureAdB2C?.SecureWebApiEndpoint}");
            System.Diagnostics.Debug.WriteLine($"!!!!!!!!!!!!!!!!  AzureAdB2C.ValidateAuthority {settings?.AzureAdB2C?.ValidateAuthority}");


            builder.Services.AddScoped<AuthorizationMessageHandler>();
            builder.Services.AddHttpClient("ServerAPI",
                client => client.BaseAddress = new Uri(settings.AzureAdB2C.SecureWebApiEndpoint))
                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(
                    authorizedUrls: new[] { settings.AzureAdB2C.SecureWebApiEndpoint },
                    scopes: new[] { settings.AzureAdB2C.Scope }
                    ));
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add(settings.AzureAdB2C.Scope);
            });

            await builder.Build().RunAsync();
        }
    }
}
