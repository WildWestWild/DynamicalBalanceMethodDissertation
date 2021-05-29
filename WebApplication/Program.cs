using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SocketCreatingLib;
using WebApplication.ParserArea;

namespace WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CSVHandler.StartReadCSVAsync();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}