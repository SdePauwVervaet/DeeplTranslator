using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DeeplTranslator
{
    public class JsonUtility
    {
        /// <summary>
        /// Extract all parts of a string which are between brackets and put them in a list.
        /// <example>
        /// "My string {{Exception1}} and {{Exception2}}"
        /// </example>>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<string> ExtractExceptions(string input)
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
        
        /// <summary>
        /// Removes all parts of a string which are between brackets.
        ///<example>
        /// "My string {{Exception1}} and {{Exception2}}"
        /// </example>>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string RemoveExceptions(string input)
        {
            // Define a regular expression pattern to match {{Exception}} parts
            const string pattern = @"\{\{.+?\}\}";

            return Regex.Replace(input, pattern, "");
        }
        
        /// <summary>
        /// Replace {{Exception}} parts in a string with stored values in order from left to right.
        ///<example>
        /// "{{FirstReplacement}} and {{SecondReplacement}}"
        /// </example>>
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="translationExceptions"></param>
        /// <returns></returns>
        public Task<string> ReplaceExceptions(string translation, List<string> translationExceptions)
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
                    Logger.LogMessage($"Replaced key: {matches[index].Value} -> {exception}");
                    index++;
                }
                else
                {
                    break;
                }
            }

            return Task.FromResult(translation);
        }
        
        /// <summary>
        /// Format a Json file to TypeScript.
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public string FormatJsonToTypeScript(string jsonText)
        {
            const string pattern = "\"([^\"]+)\":";
            string newJsonText = Regex.Replace(jsonText, pattern, "$1:");

            return newJsonText;
        }
        
        /// <summary>
        /// Read Json and returns everything between the first "{" and last "}" bracket.
        /// Removing any unused text before or after the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Read Json and returns everything between the first "{" and last "}" bracket.
        /// Returns a cleaned Json and text part before the first "{" bracket.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="needStartString"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Cleans up target file.
        /// Remove all keys in the target file which are not in the source file.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
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

        /// <summary>
        /// Sorts the targetJson to be in the same exact order as the sourceJson.
        /// Also updates the language key to the correct name.
        /// </summary>
        /// <param name="sourceJson"></param>
        /// <param name="targetJson"></param>
        /// <param name="targetFileName"></param>
        /// <returns></returns>
        public string UpdateTargetTranslationsOrder(string sourceJson, string targetJson, string targetFileName)
        {
            JObject sourceObject = JObject.Parse(sourceJson);
            JObject targetObject = JObject.Parse(targetJson);

            var sourceTranslations = (JObject)sourceObject["translations"];
            var targetTranslations = (JObject)targetObject["translations"];

            var sourceOrder = sourceTranslations.Properties()
                .ToDictionary(property => property.Name, property => property.Value);

            var newTargetTranslations = new JObject(
                sourceOrder.Keys
                    .Where(key => targetTranslations.ContainsKey(key))
                    .Select(key => new JProperty(key, SortProperties(targetTranslations[key], sourceOrder[key])))
            );

            targetObject["translations"] = newTargetTranslations;

            targetObject.Property("key")?.Remove();
            targetObject.AddFirst(new JProperty("key", FormatTargetKey(targetFileName)));

            return targetObject.ToString(Formatting.Indented);
        }
        
        /// <summary>
        /// Helper method of UpdateTargetTranslationsOrder(string sourceJson, string targetJson, string targetFileName);
        /// </summary>
        /// <param name="targetToken"></param>
        /// <param name="sourceToken"></param>
        /// <returns></returns>
        private JToken SortProperties(JToken targetToken, JToken sourceToken)
        {
            if (sourceToken.Type == JTokenType.Object && targetToken.Type == JTokenType.Object)
            {
                var sortedObject = new JObject(
                    ((JObject)sourceToken).Properties()
                    .Select(sourceProperty => new JProperty(
                        sourceProperty.Name, 
                        SortProperties(((JObject)targetToken).GetValue(sourceProperty.Name), sourceProperty.Value))
                    )
                );
                return sortedObject;
            }
            else if (targetToken.Type == JTokenType.Array)
            {
                return new JArray(targetToken.Select((t, i) => SortProperties(t, sourceToken[i])));
            }
            else
            {
                return targetToken;
            }
        }
        
        /// <summary>
        /// Will make sure the correct file-name format gets returned in the correct format.
        /// Takes a file name with a languageCode and Countrycode and returns en-GB.
        /// <example>
        /// "en-gb.js" -> "en-GB"
        /// </example>>
        /// </summary>
        /// <param name="targetFileName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private string FormatTargetKey(string targetFileName)
        {
            // Define a regular expression pattern for the "en-gb" format
            string pattern = @"^[a-z]{2}-[a-z]{2}\.js$";

            // Use Regex.IsMatch to check if the input matches the pattern
            if (!Regex.IsMatch(targetFileName, pattern, RegexOptions.IgnoreCase)) throw new FormatException($"{targetFileName} has an invalid file name. The correct format is 'en-gb.js'.");
            
            string languageCode = Path.GetFileNameWithoutExtension(targetFileName)[..2].ToLower() ?? throw new InvalidOperationException();
            string countryCode = Path.GetFileNameWithoutExtension(targetFileName)[3..].ToUpper() ?? throw new InvalidOperationException();
            return $"{languageCode}-{countryCode}";
        }
        
        /// <summary>
        /// Check if key existst in jsonObject
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyInTargetLanguage(JObject jsonObject, string key)
        {
            // Check if the key is directly present in the current JObject
            if (jsonObject.ContainsKey(key)) return true;

            // Check if the key is present in any nested JObjects
            return jsonObject.Properties().Where(property => property.Value.Type == JTokenType.Object).Any(property => IsKeyInTargetLanguage((JObject)property.Value, key));
        }
        
        /// <summary>
        /// Extract the key of a Key/Value pair giving it's token.Parent.
        /// </summary>
        /// <param name="tokenParent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public string ExtractTokenKeyValue(string? tokenParent, JToken token)
        {
            const string pattern = "\"(\\w+(\\.\\w+)*)\":\\s\".*\"";
            var regex = new Regex(pattern);

            Match match = regex.Match(tokenParent!);

            return match.Success ? match.Groups[1].Value : token.Value<string>();
        }
        
        /// <summary>
        /// Extracts the correct token Path for a given key.
        /// supports keys with dots in between.
        ///<example>
        /// Key: "token.dots.example"
        /// </example>>
        /// </summary>
        /// <param name="tokenPath"></param>
        /// <param name="tokenKey"></param>
        /// <returns></returns>
        public List<string> ExtractTokenPath(string tokenPath, string tokenKey)
        {
            List<string> newTokenPath = new List<string>();

            // Remove "en." from the beginning of the tokenPath
            if (tokenPath.StartsWith("en."))
            {
                tokenPath = tokenPath.Substring(3);
            }
            else if (tokenPath.StartsWith("en"))
            {
                tokenPath = tokenPath.Substring(2);
            }

            // Check if tokenPath has brackets and remove them along with everything within
            int bracketIndex = tokenPath.IndexOf('[');
            if (bracketIndex != -1)
            {
                tokenPath = tokenPath.Remove(bracketIndex, tokenPath.IndexOf(']') - bracketIndex + 1);
            }

            // Split the tokenPath based on dots
            string[] pathParts = tokenPath.Split('.');

            // Split the tokenKey based on dots
            string[] keyParts = tokenKey.Split('.');

            // Initialize index for traversing the keyParts
            int keyIndex = keyParts.Length - 1;

            if (pathParts.Length >= 1)
            {
                for (int i = pathParts.Length - 1; i >= 0; i--)
                {
                    // If keyPart exists and matches the current pathPart, skip it
                    if (keyIndex >= 0 && pathParts[i] == keyParts[keyIndex])
                    {
                        keyIndex--;
                        continue;
                    }

                    // Add the pathPart to the newTokenPath
                    newTokenPath.Insert(0, pathParts[i]);
                }
            }
            else
            {
                newTokenPath.Insert(0, string.Empty);
                return newTokenPath;
            }

            if (!newTokenPath.Any() || newTokenPath[0] == "" && newTokenPath.Count <= 1)
            {
                newTokenPath = new List<string>();
            }

            return newTokenPath;
        }
        
        /// <summary>
        /// Sorts alert translation file taking "en" as source.
        /// will make sure all language parts are in the exact same order as the source.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public JObject SortLanguages(JObject json)
        {
            if (!json.TryGetValue("en", out var englishPart))
            {
                return json;
            }

            var sortedJson = new JObject { ["en"] = englishPart };

            var languageCodes = json.Properties().Select(p => p.Name).Where(code => code != "en");

            var sortedLanguageCodes = languageCodes.OrderBy(code =>
            {
                var property = englishPart[code];
                return property != null ? Array.IndexOf(englishPart.ToArray(), property) : int.MaxValue;
            }).ToList();

            foreach (var languageCode in sortedLanguageCodes)
            {
                var languagePart = json[languageCode];
                if (languagePart != null)
                {
                    sortedJson[languageCode] = SortLanguagePart(languagePart, englishPart);
                }
            }

            return sortedJson;
        }
        /// <summary>
        /// Helper method of SortLanguages(JObject json);
        /// </summary>
        /// <param name="languagePart"></param>
        /// <param name="englishPart"></param>
        /// <returns></returns>
        private JToken SortLanguagePart(JToken languagePart, JToken englishPart)
        {
            if (languagePart is JObject languageObject)
            {
                var sortedLanguageObject = new JObject();

                var sortedProperties = englishPart
                    .Cast<JProperty>()
                    .OrderBy(p => Array.IndexOf(englishPart.ToArray(), languageObject.Property(p.Name)))
                    .ToList();

                foreach (var property in sortedProperties)
                {
                    var propertyName = property.Name;
                    var propertyValue = languageObject[propertyName];

                    if (propertyValue != null)
                    {
                        sortedLanguageObject[propertyName] = SortLanguagePart(propertyValue, property.Value);
                    }
                }

                return sortedLanguageObject;
            }
            else if (languagePart is JArray languageArray)
            {
                return new JArray(languageArray.Select(item => SortLanguagePart(item, englishPart)));
            }

            return languagePart;
        }
        
        /// <summary>
        /// Recursively adds a value to a JObject based on a hierarchy of keys.
        /// If only one key is provided, it directly assigns the value to that key.
        /// If multiple keys are provided, it creates nested JObjects as needed to reach the final key,
        /// then assigns the value to the final key.
        /// </summary>
        /// <param name="jObject">The JObject to which the value will be added.</param>
        /// <param name="keys">An array of keys representing the hierarchy where the value should be added.</param>
        /// <param name="value">The JToken value to be added to the JObject.</param>
        public void AddToJObject(JObject jObject, string[] keys, JToken value)
        {
            if (keys.Length == 1)
            {
                jObject[keys[0]] = value;
            }
            else
            {
                string key = keys[0];
                if (!jObject.ContainsKey(key))
                {
                    jObject[key] = new JObject();
                }

                AddToJObject((jObject[key] as JObject)!, keys.Skip(1).ToArray(), value);
            }
        }
        
        /// <summary>
        /// Adds a value to a JObject based on a specified key and a path of nested keys.
        /// If the path is empty, the value is directly assigned to the specified key.
        /// If the path is not empty, it creates nested JObjects as needed to follow the path,
        /// then assigns the value to the final nested structure under the specified key.
        /// </summary>
        /// <param name="jObject">The JObject to which the value will be added.</param>
        /// <param name="tokenPath">A list of keys representing the path where the value should be added.</param>
        /// <param name="value">The JToken value to be added to the JObject.</param>
        /// <param name="tokenKeyValue">The key under which the value should be added.</param>
        public void AddToJObjectKey(JObject jObject, List<string> tokenPath, JToken value, string tokenKeyValue)
        {
            // Check if newTokenPath is empty
            if (tokenPath.Count == 0)
            {
                // If newTokenPath is empty, set the value directly under the key
                jObject[tokenKeyValue] = value;
            }
            else
            {
                JObject nestedObject = jObject;

                foreach (string part in tokenPath)
                {
                    if (!nestedObject.ContainsKey(part))
                    {
                        nestedObject[part] = new JObject();
                    }
                    nestedObject = (nestedObject[part] as JObject)!;
                }

                // Add the value to the final nested structure under the specified key
                nestedObject[tokenKeyValue] = value;
            }
        }
    }
}