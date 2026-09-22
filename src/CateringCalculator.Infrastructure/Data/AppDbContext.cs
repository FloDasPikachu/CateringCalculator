using CateringCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringCalculator.Infrastructure.Data;

public class AppDbContext : DbContext {
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
    public DbSet<EventPlan> EventPlans => Set<EventPlan>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Precision für Decimal-Werte in SQLite festlegen
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.BottlePrice)
            .HasConversion<double>();

        modelBuilder.Entity<Ingredient>()
            .Property(i => i.BottleVolumeMl)
            .HasConversion<double>();

        modelBuilder.Entity<RecipeItem>()
            .Property(r => r.AmountMl)
            .HasConversion<double>();

        modelBuilder.Entity<EventPlan>()
            .Property(e => e.WasteBufferPercent)
            .HasConversion<double>();
    }
}