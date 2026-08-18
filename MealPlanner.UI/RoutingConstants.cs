namespace MealPlanner.UI;

public static class RoutingConstants
{
    public static class Menus
    {
        private const string BasePath = "menus";
        private const string Details = BasePath + "/day/";
        private const string Edit = BasePath + "/edit/";
        private const string Create = BasePath + "/create/";

        public static string DetailsForDate(DateOnly date) => $"/{Details}{date:yyyy-MM-dd}";

        public static string CreateForDate(DateOnly date) => $"/{Create}{date:yyyy-MM-dd}";

        public static string EditForDate(DateOnly date) => $"/{Edit}{date:yyyy-MM-dd}";
        
        public static string DeleteForDate(DateOnly date) => $"/{BasePath}{date:yyyy-MM-dd}";
    }
    
    public static class Editors
    {
        private const string BasePath = "editors";
        
        public static class Recipe
        {
            private const string BaseRecipePath = "/recipes";
            private const string BaseEditorPath = BasePath + BaseRecipePath;
            
            public const string List = BasePath + BaseRecipePath;
            public const string Create = BasePath + BaseRecipePath + "/create";
            public const string Edit = BasePath + BaseRecipePath + "/edit";
            
            public static bool IsList(string relativePath) => relativePath == List;
            public static bool IsCreate(string relativePath) => relativePath == Create;

            public static string EditFor(int recipeId) => Edit + "/" + recipeId;
        }
    }

    public static class Recipes
    {
        private const string BasePath = "recipes";
        
        public static string DetailsFor(int id) => $"/{BasePath}/{id}";
    }
}