using System.Globalization;
using MealPlanner.Client;
using MealPlanner.Client.Configuration;
using MealPlanner.Client.Handlers;
using MealPlanner.UI.Components;
using MealPlanner.UI.Services;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddOptions<MealPlannerConfigurationOptions>()
    .Bind(builder.Configuration.GetSection(MealPlannerConfigurationOptions.SectionName));
builder.Services.AddMealPlannerClient();

builder.Services.AddSingleton<IMenuStateService, MenuStateService>();
builder.Services.AddScoped<AppBarService>();
builder.Services.AddScoped<MenuDialogService>();
builder.Services.AddSingleton<MealMapper>();

// TODO: Shared localizations library?
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddTransient<AcceptLanguageHeaderHandler>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    const string defaultCulture = "en";
    string[] cultures = [defaultCulture, "pl"];
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);

    options.SupportedCultures = cultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = cultures.Select(c => new CultureInfo(c)).ToList();

    options.RequestCultureProviders =
    [
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

builder.Services.AddMudServices();

var app = builder.Build();
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();