using CoreAgent.Api;
using CoreAgent.Data;
using CoreAgent.System;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreAgent.Agent
{
    public class Agent
    {
        public string folderName;
        public List<DialogEntry> InternalDialog { get; set; } = new List<DialogEntry>();
        public List<VectorDbItem> VectorDb { get; set; } = new List<VectorDbItem>();
        public LlamaInfer llamaInfer { get; set; } = new LlamaInfer();

        public Agent(string _folderName = "")
        {
            folderName = _folderName;
            TaskList = new List<AgentTask>();
            CompletedTaskList = new List<AgentTask>();
            if (_folderName != "")
            {
                InitializeAgent();
            }
        }

        public string Goal { get; set; }
        public string ExtraInfo { get; set; }

        public string CurrentOperation { get; set; }

        public string CurrentPlan { get; set; }
        public List<AgentTask> TaskList { get; set; }
        public List<AgentTask> CompletedTaskList { get; set; }

        public async Task Run()
        {
            bool _continue = true;
            Executor executor = new Executor();

            while (_continue)
            {
                _continue = await executor.ExecuteTask(this);
            }
        }

        private void InitializeAgent()
        {
            if (AgentService.AgentDataExists(folderName))
            {
                var loadedAgent = AgentService.LoadAgent(folderName);
                Goal = loadedAgent.Goal;
                ExtraInfo = loadedAgent.ExtraInfo;
                CurrentOperation = loadedAgent.CurrentOperation;
                CurrentPlan = loadedAgent.CurrentPlan;
                TaskList = loadedAgent.TaskList;
                CompletedTaskList = loadedAgent.CompletedTaskList;
                InternalDialog = loadedAgent.InternalDialog;
            }
            else
            {
                InternalDialog = new List<DialogEntry>();
            }
            VectorDb = VectorService.LoadVectorData(folderName);
        }

        public void Save()
        {
            AgentService.SaveAgent(folderName, this);
        }

        public void PrintAgentInfo()
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine("Agent Information:");
            Console.WriteLine($"Goal: {Goal}");
            Console.WriteLine($"ExtraInfo: {ExtraInfo}");
            Console.WriteLine($"CurrentOperation: {CurrentOperation}");
            Console.WriteLine($"CurrentPlan: {CurrentPlan}");

            Console.WriteLine("Task List:");
            if (TaskList != null && TaskList.Any())
            {
                foreach (var task in TaskList)
                {
                    Console.WriteLine($"- Task Completed: {task.Completed}, Description: {task.Description}");
                }
            }
            else
            {
                Console.WriteLine("No tasks assigned.");
            }

            Console.WriteLine("Internal Dialog History:");
            if (InternalDialog != null && InternalDialog.Any())
            {
                Console.WriteLine("There is " + InternalDialog.Count() + " records.");
            }
            else
            {
                Console.WriteLine("No internal dialogs recorded.");
            }

            Console.WriteLine("=======================================");
            Console.WriteLine();
        }
    }
}
