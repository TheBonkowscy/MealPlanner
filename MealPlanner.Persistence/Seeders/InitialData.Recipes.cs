using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Persistence.Seeders;

internal static partial class InitialData
{
    public static Recipe[] Recipes(Ingredient[] ingredients)
    {
        var bacon = ingredients.Single(i => i.Name == "Boczek w plastrach Sokołów");
        var addBacon = AddIngredientAction.Create(bacon, 1.0m, MeasureUnit.Package);
        
        var onion = ingredients.Single(i => i.Name == "Cebula czerwona, średnia");
        var addOnion = AddIngredientAction.Create(onion, 1.0m, MeasureUnit.Piece);
        
        var jalapeno = ingredients.Single(i => i.Name == "Jalapeno, świeże");
        var addJalapeno = AddIngredientAction.Create(jalapeno, 1.0m, MeasureUnit.Piece);
        
        var garlic = ingredients.Single(i => i.Name == "Ząbki czosnku");
        var addGarlic = AddIngredientAction.Create(garlic, 1.0m, MeasureUnit.Piece);
        
        var olives = ingredients.Single(i => i.Name == "Oliwki zielone");
        var addOlives = AddIngredientAction.Create(olives, 10.0m, MeasureUnit.Gram);
        
        var chicken = ingredients.Single(i => i.Name == "Pierś z kurczaka");
        var addChicken = AddIngredientAction.Create(chicken, 250.0m, MeasureUnit.Gram);
        
        var mushrooms = ingredients.Single(i => i.Name == "Pieczarki");
        var addMushrooms = AddIngredientAction.Create(mushrooms, 3.0m, MeasureUnit.Piece);

        List<RecipeStep> pizza1Steps =
        [
            RecipeStep.Create(1, "Przygotuj ciasto."),
            RecipeStep.Create(2, "Boczek, czerwoną cebulę i jalapeno pokrój na mniejsze kawałki."),
            RecipeStep.Create(3, "Przygotuj sos do pizzy."),
            RecipeStep.Create(4, "Zmontuj pizzę."),
            RecipeStep.Create(5, "Piecz 5 minut w 300 stopniach lub do całkowitego zwęglenia pokarmu."),
        ];
        var pizza1 = Recipe.Create("Pizza z boczkiem, czerwoną cebulą i jalapeno", [addBacon, addOnion, addJalapeno], pizza1Steps);
        
        List<RecipeStep> pizza2Steps =
        [
            RecipeStep.Create(1, "Przygotuj ciasto."),
            RecipeStep.Create(2, "Boczek, czosnek i oliwki pokrój na mniejsze kawałki."),
            RecipeStep.Create(3, "Przygotuj sos do pizzy."),
            RecipeStep.Create(4, "Na tarce zetrzyj żółty ser."),
            RecipeStep.Create(5, "Zmontuj pizzę."),
            RecipeStep.Create(6, "Piecz 5 minut w 300 stopniach lub do całkowitego zwęglenia pokarmu."),
        ];
        var pizza2 = Recipe.Create("Pizza z boczkiem, czosnkiem i oliwkami", [addBacon, addGarlic, addOlives], pizza2Steps);
        
        List<RecipeStep> pizza3Steps =
        [
            RecipeStep.Create(1, "Przygotuj ciasto."),
            RecipeStep.Create(2, "Kurczaka przysmaż na patelni w ulubionej przyprawie. Pieczarki i cebulę pokrój na mniejsze kawałki."),
            RecipeStep.Create(3, "Przygotuj sos do pizzy."),
            RecipeStep.Create(4, "Na tarce zetrzyj żółty ser."),
            RecipeStep.Create(5, "Zmontuj pizzę."),
            RecipeStep.Create(6, "Piecz 5 minut w 300 stopniach lub do całkowitego zwęglenia pokarmu."),
            RecipeStep.Create(7, "Podawać z oliwą z oliwek."),
        ];
        var pizza3 = Recipe.Create("Pizza z kurczakiem, pieczarkami i cebulą", [addChicken, addMushrooms, addOnion], pizza3Steps);
        
        return [pizza1, pizza2, pizza3];
    }
}