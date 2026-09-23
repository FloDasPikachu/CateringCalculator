using CateringCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CateringCalculator.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Verbindungsschein / ConnectionString hier direkt angeben oder über ConfigurationBuilder laden
        optionsBuilder.UseSqlite("Data Source=app.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}