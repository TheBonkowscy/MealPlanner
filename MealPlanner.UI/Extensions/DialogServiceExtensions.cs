using MealPlanner.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MealPlanner.UI.Extensions;

public static class DialogServiceExtensions
{
    public static async Task ConfirmAndDeleteMenuAsync(
        this IDialogService dialogService,
        DateOnly date,
        Func<DateOnly, CancellationToken, Task> deleteAction,
        ISnackbar snackbar,
        NavigationManager navigation,
        CancellationToken cancellationToken = default)
    {
        if (date.IsInPast())
        {
            snackbar.Add("Nie można usunąć menu z przeszłych dni.", Severity.Error);
            return;
        }
        
        var result = await dialogService.ShowMessageBoxAsync(
            title: "Potwierdzenie usunięcia",
            message: $"Czy na pewno chcesz usunąć menu na dzień {date:dd.MM.yyyy}?",
            yesText: "Usuń",
            cancelText: "Anuluj",
            options: new DialogOptions { CloseOnEscapeKey = true }
        );

        if (result == true)
        {
            await deleteAction(date, cancellationToken);
            
            snackbar.Add("Menu zostało pomyślnie usunięte.", Severity.Success);
            navigation.NavigateTo(RoutingConstants.Menus.CreateForDate(date));
        }
    }
}