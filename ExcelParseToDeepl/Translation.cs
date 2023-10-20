using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParseToDeepl
{

    public class Translation
    {
        public string? SourceKey;
        public string? TargetKey;
        public string? SourceText;
        public string? TargetText;

        public Translation()
        {
        }

        public bool Check()
        {
            if (String.IsNullOrWhiteSpace(SourceKey) ||
                String.IsNullOrWhiteSpace(TargetKey) || 
                String.IsNullOrWhiteSpace(SourceText) ||
                String.IsNullOrWhiteSpace(TargetText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string Format()
        {
            return ($"\"{ SourceText}\",\"{ TargetText}\",\"{ SourceKey}\",\"{ TargetKey}\"");
        }



        
    }
}
