using System.ComponentModel.DataAnnotations;

namespace MrCapitalQ.EcoHive.Api.Models
{
    public record AuthenticateRequest
    {
        [Required]
        public required string AuthCode { get; init; }
    }
}
