using CateringCalculator.Core.Models;

namespace CateringCalculator.Core.Interfaces;

public interface IEventPlanRepository {
    Task<List<EventPlan>> GetAllEventPlansAsync();
    Task<EventPlan?> GetEventPlanByIdAsync(Guid id);
    Task SaveEventPlanAsync(EventPlan eventPlan);
    Task DeleteEventPlanAsync(Guid id);
}