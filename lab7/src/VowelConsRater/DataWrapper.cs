namespace VowelConsRater
{
    public class DataWrapper<T>
    {
        public DataWrapper(string dataType, T data)
        {
            DataType = dataType;
            Data = data; 
        }

        public string DataType { get; set; }
        public T Data { get; set;}
    }

    public class StatisticsEventData
    {
        public string TextId { get; set; }
        public double Rank { get; set; }
    }
}