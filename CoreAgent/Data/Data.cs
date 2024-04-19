using CoreAgent.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreAgent.Data
{
    public class DialogEntry
    {
        public string Type { get; set; }
        public string Dialog { get; set; }
    }

    public class AgentTask
    {
        public string Description { get; set; }
        public bool Completed { get; set; }

        // Constructor to initialize a new task
        public AgentTask(string description, bool completed = false)
        {
            Description = description;
            Completed = completed;
        }
    }

    public class VectorDbItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        private string _text;
        public string Text
        {
            get => _text;
            private set => _text = value;
        }
        public List<float> Embedding { get; set; } = new List<float>();

        public async Task UpdateTextAsync(string newText)
        {
            Text = newText;
            Embedding = await GenerateEmbeddingAsync(newText);
        }

        private static async Task<List<float>> GenerateEmbeddingAsync(string text)
        {
            float[] embeddingsArray = await LlamaInfer.GenerateEmbeddingsAsync(text);
            return embeddingsArray.ToList();
        }
    }


    public class InferenceResponse
    {
        public float[] Embeddings { get; set; }
    }

    public class FunctionCall
    {
        public string Name { get; set; }
        public Dictionary<string, object> Arguments { get; set; }
    }

    public class ToolCall
    {
        public FunctionCall Function { get; set; }
    }

}
