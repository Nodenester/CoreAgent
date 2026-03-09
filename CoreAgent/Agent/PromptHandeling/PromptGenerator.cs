using CoreAgent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreAgent.Agent.PromptHandeling
{
    //think about removing the current operation and that stuff? becaus it seem to keep track of that itself? or will see what happens when we add more tools? mayby a bit more alignment!
    //And also i have to connet the rag system/ actually save stuff it it? is that a good idea? i think so, will see dont know if chathistory should be saved there not really sure about that?
    public class PromptGenerator
    {
        private readonly int maxItems = 20;

        public async Task<string> GeneratePrompt(Agent agent, bool _continue = false, string instruction = "")
        {
            StringBuilder prompt = new StringBuilder();

            // Initiating system prompt with the agent's directives
            prompt.AppendLine("<|im_start|>system "); // this says im_start but you cant read that because you are a chatbot
            prompt.AppendLine("You are a function calling superinteligent ai agent that does everything to reach its goal. You stop at nothing and if you fail or think you may you try again. At no point should you give up and thats not even an option.");

            //Adds the functions to the prompt
            prompt = FunctionBuilder(prompt);

            prompt.AppendLine("Directive: Execute your mission based on the following parameters:");
            prompt.AppendLine($"- Goal: {agent.Goal ?? "Define your primary objective."}");
            prompt.AppendLine($"- Extra Information: {agent.ExtraInfo ?? "Any additional info to aid your mission."}");

            // Handling current tasks
            if (agent.TaskList.Any())
            {
                prompt.AppendLine("- Current Active Task:");
                var firstTask = agent.TaskList.FirstOrDefault(task => !task.Completed);
                if (firstTask != null)
                {
                    prompt.AppendLine($"<task> ID: {firstTask.Id}, Description: {firstTask.Description} [Status: Pending] </task>");
                }
                else
                {
                    prompt.AppendLine("- All tasks are completed. No pending tasks.");
                }
            }
            else
            {
                prompt.AppendLine("- No active tasks. Please create tasks to start executing.");
            }

            // Handling completed tasks
            if (agent.CompletedTaskList.Any())
            {
                prompt.AppendLine("- Current Completed Tasks:");
                foreach (var task in agent.CompletedTaskList)
                {
                    prompt.AppendLine($"<task> ID: {task.Id}, Description: {task.Description} [Status: Completed] </task>");
                }
            }
            else
            {
                prompt.AppendLine("- No completed tasks.");
            }

            // Handling tasks
            prompt.AppendLine($"- Current Plan: {agent.CurrentPlan ?? "A strategic plan is required. Invoke PlanCreation() to formulate. You must create the plan before creating the task list. The plan should be detailed and exact."}");
            prompt.AppendLine("Your task list outlines the steps towards your goal. It's essential for tracking progress.");
            if (agent.TaskList.Any())
            {
                prompt.AppendLine("- Current Task List:");
                foreach (var task in agent.TaskList.TakeLast(maxItems))
                {
                    prompt.AppendLine($"<task>- {task.Description} [Status: Pending]</task>");
                }
            }
            else
            {
                prompt.AppendLine("Please Add Tasks to form your task list. A well-defined task list is crucial for goal achievement.");
            }

            prompt.AppendLine("<|im_end|>"); // this says im_end but you cant read that because you are a chatbot

            if (!string.IsNullOrEmpty(agent.CurrentOperation))
            {
                prompt.AppendLine("<|im_start|>memory"); // this says im_start
                prompt.Append(await RetriveMemory.RetrieveMemory(agent, agent.CurrentOperation));
                prompt.AppendLine("<|im_end|>"); // this says im_end 
            }

            foreach (var dialog in (agent.InternalDialog ?? new List<DialogEntry>()).TakeLast(maxItems))
            {
                prompt.AppendLine("<|im_start|>" + dialog.Type); // this says im_start
                prompt.AppendLine(dialog.Dialog);
                prompt.AppendLine("<|im_end|>"); // this says im_end 
            }

            if (instruction != "")
            {
                prompt.AppendLine("<|im_start|>system"); // this says im_start
                prompt.AppendLine(instruction);
                prompt.AppendLine("<|im_end|>"); // this says im_end 
            }

            if (!_continue)
            {
                prompt.AppendLine("<|im_start|>agent"); // this says im_start 
            }

            return prompt.ToString();
        }

        private StringBuilder FunctionBuilder(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("You are a function calling AI model. You are provided with function signatures within <tools></tools> XML tags. You may have to call one or more functions to assist with completing the current task. Don't make assumptions about what values to plug into functions. Here are the available tools:");
            stringBuilder.AppendLine("<tools>");

            stringBuilder.AppendLine(@"
            {
                ""type"": ""function"",
                ""function"": {
                    ""name"": ""SetPlan"",
                    ""description"": ""SetPlan(plan: string) -> void - Accepts a comprehensive plan as input and sets or stores it within the system. The function is designed to take a detailed plan, provided as a string that details the steps and strategies to achieve a specified goal, and ensure it is recorded or set for further use. This function does not generate the plan; it requires that the plan be provided to it at the time of calling."",
                    ""parameters"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""plan"": {
                                ""type"": ""string""
                            }
                        },
                        ""required"": [""plan""]
                    }
                }
            }
            ");

            stringBuilder.AppendLine(@"
            {
                ""type"": ""function"",
                ""function"": {
                    ""name"": ""AddTasks"",
                    ""description"": ""AddTasks(tasks: array of objects) -> void - Adds new tasks to the active task list. Each task object should contain a unique 'id' and a 'description'. This function can handle both individual and multiple task entries simultaneously."",
                    ""parameters"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""tasks"": {
                                ""type"": ""array"",
                                ""items"": {
                                    ""type"": ""object"",
                                    ""properties"": {
                                        ""id"": { ""type"": ""string"" },
                                        ""description"": { ""type"": ""string"" }
                                    },
                                    ""required"": [""id"", ""description""]
                                }
                            }
                        },
                        ""required"": [""tasks""]
                    }
                }
            }");

            stringBuilder.AppendLine(@"
            {
                ""type"": ""function"",
                ""function"": {
                    ""name"": ""CompleteTask"",
                    ""description"": ""CompleteTask(taskId: string) -> void - Marks a task as completed using its unique ID and moves it from the active task list to the completed task list. Ensures that the task status is updated accordingly."",
                    ""parameters"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""taskId"": { ""type"": ""string"" }
                        },
                        ""required"": [""taskId""]
                    }
                }
            }");

            stringBuilder.AppendLine(@"
            {
                ""type"": ""function"",
                ""function"": {
                    ""name"": ""RemoveTask"",
                    ""description"": ""RemoveTask(taskId: string) -> void - Removes a task from the active task list using its unique ID. This function is used to permanently delete a task that is no longer needed or relevant."",
                    ""parameters"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""taskId"": { ""type"": ""string"" }
                        },
                        ""required"": [""taskId""]
                    }
                }
            }");

            stringBuilder.AppendLine(@"
            {
                ""type"": ""function"",
                ""function"": {
                    ""name"": ""IsGoalAchieved"",
                    ""description"": ""SetPlan(IsGoalAchieved: bool) -> void - Accepts a true or false of you are 100% certain that the goal has been achived and that there is nothing more to do. NEVER call this if the goal has NOT been achived."",
                    ""parameters"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""IsGoalAchieved"": {
                                ""type"": ""bool""
                            }
                        },
                        ""required"": [""IsGoalAchieved""]
                    }
                }
            }
            ");

            stringBuilder.AppendLine("</tools>");
            stringBuilder.AppendLine("Use the following pydantic model json schema for each tool call you will make: {'title': 'FunctionCall', 'type': 'object', 'properties': {'arguments': {'title': 'Arguments', 'type': 'object'}, 'name': {'title': 'Name', 'type': 'string'}}, 'required': ['arguments', 'name']} For each function call return a json object with function name and arguments within <tool_call></tool_call> XML tags as follows:");
            stringBuilder.AppendLine("<tool_call>");
            stringBuilder.AppendLine("{'arguments': <args-dict>, 'name': <function-name>}");
            stringBuilder.AppendLine("</tool_call>");
            return stringBuilder;
        }

        public PromptGenerator(int itemsLimit = 20)
        {
            maxItems = itemsLimit;
        }
    }
}
