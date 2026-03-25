namespace KineTutor3D.App.Fairino
{
    public readonly struct FairinoResult
    {
        public int ErrorCode { get; }
        public string Message { get; }
        public bool IsSuccess => ErrorCode == 0;

        public FairinoResult(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message ?? string.Empty;
        }

        public static FairinoResult Ok(string message = "OK")
        {
            return new FairinoResult(0, message);
        }

        public static FairinoResult Fail(int errorCode, string message)
        {
            return new FairinoResult(errorCode, message);
        }
    }

    public readonly struct FairinoResult<T>
    {
        public int ErrorCode { get; }
        public string Message { get; }
        public bool IsSuccess => ErrorCode == 0;
        public T Value { get; }

        public FairinoResult(int errorCode, string message, T value)
        {
            ErrorCode = errorCode;
            Message = message ?? string.Empty;
            Value = value;
        }

        public static FairinoResult<T> Ok(T value, string message = "OK")
        {
            return new FairinoResult<T>(0, message, value);
        }

        public static FairinoResult<T> Fail(int errorCode, string message)
        {
            return new FairinoResult<T>(errorCode, message, default);
        }
    }
}
