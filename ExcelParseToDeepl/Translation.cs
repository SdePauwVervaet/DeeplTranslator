namespace DeeplTranslator
{

    public class Translation
    {
        public string? SourceKey;
        public string? TargetKey;
        public string? SourceText;
        public string? TargetText;

        public bool Check()
        {
            return !String.IsNullOrWhiteSpace(SourceKey) &&
                   !String.IsNullOrWhiteSpace(TargetKey) && 
                   !String.IsNullOrWhiteSpace(SourceText) &&
                   !String.IsNullOrWhiteSpace(TargetText);
        }

        public string Format()
        {
            return ($"\"{ SourceText}\",\"{ TargetText}\",\"{ SourceKey}\",\"{ TargetKey}\"");
        }
    }
}
