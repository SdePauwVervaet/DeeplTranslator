namespace DeeplTranslator
{
    public class TranslatorService
    {
        private const string DeeplAuthKey = "059d0c16-9fed-8d68-544b-2b9d0413c4b3:fx";
        private readonly BatchTranslator _line = new BatchTranslator(DeeplAuthKey);
        private readonly GlossaryManager _glossaryManager = new GlossaryManager(DeeplAuthKey);
        public readonly ExcelParser ExcelParser = new ExcelParser();

        public async Task TranslateAlertFiles(List<string> files)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");
             
                //await _translatePerLine.UpdateTranslationInFile(path, fileName);
                _line.UpdateTranslationInSameFile(path, fileName);
                
                System.Diagnostics.Debug.WriteLine(fileName);
            }
        }
        
        // Translate connect language files
        public async Task TranslateLanguageFiles(List<string> files)
        {
            foreach (string file in files)
            {
                string targetFileName = file.Substring(file.Length - 8).ToLower();
                const string sourceFileName = "en-gb.js";
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");

                if (targetFileName.Equals(sourceFileName)) continue;
                
                await _line.UpdateSourceToTargetLanguage(sourceFileName, targetFileName, path);

                System.Diagnostics.Debug.WriteLine(targetFileName);
            }
        }
        
        public async void UpdateDeeplGlossary()
        {
            //get current usage and prompt user
            Logger.LogMessage(await _glossaryManager.CheckUsage() + Environment.NewLine);


            //delete all existing glossaries
            Logger.LogMessage(await _glossaryManager.DeleteGlossaries() + Environment.NewLine);

            //create glossaries
            foreach (Glossary glossary in ExcelParser.TranslationList)
            {
                Logger.LogMessage(await _glossaryManager.CreateGlossaryFromDictionary(glossary.SourceLanguage, glossary.TargetLanguage, glossary.Translations));
            }

            //Show current glossaries
            Logger.LogMessage(await _glossaryManager.CheckForGlossaries() + Environment.NewLine);
            Logger.LogMessage($"~~~Done with glossaries~~~" + Environment.NewLine);
        }
    }
}