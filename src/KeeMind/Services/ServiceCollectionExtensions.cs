using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace KeeMind.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddKeeMindMauiServices(this IServiceCollection services)
        {
            services.AddSingleton<ISettingsStorage, Implementation.SettingsStorage>();
        }
    }
}