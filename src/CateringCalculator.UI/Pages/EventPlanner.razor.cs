using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.Core.Services;
using CateringCalculator.Resources.Resources.Internationalization;
using CateringCalculator.UI.Services;
using CateringCalculator.UI.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CateringCalculator.UI.Pages;

public partial class EventPlanner : LocalizedComponentBase, IAsyncDisposable {
    [Inject]
    private CalculationService CalculationService { get; set; } = default!;
    [Inject]
    private IEventPlanRepository EventPlanRepository { get; set; } = default!;
    [Inject]
    private IJSRuntime JS { get; set; } = default!;
    [Inject]
    private PageTitleService TitleService { get; set; } = default!;

    private CalculationResult? _calculationResult;
    private DotNetObjectReference<EventPlanner>? _dotNetRef;
    private EventPlan _eventPlan = CreateEmptyEventPlan();
    private IJSObjectReference? _jsModule;
    private ElementReference _pageHeaderRef;
    private List<EventPlan> _savedEventPlans = [];

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/scrollObserver.js");
            await _jsModule.InvokeVoidAsync("observeHeader", _pageHeaderRef, _dotNetRef);
        }
    }

    protected override async Task OnInitializedAsync() {
        TitleService.SetTitle("Catering Calculator");
        await LoadSavedEventPlansAsync();
    }

    public async ValueTask DisposeAsync() {
        if (_jsModule != null) {
            await _jsModule.InvokeVoidAsync("unobserveHeader");
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }

    [JSInvokable]
    public void TargetVisibilityChanged(bool isIntersecting) {
        if (isIntersecting) {
            TitleService.SetTitle("Catering Calculator");
        } else {
            TitleService.SetTitle(AppResources.Event_Title);
        }
    }

    private void CalculateEvent() {
        _calculationResult = CalculationService.Calculate(_eventPlan);
    }

    private static EventPlan CreateEmptyEventPlan() => new() {
        Id = Guid.NewGuid(),
        Title = "Neues Event",
        GuestCount = 30,
        AverageDrinksPerGuest = 3,
        WasteBufferPercent = 10m,
        FixedCosts = 150m,
        PersonnelCosts = 300m,
        TargetProfit = 500m,
        FreeDrinksCount = 10,
        SelectedRecipes = []
    };

    private void CreateNewEvent() {
        _eventPlan = CreateEmptyEventPlan();
        _calculationResult = null;
    }

    private async Task DeleteCurrentEvent() {
        if (_eventPlan.Id != Guid.Empty) {
            await EventPlanRepository.DeleteEventPlanAsync(_eventPlan.Id);
            await LoadSavedEventPlansAsync();
            CreateNewEvent();
        }
    }

    private async Task LoadSavedEventPlansAsync() {
        _savedEventPlans = await EventPlanRepository.GetAllEventPlansAsync();
    }

    private async Task SaveCurrentEvent() {
        await EventPlanRepository.SaveEventPlanAsync(_eventPlan);
        await LoadSavedEventPlansAsync();
    }

    private async Task OnEventSelected(ChangeEventArgs e) {
        if (Guid.TryParse(e.Value?.ToString(), out Guid id) && id != Guid.Empty) {
            var loaded = await EventPlanRepository.GetEventPlanByIdAsync(id);
            if (loaded != null) {
                _eventPlan = loaded;
                _calculationResult = null;
            }
        } else {
            CreateNewEvent();
        }
    }
}