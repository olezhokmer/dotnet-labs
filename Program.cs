using System;
using CustomDynamicListLibrary;

class Program
{
    static void Main()
    {
        
        RunDynamicListOperations();
    }

    static void RunDynamicListOperations()
    {
        // Creating an instance of the CustomDynamicList
        CustomDynamicList<int> customList = new CustomDynamicList<int>();

        // Subscribing to the ItemAdded event
        customList.ItemAdded += (sender, args) =>
        {
            Console.WriteLine($"Item added: {args.Item}");
        };

        // Adding elements to the collection
        customList.Add(1);
        customList.Add(2);
        customList.Add(3);

        // Displaying elements in the collection using foreach
        Console.WriteLine("Elements in the collection:");
        foreach (int item in customList)
        {
            Console.WriteLine(item);
        }

        Console.WriteLine("Removing the element");
        customList.Remove(3);

        Console.WriteLine("Elements in the updated collection:");
        foreach (int item in customList)
        {
            Console.WriteLine(item);
        }
    }
}