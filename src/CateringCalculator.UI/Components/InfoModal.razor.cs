using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class InfoModal {
    [Parameter]
    public bool IsVisible { get; set; }
    [Parameter]
    public string Title { get; set; } = string.Empty;
    [Parameter]
    public string Message { get; set; } = string.Empty;
    [Parameter]
    public EventCallback OnClose { get; set; }
}
