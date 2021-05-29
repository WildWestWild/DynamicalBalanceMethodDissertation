using System;

namespace ExecuteServerSideApp
{
    class Program
    {
        private const string server = "127.0.0.1";
 
        // Args:
            // Port - номер порта, где будет запускаться сервер
            // WeightServer - Вес (приоритет) сервера в балансировке нагрузки
        
        static void Main(string[] args)
        {
            if ( args.Length == 0 )
            {
                args = new[] {"3010", "1", "0"};
            }
            int port = Convert.ToInt32(args[0]);
            int weightServer = Convert.ToInt32(args[1]);
            Console.WriteLine($"Port - {port}");
            Console.WriteLine($"Weight - {weightServer}");
            ServerExecutor serverExecutor = new ServerExecutor(server, port, weightServer);
            serverExecutor.ActivateServer();
        }
    }
}