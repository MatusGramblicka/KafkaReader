namespace KafkaReaderServer.Interfaces;

public interface IWebSocketSender
{
    Task SendWebSocketMessage(string message);
}