using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketCreatingLib
{
    public abstract class BaseServerExecutor: WorkerSideCreator
    {
        
        protected void WaitRequestAndGet()
        {
            while (true)
            {
                TcpClient tcpClient = Server.AcceptTcpClient();
                NetworkStream stream = tcpClient.GetStream();

                Task.Run(() =>
                {
                    string requestString = stream.GetStringByStream();
                    WebServerRequest webServerRequest = ProcessingRequest(requestString);
                    SendResponce(stream, webServerRequest);
                });
            }
        }

        private void SendResponce(NetworkStream stream, WebServerRequest webServerRequest)
        {
            string responce = WebServerRequest.Serialize(webServerRequest);
            byte[] data = Encoding.UTF8.GetBytes(responce);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        protected abstract WebServerRequest ProcessingRequest(string request);
    }
}