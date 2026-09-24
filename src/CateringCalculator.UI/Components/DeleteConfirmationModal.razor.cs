using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class DeleteConfirmationModal {
    [Parameter]
    public bool IsVisible { get; set; }
    [Parameter]
    public string Title { get; set; } = string.Empty;
    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnConfirm { get; set; }
    [Parameter]
    public EventCallback OnCancel { get; set; }
}
