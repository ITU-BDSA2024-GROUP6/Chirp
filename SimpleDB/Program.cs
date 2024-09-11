namespace SimpleDB
{
    class Database
    {
        static void Main(string[] args)
        {
            // Assuming CSVDatabase is a class that implements IDatabaseRepository<Chirp>
            IDatabaseRepository<Chirp> database = new CSVDatabase<Chirp>();

            var chirp = new Chirp("Helge", "Hej!", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            database.Store(chirp);
        }

        // Record definition
        public record Chirp(string Author, string Message, long Timestamp);
    }
}
