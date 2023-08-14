using System.ComponentModel.DataAnnotations;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public record AuthToken
    {
        public int Id { get; set; }

        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public DateTimeOffset Expiration { get; set; }
    }
}
