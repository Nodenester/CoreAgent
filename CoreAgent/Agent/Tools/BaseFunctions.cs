using CoreAgent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreAgent.Agent.Tools
{
    public class BaseFunctions
    {
        public async Task<string> ProcessToolCall(Agent agent, string toolCallJson)
        {
            try
            {
                toolCallJson = toolCallJson.Trim();

                // Extract the function name
                var nameStartIndex = toolCallJson.IndexOf("'name': '") + "'name': '".Length;
                var nameEndIndex = toolCallJson.IndexOf("'", nameStartIndex);
                var functionName = toolCallJson.Substring(nameStartIndex, nameEndIndex - nameStartIndex);

                // Extract the arguments as a string
                var argsStartIndex = toolCallJson.IndexOf("'arguments': {") + "'arguments': {".Length;
                var argsEndIndex = toolCallJson.LastIndexOf("}, 'name'");
                var argumentsString = toolCallJson.Substring(argsStartIndex, argsEndIndex - argsStartIndex);

                switch (functionName)
                {
                    case "AddTasks":
                        return HandleAddTasks(agent, argumentsString);
                    case "CompleteTask":
                        return HandleCompleteTask(agent, argumentsString);
                    case "RemoveTask":
                        return HandleRemoveTask(agent, argumentsString);
                    case "SetPlan":
                        return HandlePlanCreation(agent, argumentsString);
                    case "IsGoalAchieved":
                        if (argumentsString.Contains("true"))
                        {
                            return "Goal Achieved";
                        }
                        else
                        {
                            return "Only use this function if you have reached the goal by completing all the tasks.";
                        }
                    default:
                        return "|Error| Unrecognized function call. Please reevaluate what you just did and try again if necessary.";
                }
            }
            catch (Exception ex)
            {
                return $"|Error| executing function call: You constructed the json wrongly. Please reevaluate what you just did and try again if necessary.";
            }
        }

        private string HandleAddTasks(Agent agent, string arguments)
        {
            int tasksStartIndex = arguments.IndexOf("\"tasks\": [") + "\"tasks\": [".Length;
            int tasksEndIndex = arguments.IndexOf("]", tasksStartIndex);
            string tasksSubstring = arguments.Substring(tasksStartIndex, tasksEndIndex - tasksStartIndex);

            // Splitting the tasks substring into individual tasks assuming they are separated by '},'
            var tasks = tasksSubstring.Split(new string[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var taskString in tasks)
            {
                int idStartIndex = taskString.IndexOf("\"id\": \"") + "\"id\": \"".Length;
                int idEndIndex = taskString.IndexOf("\"", idStartIndex);
                string id = taskString.Substring(idStartIndex, idEndIndex - idStartIndex);

                int descriptionStartIndex = taskString.IndexOf("\"description\": \"") + "\"description\": \"".Length;
                int descriptionEndIndex = taskString.IndexOf("\"", descriptionStartIndex);
                string description = taskString.Substring(descriptionStartIndex, descriptionEndIndex - descriptionStartIndex);

                int completedStartIndex = taskString.IndexOf("\"completed\": ") + "\"completed\": ".Length;
                int completedEndIndex = taskString.IndexOf(",", completedStartIndex);
                bool completed = false;

                agent.TaskList.Add(new AgentTask(id, description, completed));
            }
            return "Tasks added successfully. Please reevaluate the task list and recreate it if necessary.";
        }

        private string HandleCompleteTask(Agent agent, string arguments)
        {
            int taskIdStartIndex = arguments.IndexOf("\"") + 1;  // Start after the first double quote
            int taskIdEndIndex = arguments.IndexOf("\"", taskIdStartIndex); // Find the closing double quote
            if (taskIdStartIndex < 1 || taskIdEndIndex < 0 || taskIdEndIndex <= taskIdStartIndex)
                return "Task ID format is incorrect or not provided in the arguments.";

            string taskId = arguments.Substring(taskIdStartIndex, taskIdEndIndex - taskIdStartIndex);
            var task = agent.TaskList.FirstOrDefault(t => t.Id == taskId);

            if (task != null)
            {
                task.Completed = true;
                agent.TaskList.Remove(task);
                agent.CompletedTaskList.Add(task);
                return "Task completed successfully and moved to the completed tasks list.";
            }
            return "Task not found.";
        }

        private string HandleRemoveTask(Agent agent, string arguments)
        {
            int taskIdStartIndex = arguments.IndexOf("\"") + 1;  // Start after the first double quote
            int taskIdEndIndex = arguments.IndexOf("\"", taskIdStartIndex); // Find the closing double quote
            if (taskIdStartIndex < 1 || taskIdEndIndex < 0 || taskIdEndIndex <= taskIdStartIndex)
                return "Task ID format is incorrect or not provided in the arguments.";

            string taskId = arguments.Substring(taskIdStartIndex, taskIdEndIndex - taskIdStartIndex);
            var task = agent.TaskList.FirstOrDefault(t => t.Id == taskId);

            if (task != null)
            {
                agent.TaskList.Remove(task);
                return "Task removed successfully.";
            }
            return "Task not found.";
        }

        private string HandlePlanCreation(Agent agent, string arguments)
        {
            var planStartIndex = arguments.IndexOf("'plan': '") + "'plan': '".Length;
            var planEndIndex = arguments.IndexOf("'", planStartIndex);
            var plan = arguments.Substring(planStartIndex, planEndIndex - planStartIndex);
            agent.CurrentPlan = plan;
            return $"Plan created for succesfully. Please reavaluate the plan and recreate it if neccesary. ";
        }
    }
}
