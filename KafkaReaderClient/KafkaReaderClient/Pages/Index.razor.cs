using KafkaReaderClient.Configuration;
using KafkaReaderClient.HttpInterceptor;
using KafkaReaderClient.HttpRepository;
using KafkaReaderClient.Notifiers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Net.WebSockets;
using System.Text;

namespace KafkaReaderClient.Pages;

public partial class Index : IDisposable
{
    private WebSocketConfiguration WebSocketConfiguration { get; set; }

    [Inject] public IKafkaHttpRepository KafkaRepo { get; set; }

    [Inject] public HttpInterceptorService Interceptor { get; set; }

    [Inject] public IOptions<WebSocketConfiguration> WebSocketSettings { get; set; }

    [Inject] public NotifierServiceKafkaTopic NotifierKafkaTopicChange { get; set; }

    [Inject] public NotifierServiceClear NotifierClearText { get; set; }

    [Inject] public IJSRuntime JSRuntime { get; set; }

    //https://gist.github.com/SteveSandersonMS/5aaff6b010b0785075b0a08cc1e40e01
    private readonly CancellationTokenSource _disposalTokenSource = new();
    private readonly ClientWebSocket _webSocket = new();
    private string _log = "";
    private string _logBuffer = "";

    private ElementReference _textAreaRef;

    protected override async Task OnInitializedAsync()
    {
        Interceptor.RegisterEvent();
        NotifierKafkaTopicChange.Notify += OnNotifyKafkaTopicChange;
        NotifierClearText.Notify += OnNotifyClearText;

        WebSocketConfiguration = WebSocketSettings.Value;
        await _webSocket.ConnectAsync(new Uri(WebSocketConfiguration.Connection), _disposalTokenSource.Token);
        _ = ReceiveLoop();
    }

    public async Task OnNotifyKafkaTopicChange()
    {
        await InvokeAsync(async () => { await KafkaRepo.GetKafkaTopicMessages(NotifierKafkaTopicChange.KafkaTopic); });
    }

    public async Task OnNotifyClearText()
    {
        await InvokeAsync(ClearScreen);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ReceiveLoop()
    {
        var buffer = new ArraySegment<byte>(new byte[1024]);
        while (!_disposalTokenSource.IsCancellationRequested)
        {
            // Note that the received block might only be part of a larger message. If this applies in your scenario,
            // check the received.EndOfMessage and consider buffering the blocks until that property is true.
            // Or use a higher-level library such as SignalR.
            var received = await _webSocket.ReceiveAsync(buffer, _disposalTokenSource.Token);

            if (buffer.Array != null)
            {
                var receivedAsText = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);

                if (!received.EndOfMessage)
                {
                    _logBuffer += $"{receivedAsText}";
                }
                else
                {
                    _logBuffer += $"{receivedAsText}";
                    _log += $"{_logBuffer}\n\n";
                    if (_log.Length > 6000)
                    {
                        _log = _log.Remove(0, 1000);
                    }

                    StateHasChanged();
                    _logBuffer = "";
                    // https://stackoverflow.com/questions/67011354/blazor-auto-scroll-textarea-to-bottom
                    await JSRuntime.InvokeVoidAsync("scrollToEnd", _textAreaRef);
                }
            }
        }
    }

    private void ClearScreen()
    {
        _log = "";
    }

    public void Dispose()
    {
        Interceptor.DisposeEvent();
        _disposalTokenSource.Cancel();
        _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocketClose", CancellationToken.None);
        NotifierKafkaTopicChange.Notify -= OnNotifyKafkaTopicChange;
        NotifierClearText.Notify -= OnNotifyClearText;
    }
}