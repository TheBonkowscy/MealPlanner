using System.Net.Http.Json;
using MealPlanner.Client.Models;

namespace MealPlanner.Client.Extensions;

public static class HttpResponseExtensions
{
    public static async Task<ApiResult<T>> ToApiResult<T>(
        this HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);

            return ApiResult<T>.Success(value!);
        }

        var problem = await response.Content
            .ReadFromJsonAsync<ApiProblemDetails>(cancellationToken: ct);

        var error = problem?.Errors.FirstOrDefault();

        return ApiResult<T>.Failure(
            new ApiError(
                problem?.Status ?? (int)response.StatusCode,
                error?.Code ?? "unknown",
                error?.Message ?? "Unknown error",
                error?.Metadata ?? []));
    }
}