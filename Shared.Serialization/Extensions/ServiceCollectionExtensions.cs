using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Serialization.Interfaces;
using Shared.Serialization.Services;

namespace Shared.Serialization.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnterpriseSerialization(this IServiceCollection services)
        {
            // Register as Singleton because Options are stateless and thread-safe
            services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
            services.AddSingleton(Options.Create(SystemTextJsonSerializer.DefaultOptions));
            return services;
        }
    }
}