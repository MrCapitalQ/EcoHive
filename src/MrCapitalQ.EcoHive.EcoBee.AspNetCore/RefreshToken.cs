using System.ComponentModel.DataAnnotations;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public required string Token { get; set; }
    }
}
