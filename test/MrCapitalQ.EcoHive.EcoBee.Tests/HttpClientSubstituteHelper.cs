namespace MrCapitalQ.EcoHive.EcoBee.Tests
{
    internal static class HttpSubstitute
    {
        public static SubstituteHandler ForHandler() => Substitute.ForPartsOf<SubstituteHandler>();
    }

    internal static class HttpArg
    {
        public static ref HttpRequestMessage IsRequest(HttpMethod method, Uri requestUri)
        {
#pragma warning disable NS1004 // Argument matcher used with a non-virtual member of a class.
            return ref Arg.Is<HttpRequestMessage>(x => x.Method == method && x.RequestUri == requestUri);
#pragma warning restore NS1004 // Argument matcher used with a non-virtual member of a class.
        }
    }

    public class SubstituteHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(SendSubstitute(request, cancellationToken));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            => SendSubstitute(request, cancellationToken);

        public virtual HttpResponseMessage SendSubstitute(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
