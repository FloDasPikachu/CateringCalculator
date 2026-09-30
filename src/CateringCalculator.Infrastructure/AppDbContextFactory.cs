using CateringCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CateringCalculator.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use an environment variable for the design-time connection string when available.
        // Falls back to a local file name that will not be committed in the repo.
        var connectionString = Environment.GetEnvironmentVariable("CATERINGCALC_DB") ?? "Data Source=catering_calculator.db";
        optionsBuilder.UseSqlite(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}