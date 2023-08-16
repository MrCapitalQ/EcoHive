using Moq;
using Moq.Protected;
using System.Net;

namespace MrCapitalQ.EcoHive.EcoBee.Tests
{
    internal static class MockHttpClientExtensions
    {
        private const string SendAsyncMethodName = "SendAsync";

        public static Moq.Language.Flow.ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupSend(this Mock<HttpMessageHandler> httpMessageHandler,
            HttpMethod method,
            Uri requestUri)
        {
            return httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(SendAsyncMethodName,
                    ItExpr.Is<HttpRequestMessage>(m => (method == null || m.Method == method) && (requestUri == null || m.RequestUri == requestUri)),
                    ItExpr.IsAny<CancellationToken>());
        }

        public static void ReturnsResponse(this Moq.Language.Flow.ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode,
            string? content = null)
        {
            setup.ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = content is null ? null : new StringContent(content),
            }).Verifiable();
        }

        public static void VerifySend(this Mock<HttpMessageHandler> httpMessageHandler, HttpMethod method, Uri requestUri, Func<Times> times)
        {
            httpMessageHandler.VerifySend(method, requestUri, times.Invoke());
        }

        public static void VerifySend(this Mock<HttpMessageHandler> httpMessageHandler, HttpMethod method, Uri requestUri, Times times)
        {
            httpMessageHandler.Protected().Verify(SendAsyncMethodName,
                times,
                ItExpr.Is<HttpRequestMessage>(m => (method == null || m.Method == method) && (requestUri == null || m.RequestUri == requestUri)),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
