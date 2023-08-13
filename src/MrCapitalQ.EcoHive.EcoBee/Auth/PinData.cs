namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public record PinData
    {
        public required string Pin { get; init; }
        public required string AuthCode { get; init; }
        public required string Scope { get; init; }
        public required DateTimeOffset Expiration { get; init; }
    }
}
