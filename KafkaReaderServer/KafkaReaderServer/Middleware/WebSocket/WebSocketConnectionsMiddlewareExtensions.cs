using WebSocket.Middlewares;

namespace KafkaReaderServer.Middleware.WebSocket;

public static class WebSocketConnectionsMiddlewareExtensions
{
    public static IApplicationBuilder MapWebSocketConnections(this IApplicationBuilder app, PathString pathMatch,
        WebSocketConnectionsOptions options)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.Map(pathMatch, branchedApp => branchedApp.UseMiddleware<WebSocketConnectionsMiddleware>(options));
    }
}