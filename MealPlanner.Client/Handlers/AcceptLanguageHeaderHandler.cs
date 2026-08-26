using System.Globalization;
using System.Net.Http.Headers;

namespace MealPlanner.Client.Handlers;

public class AcceptLanguageHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));

        return await base.SendAsync(request, cancellationToken);
    }
}