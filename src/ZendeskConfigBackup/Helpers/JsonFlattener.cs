using System.Text.Json;

namespace ZendeskConfigBackup.Helpers;
/// <summary>
/// Flattens a JSON object into a dictionary of key-value pairs
/// </summary>
public static class JsonFlattener
{
    //Method called by excel export service
    public static Dictionary<string, string> Flatten(JsonElement element)
    {
        // Initialise a dictionary to hold the flattened key-value pairs
        var values = new Dictionary<string, string>();
        // Start the recursive flattening process
        FlattenElement(element, values);

        return values;
    }
   
    /**Recursive method to flatten a JSON element
     * This method handles different types of JSON elements:
     * - Objects: Recursively flattens each property, appending the property name to the path.
     * - Arrays: Recursively flattens each item, appending the index to the path.
     * - Primitives (string, number, boolean): Adds the value to the dictionary with the current path as the key.
     * - Null or undefined: Adds an empty string to the dictionary with the current path as the key.
     * @param element The JSON element to flatten.
     * @param values The dictionary to store the flattened key-value pairs.
     * @param path The current path within the JSON structure.
     */
    private static void FlattenElement(JsonElement element, Dictionary<string, string> values, string path = "")
    {
        // Determine the type of the JSON element and handle accordingly
        switch (element.ValueKind)
        {
            // If the element is an object, iterate through its properties
            case JsonValueKind.Object:
                // Recursively flatten each property, appending the property name to the path
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    // Construct the new path for the property
                    string newPath =
                        string.IsNullOrEmpty(path)
                            ? property.Name
                            : $"{path}.{property.Name}";

                    FlattenElement(property.Value, values, newPath);
                }
                // Exit the switch statement after processing the object
                break;

            // If the element is an array, iterate through its items
            case JsonValueKind.Array:

                int index = 0;
                
                foreach (JsonElement item in element.EnumerateArray())
                {
                    // Construct the new path for the array item, using the index
                    string newPath =
                        $"{path}[{index}]";
                    // Recursively flatten the array item
                    FlattenElement(item, values, newPath);

                    index++;
                }

                break;

            // If the element is a string, add it to the dictionary
            case JsonValueKind.String:

                values[path] = element.GetString() ?? "";

                break;

            // If the element is a number, convert it to a string and add it to the dictionary
            case JsonValueKind.Number:

                values[path] = element.ToString();
                break;

            // If the element is a boolean true, add "TRUE" to the dictionary   
            case JsonValueKind.True:

                values[path] = "TRUE";

                break;

            // If the element is a boolean false, add "FALSE" to the dictionary
            case JsonValueKind.False:

                values[path] = "FALSE";

                break;

            // If the element is null or undefined, add an empty string to the dictionary
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:

                values[path] = "";

                break;

            // For any other value kind, convert it to a string and add it to the dictionary
            default:

                values[path] =
                    element.ToString();

                break;
        }
    }
}