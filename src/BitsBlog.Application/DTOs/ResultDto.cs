namespace BitsBlog.Application.DTOs
{
    public class ResultDto
    {
        public bool Ok { get; set; }
        public string? Error { get; set; }

        public static ResultDto Success() => new ResultDto { Ok = true };
        public static ResultDto Fail(string? error) => new ResultDto { Ok = false, Error = error };
    }

    public class ResultDto<T> : ResultDto
    {
        public T? Data { get; set; }

        public static ResultDto<T> Success(T data) => new ResultDto<T> { Ok = true, Data = data };
        public static new ResultDto<T> Fail(string? error) => new ResultDto<T> { Ok = false, Error = error };
    }
}

