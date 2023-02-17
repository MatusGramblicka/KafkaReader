using KafkaReaderServer.Configuration;
using KafkaReaderServer.Core;
using KafkaReaderServer.Extensions;
using KafkaReaderServer.Interfaces;
using KafkaReaderServer.Middleware.WebSocket;
using Microsoft.AspNetCore.HttpOverrides;
using WebSocket.Contracts;
using WebSocket.Infrastructure;
using WebSocket.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKafkaUnitOfWork, KafkaUnitOfWork>();
builder.Services.AddSingleton<IWebSocketSender, WebSocketSender>();


builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection("ServerSettings"));
//builder.Services.AddSingleton<IHostedService, HeartbeatService>();

builder.Services.AddWebSocketConnections();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

var wsSettings = builder.Configuration.GetSection("WebSocket");

ITextWebSocketSubprotocol textWebSocketSubprotocol = new PlainTextWebSocketSubprotocol();
var webSocketConnectionsOptions = new WebSocketConnectionsOptions
{
    AllowedOrigins = new HashSet<string> { wsSettings["AllowedOrigins"] },
    SupportedSubProtocols = new List<ITextWebSocketSubprotocol>
    {
        new JsonWebSocketSubprotocol(),
        textWebSocketSubprotocol
    },
    DefaultSubProtocol = textWebSocketSubprotocol,
    SendSegmentSize = 4 * 1024
};

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
}).MapWebSocketConnections("/socket", webSocketConnectionsOptions);

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    //endpoints.MapFallbackToFile("index.html");
});

app.Run();
