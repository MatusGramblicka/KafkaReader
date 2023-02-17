namespace KafkaReaderClient.HttpRepository;

public interface IKafkaHttpRepository
{
    Task<IEnumerable<string>> GetKafkaTopics();
    Task GetKafkaTopicMessages(string topic);
}