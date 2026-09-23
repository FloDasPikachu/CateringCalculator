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
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task SaveEventPlanAsync(EventPlan eventPlan) {
        var existingPlan = await context.EventPlans
            .Include(e => e.SelectedRecipes)
            .FirstOrDefaultAsync(e => e.Id == eventPlan.Id);

        if (existingPlan == null) {
            // Neues Event anlegen
            foreach (var selected in eventPlan.SelectedRecipes) {
                selected.Recipe = null!; // Referenz entkoppeln für FK-Insert
            }

            await context.EventPlans.AddAsync(eventPlan);
        } else {
            // Bestehendes Event aktualisieren
            existingPlan.Title = eventPlan.Title;
            existingPlan.GuestCount = eventPlan.GuestCount;
            existingPlan.AverageDrinksPerGuest = eventPlan.AverageDrinksPerGuest;
            existingPlan.WasteBufferPercent = eventPlan.WasteBufferPercent;

            // Abgleich der Zuordnungstabelle
            var selectedRecipeIds = eventPlan.SelectedRecipes.Select(r => r.RecipeId).ToList();
            existingPlan.SelectedRecipes.RemoveAll(r => !selectedRecipeIds.Contains(r.RecipeId));

            foreach (var updatedRecipe in eventPlan.SelectedRecipes) {
                var existingRecipe = existingPlan.SelectedRecipes
                    .FirstOrDefault(r => r.RecipeId == updatedRecipe.RecipeId);

                if (existingRecipe != null) {
                    existingRecipe.Percentage = updatedRecipe.Percentage;
                } else {
                    existingPlan.SelectedRecipes.Add(new EventRecipe {
                        EventPlanId = eventPlan.Id,
                        RecipeId = updatedRecipe.RecipeId,
                        Percentage = updatedRecipe.Percentage
                    });
                }
            }
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
}