using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeminiAIAddin
{
    public class GeminiAI
    {
        private readonly string apiKey = "AIzaSyDFyzFTiTj97HFw1clxRX65gxDOWIjd3Eo";

        // Model text
        private readonly string textModelEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        // Model image (preview)
        private readonly string imageModelEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-preview-image-generation:generateContent";

        public async Task<string> GenerateTextAsync(string prompt)
        {
            using (var client = new HttpClient())
            {
                var json = $@"{{
                    ""contents"": [{{
                        ""parts"": [{{ ""text"": ""{prompt.Replace("\"", "\\\"")}"" }}]
                    }}]
                }}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{textModelEndpoint}?key={apiKey}", content);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Lỗi gọi API Gemini (text): {response.StatusCode}\nChi tiết: {errorContent}";
            }
        }

        public async Task<string> GenerateImageAsync(string prompt)
        {
            using (var client = new HttpClient())
            {
                var json = $@"{{
                    ""contents"": [{{
                        ""parts"": [{{ ""text"": ""{prompt.Replace("\"", "\\\"")}"" }}]
                    }}]
                }}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{imageModelEndpoint}?key={apiKey}", content);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Lỗi gọi API Gemini (image): {response.StatusCode}\nChi tiết: {errorContent}";
            }
        }

        public async Task<string> ListModelsAsync()
        {
            using (var client = new HttpClient())
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                var response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
