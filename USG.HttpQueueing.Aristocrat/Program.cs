using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace USG.HttpQueueing.Aristocrat
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var storage = CloudStorageAccount.Parse(
                configuration["AzureWebJobsStorage"]);

            var handler = new QueueingHttpHandler(storage);
            var client = new HttpClient(handler);

            while (true)
            {
                Console.WriteLine("Press any key to generate work");
                Console.ReadKey();

                Console.WriteLine("GET\thttps://httpbin.org/status/200");
                await client.GetAsync("https://httpbin.org/status/200");

                Console.WriteLine("POST\thttps://httpbin.org/status/201");
                await client.PostAsync("https://httpbin.org/status/201",
                    new StringContent("Hello, World!"));

                Console.WriteLine("DELETE\thttps://httpbin.org/status/405");
                await client.DeleteAsync("https://httpbin.org/status/405");
            }
        }
    }
}
