using CateringCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringCalculator.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
    public DbSet<EventPlan> EventPlans => Set<EventPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Precision für Decimal-Werte in SQLite festlegen
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.PackagePrice)
            .HasConversion<double>();

        modelBuilder.Entity<Ingredient>()
            .Property(i => i.PackageSize)
            .HasConversion<double>();

        modelBuilder.Entity<RecipeItem>()
            .Property(r => r.Amount)
            .HasConversion<double>();

        modelBuilder.Entity<EventPlan>()
            .Property(e => e.WasteBufferPercent)
            .HasConversion<double>();

        modelBuilder.Entity<Recipe>()
        .HasMany(r => r.Items)
        .WithOne()
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventRecipe>()
            .HasKey(er => new { er.EventPlanId, er.RecipeId });

        modelBuilder.Entity<EventRecipe>()
            .HasOne(er => er.EventPlan)
            .WithMany(ep => ep.SelectedRecipes)
            .HasForeignKey(er => er.EventPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventRecipe>()
            .HasOne(er => er.Recipe)
            .WithMany()
            .HasForeignKey(er => er.RecipeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}