﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;

namespace USG.HttpQueueing
{
    public class QueueingHttpHandler : HttpMessageHandler
    {
        CloudQueue _queue;
        CloudBlobContainer _blobContainer;

        public QueueingHttpHandler(
            CloudQueue queue,
            CloudBlobContainer blobContainer)
        {
            _queue = queue;
            _blobContainer = blobContainer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string name = $"{request.RequestUri.Host}-{Guid.NewGuid()}";
            var blob = _blobContainer.GetBlockBlobReference(name);

            try
            {
                using (var stream = await blob.OpenWriteAsync())
                    await RequestSerialization.Serialize(request, stream);

                await _queue.CreateIfNotExistsAsync();
                await _queue.AddMessageAsync(new CloudQueueMessage(name));
            }
            catch
            {
                await blob.DeleteAsync();
                throw;
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
