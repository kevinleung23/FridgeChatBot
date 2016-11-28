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
        public string ShoppingList { get; set; }

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
                            answer = "I'm sorry. I didn't understand that. Try: \"What can we cook for dinner?\" or  \"What ingredients are we missing?\"";
                            break;

                        case "greeting":
                            answer = "Hello! Feeling hungry and adventurous? Try: \"What can we cook for dinner?\" or  \"I'm hungry!\"";

                            break;

                        case "addList":
                            // add ingredient(s) to the list
                            // Store the values into a local text file
                            var IngredientsListAdd = new StateList();
                            IngredientsListAdd.AddIngredients(activity.Text);

                            answer = "Successfully added ingredients.";
                            break;

                        case "removeList":
                            // remove ingredient(s) to the list
                            // Remo e the values from the local text file
                            var IngredientsListRemove = new StateList();
                            IngredientsListRemove.RemoveIngredients(activity.Text);

                            answer = "Sucesfully removed ingredients.";
                            break;

                        case "newList":
                            // clear the current list to start new
                            answer = "newList";
                            break;

                        case "displayList":
                            // read current ingredients list
                            var IngredientsListRead = new StateList();
                            var list = IngredientsListRead.ReadIngredients();
                            var items = "";
                            for (int i = 0; i < list.Length; i++)
                            {
                                items += " " + (i + 1) + ". " + list[i] + '\n';
                            }

                            answer = "You currently have: \n" + items;
                            break;

                        case "findRecipe":
                            // read current ingredients list
                            var IngredientsListAPI = new StateList();
                            var arg = IngredientsListAPI.ReadIngredients();
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

                                // GetIngredients - complete ingredients from the API response
                                // Load the list into a sorted dictionary with the aisle for user's convenience
                                var dict = new SortedDictionary<string, string>();
                                foreach (var item in recipeLink.extendedIngredients)
                                {
                                    dict.Add(item.name, item.aisle);
                                }

                                ShoppingList = "Shopping List: \n";
                                foreach (KeyValuePair<string, string> item in dict.OrderBy(key => key.Value))
                                {
                                    ShoppingList += String.Format("{0} ({1}).\n", item.Key, item.Value);
                                }

                                // Store the shoppingList string in the current client state
                                // Allows the string to be accessed in preceeding reply
                                userData.SetProperty<string>("ShoppingList", ShoppingList);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            }
                            answer = "It looks like you may be missing some ingredients! Try: \"Send me a grocery list!\"";
                            break;

                        case "getShoppingList":
                            // Retrieve ShoppingList from state              
                            answer = userData.GetProperty<string>("ShoppingList");
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