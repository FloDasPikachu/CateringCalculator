using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Services;
using CateringCalculator.Infrastructure.Data;
using CateringCalculator.Infrastructure.Repositories;
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

        // 2. DbContext registrieren
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // 3. Application Services & Repositories registrieren
        builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
        builder.Services.AddScoped<CalculationService>();

        var app = builder.Build();

        // 4. Automatische Migration / Datenbankerstellung beim Start
        using (var scope = app.Services.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }

        return app;
    }
}