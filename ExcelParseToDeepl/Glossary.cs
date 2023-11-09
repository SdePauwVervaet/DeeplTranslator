using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeeplTranslator
{
    public class Glossary
    {
        public string SourceLanguage;
        public string TargetLanguage;
        public readonly Dictionary<string, string> Translations;

        public Glossary(string sourceLanguage, string targetLanguage)
        {
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
            Translations = new Dictionary<string, string>();
        }
    }
}
