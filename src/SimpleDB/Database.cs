using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace SimpleDB
{
    public record Chirp(string Author, string Message, long Timestamp);

    public interface IDatabaseRepository<T>
    {
        IEnumerable<T> Read(int? limit = null);
        void Store(T record);
    }

    public sealed class CSVDatabase<T> : IDatabaseRepository<T> where T : Chirp
    {   //the following makes the csv database a singleton objec(only one instance)
        private static readonly CSVDatabase<T> instance = new CSVDatabase<T>();
        static CSVDatabase()
        {
            
        }
        private CSVDatabase()
        {
             _filePath = "../SimpleDB/data/chirp_cli_db.csv";
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Close();
            }
        }
        public static CSVDatabase<T> Instance
        {

            get
            {
                return instance;
            }
        }


        private readonly string _filePath;
        /*
        public CSVDatabase(string filePath = "../SimpleDB/data/chirp_cli_db.csv")
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Close();
            }
        }
        */

        public void Store(T record)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var stream = File.Open(_filePath, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }

        public IEnumerable<T> Read(int? limit = null)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<T>().ToList();

                if (limit.HasValue)
                {
                    return records.TakeLast(limit.Value);
                }
                else
                {
                    return records;
                }
            }
        }
    }
}