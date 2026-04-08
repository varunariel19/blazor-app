using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SolveIt.Services
{
    public class EmbeddingResponse
    {
        public float[] embedding { get; set; } = Array.Empty<float>();
    }


    public class EmbeddingService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<float[]?> GetEmbeddingsAsync(string text)
        {
            try
            {
                var url = "http://localhost:11434/api/embeddings";
                var requestBody = new
                {
                    model = "nomic-embed-text",
                    prompt = text
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseString);

                return result?.embedding;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

    }
    }
