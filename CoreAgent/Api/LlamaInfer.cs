using CoreAgent.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreAgent.Api
{
    public class LlamaInfer
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<float[]> GenerateEmbeddingsAsync(string prompt)
        {
            var requestBody = new
            {
                prompt,
                antiPrompt = new List<string>(),
                maxTokens = 0,
                temperature = 0,
                doEmbeddings = true
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5037/Inference/infer", content);

            response.EnsureSuccessStatusCode();

            string responseBody = "[" + await response.Content.ReadAsStringAsync() + "]";

            var embeddings = JsonSerializer.Deserialize<float[]>(responseBody);

            return embeddings;
        }

        public static async Task<string> GenerateTextAsync(string prompt, List<string> antiPrompt, int maxTokens = 256, double temperature = 0.6)
        {
            var requestBody = new
            {
                prompt,
                antiPrompt,
                maxTokens, 
                temperature,
                doEmbeddings = false
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5037/Inference/infer", content);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            //convert from json to just the string
            return responseBody;
        }
    }
}
