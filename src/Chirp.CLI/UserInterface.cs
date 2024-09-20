using System;
using System.Collections.Generic;
using SimpleDB;

namespace ChirpClient
{
    public class UserInterface
    {   
        public static void WelcomeMessage(){
            Console.WriteLine("Hello " + Environment.UserName + "!");
            Console.WriteLine("Type \"Read\" to read previous Chirps");
            Console.WriteLine("Type \"Write\" to write a new Chirp");
            Console.WriteLine("Type \"Quit\" to quit program");
            Console.WriteLine("--------------------------------------------------");
        }
        public static void PrintChirp(Chirp chirp)
        {
            DateTime utcTime = DateTimeOffset.FromUnixTimeSeconds(chirp.Timestamp).UtcDateTime;

            // Convert UTC time to local time
            DateTime LocalTime = utcTime.ToLocalTime();
            
            Console.WriteLine($"{chirp.Author} @ {LocalTime}: {chirp.Message}");
        }

        public static void PrintChirps(IEnumerable<Chirp> chirps)
        {
            foreach (var chirp in chirps)
            {
                PrintChirp(chirp);
            }
        }
    }
}