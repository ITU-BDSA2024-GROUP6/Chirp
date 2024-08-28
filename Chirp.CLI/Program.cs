﻿try
{
    // Open the text file using a stream reader.
    using StreamReader reader = new("csv/chirp_cli_db.csv");

    // Read the stream as a string.
    string text = reader.ReadToEnd();

    // Write the text to the console.
    Console.WriteLine(text);
}
catch (IOException e)
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message);
}