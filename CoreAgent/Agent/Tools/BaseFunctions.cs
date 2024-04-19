using CoreAgent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    case "TaskListCreationOrUpdate":
                        return HandleTaskListCreationOrUpdate(agent, argumentsString);
                    case "SetPlan":
                        return HandlePlanCreation(agent, argumentsString);
                    case "IsGoalAchieved":
                        if (argumentsString.Contains("true"))
                        {
                            return "GoalAchived";
                        }
                        else
                        {
                            return ("Only use this function if you have reached the goal buy completing all the tasks.");
                        }
                    default:
                        return "|Error| Unrecognized function call. You can only use the functions you see and only in the way they are show. Plese reavaluate what you just did and try again if necessary.";
                }
            }
            catch (Exception ex)
            {
                return $"|Error| executing function call. You can only use the functions you see and only in the way they are show. Plese reavaluate what you just did and try again if necessary.";
            }
        }

        private string HandleTaskListCreationOrUpdate(Agent agent, string arguments)
        {
            List<AgentTask> taskList = new List<AgentTask>();

            int tasksStartIndex = arguments.IndexOf("'tasks': [") + "'tasks': [".Length;
            int tasksEndIndex = arguments.IndexOf("], 'createNew'");
            string tasksString = arguments.Substring(tasksStartIndex, tasksEndIndex - tasksStartIndex);

            var taskStrings = tasksString.Split(new string[] { "}, {" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var taskString in taskStrings)
            {
                int descriptionStartIndex = taskString.IndexOf("'description': '") + "'description': '".Length;
                int descriptionEndIndex = taskString.IndexOf("'", descriptionStartIndex);
                string description = taskString.Substring(descriptionStartIndex, descriptionEndIndex - descriptionStartIndex);

                int completedStartIndex = taskString.IndexOf("'completed': ") + "'completed': ".Length;
                string completedString = taskString.Substring(completedStartIndex).Split(',')[0];
                bool completed = completedString.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);

                taskList.Add(new AgentTask(description, completed));
            }

            agent.TaskList = taskList;

            return $"Task list created and set. Please reavaluate the task list and recreate it if necessary.";
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
