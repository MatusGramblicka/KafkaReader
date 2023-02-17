namespace KafkaReaderClient.Notifiers;

public class NotifierServiceKafkaTopic
{
    // https://stackoverflow.com/questions/61783010/how-do-i-communicate-between-two-sibling-blazor-components
    // https://stackoverflow.com/questions/71094553/blazor-wasm-getting-sibling-components-to-communicate-in-order-to-update-data
    private string _kafkaTopic;

    public string KafkaTopic
    {
        get => _kafkaTopic;
        set
        {
            if (_kafkaTopic != value)
            {
                _kafkaTopic = value;
                Notify?.Invoke();
            }
        }
    }

    public event Func<Task> Notify;
}