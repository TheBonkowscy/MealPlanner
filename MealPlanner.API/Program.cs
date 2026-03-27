using MealPlanner.API.Menus;
using MealPlanner.Persistence;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<MealPlannerDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddSingleton<InMemoryDatabase>();
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

app.MapControllers();

app.Run();