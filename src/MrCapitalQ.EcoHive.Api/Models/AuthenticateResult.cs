namespace MrCapitalQ.EcoHive.Api.Models
{
    public record AuthenticateResult
    {
        public required bool IsAuthenticated { get; init; }
    }
}
