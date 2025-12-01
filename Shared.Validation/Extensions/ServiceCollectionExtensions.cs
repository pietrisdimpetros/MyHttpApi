using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Validation.Configuration;
using Shared.Validation.Filters;
using System.Reflection;

namespace Shared.Validation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Enterprise Validation pipeline.
        /// </summary>
        /// <param name="validatorAssemblies">The assemblies containing your API's specific validators.</param>
        public static IServiceCollection AddEnterpriseValidation(
            this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] validatorAssemblies)
        {
            // 1. Config Options
            services.AddOptions<ValidationOptions>()
                .Bind(configuration.GetSection(ValidationOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 2. Register Validators (Scan the Consumer API)
            services.AddValidatorsFromAssemblies(validatorAssemblies, ServiceLifetime.Scoped);

            // 3. Suppress native .NET Model Validation
            // This prevents the framework from returning its own 400 response
            // before our filter gets a chance to run.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // 4. Register our Filter globally for all Controllers
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });

            return services;
        }
    }
}
