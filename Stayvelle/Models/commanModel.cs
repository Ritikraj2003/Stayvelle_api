namespace Stayvelle.Models
{
    public class CommonModel
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}
