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
            // Use system's temp directory if CHIRPDBPATH is not set
            dbPath = Path.Combine(Path.GetTempPath(), "chirp.db");
        }
        _connectionString = $"Data Source={dbPath}";
        Console.WriteLine($"Using database at: {dbPath}");

        // Ensure the database file exists and is initialized
        if (!File.Exists(dbPath))
        {
            CreateDatabase(dbPath);
        }
    }

    private void CreateDatabase(string dbPath)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            
            // Execute schema.sql
            ExecuteSqlFile(connection, "schema.sql");

            // Execute dump.sql
            ExecuteSqlFile(connection, "dump.sql");
        }
    }

    private void ExecuteSqlFile(SqliteConnection connection, string fileName)
    {
        // Get the current working directory
        string currentDirectory = Directory.GetCurrentDirectory();
        
        // Construct the path to the SQL file
        string sqlFilePath = Path.Combine(currentDirectory, "data", fileName);
        
        if (!File.Exists(sqlFilePath))
        {
            Console.WriteLine($"Warning: SQL file not found: {sqlFilePath}");
            return; // Skip this file and continue with the rest of the initialization
        }

        try
        {
            string sqlScript = File.ReadAllText(sqlFilePath);
            using var command = new SqliteCommand(sqlScript, connection);
            command.ExecuteNonQuery();
            Console.WriteLine($"Successfully executed SQL file: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing SQL file {fileName}: {ex.Message}");
        }
    }

    public List<CheepViewModel> GetCheeps(int? page = 0, int? pageSize = 32)
    {
        var cheeps = new List<CheepViewModel>();
        var sqlQuery = @"SELECT u.username, m.text, m.pub_date 
                         FROM user u 
                         JOIN message m 
                         ON u.user_id = m.author_id
                         ORDER BY m.pub_date DESC";

        if (page.HasValue && pageSize.HasValue)
        {
            sqlQuery += " LIMIT @PageSize OFFSET @Offset";
        }

        sqlQuery += ";";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using var command = new SqliteCommand(sqlQuery, connection);
            if (page.HasValue && pageSize.HasValue)
            {
                command.Parameters.AddWithValue("@PageSize", pageSize.Value);
                command.Parameters.AddWithValue("@Offset", page.Value * pageSize.Value);
            }
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                cheeps.Add(CreateCheepFromReader(reader));
            }
        }

        return cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int? page = 0, int? pageSize = 32)
    {
        var cheeps = new List<CheepViewModel>();
        var sqlQuery = @"SELECT u.username, m.text, m.pub_date 
                         FROM user u 
                         JOIN message m ON u.user_id = m.author_id 
                         WHERE u.username = @Author
                         ORDER BY m.pub_date DESC";

        if (page.HasValue && pageSize.HasValue)
        {
            sqlQuery += " LIMIT @PageSize OFFSET @Offset";
        }

        sqlQuery += ";";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using var command = new SqliteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@Author", author);
            if (page.HasValue && pageSize.HasValue)
            {
                command.Parameters.AddWithValue("@PageSize", pageSize.Value);
                command.Parameters.AddWithValue("@Offset", page.Value * pageSize.Value);
            }
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