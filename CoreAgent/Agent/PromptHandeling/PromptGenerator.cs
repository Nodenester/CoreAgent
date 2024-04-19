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

            if (agent.TaskList.Any())
            {
                var nextTask = agent.TaskList.FirstOrDefault(task => !task.Completed);

                if (nextTask != null)
                {
                    prompt.AppendLine($"- Current Task: {nextTask.Description} [Status: Pending]");
                }
                else
                {
                    prompt.AppendLine("- Current Task: All tasks have been completed.");
                }
            }
            else
            {
                prompt.AppendLine("- Current Task: Create the task list before you can start executing the tasks. You must create a plan before generating the task list.");
            }

            prompt.AppendLine($"- Current Plan: {agent.CurrentPlan ?? "A strategic plan is required. Invoke PlanCreation() to formulate. You must create the plan before creating the task list. The plan should be detailed and exact."}");

            prompt.AppendLine("Your task list outlines the steps towards your goal. It's essential for tracking progress.");
            if (agent.TaskList.Any())
            {
                prompt.AppendLine("- Current Task List:");
                foreach (var task in agent.TaskList.TakeLast(maxItems))
                {
                    string taskStatus = task.Completed ? "Completed" : "Pending";
                    prompt.AppendLine($"<task>- {task.Description} [Status: {taskStatus}]</task>");
                }
            }
            else
            {
                prompt.AppendLine("Please initiate TaskListCreation() to form your task list. A well-defined task list is crucial for goal achievement.");
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
                'type': 'function',
                'function': {
                    'name': 'TaskListCreationOrUpdate',
                    'description': 'TaskListCreationOrUpdate(tasks: List<TaskUpdate>, createNew: bool = true) -> List<Task> - This function is designed to either create a new task list or update an existing one in its entirety based on the provided list of task updates. It is important to provide the complete list of tasks for either operation. Each task update should include a description and a completion status. For updates, the entire list must be provided, where tasks can be marked as completed or new tasks added as necessary.\n\n    Args:\n    tasks (List<TaskUpdate>): A complete list of task updates, including descriptions and completion statuses for each task.\n    createNew (bool): Indicates whether a new task list is to be created (true) or an existing list is to be updated (false).\n\n    Returns:\n    List<Task>: A list of Task objects, each reflecting the current state and completion status of the tasks provided.',
                    'parameters': {
                        'type': 'object',
                        'properties': {
                            'tasks': {
                                'type': 'array',
                                'items': {
                                    'type': 'object',
                                    'properties': {
                                        'description': { 'type': 'string' },
                                        'completed': { 'type': 'boolean' }
                                    },
                                    'required': ['description', 'completed']
                                }
                            },
                            'createNew': {
                                'type': 'boolean',
                                'default': true
                            }
                        },
                        'required': ['tasks']
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
