using DeepL;
using DeepL.Model;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DeeplTranslator
{
    public class TxtConverter
    {
        private readonly JsonUtility _jsonUtility;
        private readonly Translator _translator;
        private readonly Dictionary<string, string> _alarmColors;
        private readonly string[] _exceptions;
        public TxtConverter(string authKey, string[] exceptions)
        {
            this._translator = new Translator(authKey);
            this._exceptions = exceptions;
            _jsonUtility = new JsonUtility();
            _alarmColors = new Dictionary<string, string>()
            {
                {"blue", "blauw"},
                {"yellow", "geel"},
                {"red", "rood"},
                {"gray", "grijs"},
                {"black", "zwart"},
                {"purple", "paars"}
            };
        }

        public async Task ConvertFileToJson(string filePath, string fileName)
        {
            Logger.LogMessage($"Converting: {filePath}\\{fileName}");

            string alarmColor = GetAlarmColorFromFilename(fileName);
            var lines = (await File.ReadAllLinesAsync(Path.Combine(filePath, fileName), Encoding.UTF8)).ToList();
            
            //First line are the different languages in order
            Logger.LogMessage(lines[0]);
            var languagesCodes = await GetLanguagesFromFile(lines[0]);
            lines.Remove(lines[0]);
            
            var translations = new Dictionary<string, Dictionary<string, string>>();

            foreach (string line in lines)
            {
                string _line = line;
                if (line.Contains('$'))
                {
                    _line = line.Replace("$", "");
                }
                
                Logger.LogMessage(_line);
                var values = _line.Split(';').Select(value => value.Trim()).ToList();
                string id = values[0];
                values.RemoveAt(0);

                for (int i = 0; i < languagesCodes.Count; i++)
                {
                    string languageCode = languagesCodes[i];
                    string translation = values[i];

                    if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(translation)) continue;
                    
                    if (!translations.ContainsKey(languageCode))
                    {
                        translations[languageCode] = new Dictionary<string, string>();
                    }

                    if (_exceptions.Any(exception => id.Contains(exception)))
                    {
                        translations[languageCode][(id)] = translation;
                    }
                    else
                    {
                        translations[languageCode][(alarmColor + id)] = translation;
                    }
                }
            }
            
            string jsonString = JsonSerializer.Serialize(translations, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            //write json
            string newFileName = Path.ChangeExtension(fileName, ".json");
            await File.WriteAllTextAsync(Path.Combine(filePath, newFileName), jsonString, Encoding.UTF8);
        }

        public async Task ConvertOldKeysToNew(string filePath, string fileName)
        {
            Logger.LogMessage($"Started Converting old keys for {fileName}");
            // Read the JSON file into a JObject
            string jsonSource = await File.ReadAllTextAsync(Path.Combine(filePath, fileName));

            JObject fileObject = JObject.Parse(jsonSource);

            foreach (JProperty languageSection in fileObject.Children())
            {
                string languageCode = languageSection.Name;
                int count = 0;

                // Dictionary to keep track of unique values and their counts
                Dictionary<string, int> valueCounts = new Dictionary<string, int>();

                foreach (JProperty property in languageSection.Value.OfType<JProperty>().ToList())
                {
                    count++;
                    Logger.LogMessage($"Converting {count}/{languageSection.Value.OfType<JProperty>().ToList().Count} of {languageCode}");
                    string originalValue = property.Value.ToString();

                    if (!originalValue.Contains(':'))
                    {
                        Logger.LogMessage($"{originalValue} does not contain ':'");
                        continue;
                    }

                    string[] parts = originalValue.Split(':');

                    if (parts.Length == 2)
                    {
                        // Extract "red 12" and remove spaces
                        string newKey = parts[0].Trim().Replace(" ", "");

                        // Check if the new key already exists
                        JToken existingToken = languageSection.Value[newKey];
                        if (existingToken != null)
                        {
                            // If the value is already encountered, append "-double" to the key
                            if (valueCounts.TryGetValue(parts[1].Trim(), out int valueCount))
                            {
                                newKey += $"-double-{valueCount}";
                                valueCount++;
                                valueCounts[parts[1].Trim()] = valueCount;
                            }
                            else
                            {
                                // If encountering the value for the first time, add it to the dictionary
                                valueCounts.Add(parts[1].Trim(), 2);
                                newKey += "-double";
                            }

                            Logger.LogMessage($"{newKey} already exists in {languageCode}. Appending '-double'.");
                        }

                        // Update the key with the new value
                        property.Replace(new JProperty(newKey, originalValue));
                    }
                }
            }

            // Create a new file with the updated JObject
            string newFileName = "NEW" + fileName;
            await File.WriteAllTextAsync(Path.Combine(filePath, newFileName), fileObject.ToString(), Encoding.UTF8);
            Logger.LogMessage("Finished Converting!");
            Console.WriteLine("New file created: " + newFileName);
        }

        private async Task<List<string>> GetLanguagesFromFile(string languages)
        {
            var excludedLanguageNames = new HashSet<string> { "Id", "Default" };

            var translatedLanguages = await TranslateLanguageNames(languages.Split(";").Except(excludedLanguageNames).ToList());

            var specificCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
    
            var newLanguageList = translatedLanguages
                .Select(language =>
                {
                    CultureInfo? culture = specificCultures.FirstOrDefault(c =>
                        Regex.Replace(c.EnglishName, @"\s*\([^)]*\)\s*", "")
                            .Equals(language, StringComparison.OrdinalIgnoreCase));

                    return culture?.TwoLetterISOLanguageName;
                })
                .Where(code => !String.IsNullOrEmpty(code))
                .ToList();

            var reversedList = excludedLanguageNames.Reverse().ToList();
            
            foreach (string item in reversedList.Where(item => item != "Id"))
            {
                newLanguageList.Insert(0, item);
            }
            
            return newLanguageList;
        }

        private async Task<List<string>> TranslateLanguageNames(List<string> languageNames)
        {
            var translatedLanguages = new List<string>();
            foreach (string language in languageNames)
            {
                TextResult translatedText = await _translator.TranslateTextAsync
                (
                    language,
                    "nl",
                    "en-GB"
                );
                translatedLanguages.Add(translatedText.ToString());
            }

            return translatedLanguages;
        }

        private string GetAlarmColorFromFilename(string fileName)
        {
            foreach (string color in _alarmColors.Keys.Where(fileName.Contains))
            {
                return _alarmColors[color];
            }
            return String.Empty;
        }
    }
}