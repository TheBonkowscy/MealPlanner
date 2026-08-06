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
}