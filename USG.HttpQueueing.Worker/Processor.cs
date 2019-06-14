using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace USG.HttpQueueing.Worker
{
    public class Processor
    {
        IHttpClientFactory _httpClientFactory;
        StorageAccountProvider _storageProvider;

        public Processor(
            IHttpClientFactory clientFactory,
            StorageAccountProvider storageProvider)
        {
            _httpClientFactory = clientFactory;
            _storageProvider = storageProvider;
        }

        public async Task ProcessMessage(
            [QueueTrigger("requests-v1")] string message,
            ILogger logger)
        {
            var blobClient = _storageProvider.Get(null).CreateCloudBlobClient();
            var blob = new CloudBlob(new Uri(message), blobClient);

            HttpRequestMessage request;
            using (var stream = await blob.OpenReadAsync())
                request = await RequestSerialization.Deserialize(stream);

            HttpResponseMessage response;
            using (var client = _httpClientFactory.CreateClient())
                response = await client.SendAsync(request);

            logger.LogInformation(
                (int)response.StatusCode + " " +
                request.Method + " " +
                request.RequestUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(
                    request.Method + " " +
                    request.RequestUri + " failed with status code " +
                    (int)response.StatusCode);
            }

            try
            {
                await blob.DeleteAsync();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to delete blob");
            }
        }
    }
}
