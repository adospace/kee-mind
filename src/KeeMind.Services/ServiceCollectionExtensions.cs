using KeeMind.Services.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace KeeMind.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddKeeMindServices(this IServiceCollection services)
        {
            services.AddSingleton<IRepository, Implementation.Repository>();
            services.AddHttpClient();
        }
    }
}