using System;
using SimpleDB;

namespace ChirpClient
{
    public class Client
    {
        static void Main(string[] args)
        {

            bool running = true;
            while(running) {
                Console.WriteLine("Type \"Read\" to read previous Chirps");
                Console.WriteLine("Type \"Write\" to write a new Chirp");
                Console.WriteLine("Type \"Quit\" to quit program");
                Console.WriteLine("----------------------------------------------------------");

                var userInput = Console.ReadLine();
                switch (userInput.ToLower()) 
                {
                    case "read":
                        Console.Write("How many Chirps would you like to see? Type a number: ");
                        foreach(Chirp chirp in CSVDatabase<Chirp>.Instance.Read(Convert.ToInt32(Console.ReadLine()))) 
                        {
                            UI.PrintChirp(chirp);
                            Thread.Sleep(1000);
                        }
                        Console.WriteLine("----------------------------------------------------------");
                        break;
                    case "write":
                        Console.Write("Type your message to the world: ");
                        CSVDatabase<Chirp>.Instance.Store(new Chirp(Environment.UserName, Console.ReadLine(), DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                        Console.WriteLine("----------------------------------------------------------");
                        break;
                    case "quit":
                        running = false;
                        break;
                }
            }
        }
    }
}