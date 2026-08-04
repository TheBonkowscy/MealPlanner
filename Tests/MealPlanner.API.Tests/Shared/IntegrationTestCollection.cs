using Xunit;

namespace MealPlanner.API.Tests.Shared;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<MealPlannerWebApplicationFactory>
{
}
