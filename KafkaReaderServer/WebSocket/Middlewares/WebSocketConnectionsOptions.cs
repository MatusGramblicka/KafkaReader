﻿using WebSocket.Contracts;

namespace WebSocket.Middlewares;

public class WebSocketConnectionsOptions
{
    public HashSet<string> AllowedOrigins { get; set; }

    public IList<ITextWebSocketSubprotocol> SupportedSubProtocols { get; set; }

    public ITextWebSocketSubprotocol DefaultSubProtocol { get; set; }

    public int? SendSegmentSize { get; set; }

    public int ReceivePayloadBufferSize { get; set; }

    public WebSocketConnectionsOptions()
    {
        ReceivePayloadBufferSize = 4 * 1024;
    }
}