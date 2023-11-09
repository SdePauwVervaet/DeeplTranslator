using System.Text.RegularExpressions;
using DeepL;
using DeepL.Model;
using Newtonsoft.Json.Linq;

namespace DeeplTranslator
{
    public class Translator
    {
        private readonly DeepL.Translator _translator;
        private readonly string[] _exceptions = { "VF.", "VolvoEngine.", "VolvoAcm." };
        public Translator(string authKey)
        {
            this._translator = new DeepL.Translator(authKey);
        }

        public async Task<string> CheckUsage()
        {
            Usage usage = await _translator.GetUsageAsync();
            System.Diagnostics.Debug.WriteLine("Deepl:");
            if (usage.AnyLimitReached)
            {
                return "Translation limit exceeded.";
            }
            else if (usage.Character != null)
            {
                return $"Character usage: {usage.Character}";
            }
            else
            {
                return $"{usage}";
            }
        }

        #region Glossaries
        
        public async Task<string> CheckForGlossaries()
        {
            string returnString = "";
            try
            {
                var glossaries = await _translator.ListGlossariesAsync();

                System.Diagnostics.Debug.WriteLine("Current glossaries...");
                foreach (GlossaryInfo glossaryInfo in glossaries)
                {
                    returnString +=
                        $"Creating {glossaryInfo.Name}: Source:{glossaryInfo.SourceLanguageCode} Target:{glossaryInfo.TargetLanguageCode} Count:{glossaryInfo.EntryCount}  \r\n";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("---");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("---");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                System.Diagnostics.Debug.WriteLine("---");
                returnString += $"{ex.Message}";
            }

            return returnString;
        }
        private async Task<bool> CheckForExistingGlossary(string glossaryName)
        {
            var glossaries = await _translator.ListGlossariesAsync();
            return glossaries.Any(g => String.Equals(g.Name, glossaryName, StringComparison.CurrentCultureIgnoreCase));

        }
        private async Task<GlossaryInfo?> GetGlossaryByName(string glossaryName)
        {
            GlossaryInfo?[] glossaries = await _translator.ListGlossariesAsync();

            return glossaries.FirstOrDefault(g => String.Equals(g!.Name, glossaryName, StringComparison.CurrentCultureIgnoreCase));

        }
        public async Task<string> CreateGlossaryFromDictionary(string sourceLanguage, string targetLanguage,
            Dictionary<string, string> dictionary)
        {
            string glossaryName = $"{sourceLanguage}-{targetLanguage}";
            string returnString;
            try
            {
                GlossaryInfo unused = await _translator.CreateGlossaryAsync(
                    glossaryName, sourceLanguage, targetLanguage,
                    new GlossaryEntries(dictionary));
                returnString = $"Creating Glossary at Deepl{sourceLanguage}-{targetLanguage}";
            }
            catch (Exception ex)
            {
                returnString = $"Creating a glossary for {targetLanguage} failed {ex.Message}";
            }

            return returnString;
        }
        public async Task<string> DeleteGlossaries()
        {
            try
            {
                var glossaries = await _translator.ListGlossariesAsync();
                foreach (var glossaryInfo in glossaries)
                {
                    await _translator.DeleteGlossaryAsync(glossaryInfo.GlossaryId);
                }

                return "Deleting existing glossaries";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("---");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("---");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                System.Diagnostics.Debug.WriteLine("---");
                return ex.Message;
            }
        }
        
        #endregion

#region Translation

        // Update translation alerts
        public async Task<string> UpdateTranslation(string sourceLanguage, string filepath,
            Func<string, Task> notificationCallback)
        {
            // Read the JSON content from the file
            string jsonSource = await File.ReadAllTextAsync(filepath);

            JToken source = JToken.Parse(jsonSource);
            JToken target = JToken.Parse(jsonSource);

            JToken compareTo = source.SelectToken(sourceLanguage);

            string returnString = $"Started translating {filepath} to {sourceLanguage}";

            foreach (JToken? sourceLanguageToken in source)
            {
                string currentLanguage = sourceLanguageToken.Path;

                if (currentLanguage != sourceLanguage)
                {
                    JToken compare = source.SelectToken(currentLanguage);
                    JToken output = target.SelectToken(currentLanguage);

                    foreach (JProperty sourceProperty in compareTo.Children<JProperty>())
                    {
                        // Don't translate engine faults or exceptions
                        if (_exceptions.Any(exception => sourceProperty.Name.Contains(exception)))
                        {
                            Console.WriteLine($@"Exception found. Not translating {sourceProperty.Name}");
                            continue;
                        }

                        bool translationFound = compare.Children<JProperty>().Any(targetProperty =>
                            JToken.DeepEquals(sourceProperty.Name, targetProperty.Name));

                        if (!translationFound)
                        {
                            string translation = await Translate(sourceProperty.Value.ToString() ?? throw new InvalidOperationException(), sourceLanguage,
                                currentLanguage, notificationCallback);
                            output[sourceProperty.Name] = translation;
                        }
                    }
                }
            }

            // Update the file with the translated JSON
            await File.WriteAllTextAsync(filepath, target.ToString());

            return returnString + target;
        }
        // Update connect language files
        public async Task<string> UpdateLanguage(string sourceFilename, string targetFileName, string path,
            Func<string, Task> notificationCallback)
        {
            string sourceLanguage = "EN";
            await notificationCallback.Invoke($"Source Path: {path}\\{sourceFilename}");
            await notificationCallback.Invoke($"Target Path: {path}\\{targetFileName}");

            // Determine the target language based on the target file name
            string targetLanguage = targetFileName.Substring(0, 2).ToUpper();

            var (jsonSource, startString) = ReadAndCleanJson(Path.Combine(path, sourceFilename), true);
            var jsonTarget = ReadAndCleanJson(Path.Combine(path, targetFileName));

            // Parse the JSON content into JObject instances
            JObject source = JObject.Parse(jsonSource);
            JObject target = JObject.Parse(jsonTarget);

            string csvString = "";
            string returnString = "";
            DateTime dateAndTime = DateTime.Now;

            var translationExceptions = new Dictionary<string, List<string>>();

            // Process the JSON structure
            foreach (var token in source.Descendants().Where(t => t.Type == JTokenType.String))
            {
                var pathToToken = token.Path;
                string tokenValue = token.Value<string>();

                if (token.Value<string>().Contains("{{") && token.Value<string>().Contains("}}"))
                {
                    // Extract {{exception}} parts and store them
                    var examples = ExtractExceptions(tokenValue);
                    translationExceptions[pathToToken] = examples;
                }

                string logString;

                if (target.SelectToken(pathToToken) == null)
                {
                    string translation =
                        await Translate(tokenValue, sourceLanguage, targetLanguage, notificationCallback);

                    // Check if there are any translation exceptions in the bucket.
                    if (translationExceptions.TryGetValue(pathToToken, out var exceptions))
                    {
                        translation = ReplaceExceptions(translation, exceptions, notificationCallback);
                    }

                    AddToJObject(target, pathToToken.Split('.'), translation);

                    logString =
                        $"{dateAndTime};{sourceLanguage};{targetLanguage}; {pathToToken}; {token.Value<string>()}; {target.SelectToken(pathToToken)}";
                    System.Diagnostics.Debug.WriteLine($"--- Translated {logString}");
                    logString += Environment.NewLine;
                    await File.AppendAllTextAsync(Path.Combine(path, $"{dateAndTime.ToShortDateString()}-log.csv"), logString);
                }
                else
                {
                    logString =
                        $"{dateAndTime};{sourceLanguage};{targetLanguage}; {pathToToken}; {token.Value<string>()}; {target.SelectToken(pathToToken)}";
                    logString += Environment.NewLine;
                }

                csvString += logString;
            }

            //clean up the target file. Remove all keys in the target file which is not in the source file (great for removing/updating a key)
            RemoveMissingKeys(target, source);

            //write json
            string cleanTarget = startString + FormatTypeScript(target.ToString() ?? string.Empty);
            await File.WriteAllTextAsync(Path.Combine(path, targetFileName),  cleanTarget);
            //write csv
            await File.WriteAllTextAsync(Path.Combine(path, $"{sourceLanguage}-{targetLanguage}.csv"), csvString);
            returnString += cleanTarget;
            await notificationCallback("Translation finished!");
            return returnString;
        }
        private async Task<string> Translate(string translation, string sourceLanguage, string targetLanguage,
            Func<string, Task> notificationCallback)
        {
            DateTime translationStartTime = DateTime.Now;
            string glossaryName = $"{sourceLanguage}-{targetLanguage}";
            bool useGlossary = await CheckForExistingGlossary(glossaryName);
            string logMessage = $"{sourceLanguage}: {translation}";
            TextResult translatedText;
            if (useGlossary) //Translate with Glossary
            {
                GlossaryInfo? g = await GetGlossaryByName(glossaryName);

                translatedText = await _translator.TranslateTextAsync
                (
                    translation,
                    sourceLanguage,
                    targetLanguage,
                    new TextTranslateOptions { GlossaryId = g?.GlossaryId }
                );
                logMessage += " with glossary";
            }
            else //Translate without Glossary
            {
                translatedText = await _translator.TranslateTextAsync
                (
                    translation,
                    sourceLanguage,
                    targetLanguage
                );
                logMessage += " without glossary";
            }
            TimeSpan translationTime = DateTime.Now - translationStartTime;
            await notificationCallback.Invoke($"Translating: {glossaryName} Translation Time: {translationTime}");
            await notificationCallback.Invoke($"{logMessage}");
            await notificationCallback.Invoke($"{targetLanguage}: {translatedText}");
            return translatedText.ToString();
        }
        
        
        #region Cleanup Crew

        private static List<string> ExtractExceptions(string input) // Extract and store {{Exception}} parts from a string
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
        private static string RemoveExceptions(string input) // Remove {{Exception}} parts from a string for translation
        {
            // Define a regular expression pattern to match {{Exception}} parts
            const string pattern = @"\{\{.+?\}\}";

            return Regex.Replace(input, pattern, "");
        }
        private static string ReplaceExceptions(string translation, List<string> translationExceptions, Func<string, Task> notificationCallback) // Replace {{Exception}} parts in the translation with the stored values
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
                    notificationCallback.Invoke($"Replaced key: {matches[index].Value} -> {exception}");
                    index++;
                }
                else
                {
                    break;
                }
            }

            return translation;
        }
        private static string FormatTypeScript(string jsonText)
        {
            string pattern = "\"([^\"]+)\":";
            string newJsonText = Regex.Replace(jsonText, pattern, "$1:");

            return newJsonText;
        }
        private static string ReadAndCleanJson(string filePath)
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
        private static (string, string) ReadAndCleanJson(string filePath, bool needStartString)
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
        private static void RemoveMissingKeys(JObject target, JObject source)
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
        
        #endregion

        private void AddToJObject(JObject jObject, string[] keys, JToken value)
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
        
#endregion

        // public async Task<JToken?> GetNodes()
        // {
        //     return null;
        // }
    }
}