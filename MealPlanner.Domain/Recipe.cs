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
        get => _steps;
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
        var mappedIngredients = ingredientsToAdd.Select(ingredient => UsedIngredient.Create(recipe, ingredient)).ToList();
        mappedIngredients.ForEach(recipe._ingredients.Add);
        
        recipe._steps = recipeSteps;
        
        return recipe;
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
    }

    internal void UpdateIngredients(List<UsedIngredient> recipeIngredients)
    {
        ValidateIngredientsAndThrow(recipeIngredients);
        Ingredients = recipeIngredients;
    }

    private static void ValidateIngredientsAndThrow(List<UsedIngredient> ingredients)
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

    internal void UpdateSteps(List<RecipeStep> steps)
    {
        ValidateRecipeStepsAndThrow(steps);
        Steps = steps;
    }

    public UsedIngredient? GetIngredient(int ingredientId, MeasureUnit requestUnit) =>
        Ingredients.FirstOrDefault(x => x.IngredientId == ingredientId && x.Unit == requestUnit);

    public void RemoveIngredient(UsedIngredient ingredient) => _ingredients.Remove(ingredient);
}