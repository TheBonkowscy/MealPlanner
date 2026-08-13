using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace MealPlanner.API;

public static class Extensions
{
    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseLocalizationMiddleware()
        {
            var supportedCultures = new[]
            {
                new CultureInfo("pl-PL")
            };

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("pl-PL"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            return app.UseRequestLocalization(localizationOptions);
        }
    }
}