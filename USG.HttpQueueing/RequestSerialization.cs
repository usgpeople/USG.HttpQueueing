using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace USG.HttpQueueing
{
    public static class RequestSerialization
    {
        class Header
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        class Request
        {
            public string Url { get; set; }
            public string Method { get; set; }
            public Header[] Headers { get; set; }
            public Header[] ContentHeaders { get; set; }
            public byte[] Content { get; set; }
        }

        public static async Task Serialze(
            this HttpRequestMessage request,
            Stream stream)
        {
            var model = new Request
            {
                Url = request.RequestUri.ToString(),
                Method = request.Method.ToString(),
                Headers = request.Headers
                    .SelectMany(header =>
                        header.Value.Select(value =>
                            new Header { Name = header.Key, Value = value }))
                    .ToArray(),
            };

            if (request.Content != null)
            {
                model.Content = await request.Content.ReadAsByteArrayAsync();
                model.ContentHeaders = request.Content.Headers
                    .SelectMany(header =>
                        header.Value.Select(value =>
                            new Header { Name = header.Key, Value = value }))
                    .ToArray();
            }

            var writer = new StreamWriter(stream);
            await writer.WriteAsync(JsonConvert.SerializeObject(model));
            await writer.FlushAsync();
        }

        public static async Task<HttpRequestMessage> Deserialize(
            Stream stream)
        {
            var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<Request>(json);

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(model.Url);
            request.Method = new HttpMethod(model.Method);

            foreach (var header in model.Headers)
                request.Headers.Add(header.Name, header.Value);

            if (model.Content != null)
            {
                request.Content = new ByteArrayContent(model.Content);

                foreach (var header in model.ContentHeaders)
                    request.Content.Headers.Add(header.Name, header.Value);
            }

            return request;
        }
    }
}
