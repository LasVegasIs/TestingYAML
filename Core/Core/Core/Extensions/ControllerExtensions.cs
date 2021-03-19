using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Core.Extensions
{
    public static class ControllerExtensions
    {
        public static Task<JObject> GetBodyAsJsonAsync(this HttpRequest request)
        {
            using var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(streamReader);
            return JObject.LoadAsync(jsonReader);
        }
    }
}
