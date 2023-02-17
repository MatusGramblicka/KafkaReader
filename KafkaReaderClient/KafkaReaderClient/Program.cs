using Blazored.Toast;
using KafkaReaderClient;
using KafkaReaderClient.Configuration;
using KafkaReaderClient.HttpInterceptor;
using KafkaReaderClient.HttpRepository;
using KafkaReaderClient.Notifiers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddHttpClient("KafkaAPI", (sp, cl) =>
{
    var apiConfiguration = sp.GetRequiredService<IOptions<ApiConfiguration>>();
    cl.BaseAddress =
        new Uri(apiConfiguration.Value.BaseAddress + "/api/");
    cl.EnableIntercept(sp);
});

builder.Services.AddBlazoredToast();

builder.Services.AddScoped(
    sp => sp.GetService<IHttpClientFactory>().CreateClient("KafkaAPI"));

builder.Services.AddHttpClientInterceptor();

builder.Services.AddScoped<IKafkaHttpRepository, KafkaHttpRepository>();

builder.Services.AddScoped<HttpInterceptorService>();

builder.Services.AddScoped<NotifierServiceKafkaTopic>();
builder.Services.AddScoped<NotifierServiceClear>();

builder.Services.Configure<ApiConfiguration>
    (builder.Configuration.GetSection("ApiConfiguration"));

builder.Services.Configure<WebSocketConfiguration>(builder.Configuration.GetSection("WebSocket"));

await builder.Build().RunAsync();
