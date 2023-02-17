using System.Net.Http.Json;

namespace KafkaReaderClient.HttpRepository;

public class KafkaHttpRepository : IKafkaHttpRepository
{
    private readonly HttpClient _client;

    private readonly CancellationTokenSource _cancellationToken = new();

    public KafkaHttpRepository(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<string>> GetKafkaTopics()
    {
        var kafkaTopics = await _client.GetFromJsonAsync<IEnumerable<string>>($"kafka/topics");

        return kafkaTopics ?? new List<string>();
    }

    public async Task GetKafkaTopicMessages(string topic)
    {
        await _client.PostAsJsonAsync($"kafka/messages", topic, _cancellationToken.Token);
    }
}