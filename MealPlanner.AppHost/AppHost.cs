using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<MealPlanner_API>("meal-planner-api");

builder.AddProject<MealPlanner_UI>("meal-planner-ui")
    .WaitForStart(api);

builder.Build().Run();