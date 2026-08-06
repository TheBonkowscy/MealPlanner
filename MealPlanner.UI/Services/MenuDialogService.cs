using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MealPlanner.UI.Services;

public class MenuDialogService
{
    private readonly IDialogService _dialogService;
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _navigation;

    public MenuDialogService(IDialogService dialogService, ISnackbar snackbar, NavigationManager navigation)
    {
        _dialogService = dialogService;
        _snackbar = snackbar;
        _navigation = navigation;
    }

    public async Task ConfirmAndDeleteAsync(
        DateOnly date, 
        Func<DateOnly, CancellationToken, Task<bool>> deleteAction, 
        CancellationToken cancellationToken = default)
    {
        var result = await _dialogService.ShowMessageBoxAsync(
            title: "Potwierdzenie usunięcia",
            message: $"Czy na pewno chcesz usunąć menu na dzień {date:dd.MM.yyyy}?",
            yesText: "Usuń",
            cancelText: "Anuluj",
            options: new DialogOptions { CloseOnEscapeKey = true }
        );

        if (result == true)
        {
            var deleted = await deleteAction(date, cancellationToken);
            if (deleted)
            {
                _snackbar.Add("Menu zostało pomyślnie usunięte.", Severity.Success);
                _navigation.NavigateTo("/");
            }
            else
            {
                _snackbar.Add("Wystąpił błąd podczas usuwania menu. Spróbuj ponownie później.", Severity.Error);
            }
        }
    }
}