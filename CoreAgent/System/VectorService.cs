using CoreAgent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreAgent.System
{
    public class VectorService
    {
        private static string BasePath = @"F:\repos\AgentSwarmGang\CoreAgent\CoreAgent\Agent\Storage\AgentStorage\";

        public static void SaveVectorData(string folderName, List<VectorDbItem> items)
        {
            var folderPath = EnsureFolderExists(folderName);
            var filePath = Path.Combine(folderPath, "vectorDb.json");
            var options = new JsonSerializerOptions { WriteIndented = false };
            var json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(filePath, json);
        }

        public static void AppendVectorDataItem(string folderName, VectorDbItem newItem)
        {
            var folderPath = EnsureFolderExists(folderName);
            var filePath = Path.Combine(folderPath, "vectorDb.json");
            List<VectorDbItem> items;

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                items = JsonSerializer.Deserialize<List<VectorDbItem>>(json) ?? new List<VectorDbItem>();
            }
            else
            {
                items = new List<VectorDbItem>();
            }

            items.Add(newItem);
            var options = new JsonSerializerOptions { WriteIndented = false };
            var updatedJson = JsonSerializer.Serialize(items, options);
            File.WriteAllText(filePath, updatedJson);
        }

        public static List<VectorDbItem> LoadVectorData(string folderName)
        {
            var folderPath = EnsureFolderExists(folderName);
            var filePath = Path.Combine(folderPath, "vectorDb.json");
            if (!File.Exists(filePath))
            {
                var emptyList = new List<VectorDbItem>();
                var options = new JsonSerializerOptions { WriteIndented = false };
                var eJson = JsonSerializer.Serialize(emptyList, options);
                File.WriteAllText(filePath, eJson); 
                return emptyList;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<VectorDbItem>>(json) ?? new List<VectorDbItem>();
        }

        private static string EnsureFolderExists(string folderName)
        {
            var fullPath = Path.Combine(BasePath, folderName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }
    }
}
