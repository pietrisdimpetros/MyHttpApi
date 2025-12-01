using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Shared.Documentation.Configuration;

namespace Shared.Documentation.Extensions
{
    public static class WebApplicationExtensions
    {
        public static IApplicationBuilder UseEnterpriseDocumentation(this WebApplication app)
        {
            var options = app.Configuration.GetSection(DocumentationOptions.SectionName).Get<DocumentationOptions>();
            if (options is null) return app;

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{options.Version}/swagger.json", options.Title);
                c.RoutePrefix = "swagger";

                // OAUTH2 UI CONFIGURATION
                // This pre-fills the ClientId in the popup
                c.OAuthClientId(options.ClientId);
                c.OAuthUsePkce(); // Vital for Azure AD v2.0
                c.OAuthScopeSeparator(" ");
            });

            return app;
        }
    }
}
