using DeepL;
using DeepL.Model;
using Newtonsoft.Json.Linq;

namespace DeeplTranslator
{
    public class TranslatePerLine
    {
        private readonly Translator _translator;
        private readonly GlossaryManager _glossaryManager;
        private readonly JsonUtility _jsonUtility;
        private readonly string[] _exceptions = { "VF.", "VolvoEngine.", "VolvoAcm." };
        public TranslatePerLine(string authKey)
        {
            this._translator = new Translator(authKey);
            this._glossaryManager = new GlossaryManager(authKey);
            this._jsonUtility = new JsonUtility();
        }

        // Update translation alerts
        public async Task UpdateTranslationInFile(string sourceLanguage, string filepath)
        {
            // Read the JSON content from the file
            string jsonSource = await File.ReadAllTextAsync(filepath);

            JToken source = JToken.Parse(jsonSource);
            JToken target = JToken.Parse(jsonSource);

            JToken compareTo = source.SelectToken(sourceLanguage);

            Logger.LogMessage($"Started translating {filepath} to {sourceLanguage}");

            foreach (JToken? sourceLanguageToken in source)
            {
                string currentLanguage = sourceLanguageToken.Path;

                if (currentLanguage == sourceLanguage) continue;
                
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

                    if (translationFound) continue;
                    
                    string translation = await HandleTranslationRequest(sourceProperty.Value.ToString() ?? throw new InvalidOperationException(), sourceLanguage, currentLanguage);
                    output[sourceProperty.Name] = translation;
                }
            }
            // Update the file with the translated JSON
            await File.WriteAllTextAsync(filepath, target.ToString());
        }
        
        // Update connect language files
        public async Task UpdateSourceToTargetLanguage(string sourceFilename, string targetFileName, string path)
        {
            const string sourceLanguage = "EN";
            Logger.LogMessage($"Source Path: {path}\\{sourceFilename}");
            Logger.LogMessage($"Target Path: {path}\\{targetFileName}");

            // Determine the target language based on the target file name
            string targetLanguage = targetFileName.Substring(0, 2).ToUpper();

            var (jsonSource, startString) = _jsonUtility.ReadAndCleanJson(Path.Combine(path, sourceFilename), true);
            var jsonTarget = _jsonUtility.ReadAndCleanJson(Path.Combine(path, targetFileName));

            // Parse the JSON content into JObject instances
            JObject source = JObject.Parse(jsonSource);
            JObject target = JObject.Parse(jsonTarget);

            string csvString = "";
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
                    var examples = _jsonUtility.ExtractExceptions(tokenValue);
                    translationExceptions[pathToToken] = examples;
                }
                string logString;

                if (target.SelectToken(pathToToken) == null)
                {
                    string translation = await HandleTranslationRequest(tokenValue, sourceLanguage, targetLanguage);

                    // Check if there are any translation exceptions in the bucket.
                    if (translationExceptions.TryGetValue(pathToToken, out var exceptions))
                    {
                        translation = await _jsonUtility.ReplaceExceptions(translation, exceptions);
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
            _jsonUtility.RemoveMissingKeys(target, source);

            //write json
            string cleanTarget = startString + _jsonUtility.FormatTypeScript(target.ToString() ?? string.Empty);
            await File.WriteAllTextAsync(Path.Combine(path, targetFileName), cleanTarget);
            //write csv
            await File.WriteAllTextAsync(Path.Combine(path, $"{sourceLanguage}-{targetLanguage}.csv"), csvString);
            Logger.LogMessage("Translation finished!");
        }
        private async Task<string> HandleTranslationRequest(string translation, string sourceLanguage, string targetLanguage)
        {
            DateTime translationStartTime = DateTime.Now;
            string glossaryName = $"{sourceLanguage}-{targetLanguage}";
            bool useGlossary = await _glossaryManager.CheckForExistingGlossary(glossaryName);
            string logMessage = $"{sourceLanguage}: {translation}";
            TextResult translatedText;
            if (useGlossary) //Translate with Glossary
            {
                GlossaryInfo? g = await _glossaryManager.GetGlossaryByName(glossaryName);

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
            Logger.LogMessage($"Translating: {glossaryName} Translation Time: {translationTime}");
            Logger.LogMessage($"{logMessage}");
            Logger.LogMessage($"{targetLanguage}: {translatedText}");
            return translatedText.ToString();
        }

        private static void AddToJObject(JObject jObject, string[] keys, JToken value)
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
    }
}