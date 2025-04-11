using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIChatBot
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string model = "deepseek/deepseek-chat-v3-0324:free";
        private static string apiKey = "";

        static void ShowBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════╗");
            Console.WriteLine("║      AiChat-C# by Xans434    ║");
            Console.WriteLine("╚══════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static async Task Main(string[] args)
        {
            Console.Clear();
            ShowBanner();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Введите API ключ OpenRouter: ");
            Console.ResetColor();
            apiKey = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("API ключ не может быть пустым!");
                Console.ResetColor();
                return;
            }

            Console.Clear();
            ShowBanner();
            
            var messages = new List<Message>();
            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Вы: ");
                Console.ResetColor();
                var userInput = Console.ReadLine();
                
                if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "exit")
                    break;
                
                messages.Add(new Message { role = "user", content = userInput });
                
                var aiResponse = await GetAIResponse(messages);
                
                if (!string.IsNullOrEmpty(aiResponse))
                {
                    messages.Add(new Message { role = "assistant", content = aiResponse });
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("ИИ: ");
                    Console.ResetColor();
                    Console.WriteLine(aiResponse);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Не удалось получить ответ от ИИ");
                    Console.ResetColor();
                }
            }
        }

        static async Task<string?> GetAIResponse(List<Message> messages)
        {
            try
            {
                var requestData = new
                {
                    model,
                    messages
                };
                
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                
                var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка API: {response.StatusCode}");
                    Console.ResetColor();
                    return null;
                }
                
                var responseString = await response.Content.ReadAsStringAsync();
                
                using var doc = JsonDocument.Parse(responseString);
                var aiResponse = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                
                return aiResponse;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }
    }

    public class Message
    {
        public required string role { get; set; }
        public required string content { get; set; }
    }
}
