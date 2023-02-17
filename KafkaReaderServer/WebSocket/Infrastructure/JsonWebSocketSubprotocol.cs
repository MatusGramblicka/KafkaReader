using Newtonsoft.Json;
using WebSocket.Contracts;

namespace WebSocket.Infrastructure;

public class JsonWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
{
    public string SubProtocol => "aspnetcore-ws.json";

    public override Task SendAsync(string message, Func<byte[], CancellationToken, Task> sendMessageBytesAsync, CancellationToken cancellationToken)
    {
        var jsonMessage = JsonConvert.SerializeObject(new { message, timestamp = DateTime.UtcNow });

        return base.SendAsync(jsonMessage, sendMessageBytesAsync, cancellationToken);
    }

}