using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace hacker_news_wrapper_tests
{
    // Extension methods for setting up HttpMessageHandler responses
    public static class HttpMessageHandlerExtensions
    {
        public static Mock<HttpMessageHandler> SetupRequest(this Mock<HttpMessageHandler> mockHandler, HttpMethod method, string url)
        {
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri.PathAndQuery == url),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            return mockHandler;
        }

        public static void SetupHttpResponseMessage<T>(this Mock<HttpMessageHandler> mockHandler,string requestUri, T content)
        {
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.PathAndQuery == requestUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(content), System.Text.Encoding.UTF8, "application/json")
                });
        }
    }
}
