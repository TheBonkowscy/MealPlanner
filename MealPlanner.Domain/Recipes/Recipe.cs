using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Recipes;

public class Recipe
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }
    
    public int Servings { get; set; }

    private List<UsedIngredient> _ingredients = [];
    
    public IReadOnlyList<UsedIngredient> Ingredients
    {
        get => _ingredients;
        private set => _ingredients = [.. value];
    }
    
    private List<RecipeStep> _steps = [];

    public IReadOnlyList<RecipeStep> Steps
    {
        get => [.. _steps.OrderBy(x => x.Order)];
        private set => _steps = [.. value];
    }

    private Recipe()
    {
        // For EF Core
    }
    
    private Recipe(string name, int servings, List<UsedIngredient> ingredients)
    {
        Name = name;
        Servings = servings;
        Ingredients = ingredients;
    }

    private Recipe(string name, int servings) : this(name, servings, [])
    {
        // This is a helper for the factory method below.
        // It allows you to create a recipe with no ingredients and add them later.
    }
    
    public static Result<Recipe> Create(string name, int servings, List<AddIngredientAction> ingredientsToAdd, List<RecipeStep> recipeSteps)
    {
        var errors = new List<Error>();
        errors.AddRule(ValidateName(name), DomainErrors.Recipe.InvalidName);
        errors.AddRule(ValidateServings(servings), DomainErrors.Recipe.InvalidServings);
        errors.AddRule(ValidateIngredients(ingredientsToAdd), DomainErrors.Recipe.InvalidIngredients);
        errors.AddRule(ValidateRecipeSteps(recipeSteps), DomainErrors.Recipe.InvalidSteps);

        if (errors.Count > 0)
        {
            return Result.Failure<Recipe>(errors);
        }
        
        var recipe = new Recipe(name, servings);
        errors.AddRange(recipe.AddIngredients(ingredientsToAdd).Errors);
        recipe._steps = recipeSteps;
        recipe.ReindexSteps();
        
        return Result.Success(recipe);
    }

    private Result AddIngredients(List<AddIngredientAction> ingredientsToAdd)
    {
        var usedIngredients = ingredientsToAdd.Select(ingredient => UsedIngredient.Create(this, ingredient)).ToList();
        
        if (usedIngredients.Any(x => x.IsFailure))
        {
            var allErrors = usedIngredients.SelectMany(x => x.Errors).ToList();
            return Result.Failure(allErrors);
        }
        
        _ingredients.AddRange(usedIngredients.Select(x => x.Value));
        return Result.Success();
    }

    private static bool ValidateName(string name) => !string.IsNullOrWhiteSpace(name);

    private static bool ValidateServings(int servings) => servings > 0;

    private static bool ValidateIngredients(List<AddIngredientAction> ingredients) => ingredients.Count != 0 && ingredients.All(ingredient => ingredient.Quantity > 0);

    private static bool ValidateRecipeSteps(List<RecipeStep> recipeSteps)
    {
        if (recipeSteps.Count == 0)
        {
            return false;
        }
        
        var uniqueOrdersCount = recipeSteps.Select(x => x.Order).Distinct().Count();
        return uniqueOrdersCount == recipeSteps.Count;
    }

    public UsedIngredient? GetIngredient(int ingredientId, MeasureUnit requestUnit) =>
        Ingredients.FirstOrDefault(x => x.IngredientId == ingredientId && x.Unit == requestUnit);

    public void RemoveIngredient(UsedIngredient ingredient) => _ingredients.Remove(ingredient);

    public Result AddIngredient(AddIngredientAction addIngredient) => AddIngredients([addIngredient]);

    public Result UpdateName(string name)
    {
        /*
         * errors.AddRule(ValidateName(name), DomainErrors.Recipe.InvalidName);
        errors.AddRule(ValidateServings(servings), DomainErrors.Recipe.InvalidServings);
        errors.AddRule(ValidateIngredients(ingredientsToAdd), DomainErrors.Recipe.InvalidIngredients);
        errors.AddRule(ValidateRecipeSteps(recipeSteps), DomainErrors.Recipe.InvalidSteps);

         */
        if (!ValidateName(name))
        {
            return Result.Failure(DomainErrors.Recipe.InvalidName);
        }
        Name = name;
        return Result.Success();
    }

    public Result UpdateServings(int servings)
    {
        if (!ValidateServings(servings))
        {
            return Result.Failure(DomainErrors.Recipe.InvalidServings);
        }
        Servings = servings;
        return Result.Success();
    }

    public Result UpdateStep(int stepId, int newOrder, string newInstructions)
    {
        var updatedStep = _steps.FirstOrDefault(x => x.Id == stepId);
        if (updatedStep is null)
        {
            return Result.Failure(DomainErrors.RecipeStep.NotFound);
        }
        
        _steps = [.. _steps.OrderBy(x => x.Order)];

        var instructionsUpdated = updatedStep.UpdateInstructions(newInstructions);
        if (instructionsUpdated.IsFailure)
        {
            return instructionsUpdated;
        }

        _steps.Remove(updatedStep);
        var clampedOrder = Math.Clamp(newOrder, 1, _steps.Count + 1);
        _steps.Insert(clampedOrder - 1, updatedStep);

        ReindexSteps();
        return Result.Success();
    }

    public Result AddStep(int targetOrder, string instructions)
    {
        _steps = [.. _steps.OrderBy(x => x.Order)];
        var newStep = RecipeStep.Create(targetOrder, instructions);
        if (newStep.IsFailure)
        {
            return newStep;
        }
        
        var clampedOrder = Math.Clamp(targetOrder, 1, _steps.Count + 1);
        
        _steps.Insert(clampedOrder - 1, newStep.Value);
        
        ReindexSteps();
        
        return Result.Success();
    }
    
    public void RemoveStep(RecipeStep step)
    {
        _steps = [.. _steps.OrderBy(x => x.Order)];
        if (_steps.Remove(step))
        {
            ReindexSteps();
        }
    }
    
    private void ReindexSteps()
    {
        for (int i = 0; i < _steps.Count; i++)
        {
            _steps[i].UpdateOrder(i + 1);
        }
    }
}