using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            foreach (object? unused in matches)
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
            const string pattern = "\"([^\"]+)\":";
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

        public string UpdateTargetTranslationsOrder(string sourceJson, string targetJson, string targetFileName)
        {
        JObject sourceObject = JObject.Parse(sourceJson);
        JObject targetObject = JObject.Parse(targetJson);

        // Extract the translations section from both source and target
        JObject sourceTranslations = (JObject)sourceObject["translations"];
        JObject targetTranslations = (JObject)targetObject["translations"];

        // Create a dictionary to store the order of keys from the source file
        Dictionary<string, JToken> sourceOrder = new Dictionary<string, JToken>();

        // Iterate through the source translations and store the order
        foreach (var pair in sourceTranslations)
        {
            sourceOrder[pair.Key] = pair.Value;
        }

        // Create a new translations object in the target file with the order from the source
        JObject newTargetTranslations = new JObject();

        // Iterate through the source order and add properties to the target in the same order
        foreach (string key in sourceOrder.Keys.Where(key => targetTranslations.ContainsKey(key)))
        {
            if (sourceOrder[key].Type == JTokenType.Object)
            {
                // Handle nested properties
                var nestedSource = (JObject)sourceOrder[key];
                var nestedTarget = (JObject)targetTranslations[key];

                var nestedOrder = new JObject();

                foreach (var nestedKey in nestedSource.Properties().Select(p => p.Name))
                {
                    if (nestedTarget.ContainsKey(nestedKey))
                    {
                        nestedOrder[nestedKey] = nestedTarget[nestedKey];
                    }
                }

                newTargetTranslations[key] = nestedOrder;
            }
            else
            {
                newTargetTranslations[key] = targetTranslations[key];
            }
        }

        targetObject["translations"] = newTargetTranslations;
        
        var keyProperty = targetObject.Property("key");
        keyProperty.Remove();
        targetObject.AddFirst(new JProperty("key", FormatTargetKey(targetFileName)));

        return targetObject.ToString(Formatting.Indented);
        }
        
        private string FormatTargetKey(string targetFileName)
        {
            string languageCode = Path.GetFileNameWithoutExtension(targetFileName)[..2].ToLower() ?? throw new InvalidOperationException();
            string countryCode = Path.GetFileNameWithoutExtension(targetFileName)[3..].ToUpper() ?? throw new InvalidOperationException();

            return $"{languageCode}-{countryCode}";
        }
    }
}