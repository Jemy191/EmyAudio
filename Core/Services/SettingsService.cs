using System.Text.Json;
using Core.Models;

namespace Core.Services;

public class SettingsService
{
    const string SettingsFileName = "settings.json";

    public Setting Setting { get; private set; } = null!;

    public async Task Load()
    {
        if (!File.Exists(SettingsFileName))
        {
            Setting = new Setting();
            await File.WriteAllTextAsync(SettingsFileName, JsonSerializer.Serialize(Setting));
        }
        else
            Setting = JsonSerializer.Deserialize<Setting>(await File.ReadAllTextAsync(SettingsFileName))!;
    }

    public async Task Save()
    {
        await File.WriteAllTextAsync(SettingsFileName, JsonSerializer.Serialize(Setting));
    }
}