using Confluent.Kafka;
using KafkaReaderServer.Configuration;
using KafkaReaderServer.Interfaces;
using Microsoft.Extensions.Options;

namespace KafkaReaderServer.Core;

public class KafkaUnitOfWork : IKafkaUnitOfWork
{
    private readonly ServerSettings _serverSettings;

    private static readonly string CurrentDir = Directory.GetCurrentDirectory() + @"\Certificates";
    private readonly string _sslKeyLocation = @$"{CurrentDir}\kafka-client-key.pem";
    private readonly string _sslCertificateLocation = @$"{CurrentDir}\kafka-client.pem";
    private readonly string _sslCaLocation = @$"{CurrentDir}\kistler-rootca.pem";

    private readonly IConsumer<string, string> _consumer;
    private readonly IWebSocketSender _webSocketSender;

    private bool _disposed;

    public KafkaUnitOfWork(IWebSocketSender webSocketSender, IOptions<ServerSettings> serverSettings)
    {
        _webSocketSender = webSocketSender;
        _serverSettings = serverSettings.Value;
        var config = new ConsumerConfig
        {
            BootstrapServers = _serverSettings.BootstrapServer,
            GroupId = "kafkaReader",
            AutoOffsetReset = AutoOffsetReset.Earliest,

            SecurityProtocol = SecurityProtocol.Ssl,
            SslKeyLocation = _sslKeyLocation,
            SslCertificateLocation = _sslCertificateLocation,
            SslCaLocation = _sslCaLocation
        };
        var consumerBuilder = new ConsumerBuilder<string, string>(config);
        _consumer = consumerBuilder.Build();

    }

    public IEnumerable<string> GetKafkaTopics()
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _serverSettings.BootstrapServer,
            SecurityProtocol = SecurityProtocol.Ssl,
            SslKeyLocation = _sslKeyLocation,
            SslCertificateLocation = _sslCertificateLocation,
            SslCaLocation = _sslCaLocation
        };

        using var adminClient = new AdminClientBuilder(adminConfig).Build();
        Metadata? metadata;

        try
        {
            metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        }
        catch (KafkaException)
        {
            return new List<string>();
        }

        return metadata.Topics.Select(a => a.Topic).ToList();
    }

    public void ConsumeKafkaTopic(string topic, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            var subscriptions = _consumer.Subscription;
            if (subscriptions.Any())
            {
                _consumer.Unsubscribe();
            }

            _consumer.Subscribe(topic);
            while (true)
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                await _webSocketSender.SendWebSocketMessage(consumeResult.Message.Timestamp.UtcDateTime + "\n" +
                                                            consumeResult.Message.Key + "\n" +
                                                            consumeResult.Message.Value);
            }
        }, cancellationToken);
    }

    ~KafkaUnitOfWork() => Dispose();

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _consumer.Close();
        }

        _disposed = true;
    }

    // https://dotnettips.wordpress.com/2021/10/29/everything-that-every-net-developer-needs-to-know-about-disposable-types-properly-implementing-the-idisposable-interface/
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}