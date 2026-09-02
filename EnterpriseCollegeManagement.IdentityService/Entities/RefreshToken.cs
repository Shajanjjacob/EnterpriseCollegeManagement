using System.ComponentModel.DataAnnotations;

namespace EnterpriseCollegeManagement.IdentityService.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedDate { get; set; }
        public ApplicationUser user { get; set; } = null;
    }
}
