using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

public record CheepViewModel(string Author, string Message, string Timestamp);

public class DBFacade
{
    private readonly string _connectionString;

    public DBFacade()
    {
        string dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
        if (string.IsNullOrEmpty(dbPath))
        {
            dbPath = "/tmp/chirp.db";
        }
        _connectionString = $"Data Source={dbPath}";
        Console.WriteLine(_connectionString);
    }

    public List<CheepViewModel> GetCheeps()
    {
        var cheeps = new List<CheepViewModel>();
        var sqlQuery = @"SELECT u.username, m.text, m.pub_date 
                         FROM user u 
                         JOIN message m 
                         ON u.user_id = m.author_id
                         ORDER BY m.pub_date DESC;";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using var command = new SqliteCommand(sqlQuery, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                cheeps.Add(CreateCheepFromReader(reader));
            }
        }

        return cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        var cheeps = new List<CheepViewModel>();
        var sqlQuery = @"SELECT u.username, m.text, m.pub_date 
                         FROM user u 
                         JOIN message m ON u.user_id = m.author_id 
                         WHERE u.username = @Author
                         ORDER BY m.pub_date DESC;";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using var command = new SqliteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@Author", author);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                cheeps.Add(CreateCheepFromReader(reader));
            }
        }

        return cheeps;
    }

    private static CheepViewModel CreateCheepFromReader(SqliteDataReader reader)
    {
        var author = reader.GetString(reader.GetOrdinal("username"));
        var message = reader.GetString(reader.GetOrdinal("text"));
        var timestamp = UnixTimeStampToDateTimeString(reader.GetInt64(reader.GetOrdinal("pub_date")));
        return new CheepViewModel(author, message, timestamp);
    }

    private static string UnixTimeStampToDateTimeString(long unixTimeStamp)
    {
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).DateTime;
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}