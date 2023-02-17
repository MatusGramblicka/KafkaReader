using KafkaReaderServer.Interfaces;
using WebSocket.Contracts;

namespace KafkaReaderServer.Core;

public class WebSocketSender : IWebSocketSender
{
    private readonly IWebSocketConnectionsService _webSocketConnectionsService;

    public WebSocketSender(IWebSocketConnectionsService webSocketConnectionsService)
    {
        _webSocketConnectionsService = webSocketConnectionsService;
    }

    public async Task SendWebSocketMessage(string message)
    {
        await _webSocketConnectionsService.SendToAllAsync(message, default);
    }
}