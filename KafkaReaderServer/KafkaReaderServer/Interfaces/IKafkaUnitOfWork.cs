namespace KafkaReaderServer.Interfaces;

public interface IKafkaUnitOfWork : IDisposable
{
    IEnumerable<string> GetKafkaTopics();
    void ConsumeKafkaTopic(string topic, CancellationToken cancellationToken);
}