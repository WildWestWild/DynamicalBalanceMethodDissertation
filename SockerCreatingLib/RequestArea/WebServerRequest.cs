using System;
using Newtonsoft.Json;
using System.Text.Json;

namespace SocketCreatingLib
{
    [Serializable]
    public class WebServerRequest: ExtensionRequest
    {
        public int Weight { get; set; }

        public string IpAddress { get; set; }
        
        public int Port { get; set; }
        
        public StatusRequest StatusRequest { get; set; }

        public string WebApiCommandTime { get; set; }

        public long Duration;

        public WebServerRequest()
        {
            
        }

        // Инициализация запроса от сервера из класетра ( Add, Remove )
        public WebServerRequest(StatusRequest statusRequest, int weight = 0)
        {
            StatusRequest = statusRequest;
            Weight = weight;
        }
        
        public static WebServerRequest Deserialize(string requestString)
        {
            return JsonConvert.DeserializeObject<WebServerRequest>(requestString);
        }

        public static string Serialize(WebServerRequest webServerRequest)
        {
            return JsonConvert.SerializeObject(webServerRequest);
        }
    }
}