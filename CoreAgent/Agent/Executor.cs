using CoreAgent.Agent.PromptHandeling;
using CoreAgent.Agent.Tools;
using CoreAgent.Api;
using CoreAgent.Data;
using CoreAgent.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreAgent.Agent
{

    //add so we save the chathistory to both chathistory and update the vector db?
    public class Executor
    {
        bool _continue = true;
        BaseFunctions baseFunctions = new BaseFunctions();
        public async Task<bool> ExecuteTask(Agent agent)
        {
            var promptGenerator = new PromptGenerator();
            string prompt = await promptGenerator.GeneratePrompt(agent, false);

            List<string> antiPrompt = new List<string>() { "</tool_call>", "<|im_end|>" };
            int maxTokens = 512;
            double temperature = 0.6;

            // Make an inference call
            string inferenceResponse = await LlamaInfer.GenerateTextAsync(prompt, antiPrompt, maxTokens, temperature);

            if(inferenceResponse.Contains("<tool_call>"))
            {
                inferenceResponse += "</tool_call>";
            }

            var toolCallMatch = Regex.Match(inferenceResponse, "<tool_call>(.*?)</tool_call>", RegexOptions.Singleline);
            if (toolCallMatch.Success)
            {
                string toolCallJson = toolCallMatch.Groups[1].Value;

                var toolResponse = await baseFunctions.ProcessToolCall(agent, toolCallJson);

                //if (toolResponse.Contains("|Error|"))
                //{
                //    return true;
                //}

                if (toolResponse == "GoalAchived")
                {
                    return false;
                }

                agent.InternalDialog.Add(new DialogEntry { Type = "agent", Dialog = inferenceResponse + "</tool_call>" });
                agent.InternalDialog.Add(new DialogEntry { Type = "tool", Dialog = toolResponse });

                //VectorDbItem vectorDbItem = new VectorDbItem();
                //await vectorDbItem.UpdateTextAsync(inferenceResponse + "Tool response:" + toolResponse);
                //VectorService.AppendVectorDataItem(agent.folderName, vectorDbItem);
            }
            else
            {
                agent.InternalDialog.Add(new DialogEntry { Type = "agent", Dialog = inferenceResponse });

                //VectorDbItem vectorDbItem = new VectorDbItem();
                //await vectorDbItem.UpdateTextAsync(inferenceResponse);
                //VectorService.AppendVectorDataItem(agent.folderName ,vectorDbItem);
            }
            agent.Save();

            return _continue;
        }
    }
}
