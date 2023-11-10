namespace DeeplTranslator
{
    public class TranslatorService
    {
        private const string DeeplAuthKey = "059d0c16-9fed-8d68-544b-2b9d0413c4b3:fx";
        private readonly TranslatePerList _translatePerLine = new TranslatePerList(DeeplAuthKey);
        private readonly GlossaryManager _glossaryManager = new GlossaryManager(DeeplAuthKey);
        public readonly ExcelParser ExcelParser = new ExcelParser();

        public async Task TranslateAlertFiles(List<string> files)
        {
            foreach (string file in files)
            {
                await _translatePerLine.UpdateTranslationInFile("en", file);
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
                
                await _translatePerLine.UpdateSourceToTargetLanguage(sourceFileName, targetFileName, path);

                System.Diagnostics.Debug.WriteLine(targetFileName);
            }
        }
        
        public async void UpdateDeeplGlossary()
        {
            //get current usage and prompt user
            await Logger.LogMessage(await _glossaryManager.CheckUsage() + Environment.NewLine);


            //delete all existing glossaries
            await Logger.LogMessage(await _glossaryManager.DeleteGlossaries() + Environment.NewLine);

            //create glossaries
            foreach (Glossary glossary in ExcelParser.TranslationList)
            {
                await Logger.LogMessage(await _glossaryManager.CreateGlossaryFromDictionary(glossary.SourceLanguage, glossary.TargetLanguage, glossary.Translations));
            }

            //Show current glossaries
            await Logger.LogMessage(await _glossaryManager.CheckForGlossaries() + Environment.NewLine);
            await Logger.LogMessage($"~~~Done with glossaries~~~" + Environment.NewLine);
        }
    }
}