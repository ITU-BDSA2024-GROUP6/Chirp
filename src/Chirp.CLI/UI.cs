using System;
using System.Collections.Generic;
using SimpleDB;

namespace ChirpClient
{
    public class UI
    {
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