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
            // NEU: Detail-Listen beim Laden mit einbeziehen
            .Include(e => e.FixedCostItems)
            .Include(e => e.PersonnelCostItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task SaveEventPlanAsync(EventPlan eventPlan) {
        var existingPlan = await context.EventPlans
            .Include(e => e.SelectedRecipes)
            .Include(e => e.FixedCostItems)
            .Include(e => e.PersonnelCostItems)
            .FirstOrDefaultAsync(e => e.Id == eventPlan.Id);

        if (existingPlan == null) {
            // Neues Event anlegen
            foreach (var selected in eventPlan.SelectedRecipes) {
                selected.Recipe = null!; // Referenz entkoppeln für FK-Insert
            }

            // Foreign Keys für die neuen Kosten-Items sicherstellen
            foreach (var item in eventPlan.FixedCostItems) {
                item.EventPlanId = eventPlan.Id;
            }

            foreach (var item in eventPlan.PersonnelCostItems) {
                item.EventPlanId = eventPlan.Id;
            }

            await context.EventPlans.AddAsync(eventPlan);
        } else {
            // Bestehendes Event aktualisieren - Scalar-Werte übertragen
            existingPlan.Title = eventPlan.Title;
            existingPlan.GuestCount = eventPlan.GuestCount;
            existingPlan.AverageDrinksPerGuest = eventPlan.AverageDrinksPerGuest;
            existingPlan.WasteBufferPercent = eventPlan.WasteBufferPercent;
            existingPlan.FixedCosts = eventPlan.FixedCosts;
            existingPlan.PersonnelCosts = eventPlan.PersonnelCosts;
            existingPlan.TargetProfit = eventPlan.TargetProfit;
            existingPlan.FreeDrinksCount = eventPlan.FreeDrinksCount;

            // 1. Abgleich der Rezept-Zuordnungstabelle
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

            // 2. Abgleich der Fixkosten-Details (Modal-Werte)
            context.FixedCostItems.RemoveRange(existingPlan.FixedCostItems);
            foreach (var item in eventPlan.FixedCostItems) {
                existingPlan.FixedCostItems.Add(new FixedCostItem {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    EventPlanId = existingPlan.Id,
                    Description = item.Description,
                    Amount = item.Amount
                });
            }

            // 3. Abgleich der Personalkosten-Details (Modal-Werte)
            context.PersonnelCostItems.RemoveRange(existingPlan.PersonnelCostItems);
            foreach (var item in eventPlan.PersonnelCostItems) {
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