using DeepL;
using DeepL.Model;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Diagrams;

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