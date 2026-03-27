using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus.Create;
using MealPlanner.Shared.Menus.Requests;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuCreatorTests
{
    private const string MealName = "Fish and chips";

    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly MenuCreator _sut;

    private List<Menu> _menus = [];
    
    public MenuCreatorTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>())).Callback<Menu, CancellationToken>((menu, _) =>
        {
            RandomId.Set(menu);
            _menus.Add(menu);
        });
        
        _ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        
        _sut = new MenuCreator(_ctx.Object);
    }
    
    [Theory]
    [MemberData(nameof(ValidCreateRequests))]
    public async Task Create_CreatesSuccessfully_ReturnsId(CreateMenuRequest createMenuRequest)
    {
        // Act
        var result = await _sut.Create(createMenuRequest);
        
        // Assert
        result.Id.Should().NotBe(0);
    }

    [Fact]
    public async Task Create_ThrowsWhenMenuAlreadyPresentForSpecifiedDay()
    {
        // Arrange
        var tomorrow =  DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var request = new CreateMenuRequest(tomorrow, [MealName]);
        await _sut.Create(request);
        var conflictingRequest = new CreateMenuRequest(tomorrow, ["Pierogi"]);

        // Act
        var createWithConflict = async (CreateMenuRequest req) => await _sut.Create(req);
        
        // Assert
        await createWithConflict.Awaiting(x => x.Invoke(conflictingRequest))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"There is already a Menu defined for {conflictingRequest.Date}.");
    }

    public static TheoryData<CreateMenuRequest> ValidCreateRequests
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var data = new TheoryData<CreateMenuRequest>
            {
                new(today, [MealName]),
                new(today.AddDays(1), []),
                new(today.AddDays(2))
            };

            return data;
        }
    }
}