using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SocketCreatingLib;

namespace ExecuteServerSideApp
{
    public class ServerExecutor : BaseServerExecutor
    {
        private readonly string ipAddressServer;

        private readonly int portServer;

        private readonly int weightServer;
        
        private WebServerRequest WebServerRequest { get; set; }

        public ServerExecutor(string ipAddressServer, int portServer, int weightServer)
        {
            this.ipAddressServer = ipAddressServer;
            this.portServer = portServer;
            this.weightServer = weightServer;
        }

        public void ActivateServer()
        {
            CreateServerListener(ipAddressServer, portServer, new HttpClient());
            WebServerRequest = new WebServerRequest
            {
                StatusRequest = ExtensionRequest.StatusRequest.Add,
                Weight = weightServer,
                IpAddress = ipAddressServer,
                Port = portServer
            };
            Task.Factory.StartNew(Reconnect, TaskCreationOptions.LongRunning);
            ConnectWithBalancer(WebServerRequest);
            WaitRequestAndGet();
        }

        public void Reconnect()
        {
            while (true)
            {
                Console.WriteLine("Press r to reconnect or press d to disconnect");
                var consoleInput = Console.ReadLine();
                switch (consoleInput)
                {
                    case "r": ConnectWithBalancer(WebServerRequest); Console.WriteLine("Server was reconnected"); break;
                    case "d": DisconnectWithBalancer(WebServerRequest); Console.WriteLine("Server was disconnected"); break;
                    default: Console.WriteLine("Wrong command!"); break;
                }
                Console.WriteLine("<--------------------->");
            }
        }

        protected override WebServerRequest ProcessingRequest(string request)
        {
            WebServerRequest webServerRequest = WebServerRequest.Deserialize(request);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread.Sleep(Convert.ToInt32(webServerRequest.WebApiCommandTime) / weightServer);
            stopwatch.Stop();
            webServerRequest.Duration = stopwatch.ElapsedMilliseconds;
            webServerRequest.IpAddress = ipAddressServer;
            webServerRequest.Port = portServer;
            webServerRequest.Weight = weightServer;
            Console.WriteLine($"Processing - {webServerRequest.IpAddress} " +
                              $"Duration - {webServerRequest.Duration} with weight {weightServer}");
            return webServerRequest;
        }
    }
}