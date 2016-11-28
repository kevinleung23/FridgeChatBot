using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FridgeChatBot.DeserializationJson
{
    [JsonObject]
    public class JsonRecipe
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string title { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string image { get; set; }

        [JsonProperty(PropertyName = "imageType")]
        public string imageType { get; set; }

        [JsonProperty(PropertyName = "usedIngredientCount")]
        public int usedIngredientCount { get; set; }

        [JsonProperty(PropertyName = "missedIngredientCount")]
        public int missedIngredientCount { get; set; }

        [JsonProperty(PropertyName = "likes")]
        public int likes { get; set; }

        public List<String> recipeObject = new List<String>();
    }

    [JsonObject]
    public class JsonInstructions
    {

    }
}