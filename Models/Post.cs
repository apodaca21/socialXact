using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SocialX.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(140, ErrorMessage = "Máximo 140 caracteres.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }
    }
}
