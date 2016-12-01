using System;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using FridgeChatBot.DeserializationJson;
using Microsoft.Bot.Connector;
using System.Linq;

namespace FridgeChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // holds response to displayto user
                string answer = "I'm sorry. I didn't understand that. Try: \"What can we cook for dinner?\" or  \"What ingredients are we missing?\"";

                Rootobject luisObj = await LUISClient.ParseUserInput(activity.Text);
                if (luisObj.intents.Length > 0)
                {
                    switch (luisObj.intents[0].intent) // assumption: first intent is highest score
                    {
                        case "":
                            answer = "I'm sorry. I didn't understand that. You can add and remove ingredients from your list or try to find a recipe!";
                            break;

                        case "greeting":
                            answer = "Hello! Feeling hungry and adventurous? You can add and remove ingredients from your list or try to find a recipe! Ask for help and more examples!";
                            break;

                        case "help":
                            // populate the commands and examples
                            var commandList = "";
                            var commandHelp = new string[6];
                            commandHelp[0] = "Add ingredients: \"Add milk, butter and eggs to my list.\"\n";
                            commandHelp[1] = "Remove ingredients: \"Remove butter and eggs from my list.\"\n";
                            commandHelp[2] = "Show your ingredients list: \"Please show me my list.\"\n";
                            commandHelp[3] = "Start a new list: \"Start a new list.\" or \"Clear my list.\"\n";
                            commandHelp[4] = "Find a recipe: \"What can we cook tonight?\"\n";
                            commandHelp[5] = "Show your missing ingredients: \"What ingredients are we missing?\"\n";

                            // add the commands to the answer string to be displayed
                            for (int i = 0; i < commandHelp.Length; i++)
                            {
                                commandList += (i + 1) + ". " + commandHelp[i] + '\n';
                            }

                            answer = "Here are the actions you can do with examples: \n" + commandList;
                            break;

                        case "addList":
                            // add ingredient(s) to the list
                            // create array to hold entities
                            string[] items = new string[luisObj.entities.Length];

                            // parse the entities and pass in
                            for (int i = 0; i < luisObj.entities.Length; i++)
                            {
                                items[i] = luisObj.entities[i].entity;
                            }

                            // Store the values into a local text file
                            var IngredientsListAdd = new StateList();
                            IngredientsListAdd.AddIngredients(items);

                            answer = "Successfully added ingredients.";
                            break;

                        case "removeList":
                            // remove ingredient(s) to the list
                            // create array to hold entities
                            string[] removeItems = new string[luisObj.entities.Length];

                            // parse the entities and pass in
                            for (int i = 0; i < luisObj.entities.Length; i++)
                            {
                                removeItems[i] = luisObj.entities[i].entity;
                            }

                            // Remove the values from the local text file
                            var IngredientsListRemove = new StateList();
                            IngredientsListRemove.RemoveIngredients(removeItems);

                            answer = "Successfully removed ingredients.";
                            break;

                        case "newList":
                            // clear the current list to start new
                            var IngredientsListClear = new StateList();
                            IngredientsListClear.clearIngredients();

                            answer = "Successfully cleared the ingredients list. Add more ingredients back!";
                            break;

                        case "displayList":
                            // read current ingredients list
                            var IngredientsListRead = new StateList();
                            var list = IngredientsListRead.ReadIngredients();
                            var listItems = "";

                            // check if list is currently empty
                            if (list.Length == 0)
                            {
                                answer = "Your list is currently empty. Add more ingredients!";
                                break;
                            }

                            for (int i = 0; i < list.Length; i++)
                            {
                                listItems += (i + 1) + ". " + list[i] + '\n';
                            }

                            answer = "You currently have: \n" + listItems;
                            break;

                        case "findRecipe":
                            // read current ingredients list
                            var IngredientsListAPI = new StateList();
                            var arg = IngredientsListAPI.ReadIngredients();

                            // empty list check
                            if (arg.Length == 0)
                            {
                                answer = "It looks like your ingredients list is empty. Try adding some ingredients then searching for a recipe!";
                                break;
                            }

                            var API_arg = "I have";
                            for (int i = 0; i < arg.Length; i++)
                            {
                                if ((i + 1) == arg.Length)
                                {
                                    // last item in array
                                    API_arg += " " + arg[i];
                                }
                                else
                                {
                                    API_arg += " " + arg[i] + ",";
                                }
                            }

                            var caller = new CallAPI();

                            if (!caller.isLimitHit())
                            {
                                // Initialize recipeResult to hold json info
                                // Call first API to obtain recipe from ingredients
                                var recipeResult = new JsonRecipe();
                                recipeResult = caller.GetRecipe(API_arg);


                                // Initialize recipeResult to hold json info
                                // Call second API to obtain recipe link from recipeId
                                var recipeLink = new JsonLink();
                                recipeLink = caller.GetLink(recipeResult.id.ToString());


                                // reply back to user with the recipe options
                                Activity replyMessage = activity;
                                Activity replyToConversation = replyMessage.CreateReply("May I interest you in..");
                                replyToConversation.Recipient = replyMessage.From;
                                replyToConversation.Type = "message";
                                replyToConversation.Attachments = new List<Attachment>();
                                List<CardImage> cardImages = new List<CardImage>();
                                cardImages.Add(new CardImage(url: recipeResult.image));
                                List<CardAction> cardButtons = new List<CardAction>();
                                CardAction plButton = new CardAction()
                                {
                                    Value = recipeLink.sourceUrl,
                                    Type = "openUrl",
                                    Title = "Let's Get Cooking!"
                                };
                                cardButtons.Add(plButton);
                                HeroCard plCard = new HeroCard()
                                {
                                    Title = recipeResult.title,
                                    Subtitle = "Recommended by " + recipeResult.likes.ToString() + " others!",
                                    Images = cardImages,
                                    Buttons = cardButtons
                                };
                                Attachment plAttachment = plCard.ToAttachment();
                                replyToConversation.Attachments.Add(plAttachment);
                                var replyCard = await connector.Conversations.SendToConversationAsync(replyToConversation);

                                // Load our current ingredients list we we can cross check and mark what we already have.
                                var IngredientsListGrocery = new StateList();
                                var inventoryList = IngredientsListGrocery.ReadIngredients();

                                // GetIngredients - complete ingredients from the API response
                                // Load the list into a sorted dictionary with the aisle for user's convenience
                                // Before we load into the dictionary, cross-check to see if we already have the item
                                int counter = 0;
                                var dict = new SortedDictionary<string, string>();
                                foreach (var item in recipeLink.extendedIngredients)
                                {
                                    for (int searchIndex = 0; searchIndex < inventoryList.Length; searchIndex++)
                                    {
                                        if (inventoryList[searchIndex] == item.name)
                                        {
                                            item.aisle = "At Home!";
                                        }
                                    }
                                    dict.Add(item.name, item.aisle);
                                    counter++;
                                }

                                // create two arrays, keys = ingredeints, value = aisle.
                                var keyArray = new string[counter];
                                var valueArray = new string[counter];
                                int index = 0;
                                foreach (KeyValuePair<string, string> item in dict.OrderBy(key => key.Value))
                                {
                                    keyArray[index] = item.Key;
                                    valueArray[index] = item.Value;
                                    index++;
                                }
                                
                                // Save userData as properties in user session
                                userData.SetProperty<string[]>("foodList", keyArray);
                                userData.SetProperty<string[]>("aisleList", valueArray);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            }
                            answer = "It looks like you may be missing some ingredients! Try: \"Send me a grocery list!\"";
                            break;

                        case "getShoppingList":
                            // check if we have found a recipe first
                            if (userData.GetProperty<string[]>("foodList") == null)
                            {
                                answer = "We need to find you a recipe first! Try: \"Find me a recipe!\"";
                                break;
                            }

                            // Retrieve ShoppingList from state              
                            var foodList = userData.GetProperty<string[]>("foodList");
                            var aisleList = userData.GetProperty<string[]>("aisleList");
                            var toBuy = "";

                            // print the cross-checked grocery list for the user
                            for (int i = 0; i < foodList.Length; i++)
                            {
                                toBuy += (i + 1) + ". " + foodList[i] + " (" + aisleList[i] + ")" + '\n';
                            }
                            answer = "Shopping List: \n" + toBuy;
                            break;
                    }
                }
                else
                {
                    //run out of calls.. try again tomorrow... order out?..pizza #
                    answer = "Sorry! Kitchen is closed.. Time to order out! (425) 453-7200 [Domino's Pizza Bellevue Square]";
                }

                // Return response back to the user
                Activity reply = activity.CreateReply(answer);
                await connector.Conversations.ReplyToActivityAsync(reply);

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}