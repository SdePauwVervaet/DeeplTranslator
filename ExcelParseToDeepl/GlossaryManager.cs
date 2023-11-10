using DeepL;
using DeepL.Model;

namespace DeeplTranslator
{
    public class GlossaryManager
    {
        private readonly Translator _translator;
        public GlossaryManager(string authKey)
        {
            _translator = new Translator(authKey);
        }
        
        public async Task<string> CheckUsage()
        {
            Usage usage = await _translator.GetUsageAsync();
            System.Diagnostics.Debug.WriteLine("Deepl:");
            if (usage.AnyLimitReached)
            {
                return "Translation limit exceeded.";
            }
            return usage.Character != null ? $"Character usage: {usage.Character}" : $"{usage}";
        }

        public async Task<string> CheckForGlossaries()
        {
            string returnString = "";
            try
            {
                var glossaries = await _translator.ListGlossariesAsync();

                System.Diagnostics.Debug.WriteLine("Current glossaries...");
                foreach (GlossaryInfo glossaryInfo in glossaries)
                {
                    returnString += $"Creating {glossaryInfo.Name}: Source:{glossaryInfo.SourceLanguageCode} Target:{glossaryInfo.TargetLanguageCode} Count:{glossaryInfo.EntryCount}  \r\n";
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
        public async Task<bool> CheckForExistingGlossary(string glossaryName)
        {
            var glossaries = await _translator.ListGlossariesAsync();
            return glossaries.Any(g => String.Equals(g.Name, glossaryName, StringComparison.CurrentCultureIgnoreCase));

        }
        public async Task<GlossaryInfo?> GetGlossaryByName(string glossaryName)
        {
            GlossaryInfo?[] glossaries = await _translator.ListGlossariesAsync();

            return glossaries.FirstOrDefault(g => String.Equals(g!.Name, glossaryName, StringComparison.CurrentCultureIgnoreCase));

        }
        public async Task<string> CreateGlossaryFromDictionary(string sourceLanguage, string targetLanguage, Dictionary<string, string> dictionary)
        {
            string glossaryName = $"{sourceLanguage}-{targetLanguage}";
            string returnString;
            try
            {
                GlossaryInfo unused = await _translator.CreateGlossaryAsync(
                    glossaryName, sourceLanguage, targetLanguage,
                    new GlossaryEntries(dictionary));
                returnString = $"Creating Glossary at Deepl{sourceLanguage}-{targetLanguage}";
            }
            catch (Exception ex)
            {
                returnString = $"Creating a glossary for {targetLanguage} failed {ex.Message}";
            }

            return returnString;
        }
        public async Task<string> DeleteGlossaries()
        {
            try
            {
                var glossaries = await _translator.ListGlossariesAsync();
                foreach (var glossaryInfo in glossaries)
                {
                    await _translator.DeleteGlossaryAsync(glossaryInfo.GlossaryId);
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
    }
}