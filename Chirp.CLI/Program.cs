using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
class Chirp {
    public string Author;
    public string Message;
    public long Timestamp;

    public Chirp(string Author, string Message, long Timestamp) {
        this.Author = Author;
        this.Message = Message;
        this.Timestamp = Timestamp;
    }

    DateTime formatTimestamp() {
        // Convert Unix timestamp to DateTime
        DateTime utcTime = DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;

        // Convert UTC time to local time
        DateTime LocalTime = utcTime.ToLocalTime();

        return LocalTime;
    }

    public string print() {
        return Author + " @ " + formatTimestamp() + ": " + Message;
    }
}

class Program {

    public static void Main(string[] args) {
        IDatabaseRepository<Chirp> database = new CSVDatabase<Chirp>();
        bool hakkebakkefar = true;
        while(hakkebakkefar) {
            Console.WriteLine("Type \"Read\" to read previous Chirps");
            Console.WriteLine("Type \"Write\" to write a new Chirp");
            Console.WriteLine("Type \"Quit\" to quit program");

            var userInput = Console.ReadLine();
            switch (userInput.ToLower()) {
                case "read":
                    Console.WriteLine("How many Chirps would you like to see? Type a number");
                    int numberInput = Convert.ToInt32(Console.ReadLine());
                    List<Chirp> desiredChirps = getNewChirps(numberInput);
                    foreach(Chirp chirp in desiredChirps) {
                        Console.WriteLine(chirp.print());
                        Thread.Sleep(3000);
                    }
                    Console.WriteLine("----------------------------------------------------------");
                    break;
                case "write":
                    uploadNewChirp();
                    break;
                case "quit":
                    // Make the program/terminal close
                    hakkebakkefar = false;
                    break;
            }
        }

        
    }
    public static List<Chirp> getNewChirps(int amount) {
        // Open the text file using a stream reader.
        using StreamReader reader = new("csv/chirp_cli_db.csv");

        using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<Chirp>().ToList();
        var recordsToProcess = records.Skip(Math.Max(0, records.Count() - amount));
        // var recordsToProcess = records.Skip(Math.Max(0, records.Count() - amount)).Reverse();

        List<Chirp> chirps = new List<Chirp>();
        foreach (var record in recordsToProcess) {
            try {

                /*string pattern = @"(?:^|,)\s*(?:""(?<field>[^""]*(?:""[^""]*)*)""|(?<field>[^,\r\n]*))";
                var regex = new Regex(pattern);
                string csvLine = line;
                MatchCollection matches = regex.Matches(csvLine);
                List<string> fields = matches.Cast<Match>().Select(m => m.Groups["field"].Value).ToList();
                

                string author = record.Author;
                string message = record.Message;
                long timeStamp = long.Parse(record.Timestamp); */

                Chirp chirp = new Chirp(record.Author, record.Message, record.Timestamp);

                chirps.Add(chirp);
            } catch (IOException e) {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            } catch (FormatException e) {
                Console.WriteLine("The file contains improperly formatted data:");
                Console.WriteLine(e.Message);
            }
        }
        return chirps;
    }

    public static void uploadNewChirp() {
        using (StreamWriter sw = File.AppendText("csv/chirp_cli_db.csv")) {
            Console.WriteLine("Type whatever's on your mind.");
            var userInput = Console.ReadLine();
            string Author = Environment.UserName;
            long Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            sw.WriteLine(Author + ",\"" + userInput + "\"," + Timestamp);
        }	
    }
}