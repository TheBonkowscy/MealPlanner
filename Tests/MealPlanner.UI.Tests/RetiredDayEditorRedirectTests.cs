using System.Reflection;
using AwesomeAssertions;
using MealPlanner.UI.Components.Pages.Menus;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Tests;

public class RetiredDayEditorRedirectTests
{
    private static readonly string[] RetiredRoutes =
    [
        "/menus/create/{Date:datetime}",
        "/menus/edit/{Date:datetime}"
    ];

    [Fact]
    public void Routes_CoverTheCreateAndEditPagesOfADay()
    {
        // Act
        var templates = typeof(RetiredDayEditorRedirect)
            .GetCustomAttributes<RouteAttribute>()
            .Select(x => x.Template);

        // Assert
        templates.Should().BeEquivalentTo(RetiredRoutes);
    }

    [Fact]
    public void RetiredRoutes_AreNotDeclaredByAnyOtherComponent()
    {
        // Act
        var others = typeof(RetiredDayEditorRedirect).Assembly
            .GetTypes()
            .Where(t => t != typeof(RetiredDayEditorRedirect)
                        && t.GetCustomAttributes<RouteAttribute>().Any(r => RetiredRoutes.Contains(r.Template)));

        // Assert
        others.Should().BeEmpty();
    }
}
