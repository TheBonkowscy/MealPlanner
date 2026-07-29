using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add database
var userName = builder.AddParameter("pgUserName", "postgres", secret: true);
var password = builder.AddParameter("pgPassword", "12345", secret: true);
var postgres = builder.AddPostgres("postgres-db", userName, password)
    .WithHostPort(5432)
    .WithDataVolume("meal-planner-data", isReadOnly: false);
var postgresdb = postgres.AddDatabase("MealPlanner");

var api = builder.AddProject<MealPlanner_API>("meal-planner-api")
    .WithHttpsEndpoint(7094)
    .WithHttpEndpoint(5129)
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<MealPlanner_UI>("meal-planner-ui")
    .WithHttpsEndpoint(7098)
    .WithHttpEndpoint(5170)
    .WaitForStart(api);

builder.Build().Run();