using System.Net;

namespace AFFZ_Provider.Models.Partial
{
    public class SResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; } = null!;
        public object? Data { get; set; }
    }
}
