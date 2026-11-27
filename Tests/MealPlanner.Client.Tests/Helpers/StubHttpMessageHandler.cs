using System.Net;

namespace MealPlanner.Client.Tests.Helpers;

internal sealed class StubHttpMessageHandler(HttpStatusCode status, HttpContent? content = null) : HttpMessageHandler
{
    public HttpRequestMessage? Request { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = content ?? new StringContent(string.Empty)
        });
    }
}
