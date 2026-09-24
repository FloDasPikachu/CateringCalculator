using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CateringCalculator.Infrastructure.Repositories;

public class EventPlanRepository(AppDbContext context) : IEventPlanRepository {

    public async Task<List<EventPlan>> GetAllEventPlansAsync() {
        return await context.EventPlans
            .AsNoTracking()
            .OrderBy(e => e.Title)
            .ToListAsync();
    }

    public async Task<EventPlan?> GetEventPlanByIdAsync(Guid id) {
        return await context.EventPlans
            .Include(e => e.SelectedRecipes)
                .ThenInclude(er => er.Recipe)
                    .ThenInclude(r => r.Items)
                        .ThenInclude(i => i.Ingredient)
            .Include(e => e.FixedCostItems)
            .Include(e => e.PersonnelCostItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task SaveEventPlanAsync(EventPlan eventPlan) {
        ArgumentNullException.ThrowIfNull(eventPlan);

        var existingPlan = await context.EventPlans
            .Include(e => e.SelectedRecipes)
            .Include(e => e.FixedCostItems)
            .Include(e => e.PersonnelCostItems)
            .FirstOrDefaultAsync(e => e.Id == eventPlan.Id);

        if (existingPlan == null) {
            AddNewEventPlan(eventPlan);
        } else {
            UpdateExistingEventPlan(existingPlan, eventPlan);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteEventPlanAsync(Guid id) {
        var plan = await context.EventPlans.FindAsync(id);
        if (plan != null) {
            context.EventPlans.Remove(plan);
            await context.SaveChangesAsync();
        }
    }

    private static void AddNewEventPlan(EventPlan eventPlan) {
        foreach (var selected in eventPlan.SelectedRecipes) {
            selected.Recipe = null!; // Referenz entkoppeln für FK-Insert
        }

        foreach (var item in eventPlan.FixedCostItems) {
            item.EventPlanId = eventPlan.Id;
        }

        foreach (var item in eventPlan.PersonnelCostItems) {
            item.EventPlanId = eventPlan.Id;
        }
    }

    private void UpdateExistingEventPlan(EventPlan existingPlan, EventPlan updatedPlan) {
        existingPlan.UpdateDetails(updatedPlan.Title, updatedPlan.GuestCount, updatedPlan.AverageDrinksPerGuest,
          updatedPlan.WasteBufferPercent, updatedPlan.FixedCosts, updatedPlan.PersonnelCosts,
          updatedPlan.TargetProfit, updatedPlan.FreeDrinksCount);
        SyncSelectedRecipes(existingPlan, updatedPlan);
        SyncFixedCostItems(existingPlan, updatedPlan);
        SyncPersonnelCostItems(existingPlan, updatedPlan);
    }

    private void SyncSelectedRecipes(EventPlan existingPlan, EventPlan updatedPlan) {
        var updatedRecipeIds = updatedPlan.SelectedRecipes.Select(r => r.RecipeId).ToList();
        existingPlan.SelectedRecipes.RemoveAll(r => !updatedRecipeIds.Contains(r.RecipeId));

        foreach (var updatedRecipe in updatedPlan.SelectedRecipes) {
            var existingRecipe = existingPlan.SelectedRecipes
                .FirstOrDefault(r => r.RecipeId == updatedRecipe.RecipeId);

            if (existingRecipe != null) {
                existingRecipe.Percentage = updatedRecipe.Percentage;
            } else {
                existingPlan.SelectedRecipes.Add(new EventRecipe {
                    EventPlanId = existingPlan.Id,
                    RecipeId = updatedRecipe.RecipeId,
                    Percentage = updatedRecipe.Percentage
                });
            }
        }
    }

    private void SyncFixedCostItems(EventPlan existingPlan, EventPlan updatedPlan) {
        context.FixedCostItems.RemoveRange(existingPlan.FixedCostItems);
        existingPlan.FixedCostItems.Clear();

        foreach (var item in updatedPlan.FixedCostItems) {
            existingPlan.FixedCostItems.Add(new FixedCostItem {
                Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                EventPlanId = existingPlan.Id,
                Description = item.Description,
                Amount = item.Amount
            });
        }
    }

    private void SyncPersonnelCostItems(EventPlan existingPlan, EventPlan updatedPlan) {
        context.PersonnelCostItems.RemoveRange(existingPlan.PersonnelCostItems);
        existingPlan.PersonnelCostItems.Clear();

        foreach (var item in updatedPlan.PersonnelCostItems) {
            existingPlan.PersonnelCostItems.Add(new PersonnelCostItem {
                Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                EventPlanId = existingPlan.Id,
                RoleName = item.RoleName,
                Count = item.Count,
                Hours = item.Hours,
                HourlyRate = item.HourlyRate
            });
        }
    }
}