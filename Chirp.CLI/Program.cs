using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

try
{
    // Open the text file using a stream reader.
    using StreamReader reader = new("csv/chirp_cli_db.csv");
    List<Chirp> chirps = new List<Chirp>();
    // Read the stream as a string.
    string line;
    while ((line = reader.ReadLine()) != null) {
        try {
            string pattern = @"(?:^|,)\s*(?:""(?<field>[^""]*(?:""[^""]*)*)""|(?<field>[^,\r\n]*))";
            var regex = new Regex(pattern);
            string csvLine = line;
            MatchCollection matches = regex.Matches(csvLine);
            List<string> fields = matches.Cast<Match>().Select(m => m.Groups["field"].Value).ToList();

            string author = fields[0];
            string message = fields[1];
            long timeStamp = long.Parse(fields[2]);

            Chirp chirp = new Chirp(author, message, timeStamp);

            chirps.Add(chirp);
            // Write the text to the console.
            Console.WriteLine(chirp.print());
        } catch {
            Console.WriteLine("Couldn't handle following line due to non applicaple data");
            Console.WriteLine(line);
        }
        
    }
}
catch (IOException e)
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message);
}
using (StreamWriter sw = File.AppendText("csv/chirp_cli_db.csv"))
        {
            var userInput = Console.ReadLine();
            string author = Environment.UserName;
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            sw.WriteLine(author + ",\"" + userInput + "\"," + timeStamp);
        }	

class Chirp {
    string author;
    string message;
    long timeStamp;

    public Chirp(string author, string message, long timeStamp) {
        this.author = author;
        this.message = message;
        this.timeStamp = timeStamp;
    }

    DateTime formatTimeStamp() {
        // Convert Unix timestamp to DateTime
        DateTime utcTime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).UtcDateTime;

        // Convert UTC time to local time
        DateTime LocalTime = utcTime.ToLocalTime();

        return LocalTime;
    }

    public string print() {
        return author + " @ " + formatTimeStamp() + ": " + message;
    }
}