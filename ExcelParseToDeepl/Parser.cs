using ExcelParseToDeepl;
using SpreadsheetLight;

namespace DeeplTranslator
{
    internal class Parser
    {
        SLDocument wb = new SLDocument();
        SLWorksheetStatistics stats = new SLWorksheetStatistics();
        string filePath = "";
        List<Glossary> translationList = new List<Glossary> { };
        string DeeplAuthKey = "059d0c16-9fed-8d68-544b-2b9d0413c4b3:fx";
        Translator translator;

        public Parser()
        {
            translator = new Translator(DeeplAuthKey);
        }

        public async Task TranslateAlertFiles(List<string> files, Func<string, Task> notificationCallback)
        {
            foreach (string file in files)
            {
                notificationCallback.Invoke(await translator.UpdateTranslation("en", file, notificationCallback));
            }
        }

        public async Task TranslateLanguageFiles(List<string> files, Func<string, Task> notificationCallback)
        {
            foreach (string file in files)
            {
                string targetFileName = file.Substring(file.Length - 8).ToLower();
                string sourceFileName = "en-gb.js";
                string path = Path.GetDirectoryName(file);

                if (!targetFileName.Equals(sourceFileName))
                {
                    notificationCallback.Invoke(await translator.UpdateLanguage(sourceFileName, targetFileName, path, notificationCallback));

                    System.Diagnostics.Debug.WriteLine(targetFileName);
                }
            }
        }

        public void ParseExcel(string path, string fileName)
        {
            filePath = path;
            MemoryStream ms = LoadStream($"{filePath}/{fileName}");
            wb = new SLDocument(ms);
            stats = wb.GetWorksheetStatistics();

        }

        public void GenerateDictionaries(Action<string> notificationCallback)
        {
            notificationCallback.Invoke("Generating!");

            for (int c = 1; c < stats.NumberOfColumns; c++)
            {
                Glossary glossary = new Glossary();
                //Dictionary<string,string> dictionary = new Dictionary<string, string>();
                Dictionary<string, string> duplicateEntries = new Dictionary<string, string>();

                string sourceKey = wb.GetCellValueAsString(1, 1);
                string targetKey = wb.GetCellValueAsString(1, c + 1);
                notificationCallback.Invoke($"Generating glossary {sourceKey}-{targetKey}" + Environment.NewLine);

                for (int i = 2; i <= stats.NumberOfRows; i++)
                {
                    glossary.SourceLanguage = sourceKey;
                    glossary.TargetLanguage = targetKey;
                    Translation translation = new Translation();
                    translation.SourceKey = sourceKey;
                    translation.TargetKey = targetKey;
                    translation.SourceText = wb.GetCellValueAsString(i, 1).Trim();
                    translation.TargetText = wb.GetCellValueAsString(i, c + 1).Trim();

                    if (translation.Check())
                    {
                        bool canAdd = glossary.Translations.TryAdd(translation.SourceText, translation.TargetText);
                        if (!canAdd)
                        {
                            duplicateEntries.Add($"{i}_{translation.SourceText}", translation.TargetText);
                        }
                        notificationCallback.Invoke($"Added {translation.SourceText}, {translation.TargetText}, {translation.SourceText} to dictionary");
                    }
                }
                notificationCallback("Writing glossary and glossary DUPLICATES files..." + Environment.NewLine);
                //create glossary file
                WriteCSV(filePath, sourceKey, targetKey, glossary.Translations);
                //create overview of duplicates file
                WriteCSV(filePath, $"Duplicates-{sourceKey}", $"Duplicates-{targetKey}", duplicateEntries);

                translationList.Add(glossary);

            }
            //UpdateDeeplGlossary();
        }
        public async void UpdateDeeplGlossary(Action<string> notificationCallback)
        {
            //get current usage and prompt user
            notificationCallback.Invoke(await translator.CheckUsage() + Environment.NewLine);


            //delete all existing glossaries
            notificationCallback.Invoke(await translator.DeleteGlossaries() + Environment.NewLine);

            //create glossaries
            foreach (Glossary glossary in translationList)
            {
                //notificationCallback.Invoke(
                notificationCallback.Invoke(await translator.CreateGlossaryFromDictionary(glossary.SourceLanguage, glossary.TargetLanguage, glossary.Translations));
            }

            //Show current glossaries
            notificationCallback.Invoke(await translator.CheckForGlossaries() + Environment.NewLine);
            notificationCallback.Invoke($"~~~Done with glossaries~~~" + Environment.NewLine);
        }


        private static MemoryStream LoadStream(string filePath)
        {
            using var inStream = new FileStream(filePath, FileMode.Open,
                              FileAccess.Read, FileShare.ReadWrite);
            MemoryStream ms = new MemoryStream();
            inStream.CopyTo(ms);
            ms.Position = 0;

            return ms;
        }

        private static void WriteCSV(string filePath, string sourceKey, string targetKey, Dictionary<string, string> dictionary)
        {
            string path = filePath + "/" + sourceKey + "-" + targetKey + ".csv";
            string lineToWrite = "";
            foreach (var kvp in dictionary)
            {
                lineToWrite += $"\"{kvp.Key}\",\"{kvp.Value}\",\"{sourceKey}\",\"{targetKey}\"\n";
            }
            if (!String.IsNullOrEmpty(lineToWrite))
            {
                System.IO.File.WriteAllText(path, lineToWrite);
            }
            System.Diagnostics.Debug.WriteLine(path);
        }
    }
}
