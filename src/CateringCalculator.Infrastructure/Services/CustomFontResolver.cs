using System.Reflection;
using PdfSharpCore.Fonts;

namespace CateringCalculator.Infrastructure.Services;

public class CustomFontResolver : IFontResolver {
    public string DefaultFontName => "arial.ttf";

    public byte[] GetFont(string faceName) {
        var assembly = typeof(CustomFontResolver).GetTypeInfo().Assembly;
        var resourceName = $"CateringCalculator.Infrastructure.Fonts.{faceName}";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
            if (stream == null) {
                throw new FileNotFoundException($"Font-Ressource nicht gefunden: {resourceName}");
            }

            using (var memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) {
        if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase)) {
            // Wenn du arialbd.ttf im Fonts-Ordner hast, wird es hier für fette Texte genutzt
            if (isBold && FileExistsInEmbedded("arialbd.ttf")) {
                return new FontResolverInfo("arialbd.ttf");
            }
            return new FontResolverInfo("arial.ttf");
        }

        // Fallback
        return new FontResolverInfo("arial.ttf");
    }

    private bool FileExistsInEmbedded(string fileName) {
        var assembly = typeof(CustomFontResolver).GetTypeInfo().Assembly;
        var resourceName = $"CateringCalculator.Infrastructure.Fonts.{fileName}";
        return assembly.GetManifestResourceInfo(resourceName) != null;
    }
}