using Microsoft.Maui.Storage;

namespace KeeMind.Services.Implementation;

internal class SettingsStorage : ISettingsStorage
{
    public string? Get(string key, string? defaultValue = null)
    {
        return Preferences.Get(key, defaultValue);
    }

    public void Set(string key, string? value)
    {
        Preferences.Set(key, value);
    }
}