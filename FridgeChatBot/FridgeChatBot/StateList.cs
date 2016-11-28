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
                StreamWriter sw = new StreamWriter("C:\\Users\\keleung\\Documents\\GitHub\\Microsoft\\FridgeChatBot\\FridgeChatBot\\StateList.txt");

                ingredients.Replace("%2C", "%");  // replace all '%2C' with '%'

                //Write a line of text
                //Add ingredients line by line
                //Ingredients are seperated by '%2C'
                for (int i = 1; i < ingredients.Length; i++)
                {
                    var nextStart = 0;
                    //test if the next char is the '%' character or last character in string
                    if ((ingredients[i] == '%') || (i == ingredients.Length))
                    {
                        // ingredient is last through i
                        var item = ingredients.Substring(nextStart, i);
                        sw.WriteLine(item);

                        // update last to current '%'
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
    }
}