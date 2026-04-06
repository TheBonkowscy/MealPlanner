using MealPlanner.API;
using MealPlanner.API.Menus;
using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<DatabaseOptions>().Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.AddDbContext<MealPlannerDbContext>((services, ctx) =>
{
    var config = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
    {
        Host = config.Host,
        Database = config.Database,
        Username = config.Username,
        Password = config.Password,
        Port = config.Port
    };
    ctx.UseNpgsql(connectionStringBuilder.ConnectionString);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

await builder.Services.RegisterMenuServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Meal Planner API v1");
    });
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.MapControllers();

app.Run();