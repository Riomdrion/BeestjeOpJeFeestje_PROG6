namespace BeestjeOpJeFeestje_PROG6.ViewModel;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}