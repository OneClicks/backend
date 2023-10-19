namespace backend.ViewModels
{
    public class ResponseVM<T>
    {
        public ResponseVM(string _statusCode, string _message)
        {
            StatusCode = _statusCode;
            Message = _message;
        }
        public ResponseVM(string _statusCode, string _message, T _responseData)
        {
            StatusCode = _statusCode;
            Message = _message;
            ResponseData = _responseData;
        }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public T ResponseData { get; set; }

    }
}
