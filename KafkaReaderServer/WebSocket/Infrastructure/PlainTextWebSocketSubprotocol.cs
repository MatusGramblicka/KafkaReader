using WebSocket.Contracts;

namespace WebSocket.Infrastructure;

public class PlainTextWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
{
    public string SubProtocol => "aspnetcore-ws.plaintext";

}