using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DeeplTranslator
{
    public class JsonUtility
    {
        public List<string> ExtractExceptions(string input) // Extract and store {{Exception}} parts from a string
        {
            List<string> translationExceptions = new List<string>();
            // Define a regular expression pattern to match {{Exception}} parts
            const string pattern = @"\{\{(.+?)\}\}";

            MatchCollection matches = Regex.Matches(input, pattern);
            foreach (Match match in matches)
            {
                translationExceptions.Add(match.Value); // Capture the content
            }

            return translationExceptions;
        }
        public string RemoveExceptions(string input) // Remove {{Exception}} parts from a string for translation
        {
            // Define a regular expression pattern to match {{Exception}} parts
            const string pattern = @"\{\{.+?\}\}";

            return Regex.Replace(input, pattern, "");
        }
        
        // Replace {{Exception}} parts in the translation with the stored values
        public async Task<string> ReplaceExceptions(string translation, List<string> translationExceptions)
        {
            // Define a regular expression pattern to match placeholders for replacement
            const string pattern = @"\{\{(.+?)\}\}";

            MatchCollection matches = Regex.Matches(translation, pattern);
            int index = 0;
            foreach (object? match in matches)
            {
                if (index < translationExceptions.Count)
                {
                    string exception = translationExceptions[index];
                    translation = Regex.Replace(translation, matches[index].Value, $"{exception}");
                    await Logger.LogMessage($"Replaced key: {matches[index].Value} -> {exception}");
                    index++;
                }
                else
                {
                    break;
                }
            }

            return translation;
        }
        
        public string FormatTypeScript(string jsonText)
        {
            string pattern = "\"([^\"]+)\":";
            string newJsonText = Regex.Replace(jsonText, pattern, "$1:");

            return newJsonText;
        }
        
        public string ReadAndCleanJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);

                int startIndex = jsonContent.IndexOf('{');
                int endIndex = jsonContent.LastIndexOf('}');

                if (startIndex >= 0 && endIndex >= 0)
                {
                    return jsonContent.Substring(startIndex, endIndex - startIndex + 1);
                }
            }

            // If the file doesn't exist or there are no valid JSON braces, return an empty JSON object
            return "{}";
        }
        
        public (string, string) ReadAndCleanJson(string filePath, bool needStartString)
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);

                var startString = jsonContent.Substring(0, jsonContent.IndexOf('{'));

                int startIndex = jsonContent.IndexOf('{');
                int endIndex = jsonContent.LastIndexOf('}');

                if (startIndex >= 0 && endIndex >= 0)
                {
                    return (jsonContent.Substring(startIndex, endIndex - startIndex + 1), startString);
                }
            }

            // If the file doesn't exist or there are no valid JSON braces, return an empty JSON object
            return ("{}", "");
        }
        
        public void RemoveMissingKeys(JObject target, JObject source)
        {
            foreach (JProperty? prop1 in target.Properties().ToList())
            {
                if (source[prop1.Name] == null)
                {
                    prop1.Remove();
                }
                else if (prop1.Value.Type == JTokenType.Object)
                {
                    RemoveMissingKeys((JObject)prop1.Value, (JObject)source[prop1.Name]);
                }
            }
        }
    }
}