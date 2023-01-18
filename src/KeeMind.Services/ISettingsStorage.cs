namespace KeeMind.Services;

public interface ISettingsStorage
{
    string? Get(string key, string? defaultValue = default);

    void Set(string key, string? value);
}
