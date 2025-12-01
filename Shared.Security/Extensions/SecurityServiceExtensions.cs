using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Shared.Security.Configuration;
using Shared.Security.Services;
namespace Shared.Security.Extensions
{
    public static class SecurityServiceExtensions
    {
        public static IServiceCollection AddEntraIdWithRoleAugmentation<TUser>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TUser : class
        {
            // 1. Bind Options
            services.AddOptions<EntraIdOptions>()
                .Bind(configuration.GetSection(EntraIdOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 2. Register the Augmentation Service
            services.AddScoped<ClaimsAugmentationService<TUser>>();

            // 3. Configure Microsoft Identity Web (The standard Azure AD wrapper)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                {
                    // Load Azure settings
                    configuration.Bind(EntraIdOptions.SectionName, options);

                    // 4. Hook into the Token Validated Event
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // Resolve our Scoped service
                            // We must use context.HttpContext.RequestServices because this is a callback
                            var augmenter = context.HttpContext.RequestServices
                                .GetRequiredService<ClaimsAugmentationService<TUser>>();

                            // Execute the lookup and injection logic
                            await augmenter.EnrichPrincipalAsync(context.Principal!);
                        },
                        OnAuthenticationFailed = context =>
                        {
                            // FIX: Use ILoggerFactory to create a library-specific logger
                            // instead of referencing the consumer's Program class.
                            var loggerFactory = context.HttpContext.RequestServices
                                .GetRequiredService<ILoggerFactory>();

                            var logger = loggerFactory.CreateLogger("Shared.Security.Authentication");

                            logger.LogError(context.Exception, "Authentication Failed");
                            return Task.CompletedTask;
                        }
                    };
                },
                options =>
                {
                    // Configuration binding for the JwtBearer options specifically
                    configuration.Bind(EntraIdOptions.SectionName, options);
                });

            return services;
        }
    }
}
