using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            
            Console.WriteLine("Fetching repository contents...");
            
            // Get repository structure
            var response = await client.GetStringAsync("https://api.github.com/repos/rothgar/siderolabs-omni-client/contents");
            var files = JsonSerializer.Deserialize<JsonElement[]>(response);
            
            foreach (var file in files)
            {
                var name = file.GetProperty("name").GetString();
                var type = file.GetProperty("type").GetString();
                Console.WriteLine($"{type}: {name}");
            }
            
            Console.WriteLine("\nFetching main.go...");
            
            // Get main.go content
            var mainGoResponse = await client.GetStringAsync("https://api.github.com/repos/rothgar/siderolabs-omni-client/contents/main.go");
            var mainGoFile = JsonSerializer.Deserialize<JsonElement>(mainGoResponse);
            var content = mainGoFile.GetProperty("content").GetString();
            
            // Decode base64 content
            var decodedBytes = Convert.FromBase64String(content.Replace("\n", ""));
            var decodedContent = Encoding.UTF8.GetString(decodedBytes);
            
            Console.WriteLine("=== MAIN.GO CONTENT ===");
            Console.WriteLine(decodedContent);
            
            // Save to file
            await File.WriteAllTextAsync("main.go.txt", decodedContent);
            Console.WriteLine("\nContent saved to main.go.txt");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
