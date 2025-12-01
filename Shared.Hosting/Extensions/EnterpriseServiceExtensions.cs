using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Caching.Extensions;
using Shared.Documentation.Extensions;
using Shared.Hosting.Configuration;
using Shared.Hosting.Middleware;
// Libraries
using Shared.Logging.Extensions;
using Shared.Observability.Extensions;
using Shared.Security.Extensions;
using Shared.Serialization.Extensions;
using Shared.Serialization.Services;
using Shared.Validation.Extensions;
using System.Reflection;

namespace Shared.Hosting.Extensions
{
    public static class EnterpriseServiceExtensions
    {
        /// <summary>
        /// Registers the "Service Defaults": Logging, Observability, Security, Validation, Caching, and Docs.
        /// </summary>
        public static WebApplicationBuilder AddCustomServiceDefaults<TUser>(
            this WebApplicationBuilder builder,
            Assembly apiAssembly)
            where TUser : class
        {
            // 1. Load the Master Options (Fail fast if missing)
            var appOptions = new AppOptions
            {
                Name = "Unknown",
                Version = "v1",
                TenantId = "",
                ClientId = "",
                Domain = ""
            };
            builder.Configuration.GetSection(AppOptions.SectionName).Bind(appOptions);

            // Map App.Name -> Logging & Observability
            builder.Configuration["Logging:Enterprise:ApplicationName"] = appOptions.Name;

            // Map App.Identity -> Shared.Security (For validating incoming tokens)
            builder.Configuration["AzureAd:Domain"] = appOptions.Domain; 
            builder.Configuration["AzureAd:TenantId"] = appOptions.TenantId;
            builder.Configuration["AzureAd:ClientId"] = appOptions.ClientId;
            // (Instance/Domain are usually static or standard)

            // Map App.Identity -> Shared.Documentation (For Swagger "Authorize" button)
            builder.Configuration["AzureAd:Instance"] = "https://login.microsoftonline.com/";
            builder.Configuration["AzureAd:Domain"] = appOptions.Domain;
            builder.Configuration["Documentation:Title"] = appOptions.Name;
            builder.Configuration["Documentation:Version"] = appOptions.Version;
            builder.Configuration["Documentation:TenantId"] = appOptions.TenantId;
            builder.Configuration["Documentation:ClientId"] = appOptions.ClientId;
            // Default Scope: Access as User
            builder.Configuration["Documentation:Scopes:0"] = $"api://{appOptions.ClientId}/access_as_user";

            // =========================================================
            // REGISTER MIDDLEWARE
            // =========================================================
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<EnterpriseExceptionHandler>();

            // =========================================================
            // REGISTER CORS
            // =========================================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CustomCorsPolicy", policy =>
                {
                    if (appOptions.AllowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(appOptions.AllowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            // =========================================================
            // REGISTER Not Allowed Default
            // =========================================================
            builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

            // =========================================================
            // REGISTER PILLARS
            // =========================================================
            builder.Services.AddEnterpriseSerialization();
            builder.Services.AddSingleton(Options.Create(SystemTextJsonSerializer.DefaultOptions));
            builder.Services.AddEnterpriseLogging(builder.Configuration);
            builder.Services.AddAzureObservability(builder.Configuration);
            builder.Services.AddEntraIdWithRoleAugmentation<TUser>(builder.Configuration);
            builder.Services.AddEnterpriseDocumentation(builder.Configuration, apiAssembly);
            builder.Services.AddEnterpriseValidation(builder.Configuration, apiAssembly);
            builder.Services.AddCachingInfrastructure(builder.Configuration);

            // Note: We do NOT auto-register Data or ExternalIntegration here.
            // Those are heavy dependencies that might not exist in every service.
            // They should be added explicitly in Program.cs if needed.

            // Standard Health Checks
            builder.Services.AddHealthChecks();

            return builder;
        }
    }
}
