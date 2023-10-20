using DeepL;
using DeepL.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExcelParseToDeepl
{

    public class Translator

    {
        DeepL.Translator translator;
        string[] exceptions = { "VF.", "VolvoEngine.", "VolvoAcm." };

        public Translator(string authKey)
        {
            this.translator = new DeepL.Translator(authKey);
        }

        public async Task<string> UpdateTranslation(string sourceLanguage, string filepath, Action<string> notificationCallback)//string[] args)
        {
            string jsonSource = File.ReadAllText(filepath);
            jsonSource = jsonSource.Substring(jsonSource.IndexOf('{')); //remove all characters before actual Json structure begins(everything before '{')

            JToken source = JToken.Parse(jsonSource);
            JToken target = JToken.Parse(jsonSource);
            JToken compareTo = source.SelectToken(sourceLanguage);
            string returnString = "";

            foreach (JToken s in source)
            {
                string currentLanguage = s.Path.ToString();
                returnString = ($"Started translating {filepath} to {currentLanguage}");

                if (currentLanguage != sourceLanguage)
                {
                    JToken compare = JToken.Parse(jsonSource).SelectToken(currentLanguage);
                    JToken output = target.SelectToken(currentLanguage);

                    foreach (JProperty c in compareTo)
                    {
                        //don't translate engine faults.
                        if (exceptions.Any(c.Name.Contains))
                        {

                            Console.WriteLine($"exception found. Not translating {c.Name}");
                            continue;

                        }
                        bool translationFound = false;
                        foreach (JProperty t in compare)
                        {
                            if (JToken.DeepEquals(c.Name, t.Name))
                            {
                                translationFound = true;
                                break;
                            }
                        }

                        if (!translationFound)
                        {
                            var translation = await Translate(c.Value.ToString(), sourceLanguage, currentLanguage, notificationCallback);
                            output[c.Name] = translation;
                        }
                    }
                }
            }
            File.WriteAllText(filepath, target.ToString());
            returnString += target.ToString();
            return returnString;
        }

        public async Task<bool> CheckForExistingGlossary(string GlossaryName)
        {
            var glossaries = await translator.ListGlossariesAsync();
            foreach (var g in glossaries)
            {
                if (g.Name.ToUpper() == GlossaryName.ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<GlossaryInfo> GetGlossaryByName(string GlossaryName)
        {
            var glossaries = await translator.ListGlossariesAsync();

            foreach (var g in glossaries)
            {
                if (g.Name.ToUpper() == GlossaryName.ToUpper())
                {
                    return g;
                }
            }
            return null;
        }

        public async Task<string> Translate(string translation, string sourceLanguage, string targetLanguage, Action<string> notificationCallback)
        {

            //System.Diagnostics.Debug.WriteLine(g.Name);
            string glossaryName = $"{sourceLanguage}-{targetLanguage}";
            bool useGlossary = await CheckForExistingGlossary(glossaryName);
            if (useGlossary)
            {
                var g = await GetGlossaryByName(glossaryName);

                var translatedText = await translator.TranslateTextAsync
                (
                    translation,
                    sourceLanguage,
                    targetLanguage,
                    new TextTranslateOptions { GlossaryId = g.GlossaryId }
                    );
                //System.Diagnostics.Debug.WriteLine($"translating with glossary {sourceLanguage}-{targetLanguage} {translation} ||| {translatedText.ToString()}");
                notificationCallback.Invoke($"translating with glossary {sourceLanguage}-{targetLanguage} {translation} ||| {translatedText.ToString()}");
                return translatedText.ToString();
            }
            else
            {
                var translatedText = await translator.TranslateTextAsync
                (
                    translation,
                    sourceLanguage,
                    targetLanguage
                );
                //System.Diagnostics.Debug.WriteLine("translating without glossary");
                notificationCallback.Invoke($"translating without glossary {sourceLanguage}-{targetLanguage} {translation} ||| {translatedText.ToString()}");
                return translatedText.ToString();
            }

            //return translated;
        }
        public async Task<string> CheckUsage()
        {
            var usage = await translator.GetUsageAsync();
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

        public async Task<string> CheckForGlossaries()
        {
            string returnString = "";
            try
            {
                var glossaries = await translator.ListGlossariesAsync();

                System.Diagnostics.Debug.WriteLine("Current glossaries...");
                for (int i = 0; i < glossaries.Length; i++)
                {
                    returnString += $"Creating {glossaries[i].Name}: Source:{glossaries[i].SourceLanguageCode} Target:{glossaries[i].TargetLanguageCode} Count:{glossaries[i].EntryCount}  \r\n";
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

        public async Task<string> DeleteGlossaries()
        {
            try
            {
                var glossaries = await translator.ListGlossariesAsync();
                for (int i = 0; i < glossaries.Length; i++)
                {
                    await translator.DeleteGlossaryAsync(glossaries[i].GlossaryId);
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



        public async Task<string> CreateGlossaryFromDictionary(string sourceLanguage, string targetLanguage, Dictionary<string, string> dictionary)
        {
            var entriesDictionary = dictionary;
            string glossaryName = $"{sourceLanguage}-{targetLanguage}";
            string returnString = "Hi!";
            try
            {
                var glossary = await translator.CreateGlossaryAsync(
                    glossaryName, sourceLanguage, targetLanguage,
                    new GlossaryEntries(entriesDictionary));
                returnString = $"Creating Glossary at Deepl{sourceLanguage}-{targetLanguage}";
            }
            catch (Exception ex)
            {
                returnString = $"Creating a glossary for {targetLanguage} failed {ex.Message}";

            }
            return returnString;
        }

        public async Task<string> UpdateLanguage(string sourceFilename, string targetFileName, string path, Action<string> notificationCallback)
        {
            string sourceLanguage = "EN";
            string targetLanguage = targetFileName.Substring(0, 2).ToUpper();
            string jsonSource = File.ReadAllText(path + "//" + sourceFilename);
            string startString = jsonSource.Substring(0, jsonSource.IndexOf('{')); //remove all characters before actual Json structure begins(everything before '{')
            jsonSource = jsonSource.Substring(jsonSource.IndexOf('{')); //remove all characters before actual Json structure begins(everything before '{')
            jsonSource = jsonSource.Substring(0, jsonSource.LastIndexOf('}') + 1); //remove all characters after closing accolade

            string jsonTarget = File.ReadAllText(path + "//" + targetFileName);
            if (!string.IsNullOrEmpty(jsonTarget))
            {
                jsonTarget = jsonTarget.Substring(jsonTarget.IndexOf('{')); //remove all characters before actual Json structure begins(everything before '{')
                jsonTarget = jsonTarget.Substring(0, jsonTarget.LastIndexOf('}') + 1); //remove all characters after closing accolade
            }
            else
            {
                jsonTarget = "{}";
            }
            JObject source = JObject.Parse(jsonSource);
            JObject target = JObject.Parse(jsonTarget);
            JToken compareTo = source.SelectToken(sourceLanguage);
            string returnString = "";
            string csvString = "";

            using (var reader = new JsonTextReader(new StringReader(jsonSource)))
            {
                while (reader.Read())
                {

                    //check entry
                    if (reader.TokenType == JsonToken.String &&
                        (reader.TokenType != JsonToken.StartObject &&
                        reader.TokenType != JsonToken.EndObject)
                    )
                    {
                        var dateAndTime = DateTime.Now;
                        string logString = "";

                        //unknown in target
                        if (target.SelectToken(reader.Path) == null)
                        {
                            string filename = $"{path}\\{dateAndTime.ToShortDateString()}-log.csv";
                            var translation = await Translate(reader.Value.ToString(), sourceLanguage, targetLanguage, notificationCallback);
                            AddToJObject(target, reader.Path.Split('.'), translation);
                            System.Diagnostics.Debug.WriteLine($"--- Translated {reader.Path} {reader.Value} {target.SelectToken(reader.Path)}");
                            logString = $"{dateAndTime};{sourceLanguage};{targetLanguage}; {reader.Path}; {reader.Value}; {target.SelectToken(reader.Path)} \n";
                            File.AppendAllText(filename, logString + Environment.NewLine);
                        }
                        //known in target
                        else
                        {
                            logString = $"{dateAndTime};{sourceLanguage};{targetLanguage}; {reader.Path}; {reader.Value}; {target.SelectToken(reader.Path)} \n";
                        }
                        csvString += logString;
                    }
                }
            }

            //clean up the target file. Remove all keys in the target file which is not in the source file (great for removing/updating a key)
            RemoveMissingKeys(target, source);

            //write json
            File.WriteAllText($"{path}//{targetFileName}", startString + target.ToString());
            //write csv
            File.WriteAllText($"{path}//{sourceLanguage}-{targetLanguage}.csv", csvString);
            returnString += target.ToString();
            return returnString;
        }

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
                AddToJObject(jObject[key] as JObject, keys.Skip(1).ToArray(), value);
            }
        }
        public static void RemoveMissingKeys(JObject obj1, JObject obj2)
        {
            foreach (var prop1 in obj1.Properties().ToList())
            {
                if (obj2[prop1.Name] == null)
                {
                    prop1.Remove();
                }
                else if (prop1.Value.Type == JTokenType.Object)
                {
                    RemoveMissingKeys((JObject)prop1.Value, (JObject)obj2[prop1.Name]);
                }
            }
        }

        public async Task<JToken> GetNodes()
        {
            return null;
        }
    }
}
