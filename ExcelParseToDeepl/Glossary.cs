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
        public Dictionary<string, string> Translations;

        public Glossary()
        {
            Translations= new Dictionary<string, string>();
        }
    }
}
