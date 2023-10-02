using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    [ExcludeFromCodeCoverage]
    public record EcoBeeClientOptions
    {
        [Required]
        public required string ApiKey { get; init; }
    }
}
