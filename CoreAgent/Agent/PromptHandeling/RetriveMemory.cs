using CoreAgent.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreAgent.Agent.PromptHandeling
{
    public class RetriveMemory
    {
        public static async Task<StringBuilder> RetrieveMemory(Agent agent, string query)
        {
            List<float> queryEmbedding = new List<float>();
            const float relevanceThreshold = 0.5f; // Define a threshold for relevance, adjust as needed

            if (agent.VectorDb.Count != 0)
            {
                float[] embeddingsArray = await LlamaInfer.GenerateEmbeddingsAsync(query);
                queryEmbedding = embeddingsArray.ToList();
            }

            var relevantMatches = agent.VectorDb.Select(item => new
            {
                item.Text,
                Similarity = CalculateCosineSimilarity(queryEmbedding, item.Embedding)
            })
            .Where(match => match.Similarity > relevanceThreshold)
            .OrderByDescending(match => match.Similarity)
            .Take(20)
            .ToList();

            StringBuilder output = new StringBuilder();
            foreach (var match in relevantMatches)
            {
                output.AppendLine($"Match: {match.Text} - Similarity: {match.Similarity}");
            }

            return output;
        }

        private static float CalculateCosineSimilarity(List<float> vectorA, List<float> vectorB)
        {
            float dotProduct = 0f;
            float normA = 0f;
            float normB = 0f;
            for (int i = 0; i < vectorA.Count; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }
            return dotProduct / ((float)Math.Sqrt(normA) * (float)Math.Sqrt(normB));
        }
    }
}
