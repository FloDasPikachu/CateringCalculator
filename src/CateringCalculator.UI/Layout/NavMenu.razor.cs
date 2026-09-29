using Microsoft.AspNetCore.Components;
using CateringCalculator.UI.Shared;

namespace CateringCalculator.UI.Layout;

public partial class NavMenu : LocalizedComponentBase {
    [Parameter]
    public bool IsMobile { get; set; }
    [Parameter]
    public EventCallback OnItemSelected { get; set; }

    private async Task HandleClick() {
        if (OnItemSelected.HasDelegate) {
            await OnItemSelected.InvokeAsync();
        }
    }
}
