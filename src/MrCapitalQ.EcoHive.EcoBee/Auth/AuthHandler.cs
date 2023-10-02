namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly IEcoBeeAuthProvider _authProvider;

        public AuthHandler(IEcoBeeAuthProvider authProvider)
        {
            _authProvider = authProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = await _authProvider.GetAuthHeaderAsync(cancellationToken).ConfigureAwait(false);
            if (authHeader is not null)
                request.Headers.Authorization = authHeader;

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = _authProvider.GetAuthHeaderAsync(cancellationToken).GetAwaiter().GetResult();
            if (authHeader is not null)
                request.Headers.Authorization = authHeader;

            return base.Send(request, cancellationToken);
        }
    }
}
