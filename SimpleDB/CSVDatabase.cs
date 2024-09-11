using CsvHelper;

namespace SimpleDB{
    public interface IDatabaseRepository<T>
    {
        IEnumerable<T> Read(int? limit = null);
        void Store(T record);
    }
    public sealed class CSVDatabase<T> : IDatabaseRepository<T>{
        
        void Store(T record){
            if(record.message == null){
                Console.WriteLine("no message");
            }
            
        }
        IEnumerable<T> Read(int? limit = null){

        }

    }
}