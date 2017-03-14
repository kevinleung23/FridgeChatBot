using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Web.Configuration;

namespace FridgeChatBot
{
    // Class source referenced from https://github.com/jennifermarsman/MicrosoftCareerBot
    // Credit: @Jennifermarsman
    public class LUISClient
    {
        public static async Task<Rootobject> ParseUserInput(string strInput)
        {
            string strEscaped = Uri.EscapeDataString(strInput);

            using (var client = new HttpClient())
            {
                // TODO: put URI in config file
                // TODO: insert your LUIS URL here
                string luisURL = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" +
                    WebConfigurationManager.AppSettings["MicrosoftLUISId"] +
                    "?subscription-key=" +
                    WebConfigurationManager.AppSettings["MicrosoftLUISKey"];
                
                string uri = luisURL + "&verbose=true&q=" + strEscaped;
                HttpResponseMessage msg = await client.GetAsync(uri);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _Data = JsonConvert.DeserializeObject<Rootobject>(jsonResponse);
                    return _Data;
                }
            }
            return null;
        }
    }

    public class Rootobject
    {
        public string query { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }

}
