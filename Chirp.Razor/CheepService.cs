using Microsoft.Data.Sqlite; 
using System.Collections.Generic; 
using System.Linq;
using System.Data;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps();
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    // These would normally be loaded from a database for example
    private static readonly List<CheepViewModel> _cheeps = new()
        {
            new CheepViewModel("Helge", "Hello, BDSA students!", UnixTimeStampToDateTimeString(1690892208)),
            new CheepViewModel("Adrian", "Hej, velkommen til kurset.", UnixTimeStampToDateTimeString(1690895308)),
        };

    public List<CheepViewModel> GetCheeps()
    {
        List<CheepViewModel> cheeps = new List<CheepViewModel>();

        var sqlDBFilePath = "/tmp/chirp.db";
        var sqlQuery = @"SELECT u.username, m.text, m.pub_date FROM user1 u JOIN message1 m ON u.user_id = m.author_id;";
        
        using (var connection = new SqliteConnection($"Data Source={sqlDBFilePath}"))
        {
        connection.Open();

        using (var command = new SqliteCommand(sqlQuery, connection))

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var author = reader.GetString(reader.GetOrdinal"username");
                var message = reader.GetString(reader.GetOrdinal"message");
                var timestamp = UnixTimeStampToDateTimeString((double)reader.GetInt64(reader.GetOrdinal("pub_date"))); 

                cheeps.add(new CheepViewModel(author, message, timestamp));
            }
        }
        return cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        // filter by the provided author name
        return _cheeps.Where(x => x.Author == author).ToList();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

}
