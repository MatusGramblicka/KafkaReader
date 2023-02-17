using KafkaReaderClient.Notifiers;
using Microsoft.AspNetCore.Components;

namespace KafkaReaderClient.Components;

public partial class PanelButtons
{
    [Inject]
    public NotifierServiceClear NotifierClearText { get; set; }
        
    private void OnNotifyClearText()
    {
        NotifierClearText.CleanKafkaMessages = true;
    }
}