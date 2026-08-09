using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain;

public class Recipe
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }

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
    
    private Recipe(string name, List<UsedIngredient> ingredients)
    {
        Name = name;
        Ingredients = ingredients;
    }

    private Recipe(string name) : this(name, [])
    {
        // TODO: remove when meal editor is updated to require at least one ingredient
    }

    public static Recipe Create(string name)
    {
        ValidateNameAndThrow(name);

        return new Recipe(name);
    }
    
    public static Recipe Create(string name, List<AddIngredientAction> ingredientsToAdd, List<RecipeStep> recipeSteps)
    {
        ValidateNameAndThrow(name);
        ValidateIngredientsAndThrow(ingredientsToAdd);
        ValidateRecipeStepsAndThrow(recipeSteps);
        
        var recipe = new Recipe(name);
        var mappedIngredients = ingredientsToAdd.Select(ingredient => UsedIngredient.Create(recipe, ingredient)).ToList();
        mappedIngredients.ForEach(recipe._ingredients.Add);
        
        recipe._steps = recipeSteps;
        
        return recipe;
    }

    private static void ValidateNameAndThrow(string meal)
    {
        if (string.IsNullOrWhiteSpace(meal))
        {
            throw new ArgumentNullException(null, "Please specify a name of the meal");
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
}