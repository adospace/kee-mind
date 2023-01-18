using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace KeeMind.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddKeeMindDummyServices(this IServiceCollection services, string dbName)
        {
            services.AddSingleton<ISettingsStorage>(sp => 
            {
                var settingsStorage = new Implementation.SettingsStorage();
                settingsStorage.Set("DB_NAME", dbName);
                return settingsStorage;
            });
            services.AddHttpClient();
        }
    }
}