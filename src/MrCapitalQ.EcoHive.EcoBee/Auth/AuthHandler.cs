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

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                _authProvider.ClearCached();

            return response;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = _authProvider.GetAuthHeaderAsync(cancellationToken).GetAwaiter().GetResult();
            if (authHeader is not null)
                request.Headers.Authorization = authHeader;

            var response = base.Send(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                _authProvider.ClearCached();

            return response;
        }
    }
}
