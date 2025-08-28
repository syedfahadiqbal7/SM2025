using System.Text;
using System.Text.Json;

namespace AFFZ_Customer.Utils
{
    public static class Customs
    {
        public static StringContent GetJsonContent(object model)
        {
            var json = JsonSerializer.Serialize(model);
            var res = new StringContent(json, Encoding.UTF8, "application/json");
            return res;
        }

    }
}
