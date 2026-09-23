using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Services;
using CateringCalculator.Infrastructure.Data;
using CateringCalculator.Infrastructure.Repositories;
using CateringCalculator.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CateringCalculator.UI;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // 1. Pfad für lokale SQLite-Datenbank auf dem Gerät ermitteln
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "catering_calculator.db");

        // 2. DbContextFactory registrieren (stabile Option für Blazor / Repositories)
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // 3. Application Services & Repositories registrieren
        builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
        builder.Services.AddScoped<IEventPlanRepository, EventPlanRepository>(); // Neu hinzugefügt
        builder.Services.AddScoped<CalculationService>();

        builder.Services.AddSingleton<FileSaveService>();

        var app = builder.Build();

        // 4. Migrationen beim Start automatisch auf die Gerätedatenbank anwenden
        using (var scope = app.Services.CreateScope()) {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var dbContext = dbContextFactory.CreateDbContext();

            // Führt alle ausstehenden Migrationen (wie FixEventRecipeNavigation) auf catering_calculator.db aus
            dbContext.Database.Migrate();

            DbInitializer.InitializeAsync(dbContext).GetAwaiter().GetResult();
        }

        return app;
    }
}