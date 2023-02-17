using KafkaReaderClient.HttpInterceptor;
using KafkaReaderClient.HttpRepository;
using KafkaReaderClient.Notifiers;
using Microsoft.AspNetCore.Components;

namespace KafkaReaderClient.Shared;

public partial class NavMenu
{
    private bool _collapseNavMenu = true;

    private string? NavMenuCssClass => _collapseNavMenu ? "collapse" : null;

    public List<string> KafkaTopicsList { get; set; } = new();

    [Inject] public HttpInterceptorService Interceptor { get; set; }
    [Inject] public IKafkaHttpRepository KafkaRepo { get; set; }
    [Inject] public NotifierServiceKafkaTopic NotifierKafkaTopicChange { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Interceptor.RegisterEvent();

        await GetKafkaTopics();
    }

    private void ToggleNavMenu()
    {
        _collapseNavMenu = !_collapseNavMenu;
    }

    private async Task GetKafkaTopics()
    {
        KafkaTopicsList = (List<string>) await KafkaRepo.GetKafkaTopics();
    }

    private void OnKafkaTopicChange(ChangeEventArgs eventArgs)
    {
        NotifierKafkaTopicChange.KafkaTopic = eventArgs.Value?.ToString() ?? string.Empty;
    }
}