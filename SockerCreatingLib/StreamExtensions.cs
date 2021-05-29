using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SocketCreatingLib
{
    public static class StreamExtensions
    {
        public static string GetStringByStream(this Stream stream)
        {
            try
            {
                string result;
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Problem with reading stream!");
            }
        }
        public static string GetStringByStream(this NetworkStream stream)
        {
            try
            {
                byte[] data = new byte[256];
                StringBuilder response = new StringBuilder();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable); // пока данные есть в потоке

                return response.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Problem with reading stream!");
            }
        }
    }
}