namespace TextListener
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
}