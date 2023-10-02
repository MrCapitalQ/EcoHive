namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public record EcoBeeAuthTokenData
    {
        public required string TokenType { get; init; }
        public required string AccessToken { get; init; }
    }
}
