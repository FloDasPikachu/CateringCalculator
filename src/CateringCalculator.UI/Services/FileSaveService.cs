namespace CateringCalculator.UI.Services;

public class FileSaveService {
    public async Task SaveAndOpenFileAsync(string fileName, byte[] data) {
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, data);

        await Launcher.Default.OpenAsync(new OpenFileRequest {
            File = new ReadOnlyFile(filePath)
        });
    }
}