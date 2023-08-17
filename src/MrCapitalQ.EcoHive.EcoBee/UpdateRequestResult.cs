namespace MrCapitalQ.EcoHive.EcoBee
{
    public record UpdateRequestResult
    {
        public required bool IsSuccessful { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}