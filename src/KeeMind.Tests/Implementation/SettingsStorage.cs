using Microsoft.Maui.Storage;

namespace KeeMind.Services.Implementation;

internal class SettingsStorage : ISettingsStorage
{
    private readonly Dictionary<string, string?> _settings = new();

    public string? Get(string key, string? defaultValue = null)
    {
        if (_settings.TryGetValue(key, out var res))
        {
            return res;
        }

        return defaultValue;
    }

    public void Set(string key, string? value)
    {
        _settings[key] = value;
    }
}