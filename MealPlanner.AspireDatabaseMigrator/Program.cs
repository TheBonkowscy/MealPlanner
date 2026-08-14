using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Persistence;

var sw = Stopwatch.StartNew();

Console.WriteLine("Starting database migration...");
var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__MealPlanner");

var options = new DbContextOptionsBuilder<MealPlannerDbContext>()
    .UseNpgsql(connectionString)
    .Options;

Console.WriteLine("Obtained connection string, creating context...");
await using var db = new MealPlannerDbContext(options);

Console.WriteLine("Migrating database, please wait...");
await db.Database.MigrateAsync();

Console.WriteLine("Migration done in {0} ms", sw.ElapsedMilliseconds);