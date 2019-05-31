using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace USG.HttpQueueing.Tests
{
    [TestClass]
    public class RequestSerializationTest
    {
        [TestMethod]
        public async Task Roundtrip()
        {
            var data = new byte[] { 255, 0 };

            var input = new HttpRequestMessage();
            input.RequestUri = new Uri("https://example.com:8080/");
            input.Method = HttpMethod.Patch;
            input.Headers.Add("User-Agent", "Foo/1.0");
            input.Content = new ByteArrayContent(data);
            input.Content.Headers.Add("Content-Type", "application/x-test");

            HttpRequestMessage output;

            using (var stream = new MemoryStream())
            {
                await RequestSerialization.Serialze(input, stream);
                stream.Seek(0, SeekOrigin.Begin);
                output = await RequestSerialization.Deserialize(stream);
            }

            var outputData = await output.Content.ReadAsByteArrayAsync();

            Assert.AreEqual("https://example.com:8080/",
                output.RequestUri.AbsoluteUri);
            Assert.AreEqual(HttpMethod.Patch, output.Method);
            Assert.AreEqual("Foo/1.0",
                output.Headers.UserAgent.FirstOrDefault()?.ToString());
            Assert.AreEqual("application/x-test",
                output.Content.Headers.ContentType.MediaType);
            CollectionAssert.AreEqual(data, outputData);
        }

        [TestMethod]
        public async Task Roundtrip_NoContent()
        {
            var input = new HttpRequestMessage();
            input.RequestUri = new Uri("https://example.com:8080/");
            input.Method = HttpMethod.Patch;

            HttpRequestMessage output;

            using (var stream = new MemoryStream())
            {
                await RequestSerialization.Serialze(input, stream);
                stream.Seek(0, SeekOrigin.Begin);
                output = await RequestSerialization.Deserialize(stream);
            }

            Assert.AreEqual("https://example.com:8080/",
                output.RequestUri.AbsoluteUri);
            Assert.IsNull(output.Content);
        }
    }
}
