using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain;

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
    
    public static Recipe Create(string name, int servings, List<AddIngredientAction> ingredientsToAdd, List<RecipeStep> recipeSteps)
    {
        ValidateNameAndThrow(name);
        ValidateServingsAndThrow(servings);
        ValidateIngredientsAndThrow(ingredientsToAdd);
        ValidateRecipeStepsAndThrow(recipeSteps);
        
        var recipe = new Recipe(name, servings);
        recipe.AddIngredients(ingredientsToAdd);
        recipe._steps = recipeSteps;
        recipe.ReindexSteps(); // Zapewnia ciągłość 1..N od samego początku
        
        return recipe;
    }

    private void AddIngredients(List<AddIngredientAction> ingredientsToAdd)
    {
        var mappedIngredients = ingredientsToAdd.Select(ingredient => UsedIngredient.Create(this, ingredient)).ToList();
        mappedIngredients.ForEach(_ingredients.Add);
    }

    private static void ValidateNameAndThrow(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(null, "Please specify a name of the recipe");
        }
    }

    private static void ValidateServingsAndThrow(int servings)
    {
        if (servings < 1)
        {
            throw new ArgumentOutOfRangeException(null, "Recipe must yield at least one serving");
        }
    }

    private static void ValidateIngredientsAndThrow(List<AddIngredientAction> ingredients)
    {
        if (ingredients.Count == 0)
        {
            throw new ArgumentNullException(null, "At least one ingredient must be specified");
        }
        
        // TODO: is this required?
        ingredients.ForEach(ingredient =>
        {
            if (ingredient.Quantity <= 0)
            {
                throw new ArgumentNullException(null, "Ingredient quantity must be greater than zero");
            }
        });
    }
    
    private static void ValidateRecipeStepsAndThrow(List<RecipeStep> recipeSteps)
    {
        if (recipeSteps.Count == 0)
        {
            throw new ArgumentNullException(null, "At least one recipe step must be specified");
        }


        var uniqueOrdersCount = recipeSteps.Select(x => x.Order).Distinct().Count();
        if (uniqueOrdersCount != recipeSteps.Count)
        {
            throw new InvalidOperationException("Recipe steps must have unique orders");
        }
    }

    public UsedIngredient? GetIngredient(int ingredientId, MeasureUnit requestUnit) =>
        Ingredients.FirstOrDefault(x => x.IngredientId == ingredientId && x.Unit == requestUnit);

    public void RemoveIngredient(UsedIngredient ingredient) => _ingredients.Remove(ingredient);

    public void AddIngredient(AddIngredientAction addIngredient) => AddIngredients([addIngredient]);

    public void UpdateName(string name)
    {
        ValidateNameAndThrow(name);
        Name = name;
    }

    public void UpdateServings(int servings)
    {
        ValidateServingsAndThrow(servings);
        Servings = servings;
    }

    public void UpdateStep(int stepId, int newOrder, string newInstructions)
    {
        var updatedStep = _steps.FirstOrDefault(x => x.Id == stepId);
        if (updatedStep is null)
        {
            throw new InvalidOperationException("Recipe step could not be found");
        }

        updatedStep.UpdateInstructions(newInstructions);

        _steps.Remove(updatedStep);
        var clampedOrder = Math.Clamp(newOrder, 1, _steps.Count + 1);
        _steps.Insert(clampedOrder - 1, updatedStep);

        ReindexSteps();
    }

    public void AddStep(int targetOrder, string instructions)
    {
        var newStep = RecipeStep.Create(targetOrder, instructions);
        
        var clampedOrder = Math.Clamp(targetOrder, 1, _steps.Count + 1);
        
        _steps.Insert(clampedOrder - 1, newStep);
        
        ReindexSteps();
    }
    
    public void RemoveStep(RecipeStep step)
    {
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