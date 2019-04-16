using Newtonsoft.Json;

namespace VowelConsRater.Data
{
    public class DataWrapper
    {
        public string DataType { get; set; }
        public string Data { get; set; }

        public DataWrapper(string dataType)
        {
            DataType = dataType;
        }
        
        public T DeserializeDataFromString<T>()
        {
            return JsonConvert.DeserializeObject<T>(Data);
        }

        public void SerializeDataToString<T>(T data)
        {
            Data = JsonConvert.SerializeObject(data);
        }
    }
}