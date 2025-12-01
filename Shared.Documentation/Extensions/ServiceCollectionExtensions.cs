using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Shared.Documentation.Configuration;
using System.Reflection;
namespace Shared.Documentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnterpriseDocumentation(
            this IServiceCollection services,
            IConfiguration configuration,
            Assembly? apiAssembly = null)
        {
            // 1. Bind Options
            services.AddOptions<DocumentationOptions>()
                .Bind(configuration.GetSection(DocumentationOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var options = configuration.GetSection(DocumentationOptions.SectionName).Get<DocumentationOptions>();
            if (options is null) return services;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(options.Version, new OpenApiInfo
                {
                    Title = options.Title,
                    Version = options.Version,
                    Description = options.Description
                });

                // 2. DEFINE OAUTH2 SCHEME (The Azure Way)
                var authUrl = new Uri($"https://login.microsoftonline.com/{options.TenantId}/oauth2/v2.0/authorize");
                var tokenUrl = new Uri($"https://login.microsoftonline.com/{options.TenantId}/oauth2/v2.0/token");

                // Define the dictionary of scopes once
                var scopes = options.Scopes.ToDictionary(k => k, v => "Access API as User");

                c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Azure AD Authorization Code Flow",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = authUrl,
                            TokenUrl = tokenUrl,
                            Scopes = scopes
                        }
                    }
                });

                // 3. REQUIRE OAUTH2 GLOBALLY
                // This puts the "Lock" icon on every endpoint
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "OAuth2"
                        }
                    },
                    options.Scopes // Require these scopes for access
                }
            });

                // 4. XML Comments
                if (options.EnableXmlComments && apiAssembly != null)
                {
                    var xmlFile = $"{apiAssembly.GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                }

                c.EnableAnnotations();
            });

            return services;
        }
    }
}
