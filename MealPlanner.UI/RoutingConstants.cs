namespace MealPlanner.UI;

public static class RoutingConstants
{
    public static class Menus
    {
        private const string BasePath = "menus";
        public const string Details = BasePath + "/day/";
        public const string Edit = BasePath + "/edit/";
        public const string Create = BasePath + "/create/";

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
            public const string List = BasePath + BaseRecipePath;
            public const string Create = BasePath + BaseRecipePath + "/create";
            
            public static bool IsList(string relativePath) => relativePath == List;
            public static bool IsCreate(string relativePath) => relativePath == Create;
        }
    }
}