using WebSocket.Contracts;
using WebSocket.Infrastructure;
using WebSocket.Middlewares;

namespace KafkaReaderServer.Middleware.WebSocket;

public class WebSocketConnectionsMiddleware
{
    #region Fields

    private readonly WebSocketConnectionsOptions _options;
    private readonly IWebSocketConnectionsService _connectionsService;

    #endregion

    #region Constructor

    public WebSocketConnectionsMiddleware(RequestDelegate next, WebSocketConnectionsOptions options,
        IWebSocketConnectionsService connectionsService)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connectionsService = connectionsService ?? throw new ArgumentNullException(nameof(connectionsService));
    }

    #endregion

    #region Methods

    public async Task Invoke(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            if (ValidateOrigin(context))
            {
                var textSubProtocol = NegotiateSubProtocol(context.WebSockets.WebSocketRequestedProtocols);

                var webSocket = await context.WebSockets.AcceptWebSocketAsync(new WebSocketAcceptContext
                {
                    SubProtocol = textSubProtocol?.SubProtocol,
                    DangerousEnableCompression = true
                });

                var webSocketConnection = new WebSocketConnection(webSocket,
                    textSubProtocol ?? _options.DefaultSubProtocol, _options.SendSegmentSize,
                    _options.ReceivePayloadBufferSize);
                webSocketConnection.ReceiveText += async (sender, message) =>
                {
                    await webSocketConnection.SendAsync(message, CancellationToken.None);
                };

                _connectionsService.AddConnection(webSocketConnection);

                await webSocketConnection.ReceiveMessagesUntilCloseAsync();

                if (webSocketConnection.CloseStatus.HasValue)
                {
                    await webSocket.CloseAsync(webSocketConnection.CloseStatus.Value,
                        webSocketConnection.CloseStatusDescription, CancellationToken.None);
                }

                _connectionsService.RemoveConnection(webSocketConnection.Id);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private bool ValidateOrigin(HttpContext context)
    {
        return (_options.AllowedOrigins == null) || (_options.AllowedOrigins.Count == 0) ||
               (_options.AllowedOrigins.Contains(context.Request.Headers["Origin"].ToString()));
    }

    private ITextWebSocketSubprotocol NegotiateSubProtocol(IList<string> requestedSubProtocols)
    {
        ITextWebSocketSubprotocol subProtocol = null;

        foreach (var supportedSubProtocol in _options.SupportedSubProtocols)
        {
            if (requestedSubProtocols.Contains(supportedSubProtocol.SubProtocol))
            {
                subProtocol = supportedSubProtocol;
                break;
            }
        }

        return subProtocol;
    }

    #endregion
}