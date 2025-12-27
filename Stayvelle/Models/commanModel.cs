namespace Stayvelle.Models
{
    public class CommonModel
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
    }

    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}
