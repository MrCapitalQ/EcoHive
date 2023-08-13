namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public record EcoBeeAuthTokenData
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required DateTimeOffset Expiration { get; init; }
    }
}
