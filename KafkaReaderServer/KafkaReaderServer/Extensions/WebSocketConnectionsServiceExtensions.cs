using WebSocket;
using WebSocket.Contracts;

namespace KafkaReaderServer.Extensions;

public static class WebSocketConnectionsServiceExtensions
{
    public static IServiceCollection AddWebSocketConnections(this IServiceCollection services)
    {
        services.AddSingleton<WebSocketConnectionsService>();
        services.AddSingleton<IWebSocketConnectionsService>(serviceProvider => serviceProvider.GetService<WebSocketConnectionsService>());

        return services;
    }
}