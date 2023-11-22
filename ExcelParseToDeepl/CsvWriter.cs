namespace DeeplTranslator
{
    public abstract class CsvWriter
    {
        protected void WriteCsv(string filePath, string sourceKey, string targetKey, Dictionary<string, string> dictionary)
        {
            string path = filePath + "/" + sourceKey + "-" + targetKey + ".csv";
            string lineToWrite = dictionary.Aggregate("", (current, kvp) => current + $"\"{kvp.Key}\",\"{kvp.Value}\",\"{sourceKey}\",\"{targetKey}\"\n");
            if (!String.IsNullOrEmpty(lineToWrite))
            {
                File.WriteAllText(path, lineToWrite);
            }
            System.Diagnostics.Debug.WriteLine(path);
        }
    }
}