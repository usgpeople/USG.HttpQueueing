using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace USG.HttpQueueing.Worker
{
    public static class Program
    {
        public static void Main()
        {
            using (var host = new HostBuilder()
                    .ConfigureServices(services => services
                        .AddHttpClient())
                    .ConfigureWebJobs(builder => builder
                        .AddAzureStorageCoreServices()
                        .AddAzureStorage())
                    .ConfigureLogging(builder => builder
                        .AddConsole())
                    .Build())
                host.Run();
        }
    }
}
