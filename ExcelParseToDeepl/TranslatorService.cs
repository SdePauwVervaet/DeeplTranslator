using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;

namespace DeeplTranslator
{
    public class TranslatorService
    {
        private const string DeeplAuthKey = "059d0c16-9fed-8d68-544b-2b9d0413c4b3:fx";
        private static readonly string[] Exceptions = { "VF.", "VolvoEngine.", "VolvoAcm." };
        private readonly BatchTranslator _batchTranslator = new BatchTranslator(DeeplAuthKey, Exceptions);
        private readonly TxtConverter _txtConverter = new TxtConverter(DeeplAuthKey, Exceptions);
        private readonly JsonMerger _jsonMerger = new JsonMerger();
        private readonly GlossaryManager _glossaryManager = new GlossaryManager(DeeplAuthKey);
        public readonly ExcelParser ExcelParser = new ExcelParser();

        public async Task TranslateAlertFiles(List<string> files)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");
             
                //await _translatePerLine.UpdateTranslationInFile(path, fileName);
                await _batchTranslator.UpdateTranslationInSameFile(path, fileName);
                
                System.Diagnostics.Debug.WriteLine(fileName);
            }
        }
        
        // Translate connect language files
        public async Task TranslateLanguageFiles(List<string> files)
        {
            foreach (string file in files)
            {
                string targetFileName = file.Substring(file.Length - 9).ToLower();
                const string sourceFileName = "en-gb.jsx";
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");

                if (targetFileName.Equals(sourceFileName)) continue;
                
                await _batchTranslator.UpdateSourceToTargetLanguage(sourceFileName, targetFileName, path);

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

        public async Task ConvertTxtFiles(string folderPath, string mergedFileName)
        {
            var txtFiles = Directory.GetFiles(folderPath, "*.txt").ToList();

            
            foreach (string file in txtFiles)
            {
                string fileName = Path.GetFileName(file);
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");
                await _txtConverter.ConvertFileToJson(path, fileName);
            }

            await MergeJsonFiles(folderPath, ("MERGED" + mergedFileName), "");
        }

        public async Task MergeJsonFiles(string folderPath, string mergedFileName, string alertsFolderPath)
        {
            if (!String.IsNullOrWhiteSpace(alertsFolderPath))
            {
                await _jsonMerger.MergeJObjects(folderPath, (mergedFileName), alertsFolderPath);
                return;
            }
            
            await _jsonMerger.MergeJObjects(folderPath, (Path.Combine(folderPath, mergedFileName)));
        }

        public async Task ConvertOldKeysToNewKeys(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.json").ToList();

            
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string path = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("file path is null");
                await _txtConverter.ConvertOldKeysToNew(path, fileName);
            }
        }
    }
}