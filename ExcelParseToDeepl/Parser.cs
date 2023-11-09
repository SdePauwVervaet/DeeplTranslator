using SpreadsheetLight;

namespace DeeplTranslator
{
    internal class Parser
    {
        private SLDocument _wb = new SLDocument();
        private SLWorksheetStatistics _stats = new SLWorksheetStatistics();
        private string _filePath = "";
        private readonly List<Glossary> _translationList = new List<Glossary>();
        private const string DeeplAuthKey = "059d0c16-9fed-8d68-544b-2b9d0413c4b3:fx";
        private readonly Translator _translator = new Translator(DeeplAuthKey);

        public async Task TranslateAlertFiles(List<string> files, Func<string, Task> notificationCallback)
        {
            foreach (string file in files)
            {
                await notificationCallback.Invoke(await _translator.UpdateTranslation("en", file, notificationCallback));
            }
        }
        // Translate connect language files
        public async Task TranslateLanguageFiles(List<string> files, Func<string, Task> notificationCallback)
        {
            foreach (string file in files)
            {
                string targetFileName = file.Substring(file.Length - 8).ToLower();
                const string sourceFileName = "en-gb.js";
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");

                if (targetFileName.Equals(sourceFileName)) continue;
                
                await _translator.UpdateLanguage(sourceFileName, targetFileName, path, notificationCallback);

                System.Diagnostics.Debug.WriteLine(targetFileName);
            }
        }

        public void ParseExcel(string path, string fileName)
        {
            _filePath = path;
            MemoryStream ms = LoadStream($"{_filePath}/{fileName}");
            _wb = new SLDocument(ms);
            _stats = _wb.GetWorksheetStatistics();

        }

        public void GenerateDictionaries(Action<string> notificationCallback)
        {
            notificationCallback.Invoke("Generating!");

            for (int c = 1; c < _stats.NumberOfColumns; c++)
            {
                var duplicateEntries = new Dictionary<string, string>();

                string sourceKey = _wb.GetCellValueAsString(1, 1);
                string targetKey = _wb.GetCellValueAsString(1, c + 1);
                var glossary = new Glossary(sourceKey, targetKey);
                notificationCallback.Invoke($"Generating glossary {sourceKey}-{targetKey}" + Environment.NewLine);

                for (int i = 2; i <= _stats.NumberOfRows; i++)
                {
                    glossary.SourceLanguage = sourceKey;
                    glossary.TargetLanguage = targetKey;
                    var translation = new Translation
                    {
                        SourceKey = sourceKey,
                        TargetKey = targetKey,
                        SourceText = _wb.GetCellValueAsString(i, 1).Trim(),
                        TargetText = _wb.GetCellValueAsString(i, c + 1).Trim()
                    };

                    if (!translation.Check()) continue;
                    
                    bool canAdd = glossary.Translations.TryAdd(translation.SourceText, translation.TargetText);
                    if (!canAdd)
                    {
                        duplicateEntries.Add($"{i}_{translation.SourceText}", translation.TargetText);
                    }
                    notificationCallback.Invoke($"Added {translation.SourceText}, {translation.TargetText}, {translation.SourceText} to dictionary");
                }
                notificationCallback("Writing glossary and glossary DUPLICATES files..." + Environment.NewLine);
                
                //create glossary file
                WriteCsv(_filePath, sourceKey, targetKey, glossary.Translations);
                
                //create overview of duplicates file
                WriteCsv(_filePath, $"Duplicates-{sourceKey}", $"Duplicates-{targetKey}", duplicateEntries);

                _translationList.Add(glossary);

            }
            //UpdateDeeplGlossary();
        }
        public async void UpdateDeeplGlossary(Action<string> notificationCallback)
        {
            //get current usage and prompt user
            notificationCallback.Invoke(await _translator.CheckUsage() + Environment.NewLine);


            //delete all existing glossaries
            notificationCallback.Invoke(await _translator.DeleteGlossaries() + Environment.NewLine);

            //create glossaries
            foreach (Glossary glossary in _translationList)
            {
                //notificationCallback.Invoke(
                notificationCallback.Invoke(await _translator.CreateGlossaryFromDictionary(glossary.SourceLanguage, glossary.TargetLanguage, glossary.Translations));
            }

            //Show current glossaries
            notificationCallback.Invoke(await _translator.CheckForGlossaries() + Environment.NewLine);
            notificationCallback.Invoke($"~~~Done with glossaries~~~" + Environment.NewLine);
        }


        private static MemoryStream LoadStream(string filePath)
        {
            using var inStream = new FileStream(filePath, FileMode.Open,
                              FileAccess.Read, FileShare.ReadWrite);
            var ms = new MemoryStream();
            inStream.CopyTo(ms);
            ms.Position = 0;

            return ms;
        }

        private static void WriteCsv(string filePath, string sourceKey, string targetKey, Dictionary<string, string> dictionary)
        {
            string path = filePath + "/" + sourceKey + "-" + targetKey + ".csv";
            string lineToWrite = dictionary.Aggregate("", (current, kvp) => current + $"\"{kvp.Key}\",\"{kvp.Value}\",\"{sourceKey}\",\"{targetKey}\"\n");
            if (!String.IsNullOrEmpty(lineToWrite))
            {
                File.WriteAllText(path, lineToWrite);
            }
            System.Diagnostics.Debug.WriteLine(path);
        }
    }
}
