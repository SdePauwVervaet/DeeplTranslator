using SpreadsheetLight;

namespace DeeplTranslator
{
    public class ExcelParser : CsvWriter
    {
        public readonly List<Glossary> TranslationList = new List<Glossary>();
        private SLDocument _wb = new SLDocument();
        private SLWorksheetStatistics _stats = new SLWorksheetStatistics();
        private string _filePath = "";
        
        public void ParseExcel(string path, string fileName)
        {
            _filePath = path;
            MemoryStream ms = LoadStream($"{_filePath}/{fileName}");
            _wb = new SLDocument(ms);
            _stats = _wb.GetWorksheetStatistics();
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

        public void GenerateDictionaries()
        {
            Logger.LogMessage("Generating!");

            for (int c = 1; c < _stats.NumberOfColumns; c++)
            {
                var duplicateEntries = new Dictionary<string, string>();

                string sourceKey = _wb.GetCellValueAsString(1, 1);
                string targetKey = _wb.GetCellValueAsString(1, c + 1);
                var glossary = new Glossary(sourceKey, targetKey);
                Logger.LogMessage($"Generating glossary {sourceKey}-{targetKey}" + Environment.NewLine);

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
                    Logger.LogMessage($"Added {translation.SourceText}, {translation.TargetText}, {translation.SourceText} to dictionary");
                }
                Logger.LogMessage("Writing glossary and glossary DUPLICATES files..." + Environment.NewLine);
                
                //create glossary file
                WriteCsv(_filePath, sourceKey, targetKey, glossary.Translations);
                
                //create overview of duplicates file
                WriteCsv(_filePath, $"Duplicates-{sourceKey}", $"Duplicates-{targetKey}", duplicateEntries);

                TranslationList.Add(glossary);
            }
        }
    }
}