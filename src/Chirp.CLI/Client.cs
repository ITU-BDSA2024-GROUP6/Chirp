using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using SimpleDB;

namespace ChirpClient
{
    public class Client
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5023/")
        };

        static async Task Main(string[] args)
        {
            bool running = true;
            while(running)
            {
                UserInterface.WelcomeMessage();

                var userInput = Console.ReadLine() ?? string.Empty; 
                switch (userInput.ToLower())
                {
                    case "read":
                        await ReadChirps();
                        break;
                    case "write":
                        await WriteChirp();
                        break;
                    case "quit":
                        running = false;
                        break;
                    default: 
                    Console.WriteLine("Invalid input, please try again."); 
                    break;
                }
            }
        }

        static async Task ReadChirps()
        {
            Console.Write("How many Chirps would you like to see? Type a number: ");
            string limitInput = Console.ReadLine() ?? string.Empty;
            int limit;
            if (!int.TryParse(limitInput, out limit))
            {
                Console.WriteLine("Invalid input. Using default limit of 10.");
                limit = 10;
            }
        
            var response = await httpClient.GetAsync($"chirps?limit={limit}");
            if (response.IsSuccessStatusCode)
            {
                var chirps = await response.Content.ReadFromJsonAsync<List<Chirp>>() ?? new List<Chirp>();
                foreach (var chirp in chirps)
                {
                    UserInterface.PrintChirp(chirp);
                    await Task.Delay(1000);
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
            Console.WriteLine("----------------------------------------------------------");
        }

        static async Task WriteChirp()
        {
            Console.Write("Type your message to the world: ");
            string message = Console.ReadLine()?.Trim() ?? string.Empty; 
            if (string.IsNullOrEmpty(message)) { 
                Console.WriteLine(" \nMessage cannot be empty. Please try again.\n"); 
                return;
            }
            var newChirp = new Chirp(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var response = await httpClient.PostAsJsonAsync("chirp", newChirp);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Chirp posted successfully!");
            }
            else
            {
                Console.WriteLine($"Error posting chirp: {response.StatusCode}");
            }
            Console.WriteLine("----------------------------------------------------------");
        }
    }
}