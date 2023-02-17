namespace KafkaReaderClient.Notifiers;

public class NotifierServiceClear
{
    private bool _cleanKafkaMessages;

    public bool CleanKafkaMessages
    {
        get => _cleanKafkaMessages;
        set
        {
            _cleanKafkaMessages = value;
            Notify.Invoke();
        }
    }

    public event Func<Task> Notify;
}