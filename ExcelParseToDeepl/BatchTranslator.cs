using DeepL;
using DeepL.Model;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace DeeplTranslator
{
    public class BatchTranslator
    {
        private readonly Translator _translator;
        private readonly GlossaryManager _glossaryManager;
        private readonly JsonUtility _jsonUtility;
        private const int BatchSize = 100;
        private readonly string[] _exceptions;


        public BatchTranslator(string authKey, string[] exceptions)
        {
            this._translator = new Translator(authKey);
            this._exceptions = exceptions;
            this._glossaryManager = new GlossaryManager(authKey);
            this._jsonUtility = new JsonUtility();
        }

        // Update translation alerts
        public async Task UpdateTranslationInSameFile(string filePath, string fileName)
        {
            const string sourceLanguage = "en";
            Logger.LogMessage($"File Path: {filePath}\\{fileName}");
            DateTime dateAndTime = DateTime.Now;
            
            string jsonSource = await File.ReadAllTextAsync(Path.Combine(filePath, fileName));

            JObject fileObject = JObject.Parse(jsonSource);
            var source = (JObject)fileObject.SelectToken(sourceLanguage);

            var targetLanguages = fileObject.Properties()
                .Where(prop => prop.Name != sourceLanguage)
                .Select(prop => prop.Name)
                .ToList();

            foreach (string language in targetLanguages)
            {
                if (!fileObject.ContainsKey(language))
                {
                    fileObject[language] = new JObject();
                }

                JObject targetLanguage = (JObject)fileObject[language];

                var tokenBatches = source.Descendants().Where(t => t.Type == JTokenType.String).Batch(50);
                foreach (var tokenBatch in tokenBatches)
                {
                    var translationTasks = tokenBatch.Select(async token =>
                    {
                        string tokenParent = token.Parent.ToString()!;
                        string pathToToken = token.Path;
                        string sourceTokenValue = token.Value<string>();
                        string tokenKeyValue = _jsonUtility.ExtractTokenKeyValue(tokenParent, token);
                        string? targetTokenValue = targetLanguage[tokenKeyValue]?.Value<string>();
                        if (!TranslateEmptyToken(targetLanguage, tokenKeyValue, language, sourceTokenValue, targetTokenValue)) return;

                        // Don't translate engine faults or exceptions
                        if (_exceptions.Any(exception => tokenKeyValue.Contains(exception)))
                        {
                            Console.WriteLine($@"Exception found. Not translating {tokenKeyValue} for {language}");
                            return;
                        }

                        // Key doesn't exist in the target language, add it and translate the value
                        string translatedValue = await HandleTranslationRequest(sourceTokenValue ?? throw new InvalidOperationException(), sourceLanguage, language);
                        List<string> tokenPath;
                        try
                        {
                            tokenPath = _jsonUtility.ExtractTokenPath(pathToToken, tokenKeyValue);
                        }
                        catch (Exception e)
                        {
                            await Task.Delay(1000);
                            Console.WriteLine(e);
                            Logger.LogMessage(e + " TokenPath: " + token.Path);
                            Logger.LogMessage("Please contact your administrator!");
                            throw;
                        }
                        _jsonUtility.AddToJObjectKey(targetLanguage, tokenPath, translatedValue, tokenKeyValue);
                    });
                    await Task.WhenAll(translationTasks);
                }
                Logger.LogMessage($"Finished translating for {language}!");
            }
            
            JObject sortedJson = _jsonUtility.SortLanguages(fileObject);
            await File.WriteAllTextAsync(Path.Combine(filePath, fileName), sortedJson.ToString());
            TimeSpan translationTime = DateTime.Now - dateAndTime;
            Logger.LogMessage($"Translation finished! Time: {translationTime:mm\\:ss\\:ff}");
        }

        // Update connect language files
        public async Task UpdateSourceToTargetLanguage(string sourceFilename, string targetFileName, string path)
        {
            const string sourceLanguage = "EN";
            Logger.LogMessage($"Source Path: {path}\\{sourceFilename}");
            Logger.LogMessage($"Target Path: {path}\\{targetFileName}");

            // Determine the target language based on the target file name
            string targetLanguage = targetFileName.Substring(0, 2).ToUpper();

            (string jsonSource, string startString) = _jsonUtility.ReadAndCleanJson(Path.Combine(path, sourceFilename), true);
            string jsonTarget = _jsonUtility.ReadAndCleanJson(Path.Combine(path, targetFileName));

            // Parse the JSON content into JObject instances
            JObject source = JObject.Parse(jsonSource);
            JObject target = JObject.Parse(jsonTarget);

            string csvString = "";
            DateTime dateAndTime = DateTime.Now;

            var translationExceptions = new Dictionary<string, List<string>>();
            var tokenBatches = source.Descendants().Where(t => t.Type == JTokenType.String).Batch(BatchSize);
            foreach (var tokenBatch in tokenBatches)
            {
                var translationTasks = tokenBatch.Select(async token =>
                {
                    string pathToToken = token.Path;
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
                        _jsonUtility.AddToJObject(target, pathToToken.Split('.'), translation);

                        logString =
                            $"{dateAndTime};{sourceLanguage};{targetLanguage}; {pathToToken}; {token.Value<string>()}; {target.SelectToken(pathToToken)}";
                        System.Diagnostics.Debug.WriteLine($"--- Translated {logString}");
                        logString += Environment.NewLine;
                        // ReSharper disable once MethodHasAsyncOverload
                        File.AppendAllText(Path.Combine(path, $"{dateAndTime.ToShortDateString()}-log.csv"), logString);
                    }
                    else
                    {
                        logString =
                            $"{dateAndTime};{sourceLanguage};{targetLanguage}; {pathToToken}; {token.Value<string>()}; {target.SelectToken(pathToToken)}";
                        logString += Environment.NewLine;
                    }

                    csvString += logString;
                });

                await Task.WhenAll(translationTasks);
            }

            _jsonUtility.RemoveMissingKeys(target, source);
            
            string newTarget = _jsonUtility.UpdateTargetTranslationsOrder(source.ToString()!, target.ToString()!, targetFileName);

            //write json
            string cleanTarget = startString + _jsonUtility.FormatJsonToTypeScript(newTarget);
            await File.WriteAllTextAsync(Path.Combine(path, targetFileName), cleanTarget);
            //write csv
            await File.WriteAllTextAsync(Path.Combine(path, $"{sourceLanguage}-{targetLanguage}.csv"), csvString);
            
            TimeSpan translationTime = DateTime.Now - dateAndTime;
            Logger.LogMessage($"Translation finished! Time: {translationTime:mm\\:ss\\:ff}");
        }
        
        private async Task<string> HandleTranslationRequest(string translation, string sourceLanguage, string targetLanguage)
        {
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
            Logger.LogMessage($"Translating: {glossaryName}: {translatedText} -> {logMessage} -> {targetLanguage}: {translatedText}");
            return translatedText.ToString();
        }
        
        private bool TranslateEmptyToken(JObject targetLanguage, string tokenKeyValue, string language, string sourceTokenValue, string? targetTokenValue)
        {
            
            if (!_jsonUtility.IsKeyInTargetLanguage(targetLanguage, tokenKeyValue)) return true;
            if (language == "en")
            {
                return false;
            }
            // Check of er iets staat na de ':'
            var regex = new Regex(@":\s*$");
            
            if (regex.IsMatch(sourceTokenValue)) return false;

            if (targetTokenValue == null) return true;
            
            if (!targetTokenValue.Contains(':') || !regex.IsMatch(targetTokenValue)) return false;
            Logger.LogMessage($"Translating empty token value! {targetTokenValue}");
            return true;
        }
    }
}