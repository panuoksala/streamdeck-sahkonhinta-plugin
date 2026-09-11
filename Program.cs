using Microsoft.Extensions.Hosting;
using StreamDeckLib;
using StreamDeckLib.DependencyInjection;
using StreamDeckLib.Hosting;
using System.Threading.Tasks;

namespace StreamDeckSahkonhinta
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                        .ConfigureStreamDeckToolkit(args)
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddStreamDeck(hostContext.Configuration, typeof(Program).Assembly);
                        });
    }
}
