using System.Text;
using System.Text.Json;

namespace AFFZ_Admin.Utils
{
    public static class Customs
    {
        public static StringContent GetJsonContent(object model)
        {
            var json = JsonSerializer.Serialize(model);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

    }
}
