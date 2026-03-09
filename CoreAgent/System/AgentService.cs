using System.Text.Json;
using System.IO;
using System.Linq;
using CoreAgent.Data;
using CoreAgent.Agent;

public class AgentService
{
    private static string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Agent", "Storage", "AgentStorage");

    public static bool AgentDataExists(string folderName)
    {
        var folderPath = Path.Combine(BasePath, folderName);
        return File.Exists(Path.Combine(folderPath, "agentData.json")) ||
               File.Exists(Path.Combine(folderPath, "internalDialog.json")) ||
               File.Exists(Path.Combine(folderPath, "vectorDb.json"));
    }

    public static void SaveAgent(string folderName, Agent agent)
    {
        var folderPath = EnsureFolderExists(folderName);
        SaveAgentToJson(agent, Path.Combine(folderPath, "agentData.json"));
        SaveInternalDialog(agent, Path.Combine(folderPath, "internalDialog.json"));
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

    private static void SaveAgentToJson(Agent agent, string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var tempAgent = new Agent
        {
            Goal = agent.Goal,
            ExtraInfo = agent.ExtraInfo,
            CurrentOperation = agent.CurrentOperation,
            CurrentPlan = agent.CurrentPlan,
            TaskList = agent.TaskList,
            InternalDialog = new List<DialogEntry>(),
            VectorDb = new List<VectorDbItem>()
        };
        var json = JsonSerializer.Serialize(tempAgent, options);
        File.WriteAllText(filePath, json);
    }

    private static void SaveInternalDialog(Agent agent, string jsonPath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var dialogJson = JsonSerializer.Serialize(agent.InternalDialog, options);
        File.WriteAllText(jsonPath, dialogJson);
    }

    public static Agent LoadAgent(string folderName)
    {
        var folderPath = Path.Combine(BasePath, folderName);
        var agent = LoadAgentFromJson(Path.Combine(folderPath, "agentData.json"));
        agent.InternalDialog = LoadInternalDialogFromJson(Path.Combine(folderPath, "internalDialog.json"));
        return agent;
    }

    private static Agent LoadAgentFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        using (var doc = JsonDocument.Parse(json))
        {
            var root = doc.RootElement;

            var agent = new Agent();

            if (root.TryGetProperty("Goal", out var goal))
            {
                agent.Goal = goal.GetString();
            }
            if (root.TryGetProperty("ExtraInfo", out var extraInfo))
            {
                agent.ExtraInfo = extraInfo.GetString();
            }
            if (root.TryGetProperty("CurrentOperation", out var currentOperation))
            {
                agent.CurrentOperation = currentOperation.GetString();
            }
            if (root.TryGetProperty("CurrentPlan", out var currentPlan))
            {
                agent.CurrentPlan = currentPlan.GetString();
            }

            if (root.TryGetProperty("TaskList", out var taskList))
            {
                foreach (var item in taskList.EnumerateArray())
                {
                    var task = JsonSerializer.Deserialize<AgentTask>(item.GetRawText());
                    if (task != null)
                    {
                        agent.TaskList.Add(task);
                    }
                }
            }
            if (root.TryGetProperty("InternalDialog", out var internalDialog))
            {
                agent.InternalDialog = new List<DialogEntry>();
                foreach (var item in internalDialog.EnumerateArray())
                {
                    var entry = item.Deserialize<DialogEntry>();
                    if (entry != null)
                    {
                        agent.InternalDialog.Add(entry);
                    }
                }
            }

            return agent;
        }
    }

    private static List<DialogEntry> LoadInternalDialogFromJson(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<List<DialogEntry>>(json) ?? new List<DialogEntry>();
    }
}
