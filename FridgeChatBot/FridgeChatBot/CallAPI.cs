using FridgeChatBot.DeserializationJson;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using System.Web.Configuration;
using unirest_net.http;

namespace FridgeChatBot
{
    public class CallAPI
    {

        public CallAPI()
        {

        }

        public JsonRecipe GetRecipe(string ingredients)
        {
            // create url by passing in parameters
            string url = "https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?";

            string param_fillIngredients = "false";
            string param_ingredients = ingredients;
            string param_limitLicense = "false";
            string param_number = "1";
            string param_ranking = "1";

            // Request query string.
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["fillIngredients"] = param_fillIngredients;
            queryString["ingredients"] = ingredients;
            queryString["limitLicense"] = param_limitLicense;
            queryString["number"] = param_number;
            queryString["ranking"] = param_ranking;
            var uri = url + queryString;

            // "https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?fillIngredients=false&ingredients=apples%2Cflour%2Csugar&limitLicense=false&number=5&ranking=1"
            // These code snippets use an open-source library.
            HttpResponse<MemoryStream> responseAPI = Unirest.get(uri)
            .header("X-Mashape-Key", WebConfigurationManager.AppSettings["Mashape_Key_Kevin"])
            .header("Accept", "application/json")
            .asJson<MemoryStream>();

            // System.IO.MemoryStream encoding to string
            StreamReader reader = new StreamReader(responseAPI.Body);
            string json = reader.ReadToEnd();

            // Remove extra brackets on JSON string
            // Unsure if API is returning extra brackets or if StreamReader is adding
            json = json.TrimStart('[');
            json = json.TrimEnd(']');

            // Deserialize json string into an object instance
            JsonRecipe recipeResult = new JsonRecipe();
            JsonConvert.PopulateObject(json, recipeResult);

            return recipeResult;
        }

        public JsonLink GetLink(string id)
        {
            // Second API Call to get recipe links
            string url = "https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/";

            // Request query string.
            var queryString2 = HttpUtility.ParseQueryString(string.Empty);
            url += id;
            url += "/information?includeNutrition=false";

            HttpResponse<MemoryStream> responseAPI2 = Unirest.get(url)
            .header("X-Mashape-Key", WebConfigurationManager.AppSettings["Mashape_Key_Gabby"])
            .header("Accept", "application/json")
            .asJson<MemoryStream>();

            // System.IO.MemoryStream encoding to string
            StreamReader reader2 = new StreamReader(responseAPI2.Body);
            string json2 = reader2.ReadToEnd();

            // Remove extra brackets on JSON string
            // Unsure if API is returning extra brackets or if StreamReader is adding
            json2 = json2.TrimStart('[');
            json2 = json2.TrimEnd(']');

            // Deserialize json string into an object instance
            JsonLink recipeLink = new JsonLink();
            JsonConvert.PopulateObject(json2, recipeLink);

            return recipeLink;
        }

        public bool isLimitHit()
        {
            // Call API and parse response header for remaining requests left
            // create url by passing in parameters
            string url_Limit = "https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/food/ingredients/autocomplete";

            HttpResponse<MemoryStream> responseAPI_Limit = Unirest.get(url_Limit)
            .header("X-Mashape-Key", WebConfigurationManager.AppSettings["Mashape_Key_Kevin"])
            .header("Accept", "application/json")
            .asJson<MemoryStream>();
            // Parse Response Header string for "x-RateLimit-classifications-Remaining"
            string limit = JsonConvert.SerializeObject(responseAPI_Limit.Headers);
            string toFind = "x-RateLimit-requests-Remaining";
            int indexOfLimit = limit.IndexOf(toFind) + toFind.Length + 4;
            string limitValue = limit.Substring(indexOfLimit, 2);

            if (limitValue == "0")
            {
                // Limit reached! -- do NOT call API
                // Run out of calls.. try again tomorrow... order out?..pizza #
                return true;
            }
            else
            {
                // API Limit has not been reached.
                // Free to call API !
                return false;
            }
        }

    }
}