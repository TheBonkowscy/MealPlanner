using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain;

public class Meal
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }

    private List<UsedIngredient> _ingredients = [];
    
    public IReadOnlyList<UsedIngredient> Ingredients
    {
        get => _ingredients;
        private set => _ingredients = [.. value];
    }
    
    private List<RecipeStep> _recipeSteps = [];

    public IReadOnlyList<RecipeStep> RecipeSteps
    {
        get => _recipeSteps;
        private set => _recipeSteps = [.. value];
    }

    private Meal()
    {
        // For EF Core
    }
    
    private Meal(string name, List<UsedIngredient> ingredients)
    {
        Name = name;
        Ingredients = ingredients;
    }

    private Meal(string name) : this(name, [])
    {
        // TODO: remove when meal editor is updated to require at least one ingredient
    }

    public static Meal Create(string name)
    {
        ValidateNameAndThrow(name);

        return new Meal(name);
    }
    
    public static Meal Create(string name, List<AddIngredientAction> ingredientsToAdd, List<RecipeStep> recipeSteps)
    {
        ValidateNameAndThrow(name);
        ValidateIngredientsAndThrow(ingredientsToAdd);
        ValidateRecipeStepsAndThrow(recipeSteps);
        
        var meal = new Meal(name);
        var mappedIngredients = ingredientsToAdd.Select(ingredient => UsedIngredient.Create(meal, ingredient)).ToList();
        mappedIngredients.ForEach(meal._ingredients.Add);
        
        meal._recipeSteps = recipeSteps;
        
        return meal;
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
}