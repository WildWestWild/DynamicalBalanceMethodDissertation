using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text;

namespace SocketCreatingLib
{
    public abstract class WorkerSideCreator
    {
        private const string IpAddressBalancer = "http://localhost:5000/Server";
        protected TcpListener Server { get; private set; }
        
        private HttpClient HttpClient { get; set; }
        
        protected void CreateServerListener(string ipAddress, int port, HttpClient httpClient)
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(ipAddress);
                HttpClient = httpClient;
                Server = new TcpListener(localAddr, port);
                Server.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
        
        protected void StopServer()
        {
            Server.Stop();
        }
        
        public void ConnectWithBalancer(WebServerRequest webServerRequest)
        {
            string json = WebServerRequest.Serialize(webServerRequest);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = HttpClient.PostAsync(IpAddressBalancer, data);
            string result = response.ToString();
            Console.WriteLine(result);
        }
        
        public void DisconnectWithBalancer(WebServerRequest webServerRequest)
        {
            webServerRequest.StatusRequest = ExtensionRequest.StatusRequest.Remove;
            ConnectWithBalancer(webServerRequest);
            StopServer();
        } 
    }
}