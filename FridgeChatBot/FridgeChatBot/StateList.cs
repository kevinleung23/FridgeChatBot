using System;
using System.IO;

namespace FridgeChatBot
{
    public class StateList
    {
        internal void AddIngredients(string ingredients)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");

                //Write a line of text
                //Add ingredients line by line
                //Ingredients are seperated by '%2C' eggs%steak%mushrooms
                var nextStart = 0;
                for (int i = 1; i < ingredients.Length; i++)
                {
                    //test if the next char is the '%' character or last character in string
                    if ((ingredients[i] == '%') || (i == ingredients.Length - 1))
                    {
                        var item = " ";
                        // if last ingredient
                        if (i == ingredients.Length - 1)
                        {
                            item = ingredients.Substring(nextStart, i - nextStart + 1);
                        } else
                        {
                            // ingredient is last through i
                            item = ingredients.Substring(nextStart, i - nextStart);
                        }
                        
                        // add to list
                        sw.WriteLine(item);

                        // update last to current '% + 1'
                        nextStart = i + 1;
                    }
                }
                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        internal void RemoveIngredients(string ingredients)
        {
         
        }

        internal string[] ReadIngredients()
        {
            string[] ingredients = File.ReadAllLines(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");
            return ingredients;
        }
    }
}