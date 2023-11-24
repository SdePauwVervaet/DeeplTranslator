using System.Text;
using Newtonsoft.Json.Linq;

namespace DeeplTranslator
{
    public class JsonMerger
    {
        public async Task MergeJObjects(string folderPath, string resultFilePath)
        {
            var objectsToMerge = await GetJsonObjectsFromFolder(folderPath);
            
            Logger.LogMessage(@"Started merging files.");
            // Merge the list of JObjects into a single JObject
            var mergedObject = new JObject();
            int count = 0;
            foreach (JObject jObject in objectsToMerge)
            {
                count++;
                Logger.LogMessage($@"Merging {count}/{objectsToMerge.Count}");
                mergedObject.Merge(jObject, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Merge
                });
            }

            // Save the merged JObject to a file
            await File.WriteAllTextAsync(resultFilePath, mergedObject.ToString(), Encoding.UTF8);
            Logger.LogMessage(@"Files merged succesfully.");
        }
        
        // Merging will not replace Value if its alread there. It will just be skipped.
        public async Task MergeJObjects(string folderPath, string resultFileName, string alarmfileLocation)
        {
            var objectsToMerge = await GetJsonObjectsFromFolder(folderPath);

            if (!File.Exists(Path.Combine(alarmfileLocation, resultFileName)))
            {
                MessageBox.Show(@$"File {resultFileName} not found!", @"File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            string fileContents = await File.ReadAllTextAsync(Path.Combine(alarmfileLocation, resultFileName));
            JObject alarmFile = JObject.Parse(fileContents);
            objectsToMerge.Add(alarmFile);
            
            Logger.LogMessage(@"Started merging files.");
            // Merge the list of JObjects into a single JObject
            var mergedObject = new JObject();
            int count = 0;
            foreach (JObject jObject in objectsToMerge)
            {
                count++;
                Logger.LogMessage($@"Merging {count}/{objectsToMerge.Count}");
                mergedObject.Merge(jObject, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Merge
                });
            }

            // Save the merged JObject to a file
            await File.WriteAllTextAsync(Path.Combine(alarmfileLocation, resultFileName), mergedObject.ToString(), Encoding.UTF8);
            Logger.LogMessage(@"Files merged succesfully.");
        }
        
        static Task<List<JObject>> GetJsonObjectsFromFolder(string folderPath)
        {
            var jsonObjects = new List<JObject>();

            try
            {
                // Get all JSON files in the specified folder
                string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

                foreach (string json in jsonFiles)
                {
                    Logger.LogMessage($@"Selecting {Path.GetFileName(json)}");
                }
                
                jsonObjects.AddRange(jsonFiles.Select(File.ReadAllText).Select(JObject.Parse));
            }
            catch (Exception ex)
            {
                MessageBox.Show(@$"An error occurred: {ex.Message}", @"Unexpected Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                Console.WriteLine(@$"An error occurred: {ex.Message}");
            }

            return Task.FromResult(jsonObjects);
        }
    }
}