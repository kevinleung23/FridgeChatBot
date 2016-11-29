using System;
using System.IO;

namespace FridgeChatBot
{
    public class StateList
    {
        internal void AddIngredients(string[] ingredients)
        {
            //Save the current list so we do not lose data
            string[] currentIngredients = File.ReadAllLines(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");

            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = new StreamWriter(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");

            //Add OLD ingredients line by line
            foreach (string item in currentIngredients)
            {
                sw.WriteLine(item);
            }

            //Add NEW ingredients line by line
            foreach (string item in ingredients)
            {
                sw.WriteLine(item);
            }

            //Close the file
            sw.Close();
        }

        internal void RemoveIngredients(string[] Unwantedingredients)
        {
            //Save the current list so we do not lose data
            string[] currentIngredients = File.ReadAllLines(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");
            
            //crosscheck ingredients list to remove unwanted items.
            //call AddIngredients method on remaining to write back into file
            for (int i = 0; i < currentIngredients.Length; i++)
            {
                for (int k = 0; k < Unwantedingredients.Length; k++)
                {
                    if (currentIngredients[i] == Unwantedingredients[k])
                    {
                        // found match, mark index null
                        currentIngredients[i] = null;
                    }
                }
            }

            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = new StreamWriter(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");
            // write remaining ingredients back into the file
            foreach (string item in currentIngredients)
            {
                if (item != null)
                {
                    sw.WriteLine(item);
                }
            }
            //Close the file
            sw.Close();
        }

        internal string[] ReadIngredients()
        {
            // Read each line from the file and pass into array
            string[] ingredients = File.ReadAllLines(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");
            // Return array
            return ingredients;
        }
        internal void clearIngredients()
        {
            // Open the file
            StreamWriter sw = new StreamWriter(@"C:\Users\keleung\Documents\GitHub\Microsoft\FridgeChatBot\FridgeChatBot\StateList.txt");
            // Close the file
            sw.Close();
        }
    }
}